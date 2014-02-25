using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Ast {
    public class JSStringIdentifier : JSIdentifier {
        public readonly string Text;

        public JSStringIdentifier (string text, TypeReference type = null)
            : base(type) {
            Text = text;
        }

        public override string Identifier {
            get { return Text; }
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }
    }

    public class JSRawOutputIdentifier : JSIdentifier {
        public readonly string Format;
        public readonly object[] Arguments;

        public JSRawOutputIdentifier (TypeReference type, string format, params object[] arguments)
            : base(type) {

            if (type == null)
                throw new ArgumentNullException("type");

            Format = format;
            Arguments = arguments;
        }

        public override string Identifier {
            get {
                if ((Arguments != null) && (Arguments.Length > 0))
                    return String.Format(Format, Arguments);
                else
                    return Format;
            }
        }

        public void WriteTo (JavascriptFormatter output) {
            if ((Arguments != null) && (Arguments.Length > 0))
                output.WriteRaw(Format, Arguments);
            else
                output.WriteRaw(Format);
        }
    }

    public class JSNamespace : JSStringIdentifier {
        public JSNamespace (string name)
            : base(name) {
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }
    }

    public class JSAssembly : JSIdentifier {
        public readonly AssemblyDefinition Assembly;

        public JSAssembly (AssemblyDefinition assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            Assembly = assembly;
        }

        public override string Identifier {
            get { return Assembly.FullName; }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return typeSystem.Object;
        }

        public override JSLiteral ToLiteral () {
            return JSLiteral.New(Assembly);
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }
    }

    public class JSReflectionAssembly : JSAssembly {
        public JSReflectionAssembly (AssemblyDefinition assembly)
            : base(assembly) {
        }

        public override JSLiteral ToLiteral () {
            throw new InvalidOperationException();
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }
    }

    public class JSType : JSIdentifier {
        public readonly TypeReference Type;

        public JSType (TypeReference type) {
            if (type == null)
                throw new ArgumentNullException("type");

            Type = type;
        }

        public override string Identifier {
            get { return Type.FullName; }
        }

        public override bool HasGlobalStateDependency {
            get {
                return false;
            }
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return new TypeReference(
                typeSystem.Boolean.Namespace, "RuntimeTypeHandle",
                typeSystem.Boolean.Module, typeSystem.Boolean.Scope
            );
        }

        public override JSLiteral ToLiteral () {
            return JSLiteral.New(Type);
        }
    }

    public class JSTypeReference : JSType {
        public readonly TypeReference Context;

        public JSTypeReference (TypeReference type, TypeReference context)
            : base(type) {

            Context = context;
        }
    }

    public class JSCachedType : JSType {
        public readonly int Index;

        public JSCachedType (TypeReference type, int index)
            : base(type) {
            Index = index;
        }
    }

    public class JSField : JSIdentifier {
        public readonly FieldReference Reference;
        public readonly FieldInfo Field;

        public JSField (FieldReference reference, FieldInfo field) {
            if (reference == null)
                throw new ArgumentNullException("reference");
            if (field == null)
                throw new ArgumentNullException("field");

            Reference = reference;
            Field = field;
        }

        public override bool HasGlobalStateDependency {
            get {
                return Field.IsStatic;
            }
        }

        public override string Identifier {
            get { return Field.Name; }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Field.ReturnType;
        }

        // FIXME: Is this right?
        public override bool IsConstant {
            get {
                return true;
            }
        }
    }

    public class JSProperty : JSIdentifier {
        public readonly MemberReference Reference;
        public readonly PropertyInfo Property;

        public JSProperty (MemberReference reference, PropertyInfo property) {
            if (reference == null)
                throw new ArgumentNullException("reference");
            if (property == null)
                throw new ArgumentNullException("property");

            Reference = reference;
            Property = property;
        }

        public override bool HasGlobalStateDependency {
            get {
                return Property.IsStatic;
            }
        }

        public override string Identifier {
            get { return Property.Name; }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            if (Property.ReturnType.IsGenericParameter)
                return SubstituteTypeArgs(Property.Source, Property.ReturnType, Reference);
            else
                return Property.ReturnType;
        }

        // FIXME: Is this right?
        public override bool IsConstant {
            get {
                return true;
            }
        }
    }

    public class JSMethod : JSIdentifier {
        public readonly MethodTypeFactory MethodTypes;

        public readonly IEnumerable<TypeReference> GenericArguments;
        public readonly MethodReference Reference;
        public readonly MethodInfo Method;

        public JSMethod (
            MethodReference reference, MethodInfo method, MethodTypeFactory methodTypes,
            IEnumerable<TypeReference> genericArguments = null
        ) {
            if (reference == null)
                throw new ArgumentNullException("reference");
            if (method == null)
                throw new ArgumentNullException("method");

            Reference = reference;
            Method = method;
            MethodTypes = methodTypes;

            if (genericArguments == null) {
                var gim = Reference as GenericInstanceMethod;
                if (gim != null)
                    genericArguments = gim.GenericArguments;
            }

            GenericArguments = genericArguments;
        }

        public override string Identifier {
            get { return Method.Name; }
        }

        public QualifiedMemberIdentifier QualifiedIdentifier {
            get {
                return new QualifiedMemberIdentifier(
                    Method.DeclaringType.Identifier, Method.Identifier
                );
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return MethodTypes.Get(Reference, typeSystem);
        }

        public override string ToString () {
            return String.Format(
                "<JSMethod {0}::{1}>",
                Method.DeclaringType.Identifier,
                Method.Name
            );
        }

        public string GetNameForInstanceReference () {
            // FIXME: Enable this so MultipleGenericInterfaces2.cs passes.
            /*
            // For methods that implement a method of a closed generic interface, we need to ensure we fully-qualify their name when necessary.
            var declaringGit = Reference.DeclaringType as GenericInstanceType;
            if ((declaringGit != null) && (declaringGit.ElementType.Resolve().IsInterface)) {
                var parentTypeName = TypeUtil.GetLocalName(declaringGit.ElementType.Resolve());
                parentTypeName = parentTypeName.Substring(0, parentTypeName.IndexOf("`"));

                var genericArgsText = String.Join(",", from ga in declaringGit.GenericArguments select ga.FullName);

                return String.Format("{0}<{1}>.{2}", parentTypeName, genericArgsText, Reference.Name);
            } else
             */

            return Method.GetName(false);
        }

        public JSCachedType[] CachedGenericArguments {
            get;
            private set;
        }

        internal void SetCachedGenericArguments (IEnumerable<JSCachedType> cachedTypes) {
            var newCachedGenericArguments = cachedTypes.ToArray();

            if (CachedGenericArguments != null) {
                if (!CachedGenericArguments.SequenceEqual(newCachedGenericArguments))
                    throw new InvalidOperationException("Cached generic arguments already set and new set of cached types is different");
            }

            CachedGenericArguments = newCachedGenericArguments;
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }
    }

    [JSAstIgnoreInheritedMembers]
    public class JSFakeMethod : JSIdentifier {
        public readonly MethodTypeFactory MethodTypes;

        public readonly string Name;
        public readonly TypeReference ReturnType;
        public readonly TypeReference[] ParameterTypes;
        [JSAstTraverse(0)]
        public readonly JSExpression[] GenericArguments;

        public JSFakeMethod (
            string name, TypeReference returnType,
            TypeReference[] parameterTypes, MethodTypeFactory methodTypes,
            JSExpression[] genericArguments = null
        ) {
            Name = name;
            ReturnType = returnType;
            ParameterTypes = parameterTypes ?? new TypeReference[0];
            MethodTypes = methodTypes;
            GenericArguments = genericArguments;
        }

        public override string Identifier {
            get { return Name; }
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (GenericArguments == null)
                return;

            var oldExpression = oldChild as JSExpression;
            var newExpression = newChild as JSExpression;

            if (
                (oldExpression != null) &&
                ((newExpression != null) == (newChild != null))
            ) {
                for (var i = 0; i < GenericArguments.Length; i++) {
                    if (GenericArguments[i] == oldExpression)
                        GenericArguments[i] = newExpression;
                }
            }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return MethodTypes.Get(
                ReturnType, ParameterTypes, typeSystem
            );
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }
    }

    [JSAstIgnoreInheritedMembers]
    public class JSVariable : JSIdentifier {
        public readonly MethodReference Function;

        public readonly string Name;
        protected readonly bool _IsReference;

        [JSAstTraverse(0)]
        public JSExpression DefaultValue;

        public JSVariable (string name, TypeReference type, MethodReference function, JSExpression defaultValue = null)
            : base(type) {
            Name = name;

            if (type == null)
                throw new ArgumentNullException("type");

            if (type is ByReferenceType) {
                type = ((ByReferenceType)type).ElementType;
                _IsReference = true;
            } else {
                _IsReference = false;
            }

            Function = function;

            DefaultValue = defaultValue ?? new JSDefaultValueLiteral(type);
        }

        public override string Identifier {
            get { return Name; }
        }

        public virtual bool IsReference {
            get {
                return _IsReference;
            }
        }

        public virtual bool IsParameter {
            get {
                return false;
            }
        }

        public virtual JSParameter GetParameter () {
            throw new InvalidOperationException("Variable is not a parameter");
        }

        public virtual bool IsThis {
            get {
                return false;
            }
        }

        public static JSVariable New (string name, TypeReference type, MethodReference function) {
            return new JSVariable(name, type, function);
        }

        public static JSVariable New (ParameterReference parameter, MethodReference function) {
            return new JSParameter(parameter.Name, parameter.ParameterType, function);
        }

        public virtual JSVariable Reference () {
            return new JSVariableReference(this, Function);
        }

        public virtual JSVariable Dereference () {
            if (IsReference)
                return new JSVariableDereference(this, Function);
            else
                throw new InvalidOperationException("Dereferencing a non-reference");
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return IdentifierType;
        }

        public override bool IsConstant {
            get {
                return false;
            }
        }

        public override int GetHashCode () {
            return Identifier.GetHashCode();
        }

        public override string ToString () {
            var defaultValueText = "";

            if (!DefaultValue.IsNull && !(DefaultValue is JSDefaultValueLiteral))
                defaultValueText = String.Format(" := {0}", DefaultValue);

            if (IsReference)
                return String.Format("<{0} {1}{2}>", IdentifierType, Identifier, defaultValueText);
            else if (IsThis)
                return String.Format("<this {0}>", IdentifierType);
            else if (IsParameter)
                return String.Format("<parameter {0} {1}>", IdentifierType, Identifier);
            else
                return String.Format("<var {0} {1}{2}>", IdentifierType, Identifier, defaultValueText);
        }

        public override bool Equals (object obj) {
            var rhs = obj as JSVariable;
            if (rhs != null) {
                if (rhs.Identifier != Identifier)
                    return false;
                if (rhs.IsParameter != IsParameter)
                    return false;
                else if (rhs.IsReference != IsReference)
                    return false;
                else if (rhs.IsThis != IsThis)
                    return false;
                else if (!TypeUtil.TypesAreEqual(IdentifierType, rhs.IdentifierType))
                    return false;
                else
                    return true;
            }

            return EqualsImpl(obj, true);
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (newChild == this)
                throw new InvalidOperationException("Direct cycle formed by replacement");

            if (DefaultValue == oldChild)
                DefaultValue = (JSExpression)newChild;
        }
    }

    [JSAstIgnoreInheritedMembers]
    public class JSClosureVariable : JSVariable {
        internal JSClosureVariable (string name, TypeReference type, MethodReference function, JSExpression defaultValue = null) 
            : base (name, type, function, defaultValue) {
        }

        public static JSClosureVariable New (ILVariable variable, MethodReference function) {
            return new JSClosureVariable(variable.Name, variable.Type, function);
        }
    }

    [JSAstIgnoreInheritedMembers]
    public class JSParameter : JSVariable {
        internal JSParameter (string name, TypeReference type, MethodReference function, bool escapeName = true)
            : base(MaybeEscapeName(name, escapeName), type, function) {
        }

        public static string MaybeEscapeName (string name, bool actuallyEscape) {
            if (!actuallyEscape)
                return name;

            if (name == "this")
                return "@this";

            return name;
        }

        public override bool IsParameter {
            get {
                return true;
            }
        }

        public override JSParameter GetParameter () {
            return this;
        }
    }

    public class JSExceptionVariable : JSVariable {
        public JSExceptionVariable (TypeSystem typeSystem, MethodReference function) :
            base(
                "$exception",
                new TypeReference("System", "Exception", typeSystem.Object.Module, typeSystem.Object.Scope),
                function
            ) {
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }

        public override bool IsParameter {
            get {
                return true;
            }
        }
    }

    [JSAstIgnoreInheritedMembers]
    public class JSThisParameter : JSParameter {
        public JSThisParameter (TypeReference type, MethodReference function) :
            base("this", type, function, false) 
        {
        }

        public override bool IsThis {
            get {
                return true;
            }
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }

        public static JSVariable New (TypeReference type, MethodReference function) {
            if (type.IsValueType)
                return new JSVariableReference(new JSThisParameter(type, function), function);
            else
                return new JSThisParameter(type, function);
        }
    }

    public class JSVariableDereference : JSVariable {
        public readonly JSVariable Referent;

        public JSVariableDereference (JSVariable referent, MethodReference function)
            : base(referent.Identifier, DeReferenceType(referent.IdentifierType), function) {

            Referent = referent;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return DeReferenceType(Referent.GetActualType(typeSystem), true);
        }

        public override TypeReference IdentifierType {
            get {
                return DeReferenceType(Referent.IdentifierType, true);
            }
        }

        public override bool IsReference {
            get {
                return DeReferenceType(Referent.IdentifierType, true) is ByReferenceType;
            }
        }

        public override bool IsParameter {
            get {
                return Referent.IsParameter;
            }
        }

        public override JSParameter GetParameter () {
            return Referent.GetParameter();
        }

        public override bool IsThis {
            get {
                return Referent.IsThis;
            }
        }

        public override JSVariable Reference () {
            return Referent;
        }
    }

    [JSAstIgnoreInheritedMembers]
    public class JSIndirectVariable : JSVariable {
        public readonly IDictionary<string, JSVariable> Variables;

        public JSIndirectVariable (IDictionary<string, JSVariable> variables, string identifier, MethodReference function)
            : base(identifier, variables[identifier].IdentifierType, function) {

            Variables = variables;
        }

        private void CheckForMissingVariable () {
            if (!Variables.ContainsKey(Identifier))
                throw new KeyNotFoundException(String.Format("No variable named '{0}' currently exists.", Identifier));
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            CheckForMissingVariable();

            return Variables[Identifier].GetActualType(typeSystem);
        }

        public override TypeReference IdentifierType {
            get {
                CheckForMissingVariable();

                return Variables[Identifier].IdentifierType;
            }
        }

        public override bool IsReference {
            get {
                CheckForMissingVariable();

                return Variables[Identifier].IsReference;
            }
        }

        public override bool IsParameter {
            get {
                CheckForMissingVariable();

                return Variables[Identifier].IsParameter;
            }
        }

        public override JSParameter GetParameter () {
            CheckForMissingVariable();

            return Variables[Identifier].GetParameter();
        }

        public override bool IsConstant {
            get {
                CheckForMissingVariable();

                return Variables[Identifier].IsConstant;
            }
        }

        public override bool IsThis {
            get {
                CheckForMissingVariable();

                return Variables[Identifier].IsThis;
            }
        }

        public override JSVariable Dereference () {
            CheckForMissingVariable();

            return Variables[Identifier].Dereference();
        }

        public override JSVariable Reference () {
            CheckForMissingVariable();

            return Variables[Identifier].Reference();
        }

        public JSVariable ActualVariable {
            get {
                CheckForMissingVariable();

                return Variables[Identifier];
            }
        }

        public override bool Equals (object obj) {
            JSVariable variable;
            if (Variables.TryGetValue(Identifier, out variable))
                return variable.Equals(obj) || base.Equals(obj);
            else {
                variable = obj as JSVariable;
                if ((variable != null) && (variable.Identifier == Identifier))
                    return true;
                else
                    return base.Equals(obj);
            }
        }

        public override string ToString () {
            JSVariable variable;
            if (Variables.TryGetValue(Identifier, out variable))
                return String.Format("@{0}", variable);
            else
                return String.Format("@missing({0})", Identifier);
        }
    }

    public class JSVariableReference : JSVariable {
        public readonly JSVariable Referent;

        public JSVariableReference (JSVariable referent, MethodReference function)
            : base(referent.Identifier, new ByReferenceType(referent.IdentifierType), function) {

            Referent = referent;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return new ByReferenceType(Referent.GetActualType(typeSystem));
        }

        public override TypeReference IdentifierType {
            get {
                return new ByReferenceType(Referent.IdentifierType);
            }
        }

        public override bool IsReference {
            get {
                return true;
            }
        }

        public override JSParameter GetParameter () {
            return Referent.GetParameter();
        }

        public override bool IsParameter {
            get {
                return Referent.IsParameter;
            }
        }

        public override bool IsThis {
            get {
                return Referent.IsThis;
            }
        }

        public override JSVariable Dereference () {
            return Referent;
        }
    }
}
