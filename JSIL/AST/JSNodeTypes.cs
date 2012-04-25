using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Internal;
using JSIL.Transforms;
using Mono.Cecil;

namespace JSIL.Ast {
    public abstract class JSNode {
        /// <summary>
        /// Enumerates the children of this node.
        /// </summary>
        public virtual IEnumerable<JSNode> Children {
            get {
                yield break;
            }
        }

        public IEnumerable<JSNode> SelfAndChildrenRecursive {
            get {
                yield return this;

                foreach (var ch in AllChildrenRecursive)
                    yield return ch;
            }
        }

        public IEnumerable<JSNode> AllChildrenRecursive {
            get {
                foreach (var child in Children) {
                    if (child == null)
                        continue;

                    yield return child;

                    foreach (var subchild in child.AllChildrenRecursive) {
                        if (subchild == null)
                            continue;

                        yield return subchild;
                    }
                }
            }
        }

        /// <summary>
        /// If true, the node should be treated as a null node without any actual impact on the output javascript.
        /// </summary>
        public virtual bool IsNull {
            get {
                return false;
            }
        }

        public abstract void ReplaceChild (JSNode oldChild, JSNode newChild);

        public virtual void ReplaceChildRecursive (JSNode oldChild, JSNode newChild) {
            ReplaceChild(oldChild, newChild);

            foreach (var child in Children) {
                if ((child != null) && (child != newChild))
                    child.ReplaceChildRecursive(oldChild, newChild);
            }
        }
    }

    public abstract class JSExpression : JSNode {
        public static readonly JSNullExpression Null = new JSNullExpression();

        protected readonly IList<JSExpression> Values;

        protected JSExpression (params JSExpression[] values) {
            Values = values;
        }

        public override IEnumerable<JSNode> Children {
            get {
                // We don't want to use foreach here, since a value could be changed during iteration
                for (int i = 0, c = Values.Count; i < c; i++)
                    yield return Values[i];
            }
        }

        public override string ToString () {
            return String.Format(
                "{0}[{1}]", GetType().Name,
                String.Join(", ", (from v in Values select String.Concat(v)).ToArray())
            );
        }

        public virtual TypeReference GetActualType (TypeSystem typeSystem) {
            throw new NoExpectedTypeException(this);
        }

        public static TypeReference DeReferenceType (TypeReference type, bool once = false) {
            var brt = type as ByReferenceType;

            while (brt != null) {
                type = brt.ElementType;
                brt = type as ByReferenceType;

                if (once)
                    break;
            }

            return type;
        }

        public static TypeReference SubstituteTypeArgs (ITypeInfoSource typeInfo, TypeReference type, MemberReference member) {
            var gp = (type as GenericParameter);

            if (gp != null) {
                if (gp.Owner.GenericParameterType == GenericParameterType.Method) {
                    var ownerIdentifier = new MemberIdentifier(typeInfo, gp.Owner as MethodReference);
                    var memberIdentifier = new MemberIdentifier(typeInfo, member as dynamic);

                    if (!ownerIdentifier.Equals(memberIdentifier, typeInfo))
                        return type;

                    if (!(member is GenericInstanceMethod))
                        return type;
                } else {
                    var declaringType = member.DeclaringType;
                    var ownerIdentifier = new TypeIdentifier(gp.Owner as TypeReference);
                    var typeIdentifier = new TypeIdentifier(declaringType);

                    if (!ownerIdentifier.Equals(typeIdentifier))
                        return type;
                }
            }

            return TypeAnalysis.SubstituteTypeArgs(type, member);
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            if (newChild == this)
                throw new InvalidOperationException("Infinite recursion");

            if ((newChild != null) && !(newChild is JSExpression))
                return;

            var expr = (JSExpression)newChild;

            for (int i = 0, c = Values.Count; i < c; i++) {
                if (Values[i] == oldChild)
                    Values[i] = expr;
            }
        }

        protected bool EqualsImpl (object obj, bool fieldsChecked) {
            if (this == obj)
                return true;
            else if (obj == null)
                return false;
            else if (obj.GetType() != GetType())
                return false;

            var rhs = (JSExpression)obj;
            if (Values.Count != rhs.Values.Count)
                return false;

            if ((Values.Count == 0) && (!fieldsChecked))
                throw new NotImplementedException(String.Format("Expressions of type {0} cannot be compared", GetType().Name));

            for (int i = 0, c = Values.Count; i < c; i++) {
                var lhsV = Values[i];
                var rhsV = rhs.Values[i];

                if (!lhsV.Equals(rhsV))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// If true, this expression has at least one dependency on static (non-local) state.
        /// </summary>
        public virtual bool HasGlobalStateDependency {
            get {
                return Values.Any((v) => v.HasGlobalStateDependency);
            }
        }

        /// <summary>
        /// If true, this expression is constant and has no dependencies on local or global state.
        /// </summary>
        public virtual bool IsConstant {
            get {
                return false;
            }
        }

        public override bool Equals (object obj) {
            return EqualsImpl(obj, false);
        }

        public override int GetHashCode () {
            return 0; // :-(
        }
    }

    public abstract class JSIdentifier : JSExpression {
        protected readonly TypeReference _Type;

        public JSIdentifier (TypeReference type = null) {
            _Type = type;
        }

        public override bool Equals (object obj) {
            var id = obj as JSIdentifier;
            var str = obj as string;

            if (id != null) {
                return String.Equals(Identifier, id.Identifier) &&
                    TypeUtil.TypesAreEqual(Type, id.Type) &&
                    EqualsImpl(obj, true);
            } else {
                return EqualsImpl(obj, true);
            }
        }

        public virtual TypeReference Type {
            get {
                return _Type;
            }
        }

        public override int GetHashCode () {
            return Identifier.GetHashCode();
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            if (_Type != null)
                return _Type;
            else
                return base.GetActualType(typeSystem);
        }

        public abstract string Identifier {
            get;
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }

        public override string ToString () {
            return String.Format("<{0} '{1}'>", GetType().Name, Identifier);
        }

        public virtual JSLiteral ToLiteral () {
            return JSLiteral.New(Util.EscapeIdentifier(Identifier));
        }
    }

    public abstract class JSLiteral : JSExpression {
        internal JSLiteral (params JSExpression[] values)
            : base(values) {
        }

        public abstract object Literal {
            get;
        }

        public static JSAssemblyNameLiteral New (AssemblyDefinition value) {
            return new JSAssemblyNameLiteral(value);
        }

        public static JSTypeNameLiteral New (TypeReference value) {
            return new JSTypeNameLiteral(value);
        }

        public static JSStringLiteral New (string value) {
            return new JSStringLiteral(value);
        }

        public static JSBooleanLiteral New (bool value) {
            return new JSBooleanLiteral(value);
        }

        public static JSCharLiteral New (char value) {
            return new JSCharLiteral(value);
        }

        public static JSIntegerLiteral New (sbyte value) {
            return new JSIntegerLiteral(value, typeof(sbyte));
        }

        public static JSIntegerLiteral New (byte value) {
            return new JSIntegerLiteral(value, typeof(byte));
        }

        public static JSIntegerLiteral New (short value) {
            return new JSIntegerLiteral(value, typeof(short));
        }

        public static JSIntegerLiteral New (ushort value) {
            return new JSIntegerLiteral(value, typeof(ushort));
        }

        public static JSIntegerLiteral New (int value) {
            return new JSIntegerLiteral(value, typeof(int));
        }

        public static JSIntegerLiteral New (uint value) {
            return new JSIntegerLiteral(value, typeof(uint));
        }

        public static JSIntegerLiteral New (long value) {
            return new JSIntegerLiteral(value, typeof(long));
        }

        public static JSIntegerLiteral New (ulong value) {
            return new JSIntegerLiteral((long)value, typeof(ulong));
        }

        public static JSNumberLiteral New (float value) {
            return new JSNumberLiteral(value, typeof(float));
        }

        public static JSNumberLiteral New (double value) {
            return new JSNumberLiteral(value, typeof(double));
        }

        public static JSNumberLiteral New (decimal value) {
            return new JSNumberLiteral((double)value, typeof(decimal));
        }

        public static JSDefaultValueLiteral DefaultValue (TypeReference type) {
            return new JSDefaultValueLiteral(type);
        }

        new public static JSNullLiteral Null (TypeReference type) {
            return new JSNullLiteral(type);
        }
    }

    public abstract class JSLiteralBase<T> : JSLiteral {
        public readonly T Value;

        protected JSLiteralBase (T value) {
            Value = value;
        }

        public override object Literal {
            get {
                return this.Value;
            }
        }

        public override bool Equals (object obj) {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            var rhs = (JSLiteralBase<T>)obj;
            var comparer = Comparer<T>.Default;

            return comparer.Compare(Value, rhs.Value) == 0;
        }

        public override bool IsConstant {
            get {
                return true;
            }
        }

        public override string ToString () {
            return String.Format("<{0} {1}>", GetType().Name, Value);
        }
    }

    public abstract class JSStatement : JSNode {
        public static readonly JSNullStatement Null = new JSNullStatement();

        public string Label = null;

        protected virtual string PrependLabel (string text) {
            if (Label == null)
                return text;

            return String.Format("{0}: {1}", Label, text);
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            if (oldChild == null)
                throw new ArgumentNullException("oldChild");

            throw new NotImplementedException(
                String.Format("Statements of type '{0}' do not support child replacement", GetType().Name)
            );
        }
    }

    public class NoExpectedTypeException : NotImplementedException {
        public NoExpectedTypeException (JSExpression node)
            : base(String.Format("Node of type {0} has no expected type: {1}", node.GetType().Name, node)) {
        }
    }

    public struct AnnotatedNode {
        public readonly string Name;
        public readonly JSNode Node;

        public AnnotatedNode (string name, JSNode node) {
            Name = name;
            Node = node;
        }
    }

    public interface IAnnotatedChildren {
        IEnumerable<AnnotatedNode> AnnotatedChildren {
            get;
        }
    }

    public abstract class JSAnnotatedStatement : JSStatement, IAnnotatedChildren {
        public override IEnumerable<JSNode> Children {
            get {
                foreach (var child in AnnotatedChildren)
                    yield return child.Node;
            }
        }

        public virtual IEnumerable<AnnotatedNode> AnnotatedChildren {
            get {
                foreach (var child in base.Children)
                    yield return new AnnotatedNode(null, child);
            }
        }
    }

    public abstract class JSAnnotatedExpression : JSExpression, IAnnotatedChildren {
        protected JSAnnotatedExpression (params JSExpression[] values)
            : base (values) {
        }

        public override IEnumerable<JSNode> Children {
            get {
                foreach (var child in AnnotatedChildren)
                    yield return child.Node;
            }
        }

        public virtual IEnumerable<AnnotatedNode> AnnotatedChildren {
            get {
                foreach (var child in base.Children)
                    yield return new AnnotatedNode(null, child);
            }
        }
    }
}
