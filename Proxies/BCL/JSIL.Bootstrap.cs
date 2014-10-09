using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies.Bcl
{
    [JSProxy(typeof(Exception), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Exception
    {
    }

    [JSProxy(typeof(SystemException), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_SystemException
    {
    }

    [JSProxy(typeof(InvalidCastException), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_InvalidCastException
    {
    }

    [JSProxy(typeof(InvalidOperationException), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_InvalidOperationException
    {
    }

    [JSProxy(typeof(FileNotFoundException), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_FileNotFoundException
    {
    }

    [JSProxy(typeof(FormatException), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_FormatException
    {
    }

    [JSProxy(typeof(Console), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Console
    {
    }


    [JSProxy(typeof(EventArgs), JSProxyMemberPolicy.ReplaceDeclared, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_EventArgs
    {
        [JSExternal]
        [JSReplaceConstructor]
        public System_EventArgs()
        {
        }
    }

    [JSProxy(typeof(PropertyChangedEventArgs), JSProxyMemberPolicy.ReplaceDeclared, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_ComponentModel_PropertyChangedEventArgs
    {
        [JSExternal]
        [JSReplaceConstructor]
        public System_ComponentModel_PropertyChangedEventArgs(string propertyName)
        {
        }

        public string PropertyName
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    [JSProxy(typeof(Debug), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Diagnostics_Debug
    {
    }

    [JSProxy(typeof(Thread), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Threading_Thread
    {
    }

    [JSProxy(typeof(List<>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_List_1
    {
    }

    [JSProxy(typeof(ArrayList), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_ArrayList
    {
    }

    [JSProxy(typeof(Collection<>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_ObjectModel_Collection_1
    {
    }

    [JSProxy(typeof(ReadOnlyCollection<>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_ObjectModel_ReadOnlyCollection_1
    {
    }


    [JSProxy(typeof(Stack<>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_Stack_1
    {
    }

    [JSProxy(typeof(Queue<>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_Queue_1
    {
    }

    [JSProxy(typeof(Interlocked), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Threading_Interlocked
    {
    }

    [JSProxy(typeof(Monitor), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Threading_Monitor
    {
    }

    [JSProxy(typeof(Random), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Random
    {
    }

    [JSProxy(typeof(Math), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Math
    {
    }

    [JSProxy(typeof(Environment), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Environment
    {
    }

    [JSProxy(typeof(Dictionary<,>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_Dictionary_2
    {
    }

    [JSProxy(typeof(KeyValuePair<,>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_KeyValuePair_2
    {
    }

    [JSProxy(typeof(Nullable), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Nullable
    {
    }

    [JSProxy(typeof(Nullable<>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Nullable_1
    {
    }

    [JSProxy(typeof(XmlSerializer), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Xml_Serialization_XmlSerializer
    {
    }

    [JSProxy(typeof(StackTrace), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Diagnostics_StackTrace
    {
    }

    [JSProxy(typeof(StackFrame), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Diagnostics_StackFrame
    {
    }

    [JSProxy(typeof(Enum), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Enum
    {
    }

    [JSProxy(typeof(Activator), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Activator
    {
    }

    [JSProxy(typeof(Stopwatch), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Diagnostics_Stopwatch
    {
    }

    [JSProxy(typeof(GC), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_GC
    {
    }

    [JSProxy(typeof(HashSet<>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_HashSet_1
    {
    }

    [JSProxy(typeof(Convert), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Convert
    {
    }

    [JSProxy(typeof(BitConverter), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_BitConverter
    {
    }

    [JSProxy(typeof(LinkedList<>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Collections_Generic_LinkedList_1
    {
    }

    [JSProxy(typeof(LinkedListNode<>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Collections_Generic_LinkedListNode_1
    {
    }

    [JSProxy(typeof(Comparer<>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_Comparer_1
    {
    }

    [JSProxy(typeof(WeakReference), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_WeakReference
    {
    }

    [JSProxy(typeof(Trace), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Diagnostics_Trace
    {
    }

    [JSProxy("System.Threading.Volatile", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Threading_Volatile
    {
    }
}
