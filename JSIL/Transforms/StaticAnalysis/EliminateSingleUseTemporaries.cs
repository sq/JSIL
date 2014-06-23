using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class EliminateSingleUseTemporaries : StaticAnalysisJSAstVisitor {
        public static bool DryRun = false;
        public static int TraceLevel = 0;

        public readonly TypeSystem TypeSystem;
        public readonly ITypeInfoSource TypeInfo;
        public readonly Dictionary<string, JSVariable> Variables;
        public readonly HashSet<JSVariable> EliminatedVariables = new HashSet<JSVariable>();

        protected readonly HashSet<string> VariablesExemptedFromEffectivelyConstantStatus = new HashSet<string>();

        protected FunctionAnalysis1stPass FirstPass = null;

        public EliminateSingleUseTemporaries (
            QualifiedMemberIdentifier member, IFunctionSource functionSource, 
            TypeSystem typeSystem, Dictionary<string, JSVariable> variables,
            ITypeInfoSource typeInfo
        ) : base (member, functionSource) {
            TypeSystem = typeSystem;
            Variables = variables;
            TypeInfo = typeInfo;
        }

        protected void EliminateVariable (JSNode context, JSVariable variable, JSExpression replaceWith, QualifiedMemberIdentifier method) {
            {
                var replacer = new VariableEliminator(
                    variable,
                    JSChangeTypeExpression.New(replaceWith, variable.GetActualType(TypeSystem), TypeSystem)
                );
                replacer.Visit(context);
            }

            {
                var replacer = new VariableEliminator(variable, replaceWith);
                var assignments = (from a in FirstPass.Assignments where 
                                       variable.Equals(a.NewValue) ||
                                       a.NewValue.SelfAndChildrenRecursive.Any(variable.Equals)
                                       select a).ToArray();

                foreach (var a in assignments) {
                    if (!variable.Equals(a.NewValue))
                        replacer.Visit(a.NewValue);
                }
            }

            Variables.Remove(variable.Identifier);
            FunctionSource.InvalidateFirstPass(method);
        }

        protected bool IsEffectivelyConstant (JSVariable target, JSExpression source) {
            if ((source == null) || (source.IsNull))
                return false;

            // Can't eliminate struct temporaries, since that might eliminate some implied copies.
            if (TypeUtil.IsStruct(target.IdentifierType))
                return false;

            // Handle special cases where our interpretation of 'constant' needs to be more flexible
            {
                var ie = source as JSIndexerExpression;
                if (ie != null) {
                    if (
                        IsEffectivelyConstant(target, ie.Target) &&
                        IsEffectivelyConstant(target, ie.Index)
                    )
                        return true;
                }
            }

            {
                var ae = source as JSArrayExpression;
                if (
                    (ae != null) &&
                    (from av in ae.Values select IsEffectivelyConstant(target, av)).All((b) => b)
                )
                    return true;
            }

            {
                var de = source as JSDotExpressionBase;
                if (
                    (de != null) &&
                    IsEffectivelyConstant(target, de.Target) &&
                    IsEffectivelyConstant(target, de.Member)
                ) {
                    var pa = source as JSPropertyAccess;
                    if (pa != null) {
                        // Property accesses must not be treated as constant since they call functions
                        // TODO: Use static analysis information to figure out whether the accessor is pure/has state dependencies
                        return false;
                    }

                    return true;
                }
            }

            {
                var ie = source as JSInvocationExpression;
                if (
                    (ie != null) && ie.ConstantIfArgumentsAre &&
                    IsEffectivelyConstant(target, ie.ThisReference) &&
                    ie.Arguments.All((a) => IsEffectivelyConstant(target, a))
                )
                    return true;

                if ((ie != null) && (ie.JSMethod != null)) {
                    var sa = GetSecondPass(ie.JSMethod);
                    if (sa != null) {
                        if (sa.IsPure) {
                            if (ie.Arguments.All((a) => IsEffectivelyConstant(target, a)))
                                return true;
                            else
                                return false;
                        }
                    }
                }
            }

            if ((source is JSUnaryOperatorExpression) || (source is JSBinaryOperatorExpression)) {
                if (source.Children.OfType<JSExpression>().All((_v) => IsEffectivelyConstant(target, _v)))
                    return true;
            }

            if (source.IsConstant)
                return true;

            // Try to find a spot between the source variable's assignments where all of our
            //  copies and accesses can fit. If we find one, our variable is effectively constant.
            // FIXME: I think this section might be fundamentally flawed. Do let me know if you agree. :)
            var v = source as JSVariable;
            if (v != null) {
                // Ensure that we never treat a local variable as constant if functions we call allow it to escape
                //  or modify it, because that can completely invalidate our purity analysis.
                if (VariablesExemptedFromEffectivelyConstantStatus.Contains(v.Identifier))
                    return false;

                var sourceAssignments = (from a in FirstPass.Assignments where v.Identifier.Equals(a.Target) select a).ToArray();
                if (sourceAssignments.Length < 1)
                    return v.IsParameter;

                var sourceAccesses = (from a in FirstPass.Accesses where v.Identifier.Equals(a.Source) select a).ToArray();
                if (sourceAccesses.Length < 1)
                    return false;

                var targetAssignmentIndices = (from a in FirstPass.Assignments where target.Identifier.Equals(a.Target) select a.StatementIndex);
                var targetAccessIndices = (from a in FirstPass.Accesses where target.Identifier.Equals(a.Source) select a.StatementIndex).ToArray();
                var targetUseIndices = targetAccessIndices.Concat(targetAssignmentIndices).ToArray();

                if (sourceAssignments.Length == 1) {
                    if (targetAccessIndices.All((tai) => tai > sourceAssignments[0].StatementIndex))
                        return true;
                }

                var sourceFirstAssigned = sourceAssignments.First();
                var sourceLastAssigned = sourceAssignments.Last();

                var sourceFirstAccessed = sourceAccesses.First();
                var sourceLastAccessed = sourceAccesses.Last();

                bool foundAssignmentSlot = false;

                for (int i = 0, c = targetUseIndices.Length; i < c; i++) {
                    int assignment = targetUseIndices[i], nextAssignment = int.MaxValue;
                    if (i < c - 1)
                        nextAssignment = targetUseIndices[i + 1];

                    if (
                        (sourceFirstAssigned.StatementIndex >= assignment) &&
                        (sourceLastAssigned.StatementIndex < nextAssignment) &&
                        (sourceFirstAccessed.StatementIndex >= assignment) &&
                        (sourceLastAccessed.StatementIndex <= nextAssignment)
                    ) {
                        if (TraceLevel >= 5)
                            Console.WriteLine("Found assignment slot for {0} <- {1} between {2} and {3}", target, source, assignment, nextAssignment);

                        foundAssignmentSlot = true;
                        break;
                    }
                }

                if (!foundAssignmentSlot)
                    return false;

                return true;
            }

            return false;
        }

        protected void ExemptVariablesFromEffectivelyConstantStatus () {
            foreach (var invocation in FirstPass.Invocations) {
                var invocationSecondPass = GetSecondPass(invocation.Method);

                if (invocationSecondPass == null) {
                    foreach (var kvp in invocation.Variables) {
                        foreach (var variableName in kvp.Value.ToEnumerable()) {
                            if (!VariablesExemptedFromEffectivelyConstantStatus.Contains(variableName)) {
                                if (TraceLevel >= 2)
                                    Console.WriteLine("Exempting variable '{0}' from effectively constant status because it is passed to {1} (no static analysis data)", variableName, invocation.Method ?? invocation.NonJSMethod);
                            }

                            VariablesExemptedFromEffectivelyConstantStatus.Add(variableName);
                        }
                    }

                } else {

                    foreach (var kvp in invocation.Variables) {
                        var argumentName = kvp.Key;
                        string reason = null;

                        if (
                            (invocationSecondPass.Data != null) &&
                            invocationSecondPass.Data.SideEffects.Any((se) => se.Variable == argumentName)
                        ) {
                            reason = "touches it with side effects";
                        } else if (                            
                            invocationSecondPass.EscapingVariables.Contains(argumentName)
                        ) {
                            reason = "allows it to escape";
                        }

                        if (reason != null) {
                            foreach (var variableName in kvp.Value.ToEnumerable()) {
                                if (ShouldExemptVariableFromEffectivelyConstantStatus(variableName)) {
                                    if (!VariablesExemptedFromEffectivelyConstantStatus.Contains(variableName)) {
                                        if (TraceLevel >= 2)
                                            Console.WriteLine("Exempting variable '{0}' from effectively constant status because {1} {2}", variableName, invocation.Method ?? invocation.NonJSMethod, reason);
                                    }

                                    VariablesExemptedFromEffectivelyConstantStatus.Add(variableName);
                                }
                            }

                        }
                    }
                }
            }
        }

        private bool ShouldExemptVariableFromEffectivelyConstantStatus (string variableName) {
            // FIXME: Why does this happen?
            if (!Variables.ContainsKey(variableName))
                return false;

            var actualVariable = Variables[variableName];
            var variableType = actualVariable.GetActualType(TypeSystem);

            // Structs and primitives won't be mutated by functions we pass them to (we ensure this)
            if (!TypeUtil.IsReferenceType(variableType))
                return false;

            // Strings are immutable. Woot!
            if (variableType.FullName == "System.String")
                return false;

            var variableTypeInfo = TypeInfo.Get(variableType);

            // The object itself is immutable, so this is probably okay.
            // FIXME: Transitive modification of the immutable type's members could be a problem here?
            if ((variableTypeInfo != null) && (variableTypeInfo.IsImmutable))
                return false;

            return true;
        }

        public void VisitNode (JSFunctionExpression fn) {
            FirstPass = GetFirstPass(fn.Method.QualifiedIdentifier);
            if (FirstPass == null)
                throw new InvalidOperationException(String.Format(
                    "No first pass static analysis data for method '{0}'",
                    fn.Method.QualifiedIdentifier
                ));

            ExemptVariablesFromEffectivelyConstantStatus();

            foreach (var v in fn.AllVariables.Values.ToArray()) {
                if (v.IsThis || v.IsParameter)
                    continue;

                var assignments = (from a in FirstPass.Assignments where v.Identifier.Equals(a.Target) select a).ToArray();
                var reassignments = (from a in FirstPass.Assignments where v.Identifier.Equals(a.SourceVariable) select a).ToArray();
                var accesses = (from a in FirstPass.Accesses where v.Identifier.Equals(a.Source) select a).ToArray();
                var invocations = (from i in FirstPass.Invocations where v.Name == i.ThisVariable select i).ToArray();
                var unsafeInvocations = FilterInvocations(invocations);
                var isPassedByReference = FirstPass.VariablesPassedByRef.Contains(v.Name);

                if (assignments.FirstOrDefault() == null) {
                    if ((accesses.Length == 0) && (invocations.Length == 0) && (reassignments.Length == 0) && !isPassedByReference) {
                        if (TraceLevel >= 1)
                            Console.WriteLine(String.Format("Eliminating {0} because it is never used.", v));

                        if (!DryRun) {
                            EliminatedVariables.Add(v);
                            EliminateVariable(fn, v, new JSEliminatedVariable(v), fn.Method.QualifiedIdentifier);

                            // We've invalidated the static analysis data so the best choice is to abort.
                            break;
                        }
                    } else {
                        if (TraceLevel >= 2)
                            Console.WriteLine(String.Format("Never found an initial assignment for {0}.", v));
                    }

                    continue;
                }

                var valueType = v.GetActualType(TypeSystem);
                if (TypeUtil.IsIgnoredType(valueType))
                    continue;

                if (FirstPass.VariablesPassedByRef.Contains(v.Name)) {
                    if (TraceLevel >= 2)
                        Console.WriteLine(String.Format("Cannot eliminate {0}; it is passed by reference.", v));

                    continue;
                }

                if (unsafeInvocations.Length > 1) {
                    if (TraceLevel >= 2)
                        Console.WriteLine(String.Format("Cannot eliminate {0}; methods are invoked on it multiple times that are not provably safe.", v));

                    continue;
                }

                if ((from a in accesses where a.IsControlFlow select a).FirstOrDefault() != null) {
                    if (TraceLevel >= 2)
                        Console.WriteLine(String.Format("Cannot eliminate {0}; it participates in control flow.", v));

                    continue;
                }

                if (assignments.Length > 1) {
                    if (TraceLevel >= 2)
                        Console.WriteLine(String.Format("Cannot eliminate {0}; it is reassigned.", v));

                    continue;
                }

                var replacementAssignment = assignments.First();
                var replacement = replacementAssignment.NewValue;
                if (replacement.SelfAndChildrenRecursive.Contains(v)) {
                    if (TraceLevel >= 2)
                        Console.WriteLine(String.Format("Cannot eliminate {0}; it contains a self-reference.", v));

                    continue;
                }

                var copies = (from a in FirstPass.Assignments where v.Identifier.Equals(a.SourceVariable) select a).ToArray();
                if (
                    (copies.Length + accesses.Length) > 1
                ) {
                    if (replacement is JSLiteral) {
                        if (TraceLevel >= 5)
                            Console.WriteLine(String.Format("Skipping veto of elimination for {0} because it is a literal.", v));
                    } else {
                        if (TraceLevel >= 2)
                            Console.WriteLine(String.Format("Cannot eliminate {0}; it is used multiple times.", v));

                        continue;
                    }
                }

                if (!IsEffectivelyConstant(v, replacement)) {
                    if (TraceLevel >= 2)
                        Console.WriteLine(String.Format("Cannot eliminate {0}; {1} is not a constant expression.", v, replacement));

                    continue;
                }

                var replacementField = JSPointerExpressionUtil.UnwrapExpression(replacement) as JSFieldAccess;
                if (replacementField == null) {
                    var replacementRef = replacement as JSReferenceExpression;
                    if (replacementRef != null)
                        replacementField = replacementRef.Referent as JSFieldAccess;
                }

                var _affectedFields = replacement.SelfAndChildrenRecursive.OfType<JSField>();
                if (replacementField != null)
                    _affectedFields = _affectedFields.Concat(new[] { replacementField.Field });

                var affectedFields = new HashSet<FieldInfo>((from jsf in _affectedFields select jsf.Field));
                _affectedFields = null;

                if ((affectedFields.Count > 0) || (replacementField != null))
                {
                    var firstAssignment = assignments.FirstOrDefault();
                    var lastAccess = accesses.LastOrDefault();

                    bool invalidatedByLaterFieldAccess = false;

                    foreach (var fieldAccess in FirstPass.FieldAccesses) {
                        // Note that we only compare the FieldInfo, not the this-reference.
                        // Otherwise, aliasing (accessing the same field through two this references) would cause us
                        //  to incorrectly eliminate a local.
                        if (!affectedFields.Contains(fieldAccess.Field.Field))
                            continue;

                        // Ignore field accesses before the replacement was initialized
                        if (fieldAccess.NodeIndex <= replacementAssignment.NodeIndex)
                            continue;

                        // If the field access comes after the last use of the temporary, we don't care
                        if ((lastAccess != null) && (fieldAccess.StatementIndex > lastAccess.StatementIndex))
                            continue;

                        // It's a read, so no impact on whether this optimization is valid
                        if (fieldAccess.IsRead)
                            continue;

                        if (TraceLevel >= 2)
                            Console.WriteLine(String.Format("Cannot eliminate {0}; {1} is potentially mutated later", v, fieldAccess.Field.Field));

                        invalidatedByLaterFieldAccess = true;
                        break;
                    }

                    if (invalidatedByLaterFieldAccess)
                        continue;

                    foreach (var invocation in FirstPass.Invocations) {
                        // If the invocation comes after (or is) the last use of the temporary, we don't care
                        if ((lastAccess != null) && (invocation.StatementIndex >= lastAccess.StatementIndex))
                            continue;

                        // Same goes for the first assignment.
                        if ((firstAssignment != null) && (invocation.StatementIndex <= firstAssignment.StatementIndex))
                            continue;

                        var invocationSecondPass = GetSecondPass(invocation.Method);
                        if (
                            (invocationSecondPass == null) ||
                            (invocationSecondPass.MutatedFields == null)
                        ) {
                            if (invocation.Variables.Any((kvp) => kvp.Value.ToEnumerable().Contains(v.Identifier))) {
                                if (TraceLevel >= 2)
                                    Console.WriteLine(String.Format("Cannot eliminate {0}; a method call without field mutation data ({1}) is invoked between its initialization and use with it as an argument", v, invocation.Method ?? invocation.NonJSMethod));

                                invalidatedByLaterFieldAccess = true;
                            }
                        } else if (affectedFields.Any(invocationSecondPass.FieldIsMutatedRecursively)) {
                            if (TraceLevel >= 2)
                                Console.WriteLine(String.Format("Cannot eliminate {0}; a method call ({1}) potentially mutates a field it references", v, invocation.Method ?? invocation.NonJSMethod));

                            invalidatedByLaterFieldAccess = true;
                        }
                    }

                    if (invalidatedByLaterFieldAccess)
                        continue;
                }

                if (TraceLevel >= 1)
                    Console.WriteLine(String.Format("Eliminating {0} <- {1}", v, replacement));

                if (!DryRun) {
                    EliminatedVariables.Add(v);
                    EliminateVariable(fn, v, replacement, fn.Method.QualifiedIdentifier);

                    // We've invalidated the static analysis data so the best choice is to abort.
                    break;
                }
            }
        }

        // Filters out invocations that are provably safe to ignore, based on proxy attributes/type info
        private FunctionAnalysis1stPass.Invocation[] FilterInvocations (FunctionAnalysis1stPass.Invocation[] invocations) {
            return invocations.Where(
                (invocation) => {
                    if (invocation.Method == null)
                        return true;

                    var secondPass = GetSecondPass(invocation.Method);
                    if (secondPass == null)
                        return true;

                    return (!secondPass.IsPure);
                }
            ).ToArray();
        }
    }

    public class VariableEliminator : JSAstVisitor {
        public readonly JSVariable Variable;
        public readonly JSExpression Replacement;

        public VariableEliminator (JSVariable variable, JSExpression replacement) {
            Variable = variable;
            Replacement = replacement;
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            if ((boe.Operator == JSOperator.Assignment) && (boe.Left.Equals(Variable))) {
                // This is important because we could be eliminating an assignment that looks like:
                //  x = y = 5;
                // And if we simply replace 'y = 5' with an null, everything is ruined.
                if ((ParentNode is JSExpressionStatement) || (ParentNode is JSVariableDeclarationStatement))
                    ParentNode.ReplaceChild(boe, new JSNullExpression());
                else if (ParentNode != null)
                    ParentNode.ReplaceChild(boe, boe.Right);
            } else {
                VisitChildren(boe);
            }
        }

        public void VisitNode (JSVariable variable) {
            if (CurrentName == "FunctionSignature") {
                // In argument list
                return;
            }

            if (Variable.Equals(variable)) {
                ParentNode.ReplaceChild(variable, Replacement);
            } else {
                VisitChildren(variable);
            }
        }
    }
}
