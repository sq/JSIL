using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms.StaticAnalysis {
    public class Barrier {
        public readonly int NodeIndex;
        public readonly BarrierFlags Flags;
        public readonly BarrierSlot[] Slots;

        private Barrier (int node, BarrierFlags flags, BarrierSlot[] slots) {
            NodeIndex = node;
            Flags = flags;
            Slots = slots;
        }

        public override string ToString () {
            return String.Format(
                "{0} = [{1}, {2}]", NodeIndex, Flags,
                String.Join(", ", from s in Slots select s.ToString())
            );
        }

        public static Barrier New (int node, BarrierFlags flags, SlotDictionary slots) {
            return new Barrier(node, flags, slots.ToArray());
        }

        /// <summary>
        /// Creates an empty barrier usable for search operations.
        /// </summary>
        public static Barrier Key (int node) {
            return new Barrier(node, BarrierFlags.None, null);
        }

        public static int Order (Barrier lhs, Barrier rhs) {
            return lhs.NodeIndex.CompareTo(rhs.NodeIndex);
        }

        public class Sorter : IComparer<Barrier> {
            public int Compare (Barrier x, Barrier y) {
                return Barrier.Order(x, y);
            }
        }
    }

    [Flags]
    public enum BarrierFlags : byte {
        None = 0x0,
        Jump = 0x1,
        VaryingExecution = 0x2 // May execute multiple times or never
    }

    [Flags]
    public enum SlotFlags : byte {
        None = 0x0,
        Read = 0x1,
        Write = 0x2,
        PassByReference = 0x4,
        GlobalState = 0x8,
        Assignment = 0x10 | Write,
        Through = 0x20,
        Invoke = 0x40 | Read,
        Escapes = 0x80,

        ReadWrite = Read | Write,
        ReadReassignment = Read | Assignment,

        ReadGlobalState = Read | GlobalState,
        WriteGlobalState = Write | GlobalState,
        ReadWriteGlobalState = ReadGlobalState | WriteGlobalState,
    }

    public struct BarrierSlot {
        public readonly string Name;
        public readonly SlotFlags Flags;

        public BarrierSlot (string name, SlotFlags flags) {
            Name = name;
            Flags = flags;
        }

        public override string ToString () {
            return String.Format("{0}({1})", Name, Flags);
        }
    }

    public class BarrierCollection {
        protected readonly IComparer<Barrier> Comparer = new Barrier.Sorter();
        protected readonly List<Barrier> Barriers = new List<Barrier>();
        protected bool _SortNeeded = false;

        public void Clear () {
            Barriers.Clear();
        }

        public void Add (Barrier barrier) {
            Barriers.Add(barrier);
            _SortNeeded = true;
        }

        public bool Remove (Barrier barrier) {
            return Barriers.Remove(barrier);
        }

        public bool RemoveAt (int nodeIndex) {
            SortIfNeeded();
            int index = Barriers.BinarySearch(Barrier.Key(nodeIndex), Comparer);
            if (index >= 0) {
                Barriers.RemoveAt(index);
                return true;
            }

            return false;
        }

        protected void SortIfNeeded () {
            if (_SortNeeded) {
                _SortNeeded = false;
                Barriers.Sort(Barrier.Order);
            }
        }

        public bool TryGet (int nodeIndex, out Barrier result) {
            SortIfNeeded();
            result = default(Barrier);
            int index = Barriers.BinarySearch(Barrier.Key(nodeIndex), Comparer);

            if (index >= 0) {
                result = Barriers[index];
                return true;
            }

            return false;
        }

        public Barrier[] ToArray () {
            SortIfNeeded();
            return Barriers.ToArray();
        }
    }

    public class SlotDictionary : IEnumerable {
        protected readonly Dictionary<string, SlotFlags> Dictionary = new Dictionary<string, SlotFlags>();

        public void Clear () {
            Dictionary.Clear();
        }

        public void Add (string key, SlotFlags flags) {
            this[key] = flags;
        }

        public void Add (ILVariable key, SlotFlags flags) {
            this[key] = flags;
        }

        public void Add (JSVariable key, SlotFlags flags) {
            this[key] = flags;
        }

        public SlotFlags this[string key] {
            get {
                SlotFlags result;
                if (Dictionary.TryGetValue(key, out result))
                    return result;

                return SlotFlags.None;
            }
            set {
                Dictionary[key] = value;
            }
        }

        public SlotFlags this[ILVariable v] {
            get {
                return this[v.Name];
            }
            set {
                this[v.Name] = value;
            }
        }

        public SlotFlags this[JSVariable v] {
            get {
                return this[v.Name];
            }
            set {
                this[v.Name] = value;
            }
        }

        public int Count {
            get {
                return Dictionary.Count;
            }
        }

        public BarrierSlot[] ToArray () {
            return (from kvp in Dictionary select new BarrierSlot(kvp.Key, kvp.Value)).ToArray();
        }

        public IEnumerator GetEnumerator () {
            return Dictionary.GetEnumerator();
        }
    }

    public class BarrierGenerator : JSAstVisitor {
        private class StyledCell {
            public string StyleID;
            public object Value;
        }

        public readonly BarrierCollection Result = new BarrierCollection();

        public readonly TypeSystem TypeSystem;
        public readonly JSFunctionExpression Function;

        public BarrierGenerator (TypeSystem typeSystem, JSFunctionExpression function) {
            TypeSystem = typeSystem;
            Function = function;
        }

        public void Generate () {
            Visit(Function);
        }


        protected void CreateBarrier (SlotDictionary slots, BarrierFlags flags = BarrierFlags.None) {
            Barrier existing;

            if (Result.TryGet(NodeIndex, out existing)) {
                Result.Remove(existing);

                foreach (var slot in existing.Slots)
                    slots[slot.Name] |= slot.Flags;

                Result.Add(Barrier.New(
                    NodeIndex, 
                    flags | existing.Flags, 
                    slots
                ));                
            } else {
                Result.Add(Barrier.New(
                    NodeIndex, flags, slots
                ));
            }
        }

        protected Barrier GenerateSubtreeBarrier (int startOffset = 0, BarrierFlags flags = BarrierFlags.None) {
            var result = new SlotDictionary();
            var resultFlags = flags;

            for (var i = NodeIndex + startOffset; i < NextNodeIndex; i++) {
                Barrier barrier;
                if (!Result.TryGet(i, out barrier))
                    continue;

                foreach (var slot in barrier.Slots)
                    result[slot.Name] |= slot.Flags;

                resultFlags |= barrier.Flags;
            }

            return Barrier.New(
                NodeIndex, resultFlags, result
            );
        }

        private bool IsVarying () {
            return Stack.Any(
                n =>
                    (n is JSIfStatement) ||
                    (n is JSSwitchStatement) ||
                    (n is JSLoopStatement)
            );
        }

        private BarrierFlags GetStatementFlags (BarrierFlags defaultValue = BarrierFlags.None) {
            return IsVarying() 
                ? BarrierFlags.VaryingExecution | defaultValue 
                : defaultValue;
        }

        public void VisitNode (JSStatement s) {
            VisitChildren(s);

            BarrierFlags flags = GetStatementFlags();

            var rb = GenerateSubtreeBarrier(0, flags);
            if ((rb.Flags != BarrierFlags.None) || (rb.Slots.Length > 0))
                Result.Add(rb);
        }

        public void VisitNode (JSVariable v) {
            if (CurrentName == "FunctionSignature") {
                // In argument list
                return;
            }

            var parentBoe = ParentNode as JSBinaryOperatorExpression;
            var parentDot = ParentNode as JSDotExpressionBase;
            var parentInvocation = ParentNode as JSDelegateInvocationExpression;
            var parentAccess = ParentNode as JSDotExpressionBase;

            var escapeContext = Stack.FirstOrDefault(
                n =>
                    n is JSReturnExpression ||
                    n is JSThrowExpression ||
                    n is JSOperatorExpressionBase ||
                    n is JSInvocationExpressionBase ||
                    n is JSNewExpression
            );
            var escapeBoe = escapeContext as JSBinaryOperatorExpression;

            var isWriteTarget = (
                (escapeBoe != null) &&
                (escapeBoe.Operator is JSAssignmentOperator) &&
                NameStack.FirstOrDefault(
                    n => (n == "Left") || (n == "Right")
                ) == "Left"
            );

            var isWriteSource = (
                (escapeBoe != null) &&
                (escapeBoe.Operator is JSAssignmentOperator) &&
                NameStack.FirstOrDefault(
                    n => (n == "Left") || (n == "Right")
                ) == "Right"
            );

            var isReturned = escapeContext is JSReturnExpression;
            var isThrown = escapeContext is JSThrowExpression;
            var isArgument = escapeContext is JSInvocationExpressionBase ||
                escapeContext is JSNewExpression;

            var flags = SlotFlags.None;

            if (
                (parentBoe != null) && 
                (parentBoe.Operator is JSAssignmentOperator) && 
                (CurrentName == "Left")
            ) {
                flags |= SlotFlags.Assignment;

            } else if (
                (parentDot != null) &&
                (CurrentName == "Target")
            ) {
                flags |= isWriteTarget 
                    ? SlotFlags.Write | SlotFlags.Through
                    : SlotFlags.Read | SlotFlags.Through;

            } else if (
                (parentInvocation != null) &&
                (CurrentName == "Delegate")
            ) {
                flags |= SlotFlags.Invoke;

            } else {
                flags |= SlotFlags.Read;
            }

            if (isWriteSource || isReturned || isThrown || isArgument) {
                if (parentAccess == null)
                    flags |= SlotFlags.Escapes;
            }

            if (flags != SlotFlags.None)
                CreateBarrier(new SlotDictionary {
                    {v, flags}
                });

            VisitChildren(v);
        }

        public void VisitNode (JSFieldAccess fa) {
            var parentBoe = ParentNode as JSBinaryOperatorExpression;
            var isWrite = (parentBoe != null) && 
                (parentBoe.Operator is JSAssignmentOperator) &&
                (CurrentName == "Left");

            var targetVariable = fa.Target as JSVariable;

            if (fa.HasGlobalStateDependency) {
                CreateBarrier(new SlotDictionary {
                    {
                        fa.Field.Identifier, 
                        isWrite ? SlotFlags.WriteGlobalState 
                            : SlotFlags.ReadGlobalState
                    }
                });
            } else if (targetVariable != null) {
                CreateBarrier(new SlotDictionary {
                    {
                        String.Format("{0}.{1}", targetVariable.Identifier, fa.Field.Identifier),
                        isWrite ? SlotFlags.Assignment
                            : SlotFlags.Read
                    }
                });
            }

            VisitChildren(fa);
        }

        public void VisitNode (JSReturnExpression re) {
            VisitChildren(re);

            Result.Add(GenerateSubtreeBarrier(0, BarrierFlags.Jump));
        }

        public void VisitNode (JSBreakExpression be) {
            VisitChildren(be);

            Result.Add(GenerateSubtreeBarrier(0, BarrierFlags.Jump));
        }

        public void VisitNode (JSContinueExpression ce) {
            VisitChildren(ce);

            Result.Add(GenerateSubtreeBarrier(0, BarrierFlags.Jump));
        }


        private const string ss = "urn:schemas-microsoft-com:office:spreadsheet";
        private const string x = "urn:schemas-microsoft-com:office:excel";
        private const string o = "urn:schemas-microsoft-com:office:office";

        private string[] GetXMLColumnNames (Barrier[] barriers) {
            var names = new HashSet<string>();

            foreach (var b in barriers) {
                foreach (var slot in b.Slots)
                    names.Add(slot.Name);
            }

            return names.OrderBy(n => n).ToArray();
        }

        private void WriteCell (XmlWriter xw, object value) {
            var sc = value as StyledCell;
            if (sc != null)
                value = sc.Value;

            xw.WriteStartElement("Cell");
            if (sc != null)
                xw.WriteAttributeString("StyleID", ss, sc.StyleID);

            xw.WriteStartElement("Data");
            xw.WriteAttributeString("Type", ss, "String");

            xw.WriteString(Convert.ToString(value));

            xw.WriteEndElement();
            xw.WriteEndElement();
        }

        private void WriteRow (XmlWriter xw, params object[] cells) {
            xw.WriteStartElement("Row");

            foreach (var cell in cells)
                WriteCell(xw, cell);

            xw.WriteEndElement();
        }

        private void WriteNode (XmlWriter xw, JSNode node, string nodeName, int nodeDepth) {
            var indent = new string(' ', nodeDepth * 4);

            if (node.IsNull)
                return;

            if (
                (node is JSBlockStatement) ||
                (node is JSIfStatement) ||
                (node is JSLabelGroupStatement) ||
                (node is JSSwitchStatement) ||
                (node is JSSwitchCase)
            ) {
                WriteRow(
                    xw,
                    new StyledCell {
                        StyleID = "code",
                        Value = indent + node.GetType().Name
                    }
                );
                return;
            } else if (node is JSExpressionStatement)
                return;

            WriteRow(
                xw,
                new StyledCell {
                    StyleID = "code",
                    Value = indent + node.ToString()
                }
            );
        }

        private void WriteBarrier (XmlWriter xw, Barrier barrier, string[] columnNames) {
            var cols = new List<StyledCell>();

            cols.Add(new StyledCell {
                StyleID = "cell",
                Value = barrier.Flags.ToString()
            });

            foreach (var name in columnNames) {
                cols.Add(new StyledCell {
                    StyleID = "cell" + name,
                    Value = ""
                });
            }

            foreach (var slot in barrier.Slots) {
                var i = Array.IndexOf(columnNames, slot.Name);

                cols[i + 1].Value = slot.Flags.ToString();
            }

            WriteRow(xw, cols.ToArray());
        }

        private void WriteNodesAndBarriers (XmlWriter xw, BarrierCollection barriers, string[] columnNames) {
            using (var cursor = new JSAstCursor(
                Function,
                "FunctionSignature"
            )) {
                Barrier barrier;

                while (cursor.MoveNext()) {
                    if (barriers.TryGet(cursor.NodeIndex, out barrier))
                        WriteBarrier(xw, barrier, columnNames);

                    WriteNode(xw, cursor.CurrentNode, cursor.CurrentName, cursor.Depth);
                }
            }
        }

        private void WriteStyle (
            XmlWriter xw, string id,
            string horizontalAlignment = null,
            string backgroundColor = null, 
            string fontName = null, float? fontSize = null, bool? fontBold = null,
            string textColor = null
        ) {
            xw.WriteStartElement("Style");
            xw.WriteAttributeString("ID", ss, id);

            xw.WriteStartElement("Alignment");

            if (horizontalAlignment != null)
                xw.WriteAttributeString("Horizontal", ss, horizontalAlignment);

            xw.WriteEndElement();

            xw.WriteStartElement("Font");

            if (fontName != null)
                xw.WriteAttributeString("FontName", ss, fontName);
            if (textColor != null)
                xw.WriteAttributeString("Color", ss, textColor);
            if (fontSize.HasValue)
                xw.WriteAttributeString("Size", ss, fontSize.Value.ToString());
            if (fontBold.HasValue)
                xw.WriteAttributeString("Bold", ss, fontBold.Value ? "1" : "0");

            xw.WriteEndElement();

            xw.WriteStartElement("Interior");

            if (backgroundColor != null) {
                xw.WriteAttributeString("Color", ss, backgroundColor);

                xw.WriteAttributeString("Pattern", ss, "Solid");
            }

            xw.WriteEndElement();

            xw.WriteEndElement();
        }

        public void SaveXML (string filename) {
            if (File.Exists(filename)) {
                File.SetAttributes(filename, FileAttributes.Normal);
                File.Delete(filename);
            }

            var barriers = Result.ToArray();
            var columnNames = GetXMLColumnNames(barriers);

            var settings = new XmlWriterSettings {
                CheckCharacters = false,
                CloseOutput = false,
                ConformanceLevel = ConformanceLevel.Document,
                IndentChars = "  ",
                Indent = true,
                Encoding = Encoding.UTF8
            };

            var ms = new MemoryStream();

            using (var xw = XmlWriter.Create(ms, settings)) {
                xw.WriteStartDocument();
                xw.WriteProcessingInstruction("mso-application", "progid=\"Excel.Sheet\"");

                xw.WriteStartElement("Workbook" /* , xmlns */);
                xw.WriteAttributeString("xmlns", "o", null, o);
                xw.WriteAttributeString("xmlns", "x", null, x);
                xw.WriteAttributeString("xmlns", "ss", null, ss);

                xw.WriteStartElement("Styles");

                WriteStyle(
                    xw, "code",
                    fontName: "Consolas",
                    fontSize: 11
                );

                WriteStyle(
                    xw, "header",
                    horizontalAlignment: "Center",
                    fontName: "Calibri",
                    fontSize: 11,
                    fontBold: true,
                    backgroundColor: HTMLColor.FromHSV(0, 0, HTMLColor.ValueMax * 90 / 100)
                );

                WriteStyle(
                    xw, "cell",
                    horizontalAlignment: "Center",
                    fontName: "Calibri",
                    fontSize: 10,
                    fontBold: false,
                    backgroundColor: HTMLColor.FromHSV(0, 0, HTMLColor.ValueMax * 96 / 100)
                );

                int i = 0; 
                foreach (var name in columnNames) {
                    ushort hue = (ushort)((HTMLColor.HueUnit * i / 2) % HTMLColor.HueMax);

                    WriteStyle(
                        xw, "header" + name,
                        horizontalAlignment: "Center",
                        fontName: "Calibri",
                        fontSize: 11,
                        fontBold: true,
                        backgroundColor: HTMLColor.FromHSV(hue, HTMLColor.SaturationMax * 50 / 100, HTMLColor.ValueMax * 92 / 100)
                    );

                    WriteStyle(
                        xw, "cell" + name,
                        horizontalAlignment: "Center",
                        fontName: "Calibri",
                        fontSize: 10,
                        fontBold: false,
                        backgroundColor: HTMLColor.FromHSV(hue, HTMLColor.SaturationMax * 20 / 100, HTMLColor.ValueMax * 96 / 100)
                    );

                    i += 1;
                }

                xw.WriteEndElement();

                xw.WriteStartElement("Worksheet");
                xw.WriteAttributeString("Name", ss, "Function");

                xw.WriteStartElement("Table");

                xw.WriteStartElement("Column");
                xw.WriteAttributeString("Width", ss, "90");
                xw.WriteEndElement();

                foreach (var name in columnNames) {
                    xw.WriteStartElement("Column");

                    xw.WriteAttributeString("Width", ss, "110");

                    xw.WriteEndElement();
                }

                var header = 
                    new [] { new StyledCell {
                        StyleID = "header",
                        Value = "Statement"
                    }}.Concat(
                        from cn in columnNames select new StyledCell {
                            StyleID = "header" + cn,
                            Value = cn
                    });

                WriteRow(xw, header.ToArray());

                WriteNodesAndBarriers(xw, Result, columnNames);

                xw.WriteEndElement(); // Table

                xw.WriteStartElement("WorksheetOptions");

                xw.WriteStartElement("Selected");
                xw.WriteEndElement();

                xw.WriteStartElement("FreezePanes");
                xw.WriteEndElement();

                xw.WriteStartElement("FrozenNoSplit");
                xw.WriteEndElement();

                xw.WriteStartElement("SplitHorizontal");
                xw.WriteString("1");
                xw.WriteEndElement();

                xw.WriteStartElement("TopRowBottomPane");
                xw.WriteString("1");
                xw.WriteEndElement();

                xw.WriteStartElement("ActivePane");
                xw.WriteString("2");
                xw.WriteEndElement();

                xw.WriteStartElement("Panes");

                xw.WriteStartElement("Pane");

                xw.WriteStartElement("Number");
                xw.WriteString("3");
                xw.WriteEndElement();

                xw.WriteEndElement(); // Pane

                xw.WriteStartElement("Pane");

                xw.WriteStartElement("Number");
                xw.WriteString("2");
                xw.WriteEndElement();

                xw.WriteStartElement("ActiveRow");
                xw.WriteString("0");
                xw.WriteEndElement();

                xw.WriteStartElement("RangeSelection");
                xw.WriteString("R1");
                xw.WriteEndElement();

                xw.WriteEndElement(); // Pane

                xw.WriteEndElement(); // Panes

                xw.WriteEndElement(); // WorksheetOptions

                xw.WriteEndElement(); // Worksheet

                xw.WriteEndElement();
            }

            var l = (int)ms.Length;
            var bytes = ms.GetBuffer();
            var allChars = Encoding.UTF8.GetString(bytes, 0, l);

            // XMLWriter is totally fucking broken
            var fixedXml = allChars
                .Replace("<Workbook ", "<Workbook xmlns=\"" + ss + "\" ")
                .Replace("<WorksheetOptions>", "<WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">");

            File.WriteAllText(filename, fixedXml, Encoding.UTF8);

            // This ensures that we can update the barrier XML even if it's open in excel
            File.SetAttributes(filename, FileAttributes.ReadOnly);
        }
    }
}
