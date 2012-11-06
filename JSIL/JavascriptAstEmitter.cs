using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Transforms;
using JSIL.Translator;
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
        public readonly Configuration Configuration;
        public readonly TypeSystem TypeSystem;
        public readonly JSILIdentifier JSIL;

        public readonly TypeReferenceContext ReferenceContext = new TypeReferenceContext();

        protected readonly Stack<JSExpression> ThisReplacementStack = new Stack<JSExpression>();
        protected readonly Stack<bool> IncludeTypeParens = new Stack<bool>();
        protected readonly Stack<Func<string, bool>> GotoStack = new Stack<Func<string, bool>>();
        protected readonly Stack<BlockType> BlockStack = new Stack<BlockType>();

        public JavascriptAstEmitter (
            JavascriptFormatter output, JSILIdentifier jsil, 
            TypeSystem typeSystem, ITypeInfoSource typeInfo,
            Configuration configuration
        ) {
            Configuration = configuration;
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

            Output.WriteRaw("var");
            Output.Space();
            Output.Identifier(labelVar);
            Output.WriteRaw(" = ");
            Output.Value(firstLabel);
            Output.Semicolon();

            Output.Label(stepLabel);
            Output.WriteRaw("while");
            Output.Space();
            Output.LPar();

            Output.WriteRaw("true");

            Output.RPar();
            Output.Space();
            Output.OpenBrace();

            Output.WriteRaw("switch");
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
                    Output.WriteRaw(" = ");
                    Output.Value(labelName);
                    Output.Semicolon();
                }

                Output.WriteRaw("continue");
                Output.Space();
                Output.Identifier(stepLabel);

                return true;
            };

            GotoStack.Push(emitGoto);

            bool needsTrailingBreak = true;

            foreach (var kvp in labelGroup.Labels) {
                if (!isFirst && needsTrailingBreak) {
                    Output.Indent();
                    emitGoto(kvp.Key);
                    Output.Semicolon(true);
                    Output.Unindent();
                }

                Output.WriteRaw("case");
                Output.Space();
                Output.Value(kvp.Key);
                Output.WriteRaw(":");
                Output.Indent();
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

                var originalLastStatement = nonNullChildren.LastOrDefault();
                var lastStatement = originalLastStatement;
                JSBlockStatement lastBlockStatement;

                while ((lastBlockStatement = lastStatement as JSBlockStatement) != null) {
                    if (
                        (lastBlockStatement.IsControlFlow) &&
                        !(
                            (lastBlockStatement == originalLastStatement) &&
                            (originalLastStatement is JSBlockStatement)
                        )
                    ) {
                        break;
                    } else {
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

                Output.Unindent();

                Output.NewLine();
            }

            GotoStack.Pop();

            if (needsTrailingBreak) {
                Output.Indent();
                Output.WriteRaw("break");
                Output.Space();
                Output.Identifier(stepLabel);
                Output.Semicolon(true);
                Output.Unindent();
            }

            Output.CloseBrace();

            Output.CloseBrace();
        }

        public void VisitNode (JSVariableDeclarationStatement vars) {
            if (vars.Declarations.Count == 0)
                return;

            Output.WriteRaw("var");
            Output.Space();

            CommaSeparatedList(vars.Declarations);

            if ((BlockStack.Count == 0) ||
                (BlockStack.Peek() != BlockType.ForHeader)
            ) {
                Output.Semicolon();
            }
        }

        public void VisitNode (JSNoOpStatement nop) {
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
            VisitDotExpression(dot);
        }

        protected void VisitDotExpression (JSDotExpressionBase dot) {
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

        public void VisitNode (JSFieldAccess fa) {
            VisitDotExpression(fa);
        }

        public void VisitNode (JSPropertyAccess pa) {
            var parens = (pa.Target is JSNumberLiteral) ||
                (pa.Target is JSIntegerLiteral);

            if (parens)
                Output.LPar();

            Visit(pa.Target);

            if (parens)
                Output.RPar();

            Output.Dot();

            var prop = pa.Property.Property;

            if (
                prop.IsAutoProperty && 
                !prop.IsVirtual && 
                !prop.DeclaringType.IsInterface
            ) {
                // When possible, access the property's backing field instead of using the property itself.
                // Property accesses are stupid slow in JavaScript :(
                Output.WriteRaw(prop.BackingFieldName);
            } else {
                if (pa.TypeQualified) {
                    // FIXME: Oh god, terrible hack
                    Output.WriteRaw(Util.EscapeIdentifier(prop.DeclaringType.Name) + "$");
                }

                Visit(pa.Member);
            }
        }

        public void VisitNode (JSMethodAccess ma) {
            var parens = (ma.Target is JSNumberLiteral) ||
                (ma.Target is JSIntegerLiteral);

            if (parens)
                Output.LPar();

            Visit(ma.Target);

            if (parens)
                Output.RPar();

            if (!ma.IsStatic) {
                Output.Dot();
                Output.Identifier("prototype");
            }

            Output.Dot();
            Visit(ma.Member);
        }

        private void WritePossiblyCachedTypeIdentifier (TypeReference type, int? index) {
            if (index.HasValue) {
                if (IncludeTypeParens.Peek())
                    Output.WriteRaw("($T{0:X2}())", index.Value);
                else
                    Output.WriteRaw("$T{0:X2}()", index.Value);
            } else
                Output.Identifier(type, ReferenceContext, false);
        }

        public void VisitNode (JSIsExpression ie) {
            IncludeTypeParens.Push(false);
            try {
                WritePossiblyCachedTypeIdentifier(ie.Type, ie.CachedTypeIndex);
            } finally {
                IncludeTypeParens.Pop();
            }

            Output.Dot();
            Output.WriteRaw("$Is");
            Output.LPar();

            Visit(ie.Expression);

            Output.RPar();
        }

        public void VisitNode (JSAsExpression ae) {
            IncludeTypeParens.Push(false);
            try {
                WritePossiblyCachedTypeIdentifier(ae.NewType, ae.CachedTypeIndex);
            } finally {
                IncludeTypeParens.Pop();
            }

            Output.Dot();
            Output.WriteRaw("$As");
            Output.LPar();

            Visit(ae.Expression);

            Output.RPar();
        }

        public void VisitNode (JSCastExpression ce) {
            IncludeTypeParens.Push(false);
            try {
                WritePossiblyCachedTypeIdentifier(ce.NewType, ce.CachedTypeIndex);
            } finally {
                IncludeTypeParens.Pop();
            }

            Output.Dot();
            Output.WriteRaw("$Cast");
            Output.LPar();

            Visit(ce.Expression);

            Output.RPar();
        }

        public void VisitNode (JSTruncateExpression te) {
            Output.LPar();
            Output.LPar();
            Visit(te.Expression);
            Output.RPar();

            Output.WriteRaw(" | 0");
            Output.RPar();
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

        public void VisitNode (JSConditionalStructCopyExpression sce) {
            Output.WriteRaw("JSIL.CloneParameter");
            Output.LPar();
            Output.Identifier(sce.Parameter, ReferenceContext, false);
            Output.Comma();
            Visit(sce.Struct);
            Output.RPar();
        }

        public void VisitNode (JSIndexerExpression idx) {
            Visit(idx.Target);
            Output.OpenBracket();
            Visit(idx.Index);
            Output.CloseBracket();
        }

        public void VisitNode (JSFakeMethod fakeMethod) {
            Output.Identifier(fakeMethod.Name);

            var ga = fakeMethod.GenericArguments;
            if (ga != null) {
                Output.LPar();
                CommaSeparatedList(ga);
                Output.RPar();
            }
        }

        public void VisitNode (JSMethod method) {
            Output.Identifier(method.GetNameForInstanceReference());

            var ga = method.GenericArguments;
            if (ga != null) {
                Output.LPar();

                var cga = method.CachedGenericArguments;
                if (cga != null) {
                    CommaSeparatedList(
                        cga.Zip(ga, (cached, uncached) => cached ?? new JSType(uncached)),
                        false
                    );
                } else {
                    Output.CommaSeparatedList(ga, ReferenceContext, ListValueType.Identifier);
                }
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

            var regex = new Regex(@"(\$\$|\$(?'name'(typeof\(this\))|([a-zA-Z_]([a-zA-Z0-9_]*)))|(?'text'[^\$]*)|)", RegexOptions.ExplicitCapture);

            if (parens)
                Output.LPar();

            bool isFirst = true;
            foreach (var line in verbatim.Expression.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
                if (String.IsNullOrWhiteSpace(line))
                    continue;

                if (!isFirst)
                    Output.NewLine();

                var matches = regex.Matches(line);

                foreach (Match m in matches) {
                    if (m.Groups["text"].Success) {
                        Output.WriteRaw(m.Groups["text"].Value);
                    } else if (m.Groups["name"].Success) {
                        var key = m.Groups["name"].Value;

                        if (verbatim.Variables.ContainsKey(key))
                            Visit(verbatim.Variables[key]);
                        else
                            Output.WriteRaw("null");
                    } else {
                        if (m.Value == "$$")
                            Output.WriteRaw("$");
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
            if (enm.CachedEnumType != null)
                Visit(enm.CachedEnumType);
            else
                Output.Identifier(enm.EnumType, ReferenceContext);

            Output.Dot();

            if (enm.Names.Length == 1) {
                Output.Identifier(enm.Names[0]);
            } else {
                Output.Identifier("Flags");
                Output.LPar();

                Output.CommaSeparatedList(
                    enm.Names, ReferenceContext, ListValueType.Primitive
                );

                Output.RPar();
            }
        }

        public void VisitNode (JSNullLiteral nil) {
            Output.WriteRaw("null");
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
            Output.WriteRaw(
                (imr.Member != null) ?
                    "JSIL.IgnoredMember" :
                    "JSIL.UnknownMember"
            );
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
            if (TypeUtil.IsEnum(defaultValue.Value)) {
                var enumInfo = TypeInfo.Get(defaultValue.Value);
                if (enumInfo.FirstEnumMember != null) {
                    Output.Identifier(defaultValue.Value, ReferenceContext);
                    Output.Dot();
                    Output.Identifier(enumInfo.FirstEnumMember.Name);
                } else {
                    Output.WriteRaw("0");
                }
            } else if (TypeUtil.IsIntegralOrEnum(defaultValue.Value)) {
                Output.Value(0);
            } else if (defaultValue.Value.IsGenericParameter) {
                VisitNode(new JSTernaryOperatorExpression(
                    new JSMemberReferenceExpression(new JSDotExpression(new JSType(defaultValue.Value),
                                                                        new JSStringIdentifier("IsValueType"))),
                    JSIL.CreateInstanceOfType(defaultValue.Value),
                    JSLiteral.Null(defaultValue.Value),
                    defaultValue.Value));
            } else if (!defaultValue.Value.IsValueType) {
                Output.WriteRaw("null");
            } else {
                switch (defaultValue.Value.FullName) {
                    case "System.Nullable`1":
                        Output.WriteRaw("null");
                        break;
                    case "System.Single":
                    case "System.Double":
                    case "System.Decimal":
                        Output.Value(0.0);
                        break;
                    case "System.Char":
                        Output.Value("\0");
                        break;
                    case "System.Boolean":
                        Output.WriteRaw("false");
                        break;
                    default:
                        VisitNode(new JSNewExpression(new JSType(defaultValue.Value), null, null));
                        break;
                }
            }
        }

        public void VisitNode (JSAssembly asm) {
            Output.AssemblyReference(asm.Assembly);
        }

        public void VisitNode (JSTypeReference tr) {
            Output.TypeReference(tr.Type, new TypeReferenceContext {
                EnclosingType = tr.Context,
                EnclosingMethod = Output.CurrentMethod
            });
        }

        public void VisitNode (JSType type) {
            Output.Identifier(
                type.Type, ReferenceContext, IncludeTypeParens.Peek()
            );
        }

        public void VisitNode (JSCachedType cachedType) {
            bool needParens = IncludeTypeParens.Peek();

            if (needParens)
                Output.WriteRaw("($T{0:X2}())", cachedType.Index);
            else
                Output.WriteRaw("$T{0:X2}()", cachedType.Index);
        }

        public void VisitNode (JSCachedTypeOfExpression cachedTypeOf) {
            IncludeTypeParens.Push(false);

            try {
                VisitNode((JSCachedType)cachedTypeOf);
            } finally {
                IncludeTypeParens.Pop();
            }

            Output.Dot();
            Output.Identifier("__Type__");
        }

        public void VisitNode (JSTypeOfExpression toe) {
            Output.Identifier(
                toe.Type, ReferenceContext, IncludeTypeParens.Peek()
            );

            if (toe.Type is GenericParameter) {
                // Generic parameters are type objects, not public interfaces
            } else {
                Output.Dot();
                Output.Identifier("__Type__");
            }
        }

        public void VisitNode (JSPublicInterfaceOfExpression poe) {
            VisitChildren(poe);

            Output.Dot();
            Output.Identifier("__PublicInterface__");
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
                    Output.WriteRaw("this");
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
                    if (JSExpression.DeReferenceType(variable.IdentifierType).IsValueType)
                        return;
                    else
                        throw new InvalidOperationException(String.Format(
                            "The this-reference '{0}' was a reference to a non-value type: {1}",
                            variable, variable.IdentifierType
                        ));
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
                Output.Value(byref.Referent.ToString());
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
                Output.WriteRaw("bind");
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
                (o) => o.WriteParameterList(function.Parameters)
            );

            if (function.TemporaryVariableCount > 0) {
                Output.WriteRaw("var ");
                for (var i = 0; i < function.TemporaryVariableCount; i++) {
                    Output.WriteRaw("$temp{0:X2}", i);

                    if (i < (function.TemporaryVariableCount - 1))
                        Output.WriteRaw(", ");
                    else
                        Output.Semicolon();
                }
            }

            Visit(function.Body);

            Output.CloseBrace(false);
            Output.CurrentMethod = oldCurrentMethod;
        }

        public void VisitNode (JSSwitchStatement swtch) {
            BlockStack.Push(BlockType.Switch);
            WriteLabel(swtch);

            Output.WriteRaw("switch");
            Output.Space();

            Output.LPar();
            Visit(swtch.Condition);
            Output.RPar();
            Output.Space();

            Output.OpenBrace();

            foreach (var c in swtch.Cases) {
                if (c.Values != null) {
                    foreach (var value in c.Values) {
                        Output.WriteRaw("case ");
                        Visit(value);
                        Output.WriteRaw(": ");
                        Output.NewLine();
                    }
                } else {
                    Output.WriteRaw("default: ");
                    Output.NewLine();
                }

                Output.Indent();
                Visit(c.Body);
                Output.Unindent();
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
            Output.WriteRaw("if");
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
                            throw new InvalidOperationException("Output mismatch");

                        o.WriteRaw("else if");
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

            Output.WriteRaw("try");
            Output.Space();
            Output.OpenBrace();

            Visit(tcb.Body);

            if (tcb.Catch != null) {
                Output.CloseAndReopenBrace((o) => {
                    if (o != Output)
                        throw new InvalidOperationException("Output mismatch");

                    o.WriteRaw("catch");
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

            Output.WriteRaw("for");
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

            Output.WriteRaw("while");
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

            Output.WriteRaw("do");
            Output.Space();
            Output.OpenBrace();

            VisitNode((JSBlockStatement)loop, false);

            Output.CloseBrace(false);
            Output.Space();
            Output.WriteRaw("while");
            Output.Space();

            Output.LPar();
            Visit(loop.Condition);
            Output.RPar();
            Output.Semicolon();

            BlockStack.Pop();
        }

        public void VisitNode (JSReturnExpression ret) {
            Output.WriteRaw("return");

            if (ret.Value != null) {
                Output.Space();
                Visit(ret.Value);
            }
        }

        public void VisitNode (JSThrowExpression ret) {
            Output.WriteRaw("throw");
            Output.Space();
            Visit(ret.Exception);
        }

        public void VisitNode (JSBreakExpression brk) {
            if (brk.TargetLoop.HasValue) {
                Output.WriteRaw("break");
                Output.Space();
                Output.Identifier(String.Format("$loop{0}", brk.TargetLoop.Value));
                return;
            }

            if (BlockStack.Count == 0) {
                throw new NotImplementedException("Break expression found outside of block");
            }

            switch (BlockStack.Peek()) {
                case BlockType.Switch:
                    Output.WriteRaw("break");
                    break;
                default:
                    throw new NotImplementedException("Break statement found outside of switch statement or loop");
                    break;
            }
        }

        public void VisitNode (JSContinueExpression cont) {
            if (cont.TargetLoop.HasValue) {
                Output.WriteRaw("continue");
                Output.Space();
                Output.Identifier(String.Format("$loop{0}", cont.TargetLoop.Value));
            } else if (GotoStack.Count > 0) {
                GotoStack.Peek()(null);
            } else {
                Output.WriteRaw("continue");
            }
        }

        public void VisitNode (JSUnaryOperatorExpression uop) {
            if (!uop.IsPostfix)
                Output.WriteRaw(uop.Operator.Token);

            Visit(uop.Expression);

            if (uop.IsPostfix)
                Output.WriteRaw(uop.Operator.Token);
        }

        private bool NeedParensForBinaryOperator (JSBinaryOperatorExpression bop) {
            if (ParentNode is JSIfStatement)
                return false;
            else if ((ParentNode is JSWhileLoop) && ((JSWhileLoop)ParentNode).Condition == bop)
                return false;
            else if ((ParentNode is JSDoLoop) && ((JSDoLoop)ParentNode).Condition == bop)
                return false;
            else if (ParentNode is JSForLoop) {
                var fl = (JSForLoop)ParentNode;
                if (
                    (fl.Condition == bop) ||
                    (fl.Increment.SelfAndChildrenRecursive.Any((n) => bop.Equals(n))) ||
                    (fl.Initializer.SelfAndChildrenRecursive.Any((n) => bop.Equals(n)))
                ) {
                    return false;
                }
            } else if ((ParentNode is JSSwitchStatement) && ((JSSwitchStatement)ParentNode).Condition == bop)
                return false;
            else if (
                (ParentNode is JSBinaryOperatorExpression) &&
                ((JSBinaryOperatorExpression)ParentNode).Operator == bop.Operator &&
                bop.Operator is JSLogicalOperator
            ) {
                return false;
            } else if (ParentNode is JSVariableDeclarationStatement)
                return false;
            else if (ParentNode is JSExpressionStatement)
                return false;

            return true;
        }

        private bool NeedTruncationForBinaryOperator (JSBinaryOperatorExpression bop, TypeReference resultType) {
            var leftType = bop.Left.GetActualType(TypeSystem);
            var rightType = bop.Right.GetActualType(TypeSystem);

            if (bop.Operator == JSOperator.Divide) {
                // We need to perform manual truncation to maintain the semantics of C#'s division operator
                return
                    (TypeUtil.IsIntegral(leftType) ||
                    TypeUtil.IsIntegral(rightType)) &&
                    TypeUtil.IsIntegral(resultType);
            }

            if (
                !(bop.Operator is JSAssignmentOperator) &&
                Configuration.Optimizer.HintIntegerArithmetic.GetValueOrDefault(true)
            ) {
                // If type hinting is enabled, we want to truncate after every binary operator we apply to integer values.
                // This allows JS runtimes to more easily determine that code is using integers, and omit overflow checks.
                return
                    TypeUtil.Is32BitIntegral(leftType) &&
                    TypeUtil.Is32BitIntegral(rightType) &&
                    TypeUtil.Is32BitIntegral(resultType);
            }

            return false;
        }

        public void VisitNode (JSBinaryOperatorExpression bop) {
            var resultType = bop.GetActualType(TypeSystem);

            bool needsCast = (bop.Operator is JSArithmeticOperator) && 
                TypeUtil.IsEnum(TypeUtil.StripNullable(resultType));
            bool needsTruncation = NeedTruncationForBinaryOperator(bop, resultType);
            bool parens = NeedParensForBinaryOperator(bop);

            if (needsTruncation) {
                if (bop.Operator is JSAssignmentOperator)
                    throw new NotImplementedException("Truncation of assignment operations not implemented");
            } else if (needsCast) {
                Output.Identifier(TypeUtil.StripNullable(resultType), ReferenceContext);
                Output.WriteRaw(".$Cast");
            }

            parens |= needsCast;

            if (needsTruncation)
                Output.LPar();
            if (parens)
                Output.LPar();

            Visit(bop.Left);
            Output.Space();
            Output.WriteRaw(bop.Operator.Token);
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

            if (needsTruncation) {
                Output.WriteRaw(" | 0");
                Output.RPar();
            }
        }

        public void VisitNode (JSTernaryOperatorExpression ternary) {
            Output.LPar();

            Visit(ternary.Condition);

            Output.WriteRaw(" ? ");
            Visit(ternary.True);

            Output.WriteRaw(" : ");
            Visit(ternary.False);

            Output.RPar();
        }

        public void VisitNode (JSNewArrayExpression newarray) {
            if (newarray.IsMultidimensional)
                Output.WriteRaw("JSIL.MultidimensionalArray.New");
            else
                Output.WriteRaw("JSIL.Array.New");

            Output.LPar();

            IncludeTypeParens.Push(false);
            try {
                WritePossiblyCachedTypeIdentifier(newarray.ElementType, newarray.CachedElementTypeIndex);
            } finally {
                IncludeTypeParens.Pop();
            }

            if (newarray.Dimensions != null) {
                Output.Comma();

                CommaSeparatedList(newarray.Dimensions, false);
            }

            if (newarray.SizeOrArrayInitializer != null) {
                Output.Comma();

                Visit(newarray.SizeOrArrayInitializer);
            }

            Output.RPar();
        }

        public void VisitNode (JSNewExpression newexp) {
            var outer = Stack.Skip(1).FirstOrDefault();
            var outerInvocation = outer as JSInvocationExpression;
            var outerDot = outer as JSDotExpressionBase;

            bool parens = ((outerDot != null) && (outerDot.Target == newexp)) ||
                ((outerInvocation != null) && (outerInvocation.ThisReference == newexp));

            var ctor = newexp.Constructor;
            var isOverloaded = (ctor != null) &&
                ctor.IsOverloadedRecursive &&
                !ctor.Metadata.HasAttribute("JSIL.Meta.JSRuntimeDispatch");

            bool hasArguments = newexp.Arguments.Count > 0;

            var oldInvoking = ReferenceContext.InvokingMethod;

            if (isOverloaded && CanUseFastOverloadDispatch(ctor))
                isOverloaded = false;

            try {
                if (isOverloaded) {
                    Output.MethodSignature(newexp.ConstructorReference, ctor.Signature, ReferenceContext);
                    Output.Dot();

                    ReferenceContext.InvokingMethod = newexp.ConstructorReference;

                    Output.Identifier("Construct");
                    Output.LPar();

                    IncludeTypeParens.Push(false);
                    try {
                        Visit(newexp.Type);
                    } finally {
                        IncludeTypeParens.Pop();
                    }

                    if (hasArguments) {
                        Output.Comma();
                        CommaSeparatedList(newexp.Arguments);
                    }

                    Output.RPar();
                } else {
                    if (parens)
                        Output.LPar();

                    Output.WriteRaw("new");
                    Output.Space();

                    IncludeTypeParens.Push(true);
                    try {
                        Visit(newexp.Type);
                    } finally {
                        IncludeTypeParens.Pop();
                    }

                    ReferenceContext.InvokingMethod = newexp.ConstructorReference;

                    Output.LPar();
                    CommaSeparatedList(newexp.Arguments);
                    Output.RPar();

                    if (parens)
                        Output.RPar();
                }
            } finally {
                ReferenceContext.InvokingMethod = oldInvoking;
            }
        }

        public void VisitNode (JSPairExpression pair) {
            Visit(pair.Key);
            Output.WriteRaw(": ");
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
                CountOfMatchingSubtrees<JSFunctionExpression>(new[] { invocation.Delegate }) > 0;

            if (needsParens)
                Output.LPar();

            Visit(invocation.Delegate);

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

        protected bool CanUseFastOverloadDispatch (MethodInfo method) {
            MethodSignatureSet mss;

            if (method.DeclaringType.MethodSignatures.TryGet(method.NamedSignature.Name, out mss)) {
                int overloadCount = 0;

                var gaCount = method.GenericParameterNames.Length;
                int argCount = method.Parameters.Length;

                foreach (var signature in mss.Signatures) {
                    if (
                        (signature.ParameterCount == argCount)
                    )
                        overloadCount += 1;
                    else if ((signature.GenericParameterNames.Length > 0) || (gaCount > 0)) {
                        if (
                            (signature.ParameterCount == gaCount) ||
                            (signature.GenericParameterNames.Length == argCount) ||
                            (signature.GenericParameterNames.Length == gaCount)
                        ) {
                            overloadCount += 1;
                        }
                    }
                }

                // If there's only one overload with this argument count, we don't need to use
                //  the expensive overloaded method dispatch path.
                return overloadCount < 2;
            }

            return false;
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

            if (isOverloaded && CanUseFastOverloadDispatch(method))
                isOverloaded = false;

            var oldInvoking = ReferenceContext.InvokingMethod;
            try {
                if (isOverloaded) {

                    var methodName = Util.EscapeIdentifier(jsm.GetNameForInstanceReference(), EscapingMode.MemberIdentifier);

                    Output.MethodSignature(jsm.Reference, method.Signature, ReferenceContext);
                    Output.Dot();

                    ReferenceContext.InvokingMethod = jsm.Reference;

                    Action genericArgs = () => {
                        if (hasGenericArguments) {
                            Output.OpenBracket(false);
                            Output.CommaSeparatedList(invocation.GenericArguments, ReferenceContext, ListValueType.TypeIdentifier);
                            Output.CloseBracket(false);
                        } else
                            Output.Identifier("null", null);
                    };

                    if (isStatic) {
                        Output.Identifier("CallStatic");
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

                if (jsm != null) {
                    ReferenceContext.InvokingMethod = jsm.Reference;
                } else {
                    ReferenceContext.InvokingMethod = null;
                }

                bool needLineBreak = ArgumentsNeedLineBreak(invocation.Arguments);

                if (needLineBreak)
                    Output.NewLine();

                CommaSeparatedList(invocation.Arguments, needLineBreak);

                if (needLineBreak)
                    Output.NewLine();

                Output.RPar();
            } finally {
                ReferenceContext.InvokingMethod = oldInvoking;
            }
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

        public void VisitNode (JSNestedObjectInitializerExpression noie) {
            Output.WriteRaw("new JSIL.ObjectInitializer");
            Output.LPar();
            Visit(noie.Initializer);
            Output.RPar();
        }

        public void VisitNode (JSCommaExpression comma) {
            Output.LPar();

            CommaSeparatedList(comma.SubExpressions, false);

            Output.RPar();
        }
    }
}
