using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Translator;
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

    public class TypeReferenceContext {
        public TypeReference EnclosingType;
        public TypeReference DefiningType;

        public MethodReference EnclosingMethod;
        public MethodReference DefiningMethod;
        public MethodReference InvokingMethod;
        public MethodReference SignatureMethod;

        public TypeReference EnclosingMethodType {
            get {
                if (EnclosingMethod != null)
                    return EnclosingMethod.DeclaringType;
                else
                    return null;
            }
        }

        public TypeReference DefiningMethodType {
            get {
                if (DefiningMethod != null)
                    return DefiningMethod.DeclaringType;
                else
                    return null;
            }
        }

        public TypeReference InvokingMethodType {
            get {
                if (InvokingMethod != null)
                    return InvokingMethod.DeclaringType;
                else
                    return null;
            }
        }

        public TypeReference SignatureMethodType {
            get {
                if (SignatureMethod != null)
                    return SignatureMethod.DeclaringType;
                else
                    return null;
            }
        }
    }

    public class JavascriptFormatter {
        public readonly TextWriter Output;
        public readonly AssemblyManifest Manifest;
        public readonly ITypeInfoSource TypeInfo;
        public readonly AssemblyDefinition Assembly;
        public readonly AssemblyManifest.Token PrivateToken;
        public readonly Configuration Configuration;

        public MethodReference CurrentMethod = null;

        protected readonly HashSet<string> DeclaredNamespaces = new HashSet<string>();
        protected readonly bool Stubbed;

        protected uint _IndentLevel = 0;
        protected bool _IndentNeeded = false;

        public JavascriptFormatter (
            TextWriter output, ITypeInfoSource typeInfo, 
            AssemblyManifest manifest, AssemblyDefinition assembly,
            Configuration configuration, bool stubbed
        ) {
            Output = output;
            TypeInfo = typeInfo;
            Manifest = manifest;
            Assembly = assembly;
            Configuration = configuration;
            Stubbed = stubbed;

            PrivateToken = Manifest.GetPrivateToken(assembly);
            Manifest.AssignIdentifiers();
        }

        protected void WriteToken (AssemblyManifest.Token token) {
            if (Stubbed && Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false)) {
                int id = int.Parse(token.IDString.Replace("$asm", ""), NumberStyles.HexNumber);
                WriteRaw("$asms[{0}]", id);
            } else {
                WriteRaw(token.IDString);
            }
        }

        public void AssemblyReference (AssemblyDefinition assembly) {
            string key = assembly.FullName;

            var token = Manifest.GetPrivateToken(key);
            Manifest.AssignIdentifiers();

            WriteToken(token);
        }

        public void AssemblyReference (TypeReference type) {
            string key = GetContainingAssemblyName(type);

            var token = Manifest.GetPrivateToken(key);
            Manifest.AssignIdentifiers();

            WriteToken(token);
        }

        public void Indent () {
            _IndentLevel += 1;
        }

        public void Unindent () {
            if (_IndentLevel == 0)
                Debug.WriteLine("WARNING: Indent level is already 0");

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

            if (arguments == null)
                Output.Write(format);
            else
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

        protected void CommaSeparatedListCore<T> (IEnumerable<T> _values, Action<T> writeValue, int lineSizeLimit = 2) {
            var values = _values.ToArray();

            var indentIt = values.Length > lineSizeLimit;

            if (indentIt) {
                Indent();
                NewLine();
            }

            int i = 0;
            foreach (var value in values) {
                if (i > 0) {
                    Comma();

                    if (indentIt && (i % lineSizeLimit) == 0)
                        NewLine();
                }

                writeValue(value);
                i++;
            }

            if (indentIt) {
                Unindent();
                NewLine();
            }
        }

        public void CommaSeparatedList (IEnumerable<object> values, TypeReferenceContext context, ListValueType valueType = ListValueType.Primitive) {
            CommaSeparatedListCore(
                values, (value) => {
                    if (valueType == ListValueType.Primitive)
                        Value(value as dynamic);
                    else if (valueType == ListValueType.Identifier)
                        Identifier(value as dynamic, context);
                    else if (valueType == ListValueType.TypeIdentifier)
                        TypeIdentifier(value as dynamic, context, false);
                    else if (valueType == ListValueType.TypeReference)
                        TypeReference((TypeReference)value, context);
                    else
                        WriteRaw(value.ToString());
                },

                ((valueType == ListValueType.TypeIdentifier) || (valueType == ListValueType.TypeReference)) ?
                    2 : 4
            );
        }

        public void CommaSeparatedList (IEnumerable<TypeReference> types, TypeReferenceContext context) {
            CommaSeparatedListCore(
                types, (type) => TypeReference(type, context)
            );
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

        public void WriteParameterList (IEnumerable<JSVariable> parameters) {
            bool isFirst = true;
            foreach (var p in parameters) {
                if (!isFirst)
                    Comma();

                if (p.IsReference)
                    Comment("ref");

                Identifier(p.Identifier);

                isFirst = false;
            }
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

        protected void OpenGenericParameter (string name, string context) {
            WriteRaw("new");
            Space();
            WriteRaw("JSIL.GenericParameter", null);
            LPar();

            Value(name);
            Comma();
            Value(context);

            RPar();
        }

        protected void TypeReferenceInternal (GenericParameter gp, TypeReferenceContext context) {
            var ownerType = gp.Owner as TypeReference;
            var ownerMethod = gp.Owner as MethodReference;

            if (context != null) {
                if (ownerType != null) {
                    if (ILBlockTranslator.TypesAreAssignable(TypeInfo, ownerType, context.SignatureMethodType)) {
                        TypeReference resolved = null;

                        var git = (context.SignatureMethodType as GenericInstanceType);
                        if (git != null) {
                            for (var i = 0; i < git.ElementType.GenericParameters.Count; i++) {
                                var _ = git.ElementType.GenericParameters[i];
                                if ((_.Name == gp.Name) || (_.Position == gp.Position)) {
                                    resolved = git.GenericArguments[i];
                                    break;
                                }
                            }

                            if (resolved == null)
                                throw new NotImplementedException("Could not find generic parameter in type");
                        }

                        if (resolved != null) {
                            if (resolved != gp) {
                                TypeReference(resolved, context);
                                return;
                            } else {
                                TypeIdentifier(resolved, context, false);
                                return;
                            }
                        }
                    }

                    if (ILBlockTranslator.TypesAreEqual(ownerType, context.EnclosingMethodType)) {
                        TypeIdentifier(gp, context, false);
                        return;
                    }

                    if (ILBlockTranslator.TypesAreEqual(ownerType, context.DefiningType)) {
                        OpenGenericParameter(gp.Name, context.DefiningType.FullName);
                        return;
                    }

                    if (ILBlockTranslator.TypesAreEqual(ownerType, context.EnclosingType)) {
                        WriteRaw("$.GenericParameter");
                        LPar();
                        Value(gp.Name);
                        RPar();

                        return;
                    }

                    throw new NotImplementedException("Unimplemented form of generic type parameter.");

                } else if (ownerMethod != null) {
                    Func<MethodReference, int> getPosition = (mr) => {
                        for (var i = 0; i < mr.GenericParameters.Count; i++)
                            if (mr.GenericParameters[i].Name == gp.Name)
                                return i;

                        throw new NotImplementedException("Generic parameter not found in method parameter list");
                    };

                    var ownerMethodIdentifier = new QualifiedMemberIdentifier(
                        new TypeIdentifier(ownerMethod.DeclaringType),
                        new MemberIdentifier(TypeInfo, ownerMethod)
                    );

                    if (ownerMethodIdentifier.Equals(ownerMethod, context.InvokingMethod, TypeInfo)) {
                        var gim = (GenericInstanceMethod)context.InvokingMethod;
                        TypeReference(gim.GenericArguments[getPosition(ownerMethod)], context);

                        return;
                    }

                    if (ownerMethodIdentifier.Equals(ownerMethod, context.DefiningMethod, TypeInfo)) {
                        Value(String.Format("!!{0}", getPosition(ownerMethod)));

                        return;
                    }

                    if (ownerMethodIdentifier.Equals(ownerMethod, context.EnclosingMethod, TypeInfo)) {
                        throw new NotImplementedException("Unimplemented form of generic method parameter.");
                    }

                    Value(String.Format("!!{0}", getPosition(ownerMethod)));
                    return;

                }
            } else {
                throw new NotImplementedException("Cannot resolve generic parameter without a TypeReferenceContext.");
            }

            throw new NotImplementedException("Unimplemented form of generic parameter.");
        }

        protected void TypeReferenceInternal (ByReferenceType byref, TypeReferenceContext context) {
            WriteRaw("$jsilcore");
            Dot();
            WriteRaw("TypeRef");
            LPar();

            Value("JSIL.Reference");
            Comma();
            OpenBracket(false);

            TypeReference(byref.ElementType, context);

            CloseBracket(false);
            RPar();
        }

        protected void TypeReferenceInternal (TypeReference tr, TypeReferenceContext context) {
            var type = ILBlockTranslator.DereferenceType(tr);
            var typeDef = ILBlockTranslator.GetTypeDefinition(type, false);
            var typeInfo = TypeInfo.Get(type);
            var fullName = (typeInfo != null) ? typeInfo.FullName
                    : (typeDef != null) ? typeDef.FullName
                        : type.FullName;
            var identifier = Util.EscapeIdentifier(
                fullName, EscapingMode.String
            );

            AssemblyReference(type);
            Dot();
            WriteRaw("TypeRef");

            LPar();

            Value(fullName);

            var git = tr as GenericInstanceType;
            if (git != null)
                EmitGenericTypeReferenceArguments(git, context);

            RPar();
        }

        protected void EmitGenericTypeReferenceArguments (GenericInstanceType git, TypeReferenceContext context) {
            Comma();

            OpenBracket();
            CommaSeparatedList(git.GenericArguments, context);
            CloseBracket();
        }

        protected void TypeReferenceInternal (ArrayType at, TypeReferenceContext context) {
            WriteRaw("$jsilcore");
            Dot();
            WriteRaw("TypeRef");
            LPar();
            Value("System.Array");
            Comma();
            OpenBracket();
            TypeReference(at.ElementType, context);
            CloseBracket();
            RPar();
        }

        public void TypeReference (TypeReference type, TypeReferenceContext context) {
            if (
                (context != null) &&
                (context.EnclosingType != null)
            ) {
                if (ILBlockTranslator.TypesAreEqual(type, context.EnclosingType)) {
                    // Types can reference themselves, so this prevents recursive initialization.
                    if (Stubbed && Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false)) {
                    } else {
                        WriteRaw("$.Type");
                        return;
                    }
                }

                var corlibTypes = new HashSet<string> {
                    "System.Byte", "System.UInt16", "System.UInt32", "System.UInt64",
                    "System.SByte", "System.Int16", "System.Int32", "System.Int64",
                    "System.Single", "System.Double", "System.String", "System.Object",
                    "System.Boolean", "System.Char"
                };

                // The interface builder provides helpful shorthand for corlib type references.
                if (type.Scope.Name == "mscorlib" || type.Scope.Name == "CommonLanguageRuntimeLibrary") {
                    if (corlibTypes.Contains(type.FullName)) {
                        WriteRaw("$.");
                        WriteRaw(type.Name);
                        return;
                    }
                }
            }

            if (type.FullName == "JSIL.Proxy.AnyType") {
                Value("JSIL.AnyType");
                return;
            } else if (type.FullName == "JSIL.Proxy.AnyType[]") {
                TypeReference(ILBlockTranslator.GetTypeDefinition(type), context);
                Space();
                Comment("AnyType[]");
                return;
            }

            var byref = type as ByReferenceType;
            var gp = type as GenericParameter;
            var at = type as ArrayType;

            if (byref != null)
                TypeReferenceInternal(byref, context);
            else if (gp != null)
                TypeReferenceInternal(gp, context);
            else if (at != null)
                TypeReferenceInternal(at, context);
            else
                TypeReferenceInternal(type, context);
        }

        public void TypeReference (TypeInfo type, TypeReferenceContext context) {
            TypeReference(type.Definition, context);
        }

        public void MemberDescriptor (bool isPublic, bool isStatic) {
            WriteRaw("{");

            WriteRaw("Static");
            WriteRaw(":");
            Value(isStatic);
            if (isStatic)
                WriteRaw(" ");

            Comma();

            WriteRaw("Public");
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

        internal void Identifier (string name, TypeReferenceContext context, bool includeParens = false) {
            Identifier(name);
        }

        public void Identifier (ILVariable variable, TypeReferenceContext context, bool includeParens = false) {
            if (variable.Type.IsByReference) {
                Identifier(variable.Name);
                Dot();
                Identifier("value");
            } else {
                Identifier(variable.Name);
            }
        }

        public void Identifier (TypeReference type, TypeReferenceContext context, bool includeParens = false) {
            if (type.FullName == "JSIL.Proxy.AnyType")
                WriteRaw("JSIL.AnyType");
            else
                TypeIdentifier(type as dynamic, context, includeParens);
        }

        protected void TypeIdentifier (TypeInfo type, TypeReferenceContext context, bool includeParens) {
            TypeIdentifier(type.Definition as dynamic, context, includeParens);
        }

        protected bool EmitThisForParameter (GenericParameter gp) {
            var tr = gp.Owner as TypeReference;
            if (tr != null)
                return true;

            return false;
        }

        protected void TypeIdentifier (TypeReference type, TypeReferenceContext context, bool includeParens) {
            if (type.FullName == "JSIL.Proxy.AnyType") {
                WriteRaw("JSIL.AnyType");
                return;
            }

            if (type.IsGenericParameter) {
                var gp = (GenericParameter)type;
                var ownerType = gp.Owner as TypeReference;

                if (gp.Owner == null) {
                    Value(gp.Name);
                } else if (
                    (CurrentMethod != null) &&
                    ((CurrentMethod.Equals(gp.Owner)) ||
                     (CurrentMethod.DeclaringType.Equals(gp.Owner))
                    )
                ) {
                    if (ownerType != null) {
                        Identifier(ownerType, context);
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
                        WriteToken(PrivateToken);
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

        protected void TypeIdentifier (ByReferenceType type, TypeReferenceContext context, bool includeParens) {
            if (includeParens)
                LPar();

            WriteRaw("JSIL.Reference.Of");
            LPar();
            TypeIdentifier(type.ElementType as dynamic, context, false);
            RPar();

            if (includeParens) {
                RPar();
                Space();
            }
        }

        protected void TypeIdentifier (ArrayType type, TypeReferenceContext context, bool includeParens) {
            if (includeParens)
                LPar();

            WriteRaw("System.Array.Of");
            LPar();
            TypeIdentifier(type.ElementType as dynamic, context, false);
            RPar();

            if (includeParens) {
                RPar();
                Space();
            }
        }

        protected void TypeIdentifier (OptionalModifierType modopt, TypeReferenceContext context, bool includeParens) {
            Identifier(modopt.ElementType as dynamic, context, includeParens);
        }

        protected void TypeIdentifier (RequiredModifierType modreq, TypeReferenceContext context, bool includeParens) {
            Identifier(modreq.ElementType as dynamic, context, includeParens);
        }

        protected void TypeIdentifier (PointerType ptr, TypeReferenceContext context, bool includeParens) {
            Identifier(ptr.ElementType as dynamic, context, includeParens);
        }

        protected void TypeIdentifier (GenericInstanceType type, TypeReferenceContext context, bool includeParens) {
            if (includeParens)
                LPar();

            Identifier(type.ElementType as dynamic, context, includeParens);

            Dot();
            WriteRaw("Of");
            LPar();
            CommaSeparatedList(type.GenericArguments, context, ListValueType.TypeIdentifier);
            RPar();

            if (includeParens) {
                RPar();
                Space();
            }
        }

        public void Identifier (MethodReference method, TypeReferenceContext context, bool fullyQualified = true) {
            if (fullyQualified) {
                Identifier(method.DeclaringType, context);
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
            var commentText = String.Format(commentFormat, values);
            WriteRaw("/* {0} */ ", commentText);
        }

        public void DefaultValue (TypeReference typeReference, TypeReferenceContext context) {
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
            Identifier(typeReference, context);
            LPar();
            RPar();
        }

        public void DeclareAssembly () {
            if (Stubbed && Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false)) {
                WriteRaw("var $asms = new JSIL.AssemblyCollection");
                LPar();
                OpenBrace();

                bool isFirst = true;
                foreach (var kvp in Manifest.Entries) {
                    if (!isFirst) {
                        Comma();
                        NewLine();
                    }

                    Value(int.Parse(kvp.Key.Replace("$asm", ""), NumberStyles.HexNumber));
                    WriteRaw(": ");
                    Value(kvp.Value);

                    isFirst = false;
                }

                NewLine();

                CloseBrace(false);
                RPar();
                Semicolon();
            } else {
                WriteRaw("var");
                Space();
                Identifier(PrivateToken.IDString);
                WriteRaw(" = ");
                WriteRaw("JSIL.DeclareAssembly");
                LPar();
                Value(Assembly.FullName);
                RPar();
                Semicolon();
            }
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

            WriteRaw("JSIL.DeclareNamespace");
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

        public void MethodSignature (MethodReference method, MethodSignature signature, TypeReferenceContext context) {
            // The signature cache can cause problems inside methods for generic signatures.
            var cached = Configuration.Optimizer.CacheMethodSignatures.GetValueOrDefault(true);

            // We also don't want to use the cache for method definitions in skeletons.
            if (Stubbed && Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false))
                cached = false;

            if (cached && ((context.InvokingMethod != null) || (context.EnclosingMethod != null))) {
                if (ILBlockTranslator.IsOpenType(signature.ReturnType))
                    cached = false;
                else if (signature.ParameterTypes.Any(ILBlockTranslator.IsOpenType))
                    cached = false;
            }

            if (cached) {
                WriteRaw("$sig.get");
                LPar();

                Value(TypeInfo.MethodSignatureCache.Get(signature));
                Comma();

            } else {
                LPar();

                WriteRaw("new JSIL.MethodSignature");
                LPar();
            }

            var oldSignature = context.SignatureMethod;
            context.SignatureMethod = method;

            try {
                if ((signature.ReturnType == null) || (signature.ReturnType.FullName == "System.Void"))
                    WriteRaw("null");
                else {
                    if ((context.EnclosingMethod != null) && !ILBlockTranslator.IsOpenType(signature.ReturnType))
                        TypeIdentifier(signature.ReturnType as dynamic, context, false);
                    else
                        TypeReference(signature.ReturnType, context);
                }

                Comma();
                OpenBracket(false);

                CommaSeparatedListCore(
                    signature.ParameterTypes, (pt) => {
                        if ((context.EnclosingMethod != null) && !ILBlockTranslator.IsOpenType(pt))
                            TypeIdentifier(pt as dynamic, context, false);
                        else
                            TypeReference(pt, context);
                    }
                );

                CloseBracket(false);

                if (signature.GenericParameterNames != null) {
                    Comma();
                    OpenBracket(false);
                    CommaSeparatedList(signature.GenericParameterNames, context, ListValueType.Primitive);
                    CloseBracket(false);
                }
            } finally {
                context.SignatureMethod = oldSignature;
            }

            RPar();

            if (!cached)
                RPar();
        }
    }
}
