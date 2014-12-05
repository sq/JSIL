using System;
using System.Collections.Generic;
using System.Linq;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Transforms
{
    public class ReplaceIndexerAssigments : JSAstVisitor
    {
        public readonly TypeSystem TypeSystem;
        public readonly TypeInfoProvider TypeInfo;

        public ReplaceIndexerAssigments(TypeSystem typeSystem, TypeInfoProvider typeInfo)
        {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
        }

        public void VisitNode(JSInvocationExpression invocation)
        {
            if (ParentNode is JSIndexerSetterExpression || ParentNode is JSStatement)
            {
                return;
            }

            if (invocation.JSMethod != null
                && invocation.JSMethod.Method.Property != null
                && invocation.JSMethod.Method == invocation.JSMethod.Method.Property.Setter
                && invocation.JSMethod.Method.Parameters.Length > 1)
            {
                var function = Stack.OfType<JSFunctionExpression>().First();
                var blockPair = Stack.Zip(Stack.Skip(1), Tuple.Create).First(tuple => tuple.Item2 is JSBlockStatement);
                var block = (JSBlockStatement) blockPair.Item2;
                var blockIndex = block.Statements.IndexOf((JSStatement) blockPair.Item1);

                var cacheVariableNum = Enumerable.Range(0, int.MaxValue)
                    .First(item => !function.AllVariables.ContainsKey("_ic_" + item));

                JSExpression lastArgument = null;
                var assigments = new List<JSBinaryOperatorExpression>();
                var expressions = new List<JSExpression>();

                foreach (var arg in new[] { invocation.ThisReference }.Union(invocation.Arguments))
                {
                    lastArgument = arg;
                    if (arg is JSVariable || arg is JSLiteral)
                    {
                        continue;
                    }

                    var originalType = arg.GetActualType(TypeSystem);
                    var cacheArgVariableName = "_ic_" + (cacheVariableNum + expressions.Count);
                    var argVariable = new JSVariable(cacheArgVariableName, originalType, function.Method.Reference);
                    function.AllVariables.Add(cacheArgVariableName, argVariable);

                    assigments.Add(
                        new JSBinaryOperatorExpression(
                            JSBinaryOperator.Assignment,
                            argVariable,
                            new JSNullLiteral(TypeSystem.Object),
                            originalType));

                    invocation.ReplaceChild(arg, argVariable);

                    expressions.Add(new JSBinaryOperatorExpression(
                        JSBinaryOperator.Assignment,
                        argVariable,
                        arg,
                        originalType));

                    lastArgument = argVariable;
                }

                block.Statements.Insert(
                    blockIndex,
                    new JSVariableDeclarationStatement(
                        assigments.ToArray()));

                expressions.Add(invocation);
                expressions.Add(lastArgument);
                var replacement = new JSIndexerSetterExpression(expressions.ToArray());

                ParentNode.ReplaceChild(invocation, replacement);

                VisitReplacement(replacement);
            }
        }
    }
}