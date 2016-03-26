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
    using Mono.Cecil.Cil;

    public class AttachSymbolInfoToStatements : JSAstVisitor {
        public void VisitNode(JSFunctionExpression node)
        {
            if (node.Body != null)
            {
                VisitNodeInternalDown(node.Body);
                VisitNodeInternalUp(node.Body, null);
            }
        }

        private void VisitNodeInternalDown(JSNode node)
        {
            if (node == null)
            {
                return;
            }

            foreach (var child in node.Children)
            {
                VisitNodeInternalDown(child);
            }

            if (node.SymbolInfo == null)
            {
                var childSymboInfo = node.Children.Where(item => item != null).Select(item => item.SymbolInfo).FirstOrDefault(item => item != null);
                if (childSymboInfo != null)
                {
                    node.SymbolInfo = new SymbolInfo(childSymboInfo.SequencePoints, true);
                }
            }
        }

        private void VisitNodeInternalUp(JSNode node, IEnumerable<SequencePoint> sequencePoints)
        {
            if (node == null)
            {
                return;
            }

            if (sequencePoints != null && node.SymbolInfo == null)
            {
                node.SymbolInfo = new SymbolInfo(sequencePoints, true);
            }

            foreach (var child in node.Children)
            {
                if (child == null)
                {
                    continue;
                }

                if (child.SymbolInfo != null)
                {
                    sequencePoints = child.SymbolInfo.SequencePoints;
                }

                VisitNodeInternalUp(child, sequencePoints);
            }
        }


    }
}
