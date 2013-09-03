using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Internal;

namespace JSIL.Ast {
    public class JSNullStatement : JSStatement {
        public override bool IsNull {
            get {
                return true;
            }
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            return;
        }

        public override string ToString () {
            return PrependLabel("<Null>");
        }
    }

    public class JSNoOpStatement : JSStatement {
        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            return;
        }

        public override string ToString () {
            return PrependLabel("nop");
        }
    }

    public class JSBlockStatement : JSStatement {
        [JSAstTraverse(999, "Statements")]
        public readonly List<JSStatement> Statements;

        public JSBlockStatement (params JSStatement[] statements) {
            Statements = new List<JSStatement>(statements);
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            var stmt = newChild as JSStatement;
            if (stmt == null)
                return;

            for (int i = 0, c = Statements.Count; i < c; i++) {
                if (Statements[i] == oldChild)
                    Statements[i] = stmt;
            }
        }

        public bool InsertNearChildRecursive (JSStatement relativeTo, JSStatement newChild, int offset = 0) {
            for (int i = 0, c = Statements.Count; i < c; i++) {
                if (
                    (Statements[i] == relativeTo) ||
                    Statements[i].AllChildrenRecursive.Any((n) => n == relativeTo)
                ) {
                    Statements.Insert(i + offset, newChild);
                    return true;
                }
            }

            return false;
        }

        public override string ToString () {
            return ToString(true);
        }

        protected string ToString (bool prependLabel) {
            var sb = new StringBuilder();

            foreach (var stmt in Statements)
                sb.AppendLine(String.Concat(stmt));

            if (prependLabel)
                return PrependLabel(sb.ToString());
            else
                return sb.ToString();
        }
    }

    public abstract class JSLoopStatement : JSBlockStatement {
        public int? Index;

        public JSLoopStatement () {
            IsControlFlow = true;
        }

        protected override string PrependLabel (string text) {
            if (!Index.HasValue)
                return text;

            return String.Format("$loop{0}: {1}", Index.Value, text);
        }
    }

    public class JSLabelGroupStatement : JSStatement {
        public readonly int GroupIndex;
        public readonly OrderedDictionary<string, JSStatement> Labels = new OrderedDictionary<string, JSStatement>();

        private LinkedListNode<string> EntryLabelNode, ExitLabelNode;

        public JSLabelGroupStatement (int index, JSStatement entryLabel, JSStatement exitLabel) {
            GroupIndex = index;

            MarkAsControlFlow(entryLabel);
            MarkAsControlFlow(exitLabel);

            EntryLabelNode = Labels.Enqueue(entryLabel.Label, entryLabel);
            ExitLabelNode = Labels.Enqueue(exitLabel.Label, exitLabel);
        }

        public JSStatement EntryLabel {
            get {
                if (Labels.ContainsKey(EntryLabelNode.Value))
                    return Labels[EntryLabelNode.Value];
                else
                    return null;
            }
            set {
                if ((value.Label == null) || !Labels.ContainsKey(value.Label))
                    throw new InvalidOperationException();

                Labels.Remove(EntryLabelNode.Value);
                EntryLabelNode = Labels.FindNode(value.Label);
            }
        }

        public JSStatement BeforeExitLabel {
            get {
                var priorLabel = ExitLabelNode.Previous;
                if (priorLabel == null)
                    return null;

                if (Labels.ContainsKey(priorLabel.Value))
                    return Labels[priorLabel.Value];
                else
                    return null;
            }
        }

        public JSStatement ExitLabel {
            get {
                if (Labels.ContainsKey(ExitLabelNode.Value))
                    return Labels[ExitLabelNode.Value];
                else
                    return null;
            }
            set {
                if ((value.Label == null) || !Labels.ContainsKey(value.Label))
                    throw new InvalidOperationException();

                Labels.Remove(ExitLabelNode.Value);
                ExitLabelNode = Labels.FindNode(value.Label);
            }
        }

        private void MarkAsControlFlow (JSStatement s) {
            s.IsControlFlow = true;
        }

        [JSAstTraverse(0)]
        protected static bool GetChild (JSNode parent, int index, out JSNode node, out string name) {
            var self = (JSLabelGroupStatement)parent;

            if (index >= self.Labels.Count) {
                node = null;
                name = null;
                return false;
            }

            node = self.Labels.AtIndex(index).Value;
            name = "Labels";
            return true;
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            var stmt = newChild as JSStatement;
            if (stmt == null)
                return;

            foreach (var kvp in Labels.ToArray()) {
                if (kvp.Value == oldChild) {
                    if (stmt.Label == kvp.Key)
                        Labels.Replace(kvp.Key, stmt);
                    else if (stmt.Label == null) {
                        stmt.Label = kvp.Key;

                        if (stmt.IsNull)
                            Labels.Remove(kvp.Key);
                        else
                            Labels.Replace(kvp.Key, stmt);
                    } else {
                        Labels.Remove(kvp.Key);

                        if (!stmt.IsNull) {
                            if (Labels.ContainsKey(stmt.Label))
                                throw new InvalidOperationException("Replacing LabelGroupStatement child '" + oldChild + "' with '" + newChild + "' but group already contains the label '" + stmt.Label + "'");

                            Add(stmt);
                        }
                    }
                }
            }
        }

        public override string ToString () {
            var sb = new StringBuilder();
            sb.AppendLine("{");

            foreach (var kvp in Labels) {
                var lines = String.Concat(kvp.Value).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                foreach (var line in lines)
                    sb.AppendLine("  " + line);
            }

            sb.AppendLine("}");

            return PrependLabel(sb.ToString());
        }

        public void Add (JSStatement statement) {
            if (statement.Label == null)
                throw new InvalidOperationException("Cannot add an unlabeled statement to a label group");

            MarkAsControlFlow(statement);

            Labels.EnqueueBefore(ExitLabelNode, statement.Label, statement);
        }
    }

    [JSAstIgnoreInheritedMembers]
    public class JSVariableDeclarationStatement : JSStatement {
        [JSAstTraverse(0)]
        public readonly List<JSBinaryOperatorExpression> Declarations = new List<JSBinaryOperatorExpression>();

        public JSVariableDeclarationStatement (params JSBinaryOperatorExpression[] declarations) {
            Declarations.AddRange(declarations);
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            var boe = newChild as JSBinaryOperatorExpression;

            if (boe == null)
                Declarations.RemoveAll((c) => c == oldChild);
            else if (boe.Operator != JSOperator.Assignment)
                throw new InvalidOperationException("A variable declaration statement may only contain assignments");
            else
                for (int i = 0, c = Declarations.Count; i < c; i++) {
                    if (Declarations[i] == oldChild)
                        Declarations[i] = boe;
                }
        }

        public override string ToString () {
            return PrependLabel(String.Format(
                "var {0}",
                String.Join(", ", (from d in Declarations select String.Concat(d)).ToArray())
            ));
        }
    }

    [JSAstIgnoreInheritedMembers]
    public class JSExpressionStatement : JSStatement {
        [JSAstTraverse(0, "Expression")]
        protected JSExpression _Expression;

        public JSExpressionStatement (JSExpression expression) {
            _Expression = expression;
        }

        public JSExpression Expression {
            get {
                return _Expression;
            }
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            if (oldChild == _Expression)
                _Expression = (JSExpression)newChild;
        }

        public override string ToString () {
            return PrependLabel(String.Format("~ {0}", _Expression));
        }
    }

    public class JSSwitchCase : JSStatement {
        [JSAstTraverse(0, "Case Value")]
        public readonly JSExpression[] Values;
        [JSAstTraverse(1)]
        public readonly JSBlockStatement Body;
        public readonly bool IsDefault;
        
        public JSSwitchCase (JSExpression[] values, JSBlockStatement body, bool isDefault) {
            if ((values != null) && (values.Length == 0))
                values = null;

            Values = values;
            Body = body;
            IsDefault = isDefault;
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (Values == null)
                return;

            var jse = newChild as JSExpression;

            if (jse != null) {
                for (var i = 0; i < Values.Length; i++)
                    if (oldChild.Equals(Values[i]))
                        Values[i] = jse;
            }
        }

        public override string ToString () {
            if (Values != null)
                return String.Format(
                    "{0} {{\r\n{1}\r\n}}",
                    String.Join(
                        Environment.NewLine, (
                            from v in Values select String.Format("case {0}:", v)
                        ).ToArray()
                    ), Util.Indent(Body)
                );
            else
                return String.Format(
                    "default: {{\r\n{0}\r\n}}", Util.Indent(Body)
                );
        }
    }

    public class JSSwitchStatement : JSStatement {
        [JSAstTraverse(0, "Condition")]
        protected JSExpression _Condition;
        [JSAstTraverse(1, "Case")]
        public readonly List<JSSwitchCase> Cases = new List<JSSwitchCase>();

        public JSSwitchStatement (JSExpression condition, params JSSwitchCase[] cases) {
            IsControlFlow = true;
            _Condition = condition;
            Cases.AddRange(cases);
        }

        public JSExpression Condition {
            get {
                return _Condition;
            }
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            if (_Condition == oldChild)
                _Condition = (JSExpression)newChild;

            var cse = newChild as JSSwitchCase;

            if (cse != null) {
                for (int i = 0, c = Cases.Count; i < c; i++) {
                    if (Cases[i] == oldChild)
                        Cases[i] = cse;
                }
            }
        }

        public override string ToString () {
            return String.Format(
                "switch ({0}) {{\r\n{1}\r\n}}",
                Condition, Util.Indent(
                    String.Join(Environment.NewLine, (from c in Cases select c.ToString()).ToArray())
                )
            );
        }
    }

    public class JSIfStatement : JSStatement {
        [JSAstTraverse(0, "Condition")]
        protected JSExpression _Condition;
        [JSAstTraverse(1, "TrueClause")]
        protected JSStatement _TrueClause;
        [JSAstTraverse(2, "FalseClause")]
        protected JSStatement _FalseClause;

        public JSIfStatement (JSExpression condition, JSStatement trueClause, JSStatement falseClause = null) {
            _Condition = condition;
            _TrueClause = trueClause;
            _FalseClause = falseClause;

            if (_TrueClause != null)
                _TrueClause.IsControlFlow = true;
            if (_FalseClause != null)
                _FalseClause.IsControlFlow = true;
        }

        public static JSIfStatement New (params KeyValuePair<JSExpression, JSStatement>[] conditions) {
            if ((conditions == null) || (conditions.Length == 0))
                throw new ArgumentNullException("conditions");

            JSIfStatement result = new JSIfStatement(
                conditions[0].Key, conditions[0].Value
            );
            JSIfStatement next = null, current = result;

            for (int i = 1; i < conditions.Length; i++) {
                var cond = conditions[i].Key;

                if (cond != null) {
                    next = new JSIfStatement(cond, conditions[i].Value);
                    current._FalseClause = next;
                    current = next;
                } else
                    current._FalseClause = conditions[i].Value;
            }

            return result;
        }

        public JSExpression Condition {
            get {
                return _Condition;
            }
        }

        public JSStatement TrueClause {
            get {
                return _TrueClause;
            }
        }

        public JSStatement FalseClause {
            get {
                return _FalseClause;
            }
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            if (_Condition == oldChild)
                _Condition = (JSExpression)newChild;

            if (_TrueClause == oldChild)
                _TrueClause = (JSStatement)newChild;

            if (_FalseClause == oldChild)
                _FalseClause = (JSStatement)newChild;
        }

        public override string ToString () {
            if ((_FalseClause != null) && !_FalseClause.IsNull)
                return String.Format(
                    "if ({0}) {{\r\n{1}\r\n}} else {{\r\n{2}\r\n}}",
                    _Condition, Util.Indent(_TrueClause), Util.Indent(_FalseClause)
                );
            else
                return String.Format(
                    "if ({0}) {{\r\n{1}\r\n}}",
                    _Condition, Util.Indent(_TrueClause)
                );
        }
    }

    public class JSWhileLoop : JSLoopStatement {
        [JSAstTraverse(0, "Condition")]
        protected JSExpression _Condition;

        public JSWhileLoop (JSExpression condition, params JSStatement[] body) {
            _Condition = condition;
            Statements.AddRange(body);
        }

        public JSExpression Condition {
            get {
                return _Condition;
            }
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            if (_Condition == oldChild)
                _Condition = (JSExpression)newChild;

            if (newChild is JSStatement)
                base.ReplaceChild(oldChild, newChild);
        }

        public override string ToString () {
            return PrependLabel(String.Format(
                "while ({0}) {{\r\n{1}\r\n}}",
                _Condition, Util.Indent(base.ToString(false))
            ));
        }
    }

    public class JSDoLoop : JSLoopStatement {
        [JSAstTraverse(0, "Condition")]
        protected JSExpression _Condition;

        public JSDoLoop (JSExpression condition, params JSStatement[] body) {
            _Condition = condition;
            Statements.AddRange(body);
        }

        public JSExpression Condition {
            get {
                return _Condition;
            }
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            if (_Condition == oldChild)
                _Condition = (JSExpression)newChild;

            if (newChild is JSStatement)
                base.ReplaceChild(oldChild, newChild);
        }

        public override string ToString () {
            return PrependLabel(String.Format(
                "do {{\r\n{1}\r\n}} while ({0})",
                _Condition, Util.Indent(base.ToString(false))
            ));
        }
    }

    public class JSForLoop : JSLoopStatement {
        [JSAstTraverse(0, "Initializer")]
        protected JSStatement _Initializer;
        [JSAstTraverse(1, "Condition")]
        protected JSExpression _Condition;
        [JSAstTraverse(2, "Increment")]
        protected JSStatement _Increment;

        public JSForLoop (JSStatement initializer, JSExpression condition, JSStatement increment, params JSStatement[] body) {
            _Initializer = initializer;
            _Condition = condition;
            _Increment = increment;
            Statements.AddRange(body);
        }

        public JSStatement Initializer {
            get {
                return _Initializer;
            }
        }

        public JSExpression Condition {
            get {
                return _Condition;
            }
        }

        public JSStatement Increment {
            get {
                return _Increment;
            }
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            if (_Initializer == oldChild)
                _Initializer = (JSStatement)newChild;

            if (_Condition == oldChild)
                _Condition = (JSExpression)newChild;

            if (_Increment == oldChild)
                _Increment = (JSStatement)newChild;

            if (newChild is JSStatement)
                base.ReplaceChild(oldChild, newChild);
        }

        public override string ToString () {
            return PrependLabel(String.Format(
                "for ({0}; {1}; {2}) {{\r\n{3}\r\n}}",
                _Initializer, _Condition, _Increment,
                Util.Indent(base.ToString(false))
            ));
        }
    }

    public class JSTryCatchBlock : JSStatement {
        [JSAstTraverse(0)]
        public readonly JSStatement Body;
        [JSAstTraverse(1, "Catch Variable")]
        public JSVariable CatchVariable;
        [JSAstTraverse(2, "Catch Block")]
        public JSStatement Catch;
        [JSAstTraverse(2, "Finally Block")]
        public JSStatement Finally;

        public JSTryCatchBlock (JSStatement body, JSVariable catchVariable = null, JSStatement @catch = null, JSStatement @finally = null) {
            Body = body;
            CatchVariable = catchVariable;
            Catch = @catch;
            Finally = @finally;
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            if (CatchVariable == oldChild)
                CatchVariable = (JSVariable)newChild;

            if (Catch == oldChild)
                Catch = (JSStatement)newChild;

            if (Finally == oldChild)
                Finally = (JSStatement)newChild;

            Body.ReplaceChild(oldChild, newChild);
        }

        public override string ToString () {
            return String.Format(
                "try {{ {0} }} catch ( {1} ) {{ {2} }} finally {{ {3} }}",
                Body,
                CatchVariable,
                Catch,
                Finally
            );
        }
    }

    public class JSUntranslatableStatement : JSNullStatement {
        public readonly object Type;

        public JSUntranslatableStatement (object type) {
            Type = type;
        }

        public override string ToString () {
            return String.Format("Untranslatable Statement {0}", Type);
        }
    }
}
