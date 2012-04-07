using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
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
        public readonly AssemblyManifest Manifest;
        public readonly ITypeInfoSource TypeInfo;
        public readonly AssemblyDefinition Assembly;
        public readonly AssemblyManifest.Token PrivateToken;

        public MethodReference CurrentMethod = null;

        protected readonly HashSet<string> DeclaredNamespaces = new HashSet<string>();

        protected uint _IndentLevel = 0;
        protected bool _IndentNeeded = false;

        public JavascriptFormatter (TextWriter output, ITypeInfoSource typeInfo, AssemblyManifest manifest, AssemblyDefinition assembly) {
            Output = output;
            TypeInfo = typeInfo;
            Manifest = manifest;
            Assembly = assembly;

            PrivateToken = Manifest.GetPrivateToken(assembly);
            Manifest.AssignIdentifiers();
        }

        public void AssemblyReference (AssemblyDefinition assembly) {
            string key = assembly.FullName;

            var token = Manifest.GetPrivateToken(key);
            Manifest.AssignIdentifiers();

            Identifier(token.IDString, null);
        }

        public void AssemblyReference (TypeReference type) {
            string key = GetContainingAssemblyName(type);

            var token = Manifest.GetPrivateToken(key);
            Manifest.AssignIdentifiers();

            Identifier(token.IDString, null);
        }

        public void Indent () {
            _IndentLevel += 1;
        }

        public void Unindent () {
            if (_IndentLevel == 0)
                throw new InvalidOperationException("Indent level is already 0");

            _IndentLevel -= 1;
        }

        public void NewLine () {
            Output.Write(Environment.NewLine);
            _IndentNeeded = true;
        }

        protected void WriteIndentIfNeeded () {
            if (!_IndentNeeded)
                return;

            _IndentNeeded = false;
            Output.Write(new string(' ', (int)(_IndentLevel * 2)));
        }

        public void WriteRaw (string characters) {
            WriteIndentIfNeeded();

            Output.Write(characters);
        }

        public void WriteRaw (string format, params object[] arguments) {
            WriteIndentIfNeeded();

            Output.Write(String.Format(format, arguments));
        }

        public void LPar () {
            WriteRaw("(");
            Indent();
        }

        public void RPar () {
            Unindent();
            WriteRaw(")");
        }

        public void Space () {
            WriteRaw(" ");
        }

        public void Comma () {
            WriteRaw(", ");
        }

        public void CommaSeparatedList (IEnumerable<object> values, ListValueType valueType = ListValueType.Primitive) {
            bool isFirst = true;
            foreach (var value in values) {
                if (!isFirst) {
                    Comma();

                    if ((valueType == ListValueType.TypeIdentifier) ||
                        (valueType == ListValueType.TypeReference))
                        NewLine();
                }

                if (valueType == ListValueType.Primitive)
                    Value(value as dynamic);
                else if (valueType == ListValueType.Identifier)
                    Identifier(value as dynamic);
                else if (valueType == ListValueType.TypeIdentifier)
                    TypeIdentifier(value as dynamic, false, true);
                else if (valueType == ListValueType.TypeReference)
                    TypeReference(value as dynamic);
                else
                    WriteRaw(value.ToString());

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
                    WriteRaw(kvp.Key.ToString());

                WriteRaw(": ");

                if (valueType == ListValueType.Primitive)
                    Value(kvp.Value as dynamic);
                else if (valueType == ListValueType.Identifier)
                    Identifier(kvp.Value as dynamic);
                else if (valueType == ListValueType.TypeIdentifier)
                    TypeIdentifier(kvp.Value as dynamic, false, true);
                else if (valueType == ListValueType.TypeReference)
                    TypeReference(kvp.Value as dynamic);
                else
                    WriteRaw(kvp.Value.ToString());

                isFirst = false;
            }
        }

        public void CommaSeparatedList (IEnumerable<TypeReference> types, TypeReference context = null) {
            bool isFirst = true;
            foreach (var type in types) {
                if (!isFirst)
                    Comma();

                TypeReference(type as dynamic, context);

                isFirst = false;
            }
        }

        public void OpenBracket (bool indent = false) {
            WriteRaw("[");

            if (indent) {
                Indent();
                NewLine();
            }
        }

        public void CloseBracket (bool indent = false, Action newline = null) {
            if (indent) {
                Unindent();
                NewLine();
            }

            WriteRaw("]");

            if (indent) {
                if (newline != null)
                    newline();
                else
                    NewLine();
            }
        }

        public void OpenBrace () {
            WriteRaw("{");
            Indent();
            NewLine();
        }

        public void CloseBrace (bool newLine = true) {
            Unindent();

            WriteRaw("}");

            if (newLine)
                NewLine();
        }

        public void CloseAndReopenBrace (Action<JavascriptFormatter> midtext) {
            Unindent();
            WriteRaw("} ");
            midtext(this);
            WriteRaw(" {");
            NewLine();
            Indent();
        }

        public void CloseAndReopenBrace (string midtext) {
            Unindent();
            WriteRaw("}} {0} {{", midtext);
            NewLine();
            Indent();
        }

        public void OpenFunction (string functionName, Action<JavascriptFormatter> emitParameters) {
            WriteRaw("function ");

            if (functionName != null) {
                WriteRaw(Util.EscapeIdentifier(functionName));
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
            var resolved = tr.Resolve();

            IMetadataScope scope;

            if (resolved != null)
                scope = resolved.Scope;
            else
                scope = tr.Scope;

            switch (scope.MetadataScopeType) {
                case MetadataScopeType.AssemblyNameReference:
                    return ((AssemblyNameReference)scope).FullName;
                case MetadataScopeType.ModuleReference:
                    throw new NotImplementedException();
                case MetadataScopeType.ModuleDefinition:
                    var assembly = ((ModuleDefinition)scope).Assembly;
                    if (assembly != null)
                        return assembly.FullName;
                    else
                        return "<Assembly Not Loaded>";
            }

            if (resolved != null)
                return resolved.Module.Assembly.FullName;
            else
                return tr.Module.Assembly.FullName;
        }

        public void TypeReference (TypeReference type, TypeReference context = null) {
            if ((context != null) && ILBlockTranslator.TypesAreEqual(type, context)) {
                // If the field's type is its declaring type, we need to avoid recursively initializing it.
                Identifier("$", null);
                Dot();
                Identifier("Type");
                return;
            }

            if (type.FullName == "JSIL.Proxy.AnyType") {
                Value("JSIL.AnyType");
                return;
            } else if (type.FullName == "JSIL.Proxy.AnyType[]") {
                TypeReference(ILBlockTranslator.GetTypeDefinition(type));
                Space();
                Comment("AnyType[]");
                return;
            }

            var isReference = type is ByReferenceType;
            var originalType = type;
            type = ILBlockTranslator.DereferenceType(type);
            var typeDef = ILBlockTranslator.GetTypeDefinition(type, false);
            var typeInfo = TypeInfo.Get(type);
            var fullName = (typeInfo != null) ? typeInfo.FullName
                    : (typeDef != null) ? typeDef.FullName
                        : type.FullName;
            var identifier = Util.EscapeIdentifier(
                fullName, EscapingMode.String
            );
            var git = type as GenericInstanceType;
            var at = type as ArrayType;

            if (isReference) {
                if (type is GenericParameter) {
                    Value("JSIL.Reference");
                    Space();
                    Comment("{0}", originalType);
                } else {

                    Identifier("$jsilcore", null);
                    Dot();
                    Identifier("TypeRef", null);
                    LPar();

                    Value("JSIL.Reference");
                    Comma();
                    OpenBracket(false);

                    TypeReference(type);

                    CloseBracket(false);
                    RPar();
                }
            } else if (type is GenericParameter) {
                var gp = (GenericParameter)type;
                WriteRaw("new");
                Space();
                Identifier("JSIL.GenericParameter", null);
                LPar();
                Value(gp.Name);

                if (gp.Owner is TypeReference) {
                    Comma();
                    Value(gp.Owner as TypeReference);
                } else if (gp.Owner is MethodDefinition) {
                    Comma();
                    Value((gp.Owner as MethodDefinition).FullName);
                }

                RPar();
            } else if (at != null) {
                TypeIdentifier(at, false, true);
                /*
                TypeReference(ILBlockTranslator.GetTypeDefinition(at));
                Space();
                Comment("{0}", originalType);
                */
            } else {
                AssemblyReference(type);
                Dot();
                Identifier("TypeRef", null);

                LPar();

                Value(fullName);

                if (git != null) {
                    Comma();

                    OpenBracket();
                    CommaSeparatedList(git.GenericArguments, ListValueType.TypeReference);
                    CloseBracket();
                }

                RPar();
            }
        }

        public void TypeReference (TypeInfo type) {
            TypeReference(type.Definition);
        }

        public void MemberDescriptor (bool isPublic, bool isStatic) {
            WriteRaw("{");

            Identifier("Static", null);
            WriteRaw(":");
            Value(isStatic);
            if (isStatic)
                WriteRaw(" ");

            Comma();

            Identifier("Public", null);
            WriteRaw(":");
            Value(isPublic);
            if (isPublic)
                WriteRaw(" ");

            WriteRaw("}");
        }

        public void Identifier (string name, EscapingMode? escapingMode = EscapingMode.MemberIdentifier) {
            if (escapingMode.HasValue)
                WriteRaw(Util.EscapeIdentifier(
                    name, escapingMode.Value
                ));
            else
                WriteRaw(name);
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
                var ownerType = gp.Owner as TypeReference;

                if (
                    (CurrentMethod != null) &&
                    ((CurrentMethod.Equals(gp.Owner)) ||
                     (CurrentMethod.DeclaringType.Equals(gp.Owner))
                    )
                ) {
                    if (ownerType != null) {
                        Identifier(ownerType);
                        Dot();
                        Identifier(gp.Name);
                        Dot();
                        Identifier("get");
                        LPar();
                        WriteRaw("this");
                        RPar();
                    } else {
                        Identifier(gp.Name);
                    }
                } else if (replaceGenerics) {
                    if (type.IsValueType)
                        Identifier("JSIL.AnyValueType", null);
                    else
                        Identifier("JSIL.AnyType", null);
                } else {
                    if (EmitThisForParameter(gp)) {
                        WriteRaw("this");
                        Dot();
                    }

                    Identifier(type.FullName);
                }
            } else {
                var info = TypeInfo.Get(type);
                if (info.Replacement != null) {
                    WriteRaw(info.Replacement);
                    return;
                }

                var typedef = type.Resolve();
                if (typedef != null) {
                    if (GetContainingAssemblyName(typedef) == Assembly.FullName) {
                        WriteRaw(PrivateToken.IDString);
                        WriteRaw(".");
                    } else {
                        AssemblyReference(typedef);
                        WriteRaw(".");
                    }
                }

                WriteRaw(Util.EscapeIdentifier(
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

        public void Dot () {
            WriteRaw(".");
        }

        public void Semicolon () {
            Semicolon(true);
        }

        public void Semicolon (bool lineBreak) {
            WriteRaw(";");

            if (lineBreak)
                NewLine();
            else
                Space();
        }

        public void Value (bool value) {
            WriteRaw(value ? "true" : "false");
        }

        public void Value (string value) {
            WriteRaw(Util.EscapeString(value));
        }

        public void Value (char value) {
            WriteRaw(Util.EscapeString(new string(value, 1)));
        }

        public void Value (long value) {
            WriteRaw(value.ToString());
        }

        public void Value (ulong value) {
            WriteRaw(value.ToString());
        }

        public void Value (double value) {
            WriteRaw(value.ToString("R", CultureInfo.InvariantCulture));
        }

        public void Value (TypeReference type) {
            Value(GetNameOfType(type as dynamic));
        }

        protected string GetNameOfType (GenericParameter gp) {
            return gp.Name;
        }

        protected string GetNameOfType (TypeReference type) {
            var info = TypeInfo.Get(type);
            if (info == null)
                throw new InvalidOperationException("No type information for type " + type);

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
            WriteRaw("/* {0} */", commentText);
        }

        public void DefaultValue (TypeReference typeReference) {
            string fullName = typeReference.FullName;

            if (TypeAnalysis.IsIntegerOrEnum(typeReference)) {
                Value(0);
                return;
            } else if (!typeReference.IsValueType) {
                WriteRaw("null");
                return;
            }

            switch (fullName) {
                case "System.Nullable`1":
                    WriteRaw("null");
                    return;
                case "System.Single":
                case "System.Double":
                case "System.Decimal":
                    Value(0.0);
                    return;
            }

            WriteRaw("new");
            Space();
            Identifier(typeReference);
            LPar();
            RPar();
        }

        public void DeclareAssembly () {
            WriteRaw("var");
            Space();
            Identifier(PrivateToken.IDString);
            WriteRaw(" = ");
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
            Unindent();
            Identifier(labelName);
            WriteRaw(": ");
            Indent();
            NewLine();
        }

        public void MethodSignature (TypeReference context, TypeReference returnType, IEnumerable<TypeReference> _parameterTypes, bool methodContext) {
            WriteRaw("new JSIL.MethodSignature");
            LPar();

            var parameterTypes = _parameterTypes.ToArray();
            if (parameterTypes.Length > 2)
                NewLine();

            if ((returnType == null) || (returnType.FullName == "System.Void"))
                Identifier("null", null);
            else
                TypeReference(returnType, context);

            Comma();
            OpenBracket(parameterTypes.Length > 2);

            if (methodContext) {
                CommaSeparatedList(parameterTypes, ListValueType.Identifier);
            } else {
                CommaSeparatedList(parameterTypes, context);
            }

            CloseBracket(parameterTypes.Length > 2);

            if (parameterTypes.Length > 2)
                NewLine();

            RPar();
        }
    }
}
