using JSIL.Meta;
using JSIL.Proxy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace JSIL.Proxies
{
    [JSProxy(typeof(ArrayList), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_ArrayList
    {
    }

    [JSProxy("System.Collections.ObjectModel.Collection`1", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_ObjectModel_Collection_1
    {
    }

    [JSProxy("System.Collections.ObjectModel.ReadOnlyCollection`1", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_ObjectModel_ReadOnlyCollection_1
    {
    }
    

    [JSProxy("System.Collections.Generic.List`1", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_List_1
    {
    }

    [JSProxy("System.Collections.Generic.Stack`1", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_Stack_1
    {
    }

    [JSProxy("System.Collections.Generic.List`1+Enumerator", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_List_1_Enumerator
    {
    }

    [JSProxy("System.Collections.Generic.Queue`1", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_Queue_1
    {
    }

    [JSProxy(typeof(Interlocked), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Threading_Interlocked
    {
    }

    [JSProxy("System.Collections.Generic.KeyValuePair`2", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_KeyValuePair_2
    {
    }

    [JSProxy("System.Collections.Generic.Dictionary`2", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_Dictionary_2
    {
    }

    [JSProxy("System.Collections.Generic.Dictionary`2/Enumerator", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_Dictionary_2_Enumerator
    {
    }

    [JSProxy("System.Collections.Generic.HashSet`1", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_HashSet_1
    {
    }

    [JSProxy("System.Collections.Generic.LinkedList`1", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Collections_Generic_LinkedList_1
    {
    }

    [JSProxy("System.Collections.Generic.LinkedListNode`1", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Collections_Generic_LinkedListNode_1
    {
    }

    [JSProxy("System.Collections.Generic.Comparer`1", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Collections_Generic_Comparer_1
    {
    }

    [JSProxy(typeof(Tuple), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Tuple
    {
    }

    [JSProxy(typeof(StackTrace), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Diagnostics_StackTrace
    {
    }

}
