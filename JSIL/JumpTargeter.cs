using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.NRefactory.CSharp;

namespace JSIL {
    public class JumpTargeter : ContextTrackingVisitor<object> {
        public int NextIndex = 0;
        public readonly Stack<string> BlockLabelStack = new Stack<string>();

        public JumpTargeter (DecompilerContext context)
            : base(context) {
        }

        protected string GetNewLabel () {
            return String.Format("_block{0}_", NextIndex++);
        }

        protected void PrependNewLabel (Statement statement) {
            var label = GetNewLabel();
            BlockLabelStack.Push(label);
            statement.Parent.InsertChildBefore(
                statement, new LabelStatement {
                    Label = label
                }, BlockStatement.StatementRole
            );
        }

        public override object VisitForStatement (ForStatement forStatement, object data) {
            PrependNewLabel(forStatement);
            var result = base.VisitForStatement(forStatement, data);
            BlockLabelStack.Pop();
            return result;
        }

        public override object VisitDoWhileStatement (DoWhileStatement doWhileStatement, object data) {
            PrependNewLabel(doWhileStatement);
            var result = base.VisitDoWhileStatement(doWhileStatement, data);
            BlockLabelStack.Pop();
            return result;
        }

        public override object VisitWhileStatement (WhileStatement whileStatement, object data) {
            PrependNewLabel(whileStatement);
            var result = base.VisitWhileStatement(whileStatement, data);
            BlockLabelStack.Pop();
            return result;
        }

        public override object VisitSwitchStatement (SwitchStatement switchStatement, object data) {
            PrependNewLabel(switchStatement);
            var result = base.VisitSwitchStatement(switchStatement, data);
            BlockLabelStack.Pop();
            return result;
        }

        public override object VisitBreakStatement (BreakStatement breakStatement, object data) {
            if (BlockLabelStack.Count == 0)
                return base.VisitBreakStatement(breakStatement, data);

            var result = new TargetedBreakStatement(BlockLabelStack.Peek());
            breakStatement.ReplaceWith(result);
            return null;
        }

        public override object VisitContinueStatement (ContinueStatement continueStatement, object data) {
            if (BlockLabelStack.Count == 0)
                return base.VisitContinueStatement(continueStatement, data);

            var result = new TargetedContinueStatement(BlockLabelStack.Peek());
            continueStatement.ReplaceWith(result);
            return null;
        }

        public override object VisitMethodDeclaration (MethodDeclaration methodDeclaration, object data) {
            NextIndex = 0;

            return base.VisitMethodDeclaration(methodDeclaration, data);
        }

        public override object VisitTypeDeclaration (TypeDeclaration typeDeclaration, object data) {
            NextIndex = 0;

            return base.VisitTypeDeclaration(typeDeclaration, data);
        }
    }
}
