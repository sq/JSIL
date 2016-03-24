namespace JSIL.Transforms
{
    using JSIL.Ast;

    public class FixSymbolInfo : JSAstVisitor
    {
        public override void VisitNode(JSNode node)
        {
            if (node == null)
            {
                return;
            }

            if (node.SymbolInfo != null /* && node.SymbolInfo.Inferred*/)
            {
                if ((ParentNode is JSForLoop && ((JSForLoop) ParentNode).Condition == node))
                {
                }
                else
                {
                    node.SymbolInfo = null;
                }
            }

            VisitChildren(node);
        }

        public void VisitNode(JSExpressionStatement node)
        {
            VisitChildren(node);
        }

        public void VisitNode(JSIfStatement node)
        {
            VisitChildren(node);
        }

        public void VisitNode(JSVariableDeclarationStatement node)
        {
            if (node.Declarations.Count == 0)
            {
                node.SymbolInfo = null;
            }

            VisitChildren(node);
        }

        public void VisitNode(JSReturnExpression node)
        {
            if (node.Value!= null && node.Value.SymbolInfo != null)
            {
                ParentNode.SymbolInfo = node.Value.SymbolInfo;
                node.SymbolInfo = null;
            }

            VisitChildren(node);
        }
    }
}