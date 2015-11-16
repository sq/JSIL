using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies.Bcl
{
    [JSProxy(typeof(LambdaExpression), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_Linq_Expressions_LambdaExpression : Expression
    {

        [JSExternal]
        public Delegate Compile()
        {
            throw new NotImplementedException();
        }
    }

    [JSProxy(typeof(Expression<>), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_Linq_Expressions_Expression_1 : Expression
    {
        [JSExternal]
        public AnyType Compile()
        {
            throw new NotImplementedException();
        }
    }
}
