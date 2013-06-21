using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Threading;
using JSIL.Transforms;
using JSIL.Translator;
using Mono.Cecil;

namespace JSIL.Internal {
    public delegate bool FunctionTransformPipelineStage ();

    public class FunctionTransformPipeline {
        public const int SuspendCountLogThreshold = 2;
        public const bool CheckForStaticAnalysisChanges = false;
        public const bool Trace = false;

        public readonly AssemblyTranslator Translator;
        public readonly QualifiedMemberIdentifier Identifier;
        public readonly JSFunctionExpression Function;
        public readonly SpecialIdentifiers SpecialIdentifiers;

        public readonly Queue<FunctionTransformPipelineStage> Pipeline = new Queue<FunctionTransformPipelineStage>();

        public int SuspendCount = 0;

        private FunctionAnalysis2ndPass OriginalSecondPass;
        private string OriginalFunctionBody;

        public FunctionTransformPipeline (
            AssemblyTranslator translator,
            QualifiedMemberIdentifier identifier, JSFunctionExpression function,
            SpecialIdentifiers si
        ) {
            Translator = translator;
            Identifier = identifier;
            Function = function;
            SpecialIdentifiers = si;

            FillPipeline();

            if (!Translator.FunctionCache.ActiveTransformPipelines.TryAdd(Identifier, this))
                throw new ThreadStateException();

            if (CheckForStaticAnalysisChanges) {
                OriginalFunctionBody = Function.Body.ToString();
                OriginalSecondPass = Translator.FunctionCache.GetSecondPass(function.Method, function.Method.QualifiedIdentifier);
            }
        }

        public TypeSystem TypeSystem {
            get {
                return SpecialIdentifiers.TypeSystem;
            }
        }

        public MethodTypeFactory MethodTypes {
            get {
                return Translator.FunctionCache.MethodTypes;
            }
        }

        public IFunctionSource FunctionSource {
            get {
                return Translator.FunctionCache;
            }
        }

        public Configuration Configuration {
            get {
                return Translator.Configuration;
            }
        }

        public TypeInfoProvider TypeInfoProvider {
            get {
                return Translator._TypeInfoProvider;
            }
        }

        private bool RunStaticAnalysisDependentTransform<T> (T visitor, Action<T> postVisitAction = null)
            where T : StaticAnalysisJSAstVisitor 
        {
            try {
                visitor.Visit(Function);

                if (postVisitAction != null)
                    postVisitAction(visitor);

                return true;
            } catch (TemporarilySuspendTransformPipelineException exc) {
                if (Trace)
                    Console.WriteLine(
                        "Static analysis for '{2}::{3}' suspended due to dependency on '{0}::{1}'", 
                        exc.Identifier.Type.Name, exc.Identifier.Member.Name, 
                        Identifier.Type.Name, Identifier.Member.Name
                    );

                return false;
            }
        }

        public void Enqueue (FunctionTransformPipelineStage stage) {
            Pipeline.Enqueue(stage);
        }

        public bool RunUntilCompletion () {
            bool completed = false;

            var entry = Translator.FunctionCache.GetCacheEntry(Identifier);
            TrackedLockCollection.DeadlockInfo deadlock;
            var lockResult = entry.StaticAnalysisDataLock.TryBlockingEnter(out deadlock);

            if (!lockResult) {
                Console.Error.WriteLine(String.Format("Failed to lock '{0}' for transform pipeline: {1} {2}", Identifier, lockResult.FailureReason, deadlock));
                return false;
            }

            try {
                while (Pipeline.Count > 0) {
                    var currentStage = Pipeline.Peek();

                    try {
                        if (currentStage()) {
                            Pipeline.Dequeue();

                            if (CheckForStaticAnalysisChanges) {
                                var currentSecondPass = Translator.FunctionCache.GetSecondPass(this.Function.Method, this.Function.Method.QualifiedIdentifier);

                                string[] differences;
                                if (!currentSecondPass.Equals(OriginalSecondPass, out differences)) {
                                    var currentFunctionBody = Function.Body.ToString();

                                    Console.WriteLine("// Second pass data changed by pipeline stage '" + currentStage.Method.Name + "' - " + String.Join(", ", differences));
                                    Console.WriteLine("// Original function body //");
                                    Console.WriteLine(OriginalFunctionBody);
                                    Console.WriteLine("// New function body //");
                                    Console.WriteLine(currentFunctionBody);

                                    OriginalSecondPass = currentSecondPass;
                                    OriginalFunctionBody = currentFunctionBody;
                                }
                            }
                        } else {
                            SuspendCount += 1;

                            return (completed = false);
                        }
                    } catch (Exception exc) {
                        string functionName;

                        if ((Function.Method != null) && (Function.Method.Reference != null))
                            functionName = Function.Method.Reference.FullName;
                        else
                            functionName = Function.DisplayName ?? "<unknown>";

                        throw new FunctionTransformFailureException(functionName, currentStage.Method.Name, exc);
                    }
                }

                return (completed = true);
            } finally {
                entry.TransformPipelineHasCompleted |= completed;
                entry.StaticAnalysisDataLock.Exit();

                if (completed) {
                    FunctionTransformPipeline temp;
                    if (!Translator.FunctionCache.ActiveTransformPipelines.TryRemove(Identifier, out temp))
                        throw new ThreadStateException();
                } else {
                    if (!Translator.FunctionCache.PendingTransformsQueue.TryEnqueue(Identifier))
                        throw new ThreadStateException();
                }
            }
        }

        private void FillPipeline () {
            Enqueue(BuildLabelGroups);

            Enqueue(EliminateTemporaries);

            Enqueue(EmulateInt64);

            Enqueue(EmulateStructAssignment);

            Enqueue(IntroduceVariableDeclarationsAndReferences);

            Enqueue(SimplifyLoops);

            // Temporary elimination makes it possible to simplify more operators, so do it later
            Enqueue(SimplifyOperators);

            Enqueue(ReplaceMethodCalls);

            Enqueue(HandleBooleanAsInteger);

            Enqueue(IntroduceCharCasts);

            Enqueue(IntroduceEnumCasts);

            Enqueue(ExpandCastExpressions);

            // We need another operator simplification pass to simplify expressions created by cast expressions
            Enqueue(SimplifyOperators);

            Enqueue(DeoptimizeSwitchStatements);

            Enqueue(CollapseNulls);

            Enqueue(FixupStructConstructorInvocations);

            Enqueue(EliminateTemporaries);

            Enqueue(SimplifyControlFlow);

            Enqueue(EliminatePointlessFinallyBlocks);

            Enqueue(OptimizeArrayEnumerators);

            // We need another loop simplification pass because control flow has probably changed after the previous passes
            Enqueue(SimplifyLoops);

            Enqueue(EliminateUnusedLoopNames);

            // We need to expand cast expressions again because previous passes may have made some more necessary
            Enqueue(ExpandCastExpressions);

            Enqueue(OptimizeAccessorMethods);

            Enqueue(IntroducePackedArrays);

            Enqueue(FixupPointerArithmetic);

            // If integer arithmetic hinting is enabled, we need to decompose mutation operators
            //  into normal binary operator expressions and/or comma expressions so that truncation can happen.
            Enqueue(DecomposeMutationOperators);

            Enqueue(HoistStructAllocations);
        }


        // Pipeline stage implementations

        private bool DecomposeMutationOperators () {
            if (Configuration.CodeGenerator.HintIntegerArithmetic.GetValueOrDefault(true))
                new DecomposeMutationOperators(TypeSystem, TypeInfoProvider).Visit(Function);

            return true;
        }

        private bool FixupPointerArithmetic () {
            if (Configuration.CodeGenerator.EnableUnsafeCode.GetValueOrDefault(false))
                new UnsafeCodeTransforms(TypeSystem, MethodTypes).Visit(Function);

            return true;
        }

        private bool IntroducePackedArrays () {
            new IntroducePackedArrays(TypeSystem, MethodTypes).Visit(Function);

            return true;
        }

        private bool OptimizeAccessorMethods () {
            if (Configuration.CodeGenerator.PreferAccessorMethods.GetValueOrDefault(true)) {
                new OptimizePropertyMutationAssignments(
                    TypeSystem, TypeInfoProvider
                    ).Visit(Function);

                new ConvertPropertyAccessesToInvocations(
                    TypeSystem, TypeInfoProvider
                    ).Visit(Function);
            }

            return true;
        }

        private bool EliminateUnusedLoopNames () {
            var lnd = new LoopNameDetector();
            lnd.Visit(Function);
            lnd.EliminateUnusedLoopNames();

            return true;
        }

        private bool OptimizeArrayEnumerators () {
            return RunStaticAnalysisDependentTransform(
                new OptimizeArrayEnumerators(Identifier, FunctionSource, TypeSystem)
            );
        }

        private bool EliminatePointlessFinallyBlocks () {
            return RunStaticAnalysisDependentTransform(
                new EliminatePointlessFinallyBlocks(Identifier, FunctionSource, TypeSystem, TypeInfoProvider)
            );
        }

        private bool SimplifyControlFlow () {
            if (Configuration.CodeGenerator.EliminateRedundantControlFlow.GetValueOrDefault(true)) {
                bool shouldCollapse = false;
                bool shouldRun = true;

                while (shouldRun) {
                    var cfs = new ControlFlowSimplifier();
                    cfs.Visit(Function);
                    shouldRun = cfs.MadeChanges;
                    shouldCollapse |= cfs.MadeChanges;
                }

                // HACK: Control flow simplification probably generated lots of nulls, so let's collapse them.
                // This makes it possible for loop simplification to work right later on.
                if (shouldCollapse)
                    CollapseNulls();
            }

            return true;
        }

        private bool FixupStructConstructorInvocations () {
            new FixupStructConstructorInvocations(TypeSystem).Visit(Function);

            return true;
        }

        private bool CollapseNulls () {
            new CollapseNulls().Visit(Function);

            return true;
        }

        private bool DeoptimizeSwitchStatements () {
            new DeoptimizeSwitchStatements(TypeSystem).Visit(Function);

            return true;
        }

        private bool ExpandCastExpressions () {
            new ExpandCastExpressions(
                TypeSystem, SpecialIdentifiers.JS, SpecialIdentifiers.JSIL, TypeInfoProvider, MethodTypes
                ).Visit(Function);

            return true;
        }

        private bool IntroduceEnumCasts () {
            new IntroduceEnumCasts(
                TypeSystem, SpecialIdentifiers.JS, TypeInfoProvider, MethodTypes
                ).Visit(Function);

            return true;
        }

        private bool IntroduceCharCasts () {
            new IntroduceCharCasts(
                TypeSystem, SpecialIdentifiers.JS
                ).Visit(Function);

            return true;
        }

        private bool HandleBooleanAsInteger () {
            new HandleBooleanAsInteger(
                TypeSystem, SpecialIdentifiers.JS
                ).Visit(Function);

            return true;
        }

        private bool ReplaceMethodCalls () {
            new ReplaceMethodCalls(
                Function.Method.Reference,
                SpecialIdentifiers.JSIL, SpecialIdentifiers.JS, TypeSystem
                ).Visit(Function);

            return true;
        }

        private bool SimplifyOperators () {
            if (Configuration.CodeGenerator.SimplifyOperators.GetValueOrDefault(true))
                new SimplifyOperators(
                    SpecialIdentifiers.JSIL, SpecialIdentifiers.JS, TypeSystem
                    ).Visit(Function);

            return true;
        }

        private bool SimplifyLoops () {
            if (Configuration.CodeGenerator.SimplifyLoops.GetValueOrDefault(true))
                new SimplifyLoops(
                    TypeSystem, false
                    ).Visit(Function);

            return true;
        }

        private bool IntroduceVariableDeclarationsAndReferences () {
            new IntroduceVariableDeclarations(
                Function.AllVariables,
                TypeInfoProvider
                ).Visit(Function);

            new IntroduceVariableReferences(
                SpecialIdentifiers.JSIL,
                Function.AllVariables
                ).Visit(Function);

            return true;
        }

        private bool EmulateStructAssignment () {
            return RunStaticAnalysisDependentTransform(
                new EmulateStructAssignment(
                    Identifier, FunctionSource,
                    TypeSystem,
                    TypeInfoProvider,
                    SpecialIdentifiers.CLR,
                    Configuration.CodeGenerator.EliminateStructCopies.GetValueOrDefault(true)
                )
            );
        }

        private bool EmulateInt64 () {
            new EmulateInt64(
                MethodTypes,
                SpecialIdentifiers.TypeSystem
                ).Visit(Function);

            return true;
        }

        private bool BuildLabelGroups () {
            var la = new LabelAnalyzer();
            la.BuildLabelGroups(Function);

            return true;
        }

        private bool EliminateTemporaries () {
            if (Translator.Configuration.CodeGenerator.EliminateTemporaries.GetValueOrDefault(true)) {
                var eliminated = new[] { false };

                do {
                    if (!RunStaticAnalysisDependentTransform(new EliminateSingleUseTemporaries(
                        Identifier, FunctionSource, TypeSystem, Function.AllVariables, TypeInfoProvider
                    ), (visitor) => {
                        eliminated[0] = visitor.EliminatedVariables.Count > 0;
                    }))
                        return false;

                } while (eliminated[0]);
            }

            return true;
        }

        private bool HoistStructAllocations () {
            new HoistStructAllocations(
                Identifier, FunctionSource, TypeSystem, MethodTypes
            ).Visit(Function);

            return true;
        }
    }

    public class FunctionTransformFailureException : Exception {
        public FunctionTransformFailureException (string functionName, string pipelineStageName, Exception innerException)
            : base (
                String.Format(
                    "Function transform pipeline stage '{1}' failed on function '{0}':", 
                    functionName, pipelineStageName
                ),
                innerException
            )
        {
        }
    }
}
