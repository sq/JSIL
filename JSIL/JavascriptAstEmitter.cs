using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Transforms;
using Mono.Cecil;

namespace JSIL {
    public enum BlockType {
        Switch,
        While,
        ForHeader,
        ForBody,
        Do
    }

    public class JavascriptAstEmitter : JSAstVisitor {
        public readonly ITypeInfoSource TypeInfo;
        public readonly JavascriptFormatter Output;

        public readonly TypeSystem TypeSystem;
        public readonly JSILIdentifier JSIL;

        protected readonly Stack<JSExpression> ThisReplacementStack = new Stack<JSExpression>();
        protected readonly Stack<bool> IncludeTypeParens = new Stack<bool>();
        protected readonly Stack<Func<string, bool>> GotoStack = new Stack<Func<string, bool>>();
        protected readonly Stack<BlockType> BlockStack = new Stack<BlockType>();

        public JavascriptAstEmitter (JavascriptFormatter output, JSILIdentifier jsil, TypeSystem typeSystem, ITypeInfoSource typeInfo) {
            Output = output;
            JSIL = jsil;
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
            IncludeTypeParens.Push(false);
        }

        protected void CommaSeparatedList (IEnumerable<JSExpression> values, bool withNewlines = false) {
            bool isFirst = true;
            foreach (var value in values) {
                if (!isFirst) {
                    Output.Comma();

                    if (withNewlines)
                        Output.NewLine();
                }

                Visit(value);
                isFirst = false;
            }
        }

        public override void VisitNode (JSNode node) {
            if ((node != null) && !node.IsNull) {
                Console.Error.WriteLine("Cannot emit {0}", node.GetType().Name);
                Output.Identifier("JSIL.UntranslatableNode", null);
                Output.LPar();
                Output.Value(node.GetType().Name);
                Output.RPar();
                Output.Semicolon();
            }

            base.VisitNode(node);
        }

        public void VisitNode (JSBlockStatement block) {
            VisitNode(block, false);
        }

        public void VisitNode (JSBlockStatement block, bool includeBraces) {
            if (includeBraces)
                Output.OpenBrace();

            for (var i = 0; i < block.Statements.Count; i++)
                Visit(block.Statements[i]);

            if (includeBraces)
                Output.CloseBrace();
        }

        public void VisitNode (JSLabelGroupStatement labelGroup) {
            Output.NewLine();

            var stepLabel = String.Format("$labelgroup{0}", labelGroup.GroupIndex);
            var labelVar = String.Format("$label{0}", labelGroup.GroupIndex);
            var firstLabel = labelGroup.Labels.First().Key;

            Output.Keyword("var");
            Output.Space();
            Output.Identifier(labelVar);
            Output.Token(" = ");
            Output.Value(firstLabel);
            Output.Semicolon();

            Output.Label(stepLabel);
            Output.Keyword("while");
            Output.Space();
            Output.LPar();

            Output.Keyword("true");

            Output.RPar();
            Output.Space();
            Output.OpenBrace();

            Output.Keyword("switch");
            Output.Space();
            Output.LPar();

            Output.Identifier(labelVar);

            Output.RPar();
            Output.Space();
            Output.OpenBrace();

            bool isFirst = true;
            Func<string, bool> emitGoto = (labelName) => {
                if (labelName != null) {
                    if (!labelGroup.Labels.ContainsKey(labelName))
                        return false;

                    Output.Identifier(labelVar);
                    Output.Token(" = ");
                    Output.Value(labelName);
                    Output.Semicolon();
                }

                Output.Keyword("continue");
                Output.Space();
                Output.Identifier(stepLabel);

                return true;
            };

            GotoStack.Push(emitGoto);

            bool needsTrailingBreak = true;

            foreach (var kvp in labelGroup.Labels) {
                if (!isFirst && needsTrailingBreak) {
                    Output.PlainTextFormatter.Indent();
                    emitGoto(kvp.Key);
                    Output.Semicolon(true);
                    Output.PlainTextFormatter.Unindent();
                }

                Output.Keyword("case");
                Output.Space();
                Output.Value(kvp.Key);
                Output.Token(":");
                Output.PlainTextFormatter.Indent();
                Output.NewLine();

                Visit(kvp.Value);

                Func<JSNode, bool> isNotNull = (n) => {
                    if (n.IsNull)
                        return false;
                    if (n is JSNullStatement)
                        return false;
                    if (n is JSNullExpression)
                        return false;

                    var es = n as JSExpressionStatement;
                    if (es != null) {
                        if (es.Expression.IsNull)
                            return false;
                        if (es.Expression is JSNullExpression)
                            return false;
                    }

                    return true;
                };

                var nonNullChildren = kvp.Value.Children.Where(isNotNull);

                var lastStatement = nonNullChildren.LastOrDefault();
                JSBlockStatement lastBlockStatement;

                while ((lastBlockStatement = lastStatement as JSBlockStatement) != null) {
                    if (lastBlockStatement.IsControlFlow)
                        break;
                    else {
                        nonNullChildren = lastStatement.Children.Where(isNotNull);
                        lastStatement = nonNullChildren.LastOrDefault();
                    }
                }

                var lastExpressionStatement = lastStatement as JSExpressionStatement;

                if (
                    (lastExpressionStatement != null) &&
                    (
                        (lastExpressionStatement.Expression is JSContinueExpression) || 
                        (lastExpressionStatement.Expression is JSBreakExpression) ||
                        (lastExpressionStatement.Expression is JSGotoExpression)
                    )
                ) {
                    needsTrailingBreak = false;
                } else {
                    needsTrailingBreak = true;
                }

                isFirst = false;

                Output.PlainTextFormatter.Unindent();

                Output.NewLine();
            }

            GotoStack.Pop();

            if (needsTrailingBreak) {
                Output.PlainTextFormatter.Indent();
                Output.Keyword("break");
                Output.Space();
                Output.Identifier(stepLabel);
                Output.Semicolon(true);
                Output.PlainTextFormatter.Unindent();
            }

            Output.PlainTextFormatter.Unindent();

            Output.CloseBrace();

            Output.CloseBrace();
        }

        public void VisitNode (JSVariableDeclarationStatement vars) {
            if (vars.Declarations.Count == 0)
                return;

            Output.Keyword("var");
            Output.Space();

            CommaSeparatedList(vars.Declarations);

            if ((BlockStack.Count == 0) ||
                (BlockStack.Peek() != BlockType.ForHeader)
            ) {
                Output.Semicolon();
            }
        }

        public void VisitNode (JSExpressionStatement statement) {
            bool isNull = (statement.IsNull ||
                statement.Expression.IsNull) && 
                !(statement.Expression is JSUntranslatableExpression) &&
                !(statement.Expression is JSIgnoredMemberReference);

            Visit(statement.Expression);

            if (!isNull &&
                ((BlockStack.Count == 0) ||
                (BlockStack.Peek() != BlockType.ForHeader))
            ) {
                Output.Semicolon();
            }
        }

        public void VisitNode (JSDotExpression dot) {
            var parens = (dot.Target is JSNumberLiteral) ||
                (dot.Target is JSIntegerLiteral);

            if (parens)
                Output.LPar();

            Visit(dot.Target);

            if (parens)
                Output.RPar();

            Output.Dot();
            Visit(dot.Member);
        }

        public void VisitNode (JSChangeTypeExpression cte) {
            Visit(cte.Expression);
        }

        public void VisitNode (JSStructCopyExpression sce) {
            Visit(sce.Struct);
            Output.Dot();
            Output.Identifier("MemberwiseClone");
            Output.LPar();
            Output.RPar();
        }

        public void VisitNode (JSIndexerExpression idx) {
            Visit(idx.Target);
            Output.OpenBracket();
            Visit(idx.Index);
            Output.CloseBracket();
        }

        public void VisitNode (JSMethod method) {
            Output.Identifier(method.Method.Name);

            var ga = method.GenericArguments;
            if (ga != null) {
                Output.LPar();
                Output.CommaSeparatedList(ga, ListValueType.Identifier);
                Output.RPar();
            }
        }

        public void VisitNode (JSIdentifier identifier) {
            Output.Identifier(identifier.Identifier);
        }

        public void VisitNode (JSRawOutputIdentifier identifier) {
            identifier.Emitter(Output);
        }

        public void VisitNode (JSCharLiteral ch) {
            Output.Value(ch.Value);
        }

        public void VisitNode (JSStringLiteral str) {
            Output.Value(str.Value);
        }

        public void VisitNode (JSVerbatimLiteral verbatim) {
            bool parens =
                (ParentNode is JSBinaryOperatorExpression) || (ParentNode is JSUnaryOperatorExpression);

            var regex = new Regex(@"(\$(?'name'(typeof\(this\))|([a-zA-Z_]([a-zA-Z0-9_]*)))|(?'text'[^\$]*)|)", RegexOptions.ExplicitCapture);

            if (parens)
                Output.LPar();

            bool isFirst = true;
            foreach (var line in verbatim.Expression.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
                if (String.IsNullOrWhiteSpace(line))
                    continue;

                if (!isFirst)
                    Output.PlainTextOutput.WriteLine();

                var matches = regex.Matches(line);

                foreach (Match m in matches) {
                    if (m.Groups["text"].Success) {
                        Output.PlainTextOutput.Write(m.Groups["text"].Value);
                    } else if (m.Groups["name"].Success) {
                        var key = m.Groups["name"].Value;

                        if (verbatim.Variables.ContainsKey(key))
                            Visit(verbatim.Variables[key]);
                        else
                            Output.PlainTextOutput.Write("null");
                    }
                }

                isFirst = false;
            }

            if (parens)
                Output.RPar();
        }

        public void VisitNode (JSTypeNameLiteral type) {
            Output.Value(type.Value);
        }

        public void VisitNode (JSAssemblyNameLiteral asm) {
            Output.Value(asm.Value.FullName);
        }

        public void VisitNode (JSIntegerLiteral integer) {
            Output.Value(integer.Value);
        }

        public void VisitNode (JSNumberLiteral number) {
            Output.Value(number.Value);
        }

        public void VisitNode (JSBooleanLiteral b) {
            Output.Value(b.Value);
        }

        public void VisitNode (JSEnumLiteral enm) {
            bool isFirst = true;

            if (enm.Names.Length > 1)
                Output.LPar();

            foreach (var name in enm.Names) {
                if (!isFirst)
                    Output.Token(" | ");

                Output.Identifier(enm.EnumType);
                Output.Dot();
                Output.Identifier(name);

                isFirst = false;
            }

            if (enm.Names.Length > 1)
                Output.RPar();
        }

        public void VisitNode (JSNullLiteral nil) {
            Output.Keyword("null");
        }

        public void VisitNode (JSGotoExpression go) {
            if (GotoStack.Count > 0) {
                foreach (var eg in GotoStack) {
                    if (eg(go.TargetLabel))
                        return;
                }
            }

            Output.Identifier("JSIL.UntranslatableInstruction", null);
            Output.LPar();
            Output.Value(go.ToString());
            Output.RPar();
        }

        public void VisitNode (JSUntranslatableStatement us) {
            Output.Identifier("JSIL.UntranslatableNode", null);
            Output.LPar();
            Output.Value((us.Type ?? "").ToString());
            Output.RPar();
            Output.Semicolon();
        }

        public void VisitNode (JSUntranslatableExpression ue) {
            Output.Identifier("JSIL.UntranslatableInstruction", null);
            Output.LPar();
            Output.Value((ue.Type ?? "").ToString());
            Output.RPar();
        }

        public void VisitNode (JSIgnoredMemberReference imr) {
            Output.Identifier("JSIL.IgnoredMember", null);
            Output.LPar();
            if (imr.Member != null) {
                var method = imr.Member as MethodInfo;
                if (method != null)
                    Output.Value(String.Format(
                        "{0}({1})", method.Name, String.Join(", ",
                            (from p in method.Member.Parameters select p.Name).ToArray()
                        )
                    ));
                else
                    Output.Value(imr.Member.Name);
            }

            var args = (from a in imr.Arguments where (a != null) && !a.IsNull select a).ToArray();

            if (args.Length != 0) {
                if (imr.Member != null)
                    Output.Comma();

                CommaSeparatedList(args);
            }
            Output.RPar();
        }

        public void VisitNode (JSDefaultValueLiteral defaultValue) {
            if (ILBlockTranslator.IsEnum(defaultValue.Value)) {
                var enumInfo = TypeInfo.Get(defaultValue.Value);
                Output.Identifier(defaultValue.Value);
                Output.Dot();
                Output.Identifier(enumInfo.FirstEnumMember.Name);
            } else if (TypeAnalysis.IsIntegerOrEnum(defaultValue.Value)) {
                Output.Value(0);
            } else if (!defaultValue.Value.IsValueType) {
                Output.Keyword("null");
            } else {
                switch (defaultValue.Value.FullName) {
                    case "System.Nullable`1":
                        Output.Keyword("null");
                        break;
                    case "System.Single":
                    case "System.Double":
                    case "System.Decimal":
                        Output.Value(0.0);
                        break;
                    case "System.Boolean":
                        Output.Keyword("false");
                        break;
                    default:
                        VisitNode(new JSNewExpression(new JSType(defaultValue.Value), null));
                        break;
                }
            }
        }

        public void VisitNode (JSAssembly asm) {
            Output.AssemblyReference(asm.Assembly);
        }

        public void VisitNode (JSTypeReference tr) {
            Output.TypeReference(tr.Type, tr.Context);
        }

        public void VisitNode (JSType type) {
            Output.Identifier(
                type.Type, IncludeTypeParens.Peek()
            );
        }

        public void VisitNode (JSTypeOfExpression toe) {
            Visit(toe.Type);

            if (toe.Type.Type is GenericParameter) {
                // Generic parameters are type objects, not public interfaces
            } else {
                Output.Dot();
                Output.Identifier("__Type__");
            }
        }

        public void VisitNode (JSEliminatedVariable variable) {
            throw new InvalidOperationException(String.Format("'{0}' was eliminated despite being in use.", variable.Variable));
        }

        public void VisitNode (JSVariable variable) {
            if (variable.IsThis) {
                if (ThisReplacementStack.Count > 0) {
                    var thisRef = ThisReplacementStack.Peek();
                    if (thisRef != null)
                        Visit(thisRef);

                    return;
                } else {
                    Output.Keyword("this");
                }
            } else
                Output.Identifier(variable.Identifier);

            // Don't emit .value when initializing a reference in a declaration.
            var boe = ParentNode as JSBinaryOperatorExpression;
            if (
                (boe != null) && 
                (boe.Left == variable) && 
                (Stack.Skip(2).FirstOrDefault() is JSVariableDeclarationStatement)
            ) {
                return;
            }

            if (variable.IsReference) {
                if (variable.IsThis) {
                    if (JSExpression.DeReferenceType(variable.Type).IsValueType)
                        return;
                    else
                        throw new InvalidOperationException("The this-reference should never be a reference to a non-value type");
                }

                Output.Dot();
                Output.Identifier("value");
            }
        }

        public void VisitNode (JSPassByReferenceExpression byref) {
            JSExpression referent;

            if (JSReferenceExpression.TryMaterialize(JSIL, byref.Referent, out referent)) {
                Output.Comment("ref");
                Visit(referent);
            } else {
                Output.Identifier("JSIL.UnmaterializedReference", null);
                Output.LPar();
                Output.RPar();
            }
        }

        public void VisitNode (JSReferenceExpression reference) {
            Visit(reference.Referent);
        }

        public void VisitNode (JSLambda lambda) {
            if (!lambda.UseBind)
                ThisReplacementStack.Push(lambda.This);

            Visit(lambda.Value);

            if (lambda.UseBind) {
                Output.Dot();
                Output.Keyword("bind");
                Output.LPar();
                Visit(lambda.This);
                Output.RPar();
            } else {
                ThisReplacementStack.Pop();
            }
        }

        public void VisitNode (JSFunctionExpression function) {
            var oldCurrentMethod = Output.CurrentMethod;

            if (function.Method != null) {
                Output.CurrentMethod = function.Method.Reference;
            } else {
                Output.CurrentMethod = null;
            }

            Output.OpenFunction(
                function.DisplayName,
                (o) => {
                    if (o != Output)
                        throw new InvalidOperationException();

                    bool isFirst = true;
                    foreach (var p in function.Parameters) {
                        if (!isFirst)
                            o.Comma();

                        if (p.IsReference)
                            o.Comment("ref");

                        o.Identifier(p.Identifier);

                        isFirst = false;
                    }
                }
            );

            Visit(function.Body);

            Output.CloseBrace(false);
            Output.CurrentMethod = oldCurrentMethod;
        }

        public void VisitNode (JSSwitchStatement swtch) {
            BlockStack.Push(BlockType.Switch);
            WriteLabel(swtch);

            Output.Keyword("switch");
            Output.Space();

            Output.LPar();
            Visit(swtch.Condition);
            Output.RPar();
            Output.Space();

            Output.OpenBrace();

            foreach (var c in swtch.Cases) {
                if (c.Values != null) {
                    foreach (var value in c.Values) {
                        Output.Token("case ");
                        Visit(value);
                        Output.Token(": ");
                        Output.NewLine();
                    }
                } else {
                    Output.Token("default: ");
                    Output.NewLine();
                }

                Output.PlainTextFormatter.Indent();
                Visit(c.Body);
                Output.PlainTextFormatter.Unindent();
                Output.NewLine();
            }

            Output.CloseBrace();
            BlockStack.Pop();
        }

        protected void WriteLoopLabel (JSLoopStatement loop) {
            if (loop.Index.HasValue)
                Output.Label(String.Format("$loop{0}", loop.Index.Value));
        }

        protected void WriteLabel (JSStatement stmt) {
            if (!String.IsNullOrWhiteSpace(stmt.Label))
                Output.Label(stmt.Label);
        }

        public void VisitNode (JSIfStatement ifs) {
            Output.Keyword("if");
            Output.Space();

            Output.LPar();
            Visit(ifs.Condition);
            Output.RPar();
            Output.Space();

            Output.OpenBrace();
            Visit(ifs.TrueClause);

            JSStatement falseClause = ifs.FalseClause;
            while (falseClause != null) {
                var nestedBlock = falseClause as JSBlockStatement;
                var nestedIf = falseClause as JSIfStatement;
                if ((nestedBlock != null) && (nestedBlock.Statements.Count == 1))
                    nestedIf = nestedBlock.Statements[0] as JSIfStatement;

                if (nestedIf != null) {
                    Output.CloseAndReopenBrace((o) => {
                        if (o != this.Output)
                            throw new InvalidOperationException();

                        o.Keyword("else if");
                        o.Space();
                        o.LPar();
                        Visit(nestedIf.Condition);
                        o.RPar();
                    });

                    Visit(nestedIf.TrueClause);

                    falseClause = nestedIf.FalseClause;
                } else {
                    Output.CloseAndReopenBrace("else");
                    Visit(falseClause);
                    falseClause = null;
                }
            }

            Output.CloseBrace();
        }

        public void VisitNode (JSTryCatchBlock tcb) {
            if ((tcb.Catch ?? tcb.Finally) == null) {
                Visit(tcb.Body);
                return;
            }

            Output.Keyword("try");
            Output.Space();
            Output.OpenBrace();

            Visit(tcb.Body);

            if (tcb.Catch != null) {
                Output.CloseAndReopenBrace((o) => {
                    if (o != Output)
                        throw new InvalidOperationException();

                    o.Keyword("catch");
                    o.Space();
                    o.LPar();
                    Visit(tcb.CatchVariable);
                    o.RPar();
                });

                Visit(tcb.Catch);
            }

            if (tcb.Finally != null) {
                Output.CloseAndReopenBrace("finally");

                Visit(tcb.Finally);
            }

            Output.CloseBrace();
        }

        public void VisitNode (JSForLoop loop) {
            Output.NewLine();

            BlockStack.Push(BlockType.ForHeader);
            WriteLoopLabel(loop);

            Output.Keyword("for");
            Output.Space();

            Output.LPar();

            if ((loop.Initializer != null) && !loop.Initializer.IsNull)
                Visit(loop.Initializer);
            Output.Semicolon(false);

            Visit(loop.Condition);
            Output.Semicolon(false);

            if ((loop.Increment != null) && !loop.Increment.IsNull)
                Visit(loop.Increment);

            Output.RPar();
            Output.Space();

            BlockStack.Pop();
            BlockStack.Push(BlockType.ForBody);

            VisitNode((JSBlockStatement)loop, true);

            BlockStack.Pop();
        }

        public void VisitNode (JSWhileLoop loop) {
            Output.NewLine();

            BlockStack.Push(BlockType.While);
            WriteLoopLabel(loop);

            Output.Keyword("while");
            Output.Space();

            Output.LPar();
            Visit(loop.Condition);
            Output.RPar();
            Output.Space();

            VisitNode((JSBlockStatement)loop, true);

            BlockStack.Pop();
        }

        public void VisitNode (JSDoLoop loop) {
            Output.NewLine();

            BlockStack.Push(BlockType.Do);
            WriteLoopLabel(loop);

            Output.Keyword("do");
            Output.Space();
            Output.OpenBrace();

            VisitNode((JSBlockStatement)loop, false);

            Output.CloseBrace(false);
            Output.Space();
            Output.Keyword("while");
            Output.Space();

            Output.LPar();
            Visit(loop.Condition);
            Output.RPar();
            Output.Semicolon();

            BlockStack.Pop();
        }

        public void VisitNode (JSReturnExpression ret) {
            Output.Keyword("return");

            if (ret.Value != null) {
                Output.Space();
                Visit(ret.Value);
            }
        }

        public void VisitNode (JSThrowExpression ret) {
            Output.Keyword("throw");
            Output.Space();
            Visit(ret.Exception);
        }

        public void VisitNode (JSBreakExpression brk) {
            if (brk.TargetLoop.HasValue) {
                Output.Keyword("break");
                Output.Space();
                Output.Identifier(String.Format("$loop{0}", brk.TargetLoop.Value));
                return;
            }

            if (BlockStack.Count == 0) {
                throw new NotImplementedException();
            }

            switch (BlockStack.Peek()) {
                case BlockType.Switch:
                    Output.Keyword("break");
                    break;
                default:
                    throw new NotImplementedException("Invalid break statement");
                    break;
            }
        }

        public void VisitNode (JSContinueExpression cont) {
            if (cont.TargetLoop.HasValue) {
                Output.Keyword("continue");
                Output.Space();
                Output.Identifier(String.Format("$loop{0}", cont.TargetLoop.Value));
            } else if (GotoStack.Count > 0) {
                GotoStack.Peek()(null);
            } else {
                Output.Keyword("continue");
            }
        }

        public void VisitNode (JSUnaryOperatorExpression uop) {
            if (!uop.IsPostfix)
                Output.Token(uop.Operator.Token);

            Visit(uop.Expression);

            if (uop.IsPostfix)
                Output.Token(uop.Operator.Token);
        }

        public void VisitNode (JSBinaryOperatorExpression bop) {
            bool parens = true;
            bool needsTruncation = false;

            if (ParentNode is JSIfStatement)
                parens = false;
            else if ((ParentNode is JSWhileLoop) && ((JSWhileLoop)ParentNode).Condition == bop)
                parens = false;
            else if ((ParentNode is JSDoLoop) && ((JSDoLoop)ParentNode).Condition == bop)
                parens = false;
            else if (ParentNode is JSForLoop) {
                var fl = (JSForLoop)ParentNode;
                if (
                    (fl.Condition == bop) ||
                    (fl.Increment.SelfAndChildrenRecursive.Any((n) => bop.Equals(n))) ||
                    (fl.Initializer.SelfAndChildrenRecursive.Any((n) => bop.Equals(n)))
                ) {
                    parens = false;
                }
            } else if ((ParentNode is JSSwitchStatement) && ((JSSwitchStatement)ParentNode).Condition == bop)
                parens = false;
            else if (
                (ParentNode is JSBinaryOperatorExpression) &&
                ((JSBinaryOperatorExpression)ParentNode).Operator == bop.Operator &&
                bop.Operator is JSLogicalOperator
            ) {
                parens = false;
            } else if (ParentNode is JSVariableDeclarationStatement)
                parens = false;
            else if (ParentNode is JSExpressionStatement)
                parens = false;

            // We need to perform manual truncation to maintain the semantics of C#'s division operator
            if ((bop.Operator == JSOperator.Divide)) {
                needsTruncation =                     
                    (ILBlockTranslator.IsIntegral(bop.Left.GetExpectedType(TypeSystem)) &&
                    ILBlockTranslator.IsIntegral(bop.Right.GetExpectedType(TypeSystem))) ||
                    ILBlockTranslator.IsIntegral(bop.GetExpectedType(TypeSystem));

                parens |= needsTruncation;
            }

            if (needsTruncation) {
                if (bop.Operator is JSAssignmentOperator)
                    throw new NotImplementedException();

                Output.Identifier("Math.floor", null);
            }

            if (parens)
                Output.LPar();

            Visit(bop.Left);
            Output.Space();
            Output.Token(bop.Operator.Token);
            Output.Space();

            if (
                (bop.Operator is JSLogicalOperator) &&
                (Stack.OfType<JSBinaryOperatorExpression>().Skip(1).FirstOrDefault() != null)
            ) {
                Output.NewLine();
            }

            Visit(bop.Right);

            if (parens)
                Output.RPar();
        }

        public void VisitNode (JSTernaryOperatorExpression ternary) {
            Output.LPar();

            Visit(ternary.Condition);

            Output.Token(" ? ");
            Visit(ternary.True);

            Output.Token(" : ");
            Visit(ternary.False);

            Output.RPar();
        }

        public void VisitNode (JSNewExpression newexp) {
            var outer = Stack.Skip(1).FirstOrDefault();
            var outerInvocation = outer as JSInvocationExpression;
            var outerDot = outer as JSDotExpression;

            bool parens = ((outerDot != null) && (outerDot.Target == newexp)) ||
                ((outerInvocation != null) && (outerInvocation.ThisReference == newexp));

            if (
                (newexp.Constructor != null) &&
                !(
                  newexp.Constructor.IsSealed || 
                  newexp.Constructor.DeclaringType.DerivedTypeCount == 0
                )
            ) {
                Output.Identifier("JSIL.New", null);
                Output.LPar();

                IncludeTypeParens.Push(false);
                try {
                    Visit(newexp.Type);
                } finally {
                    IncludeTypeParens.Pop();
                }

                Output.Comma();
                Output.Value(Util.EscapeIdentifier(newexp.Constructor.Name, EscapingMode.MemberIdentifier));

                Output.Comma();
                Output.OpenBracket(false);
                if (newexp.Arguments.Count > 0) {
                    CommaSeparatedList(newexp.Arguments);
                }
                Output.CloseBracket();
                Output.RPar();
            } else {
                if (parens)
                    Output.LPar();

                Output.Keyword("new");
                Output.Space();

                IncludeTypeParens.Push(true);
                try {
                    Visit(newexp.Type);
                } finally {
                    IncludeTypeParens.Pop();
                }

                Output.LPar();
                CommaSeparatedList(newexp.Arguments);
                Output.RPar();

                if (parens)
                    Output.RPar();
            }
        }

        public void VisitNode (JSPairExpression pair) {
            Visit(pair.Key);
            Output.Token(": ");
            Visit(pair.Value);
        }

        public void VisitNode (JSArrayExpression array) {
            Output.OpenBracket();
            CommaSeparatedList(array.Values);
            Output.CloseBracket();
        }

        public void VisitNode (JSMemberDescriptor desc) {
            Output.MemberDescriptor(desc.IsPublic, desc.IsStatic);
        }

        public void VisitNode (JSObjectExpression obj) {
            Output.OpenBrace();
            CommaSeparatedList(obj.Values, true);
            Output.CloseBrace();
        }

        protected static int CountOfMatchingSubtrees<TNode> (IEnumerable<JSNode> nodes) 
            where TNode : JSNode {
            if (nodes == null)
                return 0;

            return (from n in nodes
                    where (n != null) && 
                          ((n is TNode) ||
                          (n.AllChildrenRecursive.OfType<TNode>().FirstOrDefault() != null))
                    select n).Count();
        }

        protected static bool ArgumentsNeedLineBreak (IList<JSExpression> arguments) {
            return ((arguments.Count > 1) &&
                (
                    (CountOfMatchingSubtrees<JSFunctionExpression>(arguments) > 1) ||
                    (CountOfMatchingSubtrees<JSInvocationExpression>(arguments) > 1) ||
                    (CountOfMatchingSubtrees<JSDelegateInvocationExpression>(arguments) > 1)
                )) ||
                (arguments.Count > 4);
        }

        public void VisitNode (JSDelegateInvocationExpression invocation) {
            bool needsParens =
                CountOfMatchingSubtrees<JSFunctionExpression>(new[] { invocation.ThisReference }) > 0;

            if (needsParens)
                Output.LPar();

            Visit(invocation.ThisReference);

            if (needsParens)
                Output.RPar();

            Output.LPar();

            bool needLineBreak = ArgumentsNeedLineBreak(invocation.Arguments);

            if (needLineBreak)
                Output.NewLine();

            CommaSeparatedList(invocation.Arguments, needLineBreak);

            if (needLineBreak)
                Output.NewLine();

            Output.RPar();
        }

        public void VisitNode (JSInvocationExpression invocation) {
            TypeReference typeOfThisReference = null;

            var jsm = invocation.JSMethod;
            MethodInfo method = null;
            if (jsm != null)
                method = jsm.Method;

            bool isOverloaded = (method != null) &&
                method.IsOverloadedRecursive &&
                !method.Metadata.HasAttribute("JSIL.Meta.JSRuntimeDispatch");

            bool isStatic = invocation.ExplicitThis && invocation.ThisReference.IsNull;

            bool hasArguments = invocation.Arguments.Count > 0;
            bool hasGenericArguments = invocation.GenericArguments != null;

            bool needsParens =
                (CountOfMatchingSubtrees<JSFunctionExpression>(new[] { invocation.ThisReference }) > 0) ||
                (CountOfMatchingSubtrees<JSIntegerLiteral>(new[] { invocation.ThisReference }) > 0) ||
                (CountOfMatchingSubtrees<JSNumberLiteral>(new[] { invocation.ThisReference }) > 0);

            Action thisRef = () => {
                if (needsParens)
                    Output.LPar();

                Visit(invocation.ThisReference);

                if (needsParens)
                    Output.RPar();
            };

            if (isOverloaded) {
                var methodName = Util.EscapeIdentifier(method.Name, EscapingMode.MemberIdentifier);

                Output.LPar();
                Output.MethodSignature(
                    null, method.ReturnType,
                    (
                    from p in method.Parameters select 
                        JSExpression.SubstituteTypeArgs(this.TypeInfo, p.ParameterType, jsm.Reference)
                    ),
                    true
                );
                Output.RPar();
                Output.Dot();

                Action genericArgs = () => {
                    if (hasGenericArguments) {
                        Output.OpenBracket(false);
                        Output.CommaSeparatedList(invocation.GenericArguments, ListValueType.TypeReference);
                        Output.CloseBracket(false);
                    } else
                        Output.Identifier("null", null);
                };

                if (isStatic) {
                    Output.Identifier("Call");
                    Output.LPar();

                    Visit(invocation.Type);
                    Output.Comma();

                    Output.Value(methodName);
                    Output.Comma();
                    genericArgs();

                    if (hasArguments)
                        Output.Comma();
                } else if (invocation.ExplicitThis) {
                    Output.Identifier("Call");
                    Output.LPar();

                    Visit(invocation.Type);
                    Output.Dot();
                    Output.Identifier("prototype", null);
                    Output.Comma();

                    Output.Value(methodName);
                    Output.Comma();
                    genericArgs();
                    Output.Comma();
                    Visit(invocation.ThisReference);

                    if (hasArguments)
                        Output.Comma();
                } else {
                    Output.Identifier("CallVirtual");
                    Output.LPar();

                    Output.Value(methodName);
                    Output.Comma();
                    genericArgs();
                    Output.Comma();
                    Visit(invocation.ThisReference);

                    if (hasArguments)
                        Output.Comma();
                }
            } else {
                if (isStatic) {
                    if (!invocation.Type.IsNull) {
                        Visit(invocation.Type);
                        Output.Dot();
                    }

                    Visit(invocation.Method);
                    Output.LPar();
                } else if (invocation.ExplicitThis) {
                    if (!invocation.Type.IsNull) {
                        Visit(invocation.Type);
                        Output.Dot();
                        Output.Identifier("prototype", null);
                        Output.Dot();
                    }

                    Visit(invocation.Method);
                    Output.Dot();
                    Output.Identifier("call", null);
                    Output.LPar();

                    Visit(invocation.ThisReference);

                    if (hasArguments)
                        Output.Comma();
                } else {
                    thisRef();
                    Output.Dot();
                    Visit(invocation.Method);
                    Output.LPar();
                }
            }

            bool needLineBreak = ArgumentsNeedLineBreak(invocation.Arguments);

            if (needLineBreak)
                Output.NewLine();

            CommaSeparatedList(invocation.Arguments, needLineBreak);

            if (needLineBreak)
                Output.NewLine();

            Output.RPar();
        }

        public void VisitNode (JSInitializerApplicationExpression iae) {
            Output.LPar();
            Visit(iae.Target);
            Output.RPar();
            Output.Dot();
            Output.Identifier("__Initialize__");
            Output.LPar();
            Visit(iae.Initializer);
            Output.RPar();
        }
    }
}
