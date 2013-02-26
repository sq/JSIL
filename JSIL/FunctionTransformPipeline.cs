using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Transforms;
using JSIL.Translator;
using Mono.Cecil;

namespace JSIL.Internal {
    public delegate bool FunctionTransformPipelineStage ();

    public class FunctionTransformPipeline {
        public const int SuspendCountLogThreshold = 2;
        public const bool Trace = false;

        public readonly AssemblyTranslator Translator;
        public readonly QualifiedMemberIdentifier Identifier;
        public readonly JSFunctionExpression Function;
        public readonly HashSet<string> ParameterNames;
        public readonly Dictionary<string, JSVariable> Variables;
        public readonly SpecialIdentifiers SpecialIdentifiers;

        public readonly Queue<FunctionTransformPipelineStage> Pipeline = new Queue<FunctionTransformPipelineStage>();

        public int SuspendCount = 0;

        public FunctionTransformPipeline (
            AssemblyTranslator translator,
            QualifiedMemberIdentifier identifier, JSFunctionExpression function,
            SpecialIdentifiers si, HashSet<string> parameterNames,
            Dictionary<string, JSVariable> variables
        ) {
            Translator = translator;
            Identifier = identifier;
            Function = function;
            SpecialIdentifiers = si;
            ParameterNames = parameterNames;
            Variables = variables;

            FillPipeline();

            if (!Translator.FunctionCache.ActiveTransformPipelines.TryAdd(Identifier, this))
                throw new ThreadStateException();
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

            try {
                if (!Translator.FunctionCache.LockCacheEntryForTransformPipeline(Identifier))
                    throw new ThreadStateException("Failed to lock cache entry for '" + Identifier + "'");

                while (Pipeline.Count > 0) {
                    var currentStage = Pipeline.Peek();

                    try {
                        if (currentStage()) {
                            Pipeline.Dequeue();
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
                if (!Translator.FunctionCache.UnlockCacheEntryForTransformPipeline(Identifier, completed))
                    throw new ThreadStateException("Failed to unlock cache entry for '" + Identifier + "'");

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

            Enqueue(IntroduceVariableDeclarations);

            Enqueue(IntroduceVariableReferences);

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

            Enqueue(FixupPointerArithmetic);

            // If integer arithmetic hinting is enabled, we need to decompose mutation operators
            //  into normal binary operator expressions and/or comma expressions so that truncation can happen.
            Enqueue(DecomposeMutationOperators);
        }


        // Pipeline stage implementations

        private bool DecomposeMutationOperators () {
            if (Configuration.CodeGenerator.HintIntegerArithmetic.GetValueOrDefault(true))
                new DecomposeMutationOperators(TypeSystem, TypeInfoProvider).Visit(Function);

            return true;
        }

        private bool FixupPointerArithmetic () {
            if (Configuration.CodeGenerator.EnableUnsafeCode.GetValueOrDefault(false))
                new FixupPointerArithmetic(TypeSystem, MethodTypes).Visit(Function);

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
            if (Configuration.CodeGenerator.EliminateRedundantControlFlow.GetValueOrDefault(true))
                new ControlFlowSimplifier().Visit(Function);

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

        private bool IntroduceVariableReferences () {
            new IntroduceVariableReferences(
                SpecialIdentifiers.JSIL,
                Variables,
                ParameterNames
                ).Visit(Function);

            return true;
        }

        private bool IntroduceVariableDeclarations () {
            new IntroduceVariableDeclarations(
                Variables,
                TypeInfoProvider
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
                        Identifier, FunctionSource, TypeSystem, Variables
                    ), (visitor) => {
                        eliminated[0] = visitor.EliminatedVariables.Count > 0;
                    }))
                        return false;

                } while (eliminated[0]);
            }

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
