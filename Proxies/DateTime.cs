using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(TimeSpan), 
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class TimeSpanProxy {
        [JSIsPure]
        public static TimeSpanProxy operator + (TimeSpanProxy lhs, TimeSpanProxy rhs) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        public static TimeSpanProxy operator - (TimeSpanProxy lhs, TimeSpanProxy rhs) {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        typeof(DateTime),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class DateTimeProxy {
        [JSIsPure]
        public static TimeSpanProxy operator - (DateTimeProxy lhs, DateTimeProxy rhs) {
            throw new InvalidOperationException();
        }
    }
}
