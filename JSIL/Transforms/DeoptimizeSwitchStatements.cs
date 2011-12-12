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
        }

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

        public void VisitNode (JSIfStatement ifs) {
            var boe = ifs.Condition as JSBinaryOperatorExpression;
            var uoe = ifs.Condition as JSUnaryOperatorExpression;

            if ((boe != null) && (boe.Operator == JSOperator.Equal)) {
                var leftVar = boe.Left as JSVariable;
                var leftIgnored = boe.Left as JSIgnoredMemberReference;
                var rightNull = boe.Right as JSDefaultValueLiteral;

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

                            foreach (var invocation in ifs.TrueClause.AllChildrenRecursive.OfType<JSInvocationExpression>()) {
                                if (invocation.JSMethod == null)
                                    continue;
                                if (invocation.JSMethod.Identifier != "Add")
                                    continue;
                                if (invocation.Arguments.Count != 2)
                                    continue;

                                var value = invocation.Arguments[0];
                                var index = invocation.Arguments[1] as JSIntegerLiteral;
                                if (index == null)
                                    continue;

                                initializer.Values[(int)index.Value] = value;
                            }
                        }
                    }
                }
            } else if ((uoe != null) && (uoe.Operator == JSOperator.LogicalNot)) {
                var invocation = uoe.Expression as JSInvocationExpression;

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
                                    Goto = ifs.AllChildrenRecursive.OfType<JSGotoExpression>().FirstOrDefault()
                                };
                            }
                        }
                    }
                }
            }

            VisitChildren(ifs);
        }

        protected class FoldLabelResult {
            public JSLabelGroupStatement LabelScope;
            public JSGotoExpression ExitGoto;
            public JSSwitchStatement SwitchStatement;
            public JSBlockStatement Block;
        }

        protected IEnumerable<JSGotoExpression> FindGotos (JSNode context, string targetLabel) {
            return context.AllChildrenRecursive.OfType<JSGotoExpression>()
                .Where((ge) => ge.TargetLabel == targetLabel);
        }

        protected FoldLabelResult FoldLabelIntoBlock (
            JSSwitchStatement switchStatement, string labelName, JSBlockStatement block
        ) {
            var result = new FoldLabelResult {
                Block = block,
                SwitchStatement = switchStatement
            };

            // Climb up the stack to find the scope containing the default block
            result.LabelScope = Stack.SkipWhile(
                (n) => (
                    n == switchStatement
                )
            ).OfType<JSLabelGroupStatement>().Where(
                (lgs) => lgs.Labels.ContainsKey(labelName)
            ).FirstOrDefault();

            // Detect invalid gotos
            if (result.LabelScope == null)
                return null;

            var defaultCase = result.LabelScope.Labels[labelName];
            var defaultCaseBody = defaultCase.Children.OfType<JSStatement>().ToArray();

            var deadGotos = FindGotos(block, labelName).ToArray();
            foreach (var dg in deadGotos)
                block.ReplaceChildRecursive(dg, new JSNullExpression());

            result.LabelScope.ReplaceChild(defaultCase, new JSNullStatement());

            block.Statements.AddRange(defaultCaseBody);
            block.Statements.Add(new JSExpressionStatement(new JSBreakExpression()));

            result.ExitGoto = block.AllChildrenRecursive.OfType<JSGotoExpression>().LastOrDefault();
            return result;
        }

        protected void EliminateExitGoto (
            FoldLabelResult flr
        ) {
            var exitLabel = flr.ExitGoto.TargetLabel;
            var exitBlock = flr.LabelScope.Labels[exitLabel];
            flr.LabelScope.ReplaceChild(exitBlock, new JSNullStatement());
            flr.Block.ReplaceChildRecursive(flr.ExitGoto, new JSNullExpression());

            var switchBlock = flr.LabelScope.Children.Where(
                (b) => b.Children.Contains(flr.SwitchStatement)
            ).OfType<JSBlockStatement>().FirstOrDefault();
            if (switchBlock != null) {
                switchBlock.Statements.AddRange(
                    exitBlock.Children.OfType<JSStatement>()
                );
            }
        }

        public void VisitNode (JSSwitchStatement ss) {
            IndexLookup indexLookup;
            Initializer initializer;
            NullCheck nullCheck;

            // Detect switch statements using a lookup dictionary
            var switchVar = ss.Condition as JSVariable;
            if (
                (switchVar != null) && 
                IndexLookups.TryGetValue(switchVar, out indexLookup) &&
                Initializers.TryGetValue(indexLookup.Field, out initializer)
            ) {
                if (NullChecks.TryGetValue(indexLookup.SwitchVariable, out nullCheck))
                    ParentNode.ReplaceChild(nullCheck.Statement, new JSNullStatement());

                ParentNode.ReplaceChild(initializer.Statement, new JSNullStatement());
                ParentNode.ReplaceChild(indexLookup.Statement, new JSNullStatement());

                var switchCases = new List<JSSwitchCase>();
                JSExpression[] values;
                foreach (var cse in ss.Cases) {
                    var body = cse.Body;

                    if (cse.Values == null) {
                        values = null;
                        body = new JSBlockStatement(body.Statements.ToArray());
                        
                        // Locate the goto within this block that jumps to the default block
                        var theGoto = FindGotos(body, indexLookup.Goto.TargetLabel).FirstOrDefault();

                        if (theGoto != null) {
                            var flr = FoldLabelIntoBlock(ss, indexLookup.Goto.TargetLabel, body);
                            if (flr != null)
                                EliminateExitGoto(flr);
                        }
                    } else {
                        values = (from v in cse.Values
                         let il = v as JSIntegerLiteral
                         where il != null
                         select initializer.Values[(int)il.Value]).ToArray();
                    }

                    switchCases.Add(new JSSwitchCase(
                        values, body
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
                return;
            }

            // Detect switch statements with multiple default cases
            var defaultCase = (from cse in ss.Cases where (cse.Values == null) select cse).FirstOrDefault();
            if (defaultCase != null) {
                var defaultGotos = defaultCase.AllChildrenRecursive.OfType<JSGotoExpression>().ToArray();
                if (defaultGotos.Length == 1) {
                    var defaultGoto = defaultGotos[0];
                    var extraCases = (from cse in ss.Cases
                                      where cse.Values != null
                                      let caseGotos = FindGotos(cse.Body, defaultGoto.TargetLabel).ToArray()
                                      where caseGotos.Length == 1
                                      select cse).ToArray();

                    if (extraCases.Length > 0) {
                        foreach (var ec in extraCases)
                            ss.Cases.Remove(ec);
                    }

                    var flr = FoldLabelIntoBlock(ss, defaultGoto.TargetLabel, defaultCase.Body);
                    if (flr != null)
                        EliminateExitGoto(flr);
                }
            }
            
            VisitChildren(ss);
        }
    }
}
