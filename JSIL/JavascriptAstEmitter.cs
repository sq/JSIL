using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;

namespace JSIL {
    public class JavascriptAstEmitter : JSAstVisitor {
        public readonly JavascriptFormatter Output;

        public JavascriptAstEmitter (JavascriptFormatter output) {
            Output = output;
        }

        protected void CommaSeparatedList (IEnumerable<JSExpression> values) {
            bool isFirst = true;
            foreach (var value in values) {
                if (!isFirst)
                    Output.Comma();

                Visit(value);
                isFirst = false;
            }
        }

        public override void VisitNode (JSNode node) {
            if (node != null) {
                Console.Error.WriteLine("Cannot emit {0}", node.GetType().Name);
                Output.Identifier("JSIL.UntranslatableNode", true);
                Output.LPar();
                Output.Value(node.GetType().Name);
                Output.RPar();
            }

            base.VisitNode(node);
        }

        public void VisitNode (JSBlockStatement block, bool includeBraces = false) {
            if (includeBraces)
                Output.OpenBrace();

            VisitChildren(block);

            if (includeBraces)
                Output.CloseBrace();
        }

        public void VisitNode (JSExpressionStatement statement) {
            Visit(statement.Expression);
            Output.Semicolon();
        }

        public void VisitNode (JSDotExpression dot) {
            Visit(dot.Target);
            Output.Dot();
            Visit(dot.Member);
        }

        public void VisitNode (JSIndexerExpression idx) {
            Visit(idx.Target);
            Output.OpenBracket();
            Visit(idx.Index);
            Output.CloseBracket();
        }

        public void VisitNode (JSIdentifier identifier) {
            Output.Identifier(identifier.Identifier);
        }

        public void VisitNode (JSStringLiteral str) {
            Output.Value(str.Value);
        }

        public void VisitNode (JSTypeNameLiteral type) {
            Output.Value(type.Value);
        }

        public void VisitNode (JSIntegerLiteral integer) {
            Output.Value(integer.Value);
        }

        public void VisitNode (JSNullLiteral nll) {
            Output.Keyword("null");
        }

        public void VisitNode (JSType type) {
            Output.Identifier(type.Type);
        }

        public void VisitNode (JSFunctionExpression function) {
            string functionName = null;
            if (function.FunctionName != null)
                functionName = function.FunctionName.Identifier;

            Output.OpenFunction(
                functionName,
                from p in function.Parameters select p.Identifier
            );

            Visit(function.Body);

            Output.CloseBrace();
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

            if (ifs.FalseClause != null) {
                Output.CloseAndReopenBrace("else");
                Visit(ifs.FalseClause);
            }

            Output.CloseBrace();
        }

        public void VisitNode (JSWhileLoop loop) {
            Output.Keyword("while");
            Output.Space();

            Output.LPar();
            Visit(loop.Condition);
            Output.RPar();
            Output.Space();

            Output.OpenBrace();
            Visit(loop.Body);
            Output.CloseBrace();
        }

        public void VisitNode (JSReturnExpression ret) {
            Output.Keyword("return");
            Output.Space();

            if (ret.Value != null)
                Visit(ret.Value);
        }

        public void VisitNode (JSUnaryOperatorExpression bop) {
            if (!bop.Postfix)
                Output.Token(bop.Operator.Token);

            Visit(bop.Expression);

            if (bop.Postfix)
                Output.Token(bop.Operator.Token);
        }

        public void VisitNode (JSBinaryOperatorExpression bop) {
            bool parens = (bop.Operator != JSOperator.Assignment);

            if (parens)
                Output.LPar();

            Visit(bop.Left);
            Output.Space();
            Output.Token(bop.Operator.Token);
            Output.Space();
            Visit(bop.Right);

            if (parens)
                Output.RPar();
        }

        public void VisitNode (JSNewExpression newexp) {
            Output.Keyword("new");
            Output.Space();
            Visit(newexp.Type);
            Output.LPar();
            CommaSeparatedList(newexp.Arguments);
            Output.RPar();
        }

        public void VisitNode (JSArrayExpression invocation) {
            Output.OpenBracket();
            CommaSeparatedList(invocation.Values);
            Output.CloseBracket();
        }

        public void VisitNode (JSInvocationExpression invocation) {
            Visit(invocation.Target);
            Output.LPar();
            CommaSeparatedList(invocation.Arguments);
            Output.RPar();
        }
    }
}
