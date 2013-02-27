using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using JSIL.Internal;
using System.Linq.Expressions;

namespace JSIL.Ast.Traversal {
    public class JSNodeTraversalData {
        private static readonly ConcurrentCache<Type, JSNodeTraversalData> Cache = new ConcurrentCache<Type, JSNodeTraversalData>();

        public readonly Type Type;
        public readonly JSNodeTraversalRecord[] Records;

        public static JSNodeTraversalData Get (JSNode node) {
            return Get(node.GetType());
        }

        public static JSNodeTraversalData Get (Type nodeType) {
            return Cache.GetOrCreate(
                nodeType, (_) => new JSNodeTraversalData(nodeType)
            );
        }

        private JSNodeTraversalData (Type nodeType) {
            var tCompilerGenerated = typeof(CompilerGeneratedAttribute);
            var tIgnore = typeof(JSAstIgnoreAttribute);
            var tTraverse = typeof(JSAstTraverseAttribute);
            var tIgnoreInherited = typeof(JSAstIgnoreInheritedMembersAttribute);
            var records = new List<JSNodeTraversalRecord>();

            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            var typeToScan = nodeType;
            var seenMembers = new HashSet<string>();

            while (typeToScan != null) {
                foreach (var field in typeToScan.GetFields(flags)) {
                    if (seenMembers.Contains(field.Name))
                        continue;
                    seenMembers.Add(field.Name);

                    var traverseAttribute = field.GetCustomAttributes(tTraverse, true).OfType<JSAstTraverseAttribute>().FirstOrDefault();
                    if (traverseAttribute == null) {
                        if (field.GetCustomAttributes(tCompilerGenerated, true).Length > 0)
                            continue;
                        if (field.GetCustomAttributes(tIgnore, true).Length > 0)
                            continue;
                    }

                    var record = MakeFieldRecord(field);
                    if (record == null)
                        continue;

                    if (traverseAttribute != null) {
                        record.SortKey = traverseAttribute.TraversalIndex;
                        record.Name = traverseAttribute.Name ?? field.Name;
                    }

                    record.OriginalIndex = records.Count;

                    records.Add(record);
                }

                foreach (var method in typeToScan.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)) {
                    if (seenMembers.Contains(method.Name))
                        continue;
                    seenMembers.Add(method.Name);

                    var traverseAttribute = method.GetCustomAttributes(tTraverse, true).OfType<JSAstTraverseAttribute>().FirstOrDefault();
                    if (traverseAttribute == null)
                        continue;

                    var newRecord = new JSNodeTraversalMethodRecord(method);
                    
                    newRecord.SortKey = traverseAttribute.TraversalIndex;
                    newRecord.Name = traverseAttribute.Name ?? newRecord.Name;
                    newRecord.OriginalIndex = records.Count;
                    records.Add(newRecord);
                }

                if (typeToScan.GetCustomAttributes(tIgnoreInherited, false).Length > 0)
                    typeToScan = null;
                else if (typeToScan != typeToScan.BaseType)
                    typeToScan = typeToScan.BaseType;
                else
                    typeToScan = null;
            }

            Type = nodeType;
            records.Sort((lhs, rhs) => {
                var result = lhs.SortKey.CompareTo(rhs.SortKey);
                if (result == 0)
                    result = lhs.OriginalIndex.CompareTo(rhs.OriginalIndex);
                return result;
            });
            Records = records.ToArray();
        }

        private JSNodeTraversalRecord MakeFieldRecord (System.Reflection.FieldInfo field) {
            var tJSNode = typeof(JSNode);
            var tList = typeof(List<>);
            var tEnumerable = typeof(IEnumerable<>);
            var tGetFieldFunc = typeof(Func<,>).MakeGenericType(tJSNode, field.FieldType);

            Func<Delegate> makeGetter = () => {
                var parentParameter = Expression.Parameter(tJSNode, "parent");
                var parentCast = Expression.Convert(parentParameter, field.DeclaringType);
                var member = Expression.MakeMemberAccess(parentCast, field);
                var expr = Expression.Lambda(tGetFieldFunc, member, false, parentParameter);
                return expr.Compile();
            };

            Type fieldGenericDefinition = null;
            if (field.FieldType.IsGenericType)
                fieldGenericDefinition = field.FieldType.GetGenericTypeDefinition();

            if (tJSNode.IsAssignableFrom(field.FieldType)) {
                return (JSNodeTraversalRecord)Activator.CreateInstance(
                    typeof(JSNodeTraversalFieldRecord<>).MakeGenericType(field.FieldType),
                    makeGetter()
                );
            } else if (field.FieldType.IsArray) {
                var arrayElementType = field.FieldType.GetElementType();

                if (!tJSNode.IsAssignableFrom(arrayElementType))
                    return null;

                return (JSNodeTraversalRecord)Activator.CreateInstance(
                    typeof(JSNodeTraversalArrayFieldRecord<>).MakeGenericType(arrayElementType),
                    makeGetter()
                );
            } else if (tList == fieldGenericDefinition) {
                var listElementType = field.FieldType.GetGenericArguments()[0];

                if (!tJSNode.IsAssignableFrom(listElementType))
                    return null;

                return (JSNodeTraversalRecord)Activator.CreateInstance(
                    typeof(JSNodeTraversalListFieldRecord<>).MakeGenericType(listElementType),
                    makeGetter()
                );
            } else if (tEnumerable == fieldGenericDefinition) {
                var enumerableElementType = field.FieldType.GetGenericArguments()[0];

                if (!tJSNode.IsAssignableFrom(enumerableElementType))
                    return null;

                return (JSNodeTraversalRecord)Activator.CreateInstance(
                    typeof(JSNodeTraversalEnumerableFieldRecord<>).MakeGenericType(enumerableElementType),
                    makeGetter()
                );
            }

            return null;
        }
    }

    public enum JSNodeTraversalRecordType {
        Element,
        Array
    }

    public abstract class JSNodeTraversalRecord {
        public string Name;
        public int SortKey, OriginalIndex;
        public readonly JSNodeTraversalRecordType Type;

        protected JSNodeTraversalRecord (JSNodeTraversalRecordType type) {
            Type = type;
        }

        public override string ToString () {
            return String.Format("{0} {1}", Type, Name);
        }
    }

    public abstract class JSNodeTraversalElementRecord : JSNodeTraversalRecord {
        protected JSNodeTraversalElementRecord ()
            : base(JSNodeTraversalRecordType.Element) {
        }

        public abstract void Get (JSNode parent, out JSNode node, out string name);
    }

    public abstract class JSNodeTraversalArrayRecord : JSNodeTraversalRecord {
        protected JSNodeTraversalArrayRecord ()
            : base(JSNodeTraversalRecordType.Array) {
        }

        public abstract bool GetElement (JSNode parent, int index, out JSNode node, out string name);
    }

    public class JSNodeTraversalFieldRecord<T> : JSNodeTraversalElementRecord 
        where T : JSNode
    {
        public readonly Func<JSNode, T> GetField;

        public JSNodeTraversalFieldRecord (Func<JSNode, T> getField) {
            GetField = getField;
        }

        public override void Get (JSNode parent, out JSNode node, out string name) {
            node = GetField(parent);
            name = Name;
        }
    }

    public class JSNodeTraversalArrayFieldRecord<T> : JSNodeTraversalArrayRecord
        where T : JSNode
    {
        public readonly Func<JSNode, T[]> GetField;

        public JSNodeTraversalArrayFieldRecord (Func<JSNode, T[]> getField) {
            GetField = getField;
        }

        public override bool GetElement (JSNode parent, int index, out JSNode node, out string name) {
            var array = GetField(parent);
            if ((array == null) || (index >= array.Length)) {
                node = null;
                name = null;
                return false;
            } else {
                node = array[index];
                name = Name;
                return true;
            }
        }
    }

    public class JSNodeTraversalListFieldRecord<T> : JSNodeTraversalArrayRecord
        where T : JSNode
    {
        public readonly Func<JSNode, List<T>> GetField;

        public JSNodeTraversalListFieldRecord (Func<JSNode, List<T>> getField) {
            GetField = getField;
        }

        public override bool GetElement (JSNode parent, int index, out JSNode node, out string name) {
            var list = GetField(parent);
            if ((list == null) || (index >= list.Count)) {
                node = null;
                name = null;
                return false;
            } else {
                node = list[index];
                name = Name;
                return true;
            }
        }
    }

    public class JSNodeTraversalEnumerableFieldRecord<T> : JSNodeTraversalArrayRecord
        where T : JSNode {
        public readonly Func<JSNode, IEnumerable<T>> GetField;

        public JSNodeTraversalEnumerableFieldRecord (Func<JSNode, IEnumerable<T>> getField) {
            GetField = getField;
        }

        public override bool GetElement (JSNode parent, int index, out JSNode node, out string name) {
            node = null;
            name = null;

            var enumerable = GetField(parent);
            if (enumerable != null) {
                using (var e = enumerable.GetEnumerator()) {
                    while (index > 0) {
                        if (!e.MoveNext())
                            return false;

                        index -= 1;
                    }
                    
                    if (e.MoveNext()) {
                        node = e.Current;
                        name = Name;
                        return true;
                    }
                }
            }

            return false;
        }
    }

    delegate bool MethodRecordDelegate (JSNode parent, int index, out JSNode node, out string name);

    public class JSNodeTraversalMethodRecord : JSNodeTraversalArrayRecord {
        public readonly System.Reflection.MethodInfo Method;
        private readonly MethodRecordDelegate Delegate;

        public JSNodeTraversalMethodRecord (System.Reflection.MethodInfo method) {
            Method = method;
            Name = method.Name;
            Delegate = (MethodRecordDelegate)System.Delegate.CreateDelegate(typeof(MethodRecordDelegate), method, true);
        }

        public override bool GetElement (JSNode parent, int index, out JSNode node, out string name) {
            return Delegate(parent, index, out node, out name);
        }
    }
}

namespace JSIL.Ast {
    [AttributeUsage(
        AttributeTargets.Field
    )]
    public class JSAstIgnoreAttribute : Attribute {
    }

    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Method
    )]
    public class JSAstTraverseAttribute : Attribute {
        public readonly int TraversalIndex;
        public readonly string Name;

        public JSAstTraverseAttribute (int traversalIndex) {
            TraversalIndex = traversalIndex;
            Name = null;
        }

        public JSAstTraverseAttribute (int traversalIndex, string name) {
            TraversalIndex = traversalIndex;
            Name = name;
        }
    }

    [AttributeUsage(
        AttributeTargets.Class
    )]
    public class JSAstIgnoreInheritedMembersAttribute : Attribute {
    }
}