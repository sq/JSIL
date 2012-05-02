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
    }

    public class JSRawOutputIdentifier : JSIdentifier {
        public readonly Action<JavascriptFormatter> Emitter;

        public JSRawOutputIdentifier (Action<JavascriptFormatter> emitter, TypeReference type = null)
            : base(type) {
            Emitter = emitter;
        }

        public override string Identifier {
            get { return Emitter.ToString(); }
        }
    }

    public class JSNamespace : JSStringIdentifier {
        public JSNamespace (string name)
            : base(name) {
        }
    }

    public class JSAssembly : JSIdentifier {
        new public readonly AssemblyDefinition Assembly;

        public JSAssembly (AssemblyDefinition assembly) {
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
    }

    public class JSType : JSIdentifier {
        new public readonly TypeReference Type;

        public JSType (TypeReference type) {
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

        public override bool IsConstant {
            get {
                return false;
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
                return Method.GetName(true);
        }
    }

    public class JSFakeMethod : JSIdentifier {
        public readonly MethodTypeFactory MethodTypes;

        public readonly string Name;
        public readonly TypeReference ReturnType;
        public readonly TypeReference[] ParameterTypes;
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

        public override IEnumerable<JSNode> Children {
            get {
                if (GenericArguments == null)
                    yield break;

                for (var i = 0; i < GenericArguments.Length; i++)
                    yield return GenericArguments[i];
            }
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
    }

    public class JSVariable : JSIdentifier {
        public readonly MethodReference Function;

        public readonly string Name;
        protected readonly bool _IsReference;

        public JSExpression DefaultValue;

        public JSVariable (string name, TypeReference type, MethodReference function, JSExpression defaultValue = null)
            : base(type) {
            Name = name;

            if (type is ByReferenceType) {
                type = ((ByReferenceType)type).ElementType;
                _IsReference = true;
            } else {
                _IsReference = false;
            }

            Function = function;

            if (defaultValue != null)
                DefaultValue = defaultValue;
            else
                DefaultValue = new JSDefaultValueLiteral(type);
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

        public static JSVariable New (ILVariable variable, MethodReference function) {
            return new JSVariable(variable.Name, variable.Type, function);
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
            return Type;
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
                defaultValueText = String.Format(" = {0}", DefaultValue.ToString());

            if (IsReference)
                return String.Format("<ref {0} {1}{2}>", Type, Identifier, defaultValueText);
            else if (IsThis)
                return String.Format("<this {0}>", Type);
            else if (IsParameter)
                return String.Format("<parameter {0} {1}>", Type, Identifier);
            else
                return String.Format("<var {0} {1}{2}>", Type, Identifier, defaultValueText);
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
                else if (!TypeUtil.TypesAreEqual(Type, rhs.Type))
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

        public override IEnumerable<JSNode> Children {
            get {
                yield return DefaultValue;
            }
        }
    }

    public class JSParameter : JSVariable {
        internal JSParameter (string name, TypeReference type, MethodReference function)
            : base(name, type, function) {
        }

        public override bool IsParameter {
            get {
                return true;
            }
        }

        public override IEnumerable<JSNode> Children {
            get {
                yield break;
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

    public class JSThisParameter : JSParameter {
        public JSThisParameter (TypeReference type, MethodReference function) :
            base("this", type, function) {
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

        public override IEnumerable<JSNode> Children {
            get {
                yield break;
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
            : base(referent.Identifier, null, function) {

            Referent = referent;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return DeReferenceType(Referent.GetActualType(typeSystem), true);
        }

        public override TypeReference Type {
            get {
                return DeReferenceType(Referent.Type, true);
            }
        }

        public override bool IsReference {
            get {
                return DeReferenceType(Referent.Type, true) is ByReferenceType;
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

    public class JSIndirectVariable : JSVariable {
        public readonly IDictionary<string, JSVariable> Variables;

        public JSIndirectVariable (IDictionary<string, JSVariable> variables, string identifier, MethodReference function)
            : base(identifier, null, function) {

            Variables = variables;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Variables[Identifier].GetActualType(typeSystem);
        }

        public override TypeReference Type {
            get {
                return Variables[Identifier].Type;
            }
        }

        public override bool IsReference {
            get {
                return Variables[Identifier].IsReference;
            }
        }

        public override bool IsParameter {
            get {
                return Variables[Identifier].IsParameter;
            }
        }

        public override JSParameter GetParameter () {
            return Variables[Identifier].GetParameter();
        }

        public override bool IsConstant {
            get {
                return Variables[Identifier].IsConstant;
            }
        }

        public override bool IsThis {
            get {
                return Variables[Identifier].IsThis;
            }
        }

        public override JSVariable Dereference () {
            return Variables[Identifier].Dereference();
        }

        public override JSVariable Reference () {
            return Variables[Identifier].Reference();
        }

        public override IEnumerable<JSNode> Children {
            get {
                yield break;
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
                return "@undef";
        }
    }

    public class JSVariableReference : JSVariable {
        public readonly JSVariable Referent;

        public JSVariableReference (JSVariable referent, MethodReference function)
            : base(referent.Identifier, null, function) {

            Referent = referent;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return new ByReferenceType(Referent.GetActualType(typeSystem));
        }

        public override TypeReference Type {
            get {
                return new ByReferenceType(Referent.Type);
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
