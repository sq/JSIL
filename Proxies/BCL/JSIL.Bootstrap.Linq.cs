using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies.Bcl
{
    [JSProxy(typeof(Enumerable), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceDeclared, false)]
    public static class System_Linq_Enumerable
    {
        [JSExternal]
        public static bool Any<TSource>(IEnumerable<TSource> enumerable)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static bool Any<TSource>(IEnumerable<TSource> enumerable, Func<TSource, bool> predicate)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static int Count<TSource>(IEnumerable<TSource> enumerable)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> enumerable, Func<TSource, TResult> selector)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TSource[] ToArray<TSource>(IEnumerable<TSource> enumerable)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static bool Contains<TSource>(IEnumerable<TSource> enumerable, TSource item)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static IEnumerable<TResult> Cast<TResult>(IEnumerable enumerable)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static List<TSource> ToList<TSource>(IEnumerable<TSource> enumerable)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TSource ElementAt<TSource>(IEnumerable<TSource> enumerable, int index)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TSource ElementAtOrDefault<TSource>(IEnumerable<TSource> enumerable, int index)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static IEnumerable<TResult> OfType<TResult>(IEnumerable enumerable)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static IEnumerable<TSource> Where<TSource>(IEnumerable<TSource> enumerable, Func<TSource, bool> predicat)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static IEnumerable<int> Range(int from, int to)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static int Sum(IEnumerable<int> enumerable)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static float Sum(IEnumerable<float> enumerable)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static double Sum(IEnumerable<double> enumerable)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static IEnumerable<TResult> Empty<TResult>()
        {
            throw new NotImplementedException();
        }
    }


    [JSProxy(typeof(Expression), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_Linq_Expressions_Expression
    {
        [JSExternal]
        public static ConstantExpression Constant(object value)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static ConstantExpression Constant(object value, Type type)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static ParameterExpression Parameter(Type type)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static ParameterExpression Parameter(Type type, string name)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static BinaryExpression Equal(Expression left, Expression right)
        {
            throw new NotImplementedException();
        }
    }

    [JSProxy(typeof(ConstantExpression), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_Linq_Expressions_ConstantExpression
    {
        [JSExternal]
        [JSReplaceConstructor]
        public System_Linq_Expressions_ConstantExpression(object obj)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        private static ConstantExpression Make(object obj, Type type)
        {
            throw new NotImplementedException();
        }
    }

    [JSProxy(typeof(ParameterExpression), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_Linq_Expressions_ParameterExpression : Expression
    {
        [JSExternal]
        private static ParameterExpression Make(Type type, string name, bool flag)
        {
            throw new NotImplementedException();
        }

        public override Type Type
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsByRef
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }
    }

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
