using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;
using System.Globalization;
using JSIL;

namespace JSIL.Internal {
    public enum ListValueType {
        Primitive,
        Identifier,
        Raw,
        TypeReference,
        TypeIdentifier
    }

    public class JavascriptFormatter {
        public readonly TextWriter Output;
        public readonly PlainTextOutput PlainTextOutput;
        public readonly TextOutputFormatter PlainTextFormatter;
        public readonly AssemblyManifest Manifest;
        public readonly ITypeInfoSource TypeInfo;
        public readonly AssemblyDefinition Assembly;
        public readonly string PrivateToken;

        public MethodReference CurrentMethod = null;

        protected readonly HashSet<string> DeclaredNamespaces = new HashSet<string>();

        public JavascriptFormatter (TextWriter output, ITypeInfoSource typeInfo, AssemblyManifest manifest, AssemblyDefinition assembly) {
            Output = output;
            PlainTextOutput = new PlainTextOutput(Output);
            PlainTextFormatter = new TextOutputFormatter(PlainTextOutput);
            TypeInfo = typeInfo;
            Manifest = manifest;
            Assembly = assembly;
            PrivateToken = Manifest.GetPrivateToken(assembly);
        }

        public void AssemblyReference (TypeReference type) {
            string key = GetContainingAssemblyName(type);

            Identifier(Manifest.GetPrivateToken(key), null);
        }

        public void LPar () {
            PlainTextOutput.Write("(");
            PlainTextOutput.Indent();
        }

        public void RPar () {
            PlainTextOutput.Unindent();
            PlainTextOutput.Write(")");
        }

        public void Space () {
            PlainTextOutput.Write(" ");
        }

        public void Comma () {
            PlainTextOutput.Write(", ");
        }

        public void CommaSeparatedList (IEnumerable<object> values, ListValueType valueType = ListValueType.Primitive) {
            bool isFirst = true;
            foreach (var value in values) {
                if (!isFirst)
                    Comma();

                if (valueType == ListValueType.Primitive)
                    Value(value as dynamic);
                else if (valueType == ListValueType.Identifier)
                    Identifier(value as dynamic);
                else if (valueType == ListValueType.TypeIdentifier)
                    TypeIdentifier(value as dynamic, false, true);
                else if (valueType == ListValueType.TypeReference)
                    TypeReference(value as dynamic);
                else
                    PlainTextOutput.Write(value.ToString());

                isFirst = false;
            }
        }

        public void CommaSeparatedList<T, U> (
            IEnumerable<KeyValuePair<T, U>> pairs, 
            ListValueType keyType = ListValueType.Primitive,
            ListValueType valueType = ListValueType.Primitive
        ) {
            bool isFirst = true;
            foreach (var kvp in pairs) {
                if (!isFirst)
                    Comma();

                if (keyType == ListValueType.Primitive)
                    Value(kvp.Key as dynamic);
                else if (keyType == ListValueType.Identifier)
                    Identifier(kvp.Key as dynamic);
                else if (keyType == ListValueType.TypeIdentifier)
                    TypeIdentifier(kvp.Key as dynamic, false, true);
                else if (keyType == ListValueType.TypeReference)
                    TypeReference(kvp.Key as dynamic);
                else
                    PlainTextOutput.Write(kvp.Key.ToString());

                Token(": ");

                if (valueType == ListValueType.Primitive)
                    Value(kvp.Value as dynamic);
                else if (valueType == ListValueType.Identifier)
                    Identifier(kvp.Value as dynamic);
                else if (valueType == ListValueType.TypeIdentifier)
                    TypeIdentifier(kvp.Value as dynamic, false, true);
                else if (valueType == ListValueType.TypeReference)
                    TypeReference(kvp.Value as dynamic);
                else
                    PlainTextOutput.Write(kvp.Value.ToString());

                isFirst = false;
            }
        }

        public void CommaSeparatedList (IEnumerable<TypeReference> types) {
            bool isFirst = true;
            foreach (var type in types) {
                if (!isFirst)
                    Comma();

                Identifier(type, false);

                isFirst = false;
            }
        }

        public void OpenBracket (bool indent = false) {
            PlainTextOutput.Write("[");

            if (indent) {
                PlainTextOutput.Indent();
                PlainTextOutput.WriteLine();
            }
        }

        public void CloseBracket (bool indent = false, Action newline = null) {
            if (indent) {
                PlainTextOutput.Unindent();
                PlainTextOutput.WriteLine();
            }

            PlainTextOutput.Write("]");

            if (indent) {
                if (newline != null)
                    newline();
                else
                    PlainTextOutput.WriteLine();
            }
        }

        public void OpenBrace () {
            PlainTextOutput.WriteLine("{");
            PlainTextOutput.Indent();
        }

        public void CloseBrace (bool newLine = true) {
            PlainTextOutput.Unindent();
            if (newLine)
                PlainTextOutput.WriteLine("}");
            else
                PlainTextOutput.Write("}");
        }

        public void CloseAndReopenBrace (Action<JavascriptFormatter> midtext) {
            PlainTextOutput.Unindent();
            PlainTextOutput.Write("} ");
            midtext(this);
            PlainTextOutput.WriteLine(" {");
            PlainTextOutput.Indent();
        }

        public void CloseAndReopenBrace (string midtext) {
            PlainTextOutput.Unindent();
            PlainTextOutput.WriteLine(String.Format("}} {0} {{", midtext));
            PlainTextOutput.Indent();
        }

        public void OpenFunction (string functionName, Action<JavascriptFormatter> emitParameters) {
            PlainTextOutput.Write("function");
            Space();

            if (functionName != null) {
                PlainTextOutput.Write(Util.EscapeIdentifier(functionName));
                Space();
            }

            LPar();
            if (emitParameters != null)
                emitParameters(this);
            RPar();

            Space();
            OpenBrace();
        }

        public static string GetContainingAssemblyName (TypeReference tr) {
            var scope = tr.Scope;
            switch (scope.MetadataScopeType) {
                case MetadataScopeType.AssemblyNameReference:
                    return ((AssemblyNameReference)scope).FullName;
                case MetadataScopeType.ModuleReference:
                    throw new NotImplementedException();
                case MetadataScopeType.ModuleDefinition:
                    return ((ModuleDefinition)scope).Assembly.FullName;
            }

            return tr.Module.Assembly.FullName;
        }

        public void TypeReference (TypeReference type) {
            if (type.FullName == "JSIL.Proxy.AnyType") {
                Value("JSIL.AnyType");
                return;
            } else if (type.FullName == "JSIL.Proxy.AnyType[]") {
                Value("System.Array");
                Space();
                Comment("AnyType[]");
                return;
            }

            var isReference = type is ByReferenceType;
            var originalType = type;
            type = ILBlockTranslator.DereferenceType(type);
            var typeDef = ILBlockTranslator.GetTypeDefinition(type, false);
            var typeInfo = TypeInfo.Get(type);
            var identifier = Util.EscapeIdentifier(
                (typeInfo != null) ? typeInfo.FullName 
                    : (typeDef != null) ? typeDef.FullName 
                        : type.FullName
                , EscapingMode.String
            );
            var git = type as GenericInstanceType;
            var at = type as ArrayType;

            if (isReference) {
                Value("JSIL.Reference");
                Space();
                Comment("{0}", originalType);
            } else if (type is GenericParameter) {
                Keyword("new");
                Space();
                Identifier("JSIL.GenericParameter", null);
                LPar();
                Value(identifier);
                RPar();
            } else if ((git != null) || (GetContainingAssemblyName(type) != Assembly.FullName)) {
                Keyword("new");
                Space();
                Identifier("JSIL.TypeRef", null);
                LPar();

                AssemblyReference(type);
                Comma();

                Value(identifier);

                if (git != null) {
                    Comma();
                    OpenBracket();
                    CommaSeparatedList(git.GenericArguments, ListValueType.TypeReference);
                    CloseBracket();
                }

                RPar();
            } else if (at != null) {
                Value("System.Array");
                Space();
                Comment("{0}", originalType);
            } else {
                Value(identifier);
            }
        }

        public void TypeReference (TypeInfo type) {
            TypeReference(type.Definition);
        }

        public void Identifier (string name, EscapingMode? escapingMode = EscapingMode.MemberIdentifier) {
            if (escapingMode.HasValue)
                PlainTextOutput.Write(Util.EscapeIdentifier(
                    name, escapingMode.Value
                ));
            else
                PlainTextOutput.Write(name);
        }

        public void Identifier (ILVariable variable) {
            if (variable.Type.IsByReference) {
                Identifier(variable.Name);
                Dot();
                Identifier("value");
            } else {
                Identifier(variable.Name);
            }
        }

        public void Identifier (TypeReference type, bool includeParens = false, bool replaceGenerics = false) {
            if (type.FullName == "JSIL.Proxy.AnyType")
                Identifier("JSIL.AnyType", null);
            else
                TypeIdentifier(type as dynamic, includeParens, replaceGenerics);
        }

        protected void TypeIdentifier (TypeInfo type, bool includeParens, bool replaceGenerics) {
            TypeIdentifier(type.Definition as dynamic, includeParens, replaceGenerics);
        }

        protected bool EmitThisForParameter (GenericParameter gp) {
            var tr = gp.Owner as TypeReference;
            if (tr != null)
                return true;

            return false;
        }

        protected void TypeIdentifier (TypeReference type, bool includeParens, bool replaceGenerics) {
            if (type.FullName == "JSIL.Proxy.AnyType") {
                Identifier("JSIL.AnyType", null);
                return;
            }

            if (type.IsGenericParameter) {
                var gp = (GenericParameter)type;

                if (
                    (CurrentMethod != null) &&
                    ((CurrentMethod.Equals(gp.Owner)) ||
                     (CurrentMethod.DeclaringType.Equals(gp.Owner))
                    )
                ) {
                    if (EmitThisForParameter(gp)) {
                        Keyword("this");
                        Dot();
                    }

                    Identifier(type.FullName);
                } else if (replaceGenerics) {
                    if (type.IsValueType)
                        Identifier("JSIL.AnyValueType", null);
                    else
                        Identifier("JSIL.AnyType", null);
                } else {
                    if (EmitThisForParameter(gp)) {
                        Keyword("this");
                        Dot();
                    }

                    Identifier(type.FullName);
                }
            } else {
                var typedef = type.Resolve();
                if (typedef != null) {
                    if (GetContainingAssemblyName(typedef) == Assembly.FullName) {
                        PlainTextOutput.Write(PrivateToken);
                        PlainTextOutput.Write(".");
                    } else {
                        AssemblyReference(typedef);
                        PlainTextOutput.Write(".");
                    }
                }

                var info = TypeInfo.Get(type);
                PlainTextOutput.Write(Util.EscapeIdentifier(
                    info.FullName, EscapingMode.TypeIdentifier
                ));
            }
        }

        protected void TypeIdentifier (ByReferenceType type, bool includeParens, bool replaceGenerics) {
            if (includeParens)
                LPar();

            Identifier("JSIL.Reference.Of", null);
            LPar();
            TypeIdentifier(type.ElementType as dynamic, false, replaceGenerics);
            RPar();

            if (includeParens) {
                RPar();
                Space();
            }
        }

        protected void TypeIdentifier (ArrayType type, bool includeParens, bool replaceGenerics) {
            if (includeParens)
                LPar();

            Identifier("System.Array.Of", null);
            LPar();
            TypeIdentifier(type.ElementType as dynamic, false, replaceGenerics);
            RPar();

            if (includeParens) {
                RPar();
                Space();
            }
        }

        protected void TypeIdentifier (OptionalModifierType modopt, bool includeParens, bool replaceGenerics) {
            Identifier(modopt.ElementType as dynamic, includeParens, replaceGenerics);
        }

        protected void TypeIdentifier (RequiredModifierType modreq, bool includeParens, bool replaceGenerics) {
            Identifier(modreq.ElementType as dynamic, includeParens, replaceGenerics);
        }

        protected void TypeIdentifier (PointerType ptr, bool includeParens, bool replaceGenerics) {
            Identifier(ptr.ElementType as dynamic, includeParens, replaceGenerics);
        }

        protected void TypeIdentifier (GenericInstanceType type, bool includeParens, bool replaceGenerics) {
            if (includeParens)
                LPar();

            Identifier(type.ElementType as dynamic, includeParens, replaceGenerics);

            Dot();
            Identifier("Of", null);
            LPar();
            CommaSeparatedList(type.GenericArguments, ListValueType.TypeIdentifier);
            RPar();

            if (includeParens) {
                RPar();
                Space();
            }
        }

        public void Identifier (MethodReference method, bool fullyQualified = true) {
            if (fullyQualified) {
                Identifier(method.DeclaringType);
                Dot();

                if (method.HasThis) {
                    Identifier("prototype");
                    Dot();
                }
            }

            var info = TypeInfo.GetMethod(method);
            if (info != null) {
                Identifier(info.Name);
            } else {
                Debug.WriteLine("Method missing type information: {0}", method.FullName);
                Identifier(method.Name);
            }
        }

        public void Keyword (string keyword) {
            PlainTextOutput.Write(keyword);
        }

        public void Token (string token) {
            PlainTextOutput.Write(token);
        }

        public void Dot () {
            PlainTextOutput.Write(".");
        }

        public void Semicolon () {
            Semicolon(true);
        }

        public void Semicolon (bool lineBreak) {
            if (lineBreak)
                PlainTextOutput.WriteLine(";");
            else
                PlainTextOutput.Write("; ");
        }

        public void NewLine () {
            PlainTextOutput.WriteLine();
        }

        public void Value (bool value) {
            if (value)
                PlainTextOutput.Write("true");
            else
                PlainTextOutput.Write("false");
        }

        public void Value (string value) {
            PlainTextOutput.Write(Util.EscapeString(value));
        }

        public void Value (char value) {
            PlainTextOutput.Write(Util.EscapeString("" + value));
        }

        public void Value (long value) {
            PlainTextOutput.Write(value.ToString());
        }

        public void Value (ulong value) {
            PlainTextOutput.Write(value.ToString());
        }

        public void Value (double value) {
            PlainTextOutput.Write(value.ToString("R", CultureInfo.InvariantCulture));
        }

        public void Value (TypeReference type) {
            Value(GetNameOfType(type as dynamic));
        }

        protected string GetNameOfType (TypeReference type) {
            var info = TypeInfo.Get(type);
            return info.FullName;
        }

        protected string GetNameOfType (ArrayType type) {
            return GetNameOfType(type.ElementType as dynamic) + "[]";
        }

        protected string GetNameOfType (GenericInstanceType type) {
            return String.Format("{0}[{1}]",
                GetNameOfType(type.ElementType as dynamic),
                String.Join(", ", (from ga in type.GenericArguments
                                   select GetNameOfType(ga as dynamic)).ToArray())
            );
        }

        public void Comment (string commentFormat, params object[] values) {
            var commentText = String.Format(" " + commentFormat + " ", values);
            PlainTextFormatter.WriteComment(
                CommentType.MultiLine, commentText
            );
            PlainTextFormatter.Space();
        }

        public void DefaultValue (TypeReference typeReference) {
            string fullName = typeReference.FullName;

            if (TypeAnalysis.IsIntegerOrEnum(typeReference)) {
                Value(0);
                return;
            } else if (!typeReference.IsValueType) {
                Keyword("null");
                return;
            }

            switch (fullName) {
                case "System.Nullable`1":
                    Keyword("null");
                    return;
                case "System.Single":
                case "System.Double":
                case "System.Decimal":
                    Value(0.0);
                    return;
            }

            Keyword("new");
            Space();
            Identifier(typeReference);
            LPar();
            RPar();
        }

        public void DeclareAssembly () {
            Keyword("var");
            Space();
            Identifier(PrivateToken);
            Token(" = ");
            Identifier("JSIL.DeclareAssembly", null);
            LPar();
            Value(Assembly.FullName);
            RPar();
            Semicolon();
        }

        public void DeclareNamespace (string ns) {
            if (String.IsNullOrEmpty(ns))
                return;

            if (DeclaredNamespaces.Contains(ns))
                return;

            DeclaredNamespaces.Add(ns);

            var lastDot = ns.LastIndexOfAny(new char[] { '.', '/', '+', ':' });
            string parent;
            if (lastDot > 0) {
                parent = ns.Substring(0, lastDot);

                // Handle ::
                if (parent.EndsWith(":"))
                    parent = parent.Substring(0, parent.Length - 1);

                DeclareNamespace(parent);
            }

            Identifier("JSIL.DeclareNamespace", null);
            LPar();
            Value(Util.EscapeIdentifier(ns, EscapingMode.String));
            RPar();
            Semicolon();
        }

        public void Label (string labelName) {
            PlainTextFormatter.Unindent();
            Identifier(labelName);
            Token(": ");
            PlainTextFormatter.Indent();
            NewLine();
        }
    }
}
