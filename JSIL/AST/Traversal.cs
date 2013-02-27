using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using JSIL.Internal;

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
            var tJSNode = typeof(JSNode);
            var tList = typeof(IList);
            var tCompilerGenerated = typeof(CompilerGeneratedAttribute);
            var tIgnore = typeof(JSAstIgnoreAttribute);
            var tTraverse = typeof(JSAstTraverseAttribute);
            var tIgnoreInherited = typeof(JSAstIgnoreInheritedMembersAttribute);
            var records = new List<JSNodeTraversalRecord>();

            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            var typeToScan = nodeType;
            var seenMembers = new HashSet<MemberInfo>();

            while (typeToScan != null) {
                foreach (var field in nodeType.GetFields(flags)) {
                    if (seenMembers.Contains(field))
                        continue;
                    seenMembers.Add(field);

                    var traverseAttributes = field.GetCustomAttributes(tTraverse, true);
                    if (traverseAttributes.Length == 0) {
                        if (field.GetCustomAttributes(tCompilerGenerated, true).Length > 0)
                            continue;
                        if (field.GetCustomAttributes(tIgnore, true).Length > 0)
                            continue;
                    }

                    JSNodeTraversalRecord newRecord = null;
                    if (tJSNode.IsAssignableFrom(field.FieldType)) {
                        newRecord = new JSNodeTraversalFieldRecord(field);
                    } else if (field.FieldType.IsArray) {
                        var arrayElementType = field.FieldType.GetElementType();

                        if (tJSNode.IsAssignableFrom(arrayElementType))
                            newRecord = new JSNodeTraversalArrayFieldRecord(field);
                    } else if (tList.IsAssignableFrom(field.FieldType)) {
                        var listElementType = field.FieldType.GetGenericArguments()[0];
                        if (tJSNode.IsAssignableFrom(listElementType))
                            newRecord = new JSNodeTraversalListFieldRecord(field);
                    }

                    if (newRecord != null) {
                        if (traverseAttributes.Length > 0)
                            newRecord.SortKey = ((JSAstTraverseAttribute)traverseAttributes[0]).TraversalIndex;

                        newRecord.OriginalIndex = records.Count;

                        records.Add(newRecord);
                    }
                }

                if (typeToScan.GetCustomAttributes(tIgnoreInherited, true).Length > 0)
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

    public class JSNodeTraversalFieldRecord : JSNodeTraversalElementRecord {
        public readonly System.Reflection.FieldInfo Field;

        public JSNodeTraversalFieldRecord (System.Reflection.FieldInfo field) {
            Field = field;
            Name = field.Name;
        }

        public override void Get (JSNode parent, out JSNode node, out string name) {
            node = (JSNode)Field.GetValue(parent);
            name = Name;
        }
    }

    public class JSNodeTraversalArrayFieldRecord : JSNodeTraversalArrayRecord {
        public readonly System.Reflection.FieldInfo Field;

        public JSNodeTraversalArrayFieldRecord (System.Reflection.FieldInfo field) {
            Field = field;
            Name = field.Name;
        }

        public override bool GetElement (JSNode parent, int index, out JSNode node, out string name) {
            var array = (JSNode[])Field.GetValue(parent);
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

    public class JSNodeTraversalListFieldRecord : JSNodeTraversalArrayRecord {
        public readonly System.Reflection.FieldInfo Field;

        public JSNodeTraversalListFieldRecord (System.Reflection.FieldInfo field) {
            Field = field;
            Name = field.Name;
        }

        public override bool GetElement (JSNode parent, int index, out JSNode node, out string name) {
            var list = (IList)Field.GetValue(parent);
            if ((list == null) || (index >= list.Count)) {
                node = null;
                name = null;
                return false;
            } else {
                node = (JSNode)list[index];
                name = Name;
                return true;
            }
        }
    }
}

namespace JSIL.Ast {
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Property
    )]
    public class JSAstIgnoreAttribute : Attribute {
    }

    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Property
    )]
    public class JSAstTraverseAttribute : Attribute {
        public readonly int TraversalIndex;

        public JSAstTraverseAttribute (int traversalIndex) {
            TraversalIndex = traversalIndex;
        }
    }

    [AttributeUsage(
        AttributeTargets.Class
    )]
    public class JSAstIgnoreInheritedMembersAttribute : Attribute {
    }
}