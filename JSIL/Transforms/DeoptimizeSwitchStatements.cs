using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class DeoptimizeSwitchStatements : JSAstVisitor {
        public class VariableComparer : IEqualityComparer<JSVariable> {
            public bool Equals (JSVariable x, JSVariable y) {
                if (x == null)
                    return (x == y);
                else
                    return x.Equals(y);
            }

            public int GetHashCode (JSVariable obj) {
                return obj.GetHashCode();
            }
        }

        public class NullCheck {
            public JSIfStatement Statement;
            public JSVariable SwitchVariable;
            public JSGotoExpression Goto;
        }

        public class Initializer {
            public JSIfStatement Statement;
            public FieldInfo Field;
            public readonly Dictionary<int, JSExpression> Values = new Dictionary<int,JSExpression>();
        }

        public class IndexLookup {
            public JSIfStatement Statement;
            public FieldInfo Field;
            public JSVariable SwitchVariable;
            public JSVariable OutputVariable;
            public JSGotoExpression Goto;
            public bool IsInverted;
        }

        private JSFunctionExpression CurrentFunction = null;

        public readonly Dictionary<JSVariable, NullCheck> NullChecks = new Dictionary<JSVariable, NullCheck>(
            new VariableComparer()            
        );
        public readonly Dictionary<FieldInfo, Initializer> Initializers = new Dictionary<FieldInfo, Initializer>();
        public readonly Dictionary<JSVariable, IndexLookup> IndexLookups = new Dictionary<JSVariable, IndexLookup>(
            new VariableComparer()
        );

        public int SwitchStatementsDeoptimized = 0;
        public readonly TypeSystem TypeSystem;

        public DeoptimizeSwitchStatements (TypeSystem typeSystem) {
            TypeSystem = typeSystem;
        }

        public void VisitNode (JSFunctionExpression fe) {
            CurrentFunction = fe;

            VisitChildren(fe);
        }

        public void VisitNode (JSIfStatement ifs) {
            var boe = ifs.Condition as JSBinaryOperatorExpression;
            var uoe = ifs.Condition as JSUnaryOperatorExpression;
            var invocation = ifs.Condition as JSInvocationExpression;
            bool invocationIsInverted = false;

            if ((boe != null) && (boe.Operator == JSOperator.Equal)) {
                var leftVar = boe.Left as JSVariable;
                var leftIgnored = boe.Left as JSIgnoredMemberReference;
                var rightNull = (JSLiteral)(boe.Right as JSDefaultValueLiteral) ?? (JSLiteral)(boe.Right as JSNullLiteral);

                if (rightNull != null) {
                    if (leftVar != null) {
                        NullChecks[leftVar] = new NullCheck {
                            Statement = ifs,
                            SwitchVariable = leftVar,
                            Goto = ifs.AllChildrenRecursive.OfType<JSGotoExpression>().FirstOrDefault()
                        };
                    } else if (leftIgnored != null) {
                        var leftField = leftIgnored.Member as FieldInfo;

                        if (leftField != null) {
                            var initializer = Initializers[leftField] = new Initializer {
                                Field = leftField,
                                Statement = ifs,
                            };

                            foreach (var _invocation in ifs.TrueClause.AllChildrenRecursive.OfType<JSInvocationExpression>()) {
                                if (_invocation.JSMethod == null)
                                    continue;
                                if (_invocation.JSMethod.Identifier != "Add")
                                    continue;
                                if (_invocation.Arguments.Count != 2)
                                    continue;

                                var value = _invocation.Arguments[0];
                                var index = _invocation.Arguments[1] as JSIntegerLiteral;
                                if (index == null)
                                    continue;

                                initializer.Values[(int)index.Value] = value;
                            }

                            foreach (var iae in ifs.TrueClause.AllChildrenRecursive.OfType<JSInitializerApplicationExpression>()) {
                                var targetNew = iae.Target as JSNewExpression;
                                if (targetNew == null)
                                    continue;

                                var targetNewJSType = targetNew.Type as JSType;
                                if (targetNewJSType == null)
                                    continue;

                                var targetNewType = targetNewJSType.Type as GenericInstanceType;
                                if (targetNewType == null)
                                    continue;
                                if (!targetNewType.Name.Contains("Dictionary"))
                                    continue;
                                if (targetNewType.GenericArguments.Count != 2)
                                    continue;
                                if (targetNewType.GenericArguments[1].MetadataType != MetadataType.Int32)
                                    continue;

                                var initArray = iae.Initializer as JSArrayExpression;
                                if (initArray == null)
                                    continue;

                                foreach (var item in initArray.Values) {
                                    var itemArray = item as JSArrayExpression;
                                    if (itemArray == null)
                                        continue;

                                    var value = itemArray.Values.First();
                                    var index = itemArray.Values.Skip(1).First() as JSIntegerLiteral;
                                    if (index == null)
                                        continue;

                                    initializer.Values[(int)index.Value] = value;
                                }
                            }
                        }
                    }
                }
            } else if ((uoe != null) && (uoe.Operator == JSOperator.LogicalNot)) {
                var nestedUoe = uoe.Expression as JSUnaryOperatorExpression;

                if (
                    (nestedUoe != null) &&
                    (nestedUoe.Operator == JSOperator.LogicalNot)
                ) {
                    invocation = nestedUoe.Expression as JSInvocationExpression;
                    invocationIsInverted = false;
                } else {
                    invocation = uoe.Expression as JSInvocationExpression;
                    invocationIsInverted = true;
                }
            }

            if (
                (invocation != null) &&
                (invocation.Arguments.Count == 2) &&
                (invocation.JSMethod != null) &&
                (invocation.JSMethod.Identifier == "TryGetValue")
            ) {
                var thisIgnored = invocation.ThisReference as JSIgnoredMemberReference;
                var switchVar = invocation.Arguments[0] as JSVariable;
                var outRef = invocation.Arguments[1] as JSPassByReferenceExpression;

                if ((thisIgnored != null) && (switchVar != null) && (outRef != null)) {
                    var thisField = thisIgnored.Member as FieldInfo;
                    var outReferent = outRef.Referent as JSReferenceExpression;

                    if ((thisField != null) && (outReferent != null)) {
                        var outVar = outReferent.Referent as JSVariable;

                        if (outVar != null) {
                            IndexLookups[outVar] = new IndexLookup {
                                OutputVariable = outVar,
                                SwitchVariable = switchVar,
                                Field = thisField,
                                Statement = ifs,
                                Goto = ifs.TrueClause.AllChildrenRecursive.OfType<JSGotoExpression>().FirstOrDefault(),
                                IsInverted = invocationIsInverted
                            };

                            if (!invocationIsInverted) {
                                var replacement = ifs.TrueClause;
                                ParentNode.ReplaceChild(ifs, replacement);
                                VisitReplacement(replacement);
                                return;
                            }
                        }
                    }
                }
            }

            VisitChildren(ifs);
        }

        public static IEnumerable<JSGotoExpression> FindGotos (JSNode context, string targetLabel) {
            return context.AllChildrenRecursive.OfType<JSGotoExpression>()
                .Where((ge) => ge.TargetLabel == targetLabel);
        }

        public void VisitNode (JSSwitchStatement ss) {
            IndexLookup indexLookup;
            Initializer initializer;
            NullCheck nullCheck;

            // Detect switch statements using a lookup dictionary
            var switchVar = ss.Condition as JSVariable;

            // HACK: Fixup reference read-throughs
            var switchRefRead = ss.Condition as JSReadThroughReferenceExpression;
            if (switchRefRead != null)
                switchVar = switchRefRead.Variable;

            if (
                (switchVar != null) && 
                IndexLookups.TryGetValue(switchVar, out indexLookup) &&
                Initializers.TryGetValue(indexLookup.Field, out initializer)
            ) {
                if (NullChecks.TryGetValue(indexLookup.SwitchVariable, out nullCheck))
                    CurrentFunction.ReplaceChildRecursive(nullCheck.Statement, new JSNullStatement());

                CurrentFunction.ReplaceChildRecursive(initializer.Statement, new JSNullStatement());

                if (indexLookup.IsInverted)
                    CurrentFunction.ReplaceChildRecursive(indexLookup.Statement, new JSNullStatement());

                var switchCases = new List<JSSwitchCase>();
                JSExpression[] values;
                foreach (var cse in ss.Cases) {
                    var body = cse.Body;

                    if (cse.Values == null) {
                        values = null;
                        body = new JSBlockStatement(body.Statements.ToArray());
                    } else {
                        values = (from v in cse.Values
                         let il = v as JSIntegerLiteral
                         where il != null
                         select initializer.Values[(int)il.Value]).ToArray();
                    }

                    switchCases.Add(new JSSwitchCase(
                        values, body, cse.IsDefault
                    ));
                }

                var newSwitch = new JSSwitchStatement(
                    indexLookup.SwitchVariable, switchCases.ToArray()
                );

                SwitchStatementsDeoptimized += 1;

                ParentNode.ReplaceChild(ss, newSwitch);

                var outVar = indexLookup.OutputVariable;
                foreach (var fn in Stack.OfType<JSFunctionExpression>()) {
                    foreach (var vds in fn.Body.AllChildrenRecursive.OfType<JSVariableDeclarationStatement>().ToArray()) {
                        for (int i = 0, c = vds.Declarations.Count; i < c; i++) {
                            var leftVar = vds.Declarations[i].Left as JSVariable;

                            if ((leftVar != null) && (leftVar.Identifier == outVar.Identifier)) {
                                vds.Declarations.RemoveAt(i);
                                i--;
                                c--;
                            }
                        }
                    }

                    fn.AllVariables.Remove(outVar.Identifier);
                }

                VisitReplacement(newSwitch);
            }

            VisitChildren(ss);
        }
    }
}
