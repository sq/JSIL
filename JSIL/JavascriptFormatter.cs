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
        private struct _State {
            public TypeReference EnclosingType;
            public TypeReference DefiningType;

            public MethodReference EnclosingMethod;
            public MethodReference DefiningMethod;
            public MethodReference InvokingMethod;
            public MethodReference SignatureMethod;
            public MethodReference AttributesMethod;
        }

        private readonly Stack<_State> Stack = new Stack<_State>();
        private _State State;

        public void Push () {
            Stack.Push(State);
        }

        public void Pop () {
            State = Stack.Pop();
        }

        public TypeReference EnclosingType {
            get {
                return State.EnclosingType;
            }
            set {
                State.EnclosingType = value;
            }
        }
        public TypeReference DefiningType {
            get {
                return State.DefiningType;
            }
            set {
                State.DefiningType = value;
            }
        }

        public MethodReference EnclosingMethod {
            get {
                return State.EnclosingMethod;
            }
            set {
                State.EnclosingMethod = value;
            }
        }

        public MethodReference DefiningMethod {
            get {
                return State.DefiningMethod;
            }
            set {
                State.DefiningMethod = value;
            }
        }

        public MethodReference InvokingMethod {
            get {
                return State.InvokingMethod;
            }
            set {
                State.InvokingMethod = value;
            }
        }

        public MethodReference SignatureMethod {
            get {
                return State.SignatureMethod;
            }
            set {
                State.SignatureMethod = value;
            }
        }

        public MethodReference AttributesMethod {
            get {
                return State.AttributesMethod;
            }
            set {
                State.AttributesMethod = value;
            }
        }

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

        public TypeReference AttributesMethodType {
            get {
                if (AttributesMethod != null)
                    return AttributesMethod.DeclaringType;
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

        protected readonly static HashSet<string> CorlibTypes = new HashSet<string> {
            "System.Byte", "System.UInt16", "System.UInt32", "System.UInt64",
            "System.SByte", "System.Int16", "System.Int32", "System.Int64",
            "System.Single", "System.Double", "System.String", "System.Object",
            "System.Boolean", "System.Char"
        }; 

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
                WriteRaw("${1}[{0}]", id, Configuration.AssemblyCollectionName ?? "asms");
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
            var tabs = _IndentLevel;
            for (var i = 0; i < tabs; i++)
                Output.Write("  ");
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
                    throw new NotImplementedException("Module references not implemented");
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

        protected void LocalOpenGenericParameter (GenericParameter gp) {
            WriteRaw("$.GenericParameter");
            LPar();
            Value(gp.Name);
            RPar();

            WriteGenericParameterAttributes(gp);
        }

        protected void OpenGenericParameter (GenericParameter gp, string context) {
            WriteRaw("new");
            Space();
            WriteRaw("JSIL.GenericParameter", null);
            LPar();

            Value(gp.Name);
            Comma();
            Value(context);

            RPar();

            WriteGenericParameterAttributes(gp);
        }

        protected void WriteGenericParameterAttributes (GenericParameter gp) {
            if ((gp.Attributes & GenericParameterAttributes.Covariant) == GenericParameterAttributes.Covariant)
                WriteRaw(".out()");
            if ((gp.Attributes & GenericParameterAttributes.Contravariant) == GenericParameterAttributes.Contravariant)
                WriteRaw(".in()");
        }

        protected void TypeReferenceInternal (GenericParameter gp, TypeReferenceContext context) {
            var ownerType = gp.Owner as TypeReference;
            var ownerMethod = gp.Owner as MethodReference;

            if (context != null) {
                if (ownerType != null) {
                    if (TypeUtil.TypesAreAssignable(TypeInfo, ownerType, context.SignatureMethodType)) {
                        var resolved = JSIL.Internal.MethodSignature.ResolveGenericParameter(gp, context.SignatureMethodType);

                        if (resolved != null) {
                            if (resolved != gp) {
                                context.Push();
                                context.SignatureMethod = null;
                                try {
                                    TypeReference(resolved, context);
                                } finally {
                                    context.Pop();
                                }
                                return;
                            } else {
                                TypeIdentifier(resolved, context, false);
                                return;
                            }
                        }
                    }

                    if (TypeUtil.TypesAreEqual(ownerType, context.EnclosingMethodType)) {
                        TypeIdentifier(gp, context, false);
                        return;
                    }

                    if (TypeUtil.TypesAreEqual(ownerType, context.EnclosingType)) {
                        LocalOpenGenericParameter(gp);
                        return;
                    }

                    var ownerTypeResolved = ownerType.Resolve();
                    if (ownerTypeResolved != null) {
                        // Is it a generic parameter of a compiler-generated class (i.e. enumerator function, delegate, etc)
                        //  nested inside our EnclosingType? If so, uhhhh, shit.
                        if (
                            TypeUtil.TypesAreEqual(context.EnclosingType, ownerTypeResolved.DeclaringType) &&
                            ownerTypeResolved.CustomAttributes.Any(
                                (ca) => ca.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"
                            )
                        ) {
                            // FIXME: I HAVE NO IDEA WHAT I AM DOING
                            OpenGenericParameter(gp, Util.DemangleCecilTypeName(ownerTypeResolved.FullName));
                            return;
                        }
                    }

                    if (TypeUtil.TypesAreEqual(ownerType, context.AttributesMethodType)) {
                        LocalOpenGenericParameter(gp);
                        return;
                    }

                    if (TypeUtil.TypesAreEqual(ownerType, context.DefiningMethodType)) {
                        OpenGenericParameter(gp, Util.DemangleCecilTypeName(context.DefiningMethodType.FullName));
                        return;
                    }

                    if (TypeUtil.TypesAreEqual(ownerType, context.DefiningType)) {
                        OpenGenericParameter(gp, Util.DemangleCecilTypeName(context.DefiningType.FullName));
                        return;
                    }

                    throw new NotImplementedException(String.Format(
                        "Unimplemented form of generic type parameter: '{0}'.",
                        gp
                    ));

                } else if (ownerMethod != null) {
                    Func<MethodReference, int> getPosition = (mr) => {
                        for (var i = 0; i < mr.GenericParameters.Count; i++)
                            if (mr.GenericParameters[i].Name == gp.Name)
                                return i;

                        throw new NotImplementedException(String.Format(
                            "Generic parameter '{0}' not found in method '{1}' parameter list",
                            gp, ownerMethod
                        ));
                    };

                    var ownerMethodIdentifier = new QualifiedMemberIdentifier(
                        new TypeIdentifier(ownerMethod.DeclaringType.Resolve()),
                        new MemberIdentifier(TypeInfo, ownerMethod)
                    );

                    if (ownerMethodIdentifier.Equals(ownerMethod, context.InvokingMethod, TypeInfo)) {
                        var gim = (GenericInstanceMethod)context.InvokingMethod;
                        TypeReference(gim.GenericArguments[getPosition(ownerMethod)], context);

                        return;
                    }

                    if (ownerMethodIdentifier.Equals(ownerMethod, context.EnclosingMethod, TypeInfo)) {
                        Identifier(gp.Name);

                        return;
                    }

                    if (
                        ownerMethodIdentifier.Equals(ownerMethod, context.DefiningMethod, TypeInfo) ||
                        ownerMethodIdentifier.Equals(ownerMethod, context.SignatureMethod, TypeInfo)
                    ) {
                        Value(String.Format("!!{0}", getPosition(ownerMethod)));

                        return;
                    }

                    throw new NotImplementedException(String.Format(
                        "Unimplemented form of generic method parameter: '{0}'.",
                        gp
                    ));
                }
            } else {
                throw new NotImplementedException("Cannot resolve generic parameter without a TypeReferenceContext.");
            }

            throw new NotImplementedException(String.Format(
                "Unimplemented form of generic parameter: '{0}'.",
                gp
            ));
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

        protected void TypeReferenceInternal (PointerType pt, TypeReferenceContext context) {
            WriteRaw("$jsilcore");
            Dot();
            WriteRaw("TypeRef");
            LPar();

            Value("JSIL.Pointer");
            Comma();
            OpenBracket(false);

            TypeReference(pt.ElementType, context);

            CloseBracket(false);
            RPar();
        }

        protected void TypeReferenceInternal (TypeReference tr, TypeReferenceContext context) {
            var type = TypeUtil.DereferenceType(tr);
            var typeDef = TypeUtil.GetTypeDefinition(type, false);
            var typeInfo = TypeInfo.Get(type);
            var fullName = (typeInfo != null) ? typeInfo.FullName
                    : (typeDef != null) ? typeDef.FullName
                        : type.FullName;

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
                if (TypeUtil.TypesAreEqual(type, context.EnclosingType, true)) {
                    // Types can reference themselves, so this prevents recursive initialization.
                    if (Stubbed && Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false)) {
                    } else {
                        if (context.EnclosingMethod != null) {
                            // $.Type is incorrect for generics because it will be the open form.
                            // FIXME: Will this work for static methods?
                            WriteRaw("this.__Type__");
                        } else {
                            WriteRaw("$.Type");
                        }
                        return;
                    }
                }

                // The interface builder provides helpful shorthand for corlib type references.
                if (type.Scope.Name == "mscorlib" || type.Scope.Name == "CommonLanguageRuntimeLibrary") {
                    if (CorlibTypes.Contains(type.FullName)) {
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
                TypeReference(TypeUtil.GetTypeDefinition(type), context);
                Space();
                Comment("AnyType[]");
                return;
            }

            var byref = type as ByReferenceType;
            var gp = type as GenericParameter;
            var at = type as ArrayType;
            var pt = type as PointerType;

            if (byref != null)
                TypeReferenceInternal(byref, context);
            else if (gp != null)
                TypeReferenceInternal(gp, context);
            else if (at != null)
                TypeReferenceInternal(at, context);
            else if (pt != null)
                TypeReferenceInternal(pt, context);
            else
                TypeReferenceInternal(type, context);
        }

        public void TypeReference (TypeInfo type, TypeReferenceContext context) {
            TypeReference(type.Definition, context);
        }

        public void MemberDescriptor (bool isPublic, bool isStatic, bool isVirtual = false, bool isReadonly = false) {
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

            if (isVirtual) {
                Comma();

                WriteRaw("Virtual");
                WriteRaw(":");
                WriteRaw("true ");
            }

            if (isReadonly) {
                Comma();

                WriteRaw("ReadOnly");
                WriteRaw(":");
                WriteRaw("true ");
            }

            WriteRaw("}");
        }

        public void Identifier (string name, EscapingMode escapingMode = EscapingMode.MemberIdentifier) {
            WriteRaw(Util.EscapeIdentifier(
                name, escapingMode
            ));
        }

        internal void Identifier (string name, TypeReferenceContext context, bool includeParens = false) {
            if (context == null)
                throw new ArgumentNullException("context");

            Identifier(name);
        }

        public void Identifier (ILVariable variable, TypeReferenceContext context, bool includeParens = false) {
            if (variable.Type.IsByReference) {
                throw new NotImplementedException("Old-style use of JavascriptFormatter.Identifier on a ref variable");
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

            if (TypeUtil.TypesAreEqual(context.EnclosingType, type, true)) {
                WriteRaw("$thisType");
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
                if ((info != null) && (info.Replacement != null)) {
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

                if (info != null) {
                    WriteRaw(Util.EscapeIdentifier(
                        info.FullName, EscapingMode.TypeIdentifier
                    ));
                } else {
                    // FIXME: Is this right?
                    WriteRaw(Util.EscapeIdentifier(
                        type.FullName, EscapingMode.TypeIdentifier
                    ));
                }
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
                Console.WriteLine("Method missing type information: {0}", method.FullName);
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

        public void Value (float value) {
            WriteRaw(value.ToString("R", CultureInfo.InvariantCulture));
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

            if (TypeUtil.IsIntegralOrEnum(typeReference)) {
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
                WriteRaw("var ${0} = new JSIL.AssemblyCollection", Configuration.AssemblyCollectionName ?? "asms");
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

        public void ConstructorSignature (MethodReference method, MethodSignature signature, TypeReferenceContext context) {
            Signature(method, signature, context, true, true);
        }

        public void MethodSignature (MethodReference method, MethodSignature signature, TypeReferenceContext context) {
            Signature(method, signature, context, false, true);
        }

        public void Signature (MethodReference method, MethodSignature signature, TypeReferenceContext context, bool forConstructor, bool allowCache) {
            // Reduce method signature heap usage
            if (
                !forConstructor &&
                (signature.GenericParameterCount == 0)
            ) {
                if (signature.ParameterCount == 0) {
                    if (
                        (signature.ReturnType == null) ||
                        (signature.ReturnType.FullName == "System.Void")
                    ){
                        WriteRaw("JSIL.MethodSignature.Void");
                        return;
                    } else if (!TypeUtil.IsOpenType(signature.ReturnType)) {
                        WriteRaw("JSIL.MethodSignature.Return");
                        LPar();
                        TypeReference(signature.ReturnType, context);
                        RPar();
                        return;
                    }
                } else if (
                    (
                        (signature.ReturnType == null) ||
                        (signature.ReturnType.FullName == "System.Void")
                    ) &&
                    (signature.ParameterCount == 1) &&
                    !TypeUtil.IsOpenType(signature.ParameterTypes[0])
                ) {
                    WriteRaw("JSIL.MethodSignature.Action");
                    LPar();
                    TypeReference(signature.ParameterTypes[0], context);
                    RPar();
                    return;
                }
            }

            if (forConstructor)
                WriteRaw("new JSIL.ConstructorSignature");
            else
                WriteRaw("new JSIL.MethodSignature");

            LPar();

            var oldSignature = context.SignatureMethod;
            context.SignatureMethod = method;

            try {
                if (forConstructor) {
                    TypeReference(method.DeclaringType, context);

                    Comma();
                } else {
                    if ((signature.ReturnType == null) || (signature.ReturnType.FullName == "System.Void"))
                        WriteRaw("null");
                    else {
                        if ((context.EnclosingMethod != null) && !TypeUtil.IsOpenType(signature.ReturnType))
                            TypeIdentifier(signature.ReturnType as dynamic, context, false);
                        else
                            TypeReference(signature.ReturnType, context);
                    }

                    Comma();
                }

                if (signature.ParameterCount > 0) {
                    OpenBracket(false);
                    CommaSeparatedListCore(
                        signature.ParameterTypes, (pt) => {
                            if ((context.EnclosingMethod != null) && !TypeUtil.IsOpenType(pt))
                                TypeIdentifier(pt as dynamic, context, false);
                            else
                                TypeReference(pt, context);
                        }
                    );
                    CloseBracket(false);
                } else {
                    WriteRaw("null");
                }

                if (!forConstructor && 
                    (signature.GenericParameterNames != null) &&
                    (signature.GenericParameterCount > 0)
                ) {
                    Comma();
                    OpenBracket(false);
                    CommaSeparatedList(signature.GenericParameterNames, context, ListValueType.Primitive);
                    CloseBracket(false);
                }
            } finally {
                context.SignatureMethod = oldSignature;
            }

            RPar();
        }
    }
}
