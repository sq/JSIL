using System;
using JSIL.Meta;
using JSIL.Proxy;
using System.Globalization;

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
        typeof(TimeSpan),
        JSProxyMemberPolicy.ReplaceDeclared)]
    public abstract class TSProxy
    {
        [JSReplacement("System.TimeSpan.FromMilliseconds($type)")]
        [JSIsPure]
        public static TimeSpan FromMilliseconds(double d)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.TimeSpan.FromSeconds($type)")]
        [JSIsPure]
        public static TimeSpan FromSeconds(double d)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.TimeSpan.FromMinutes($type)")]
        [JSIsPure]
        public static TimeSpan FromMinutes(double d)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.TimeSpan.FromHours($type)")]
        [JSIsPure]
        public static TimeSpan FromHours(double d)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.TimeSpan.FromDays($type)")]
        [JSIsPure]
        public static TimeSpan FromDays(double d)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.TimeSpan.FromTicks($type)")]
        [JSIsPure]
        public static TimeSpan FromTicks(long d)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.TimeSpan.ctor($type)")]
        [JSIsPure]
        public TSProxy(long arg)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.TimeSpan.ctor($a1, $a2, $a3)")]
        [JSIsPure]
        public TSProxy(int arg1, int arg2, int arg3)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.TimeSpan.ctor($a1, $a2, $a3, $a4)")]
        [JSIsPure]
        public TSProxy(int arg1, int arg2, int arg3, int arg4)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.TimeSpan.ctor($a1, $a2, $a3, $a4, $a5)")]
        [JSIsPure]
        public TSProxy(int arg1, int arg2, int arg3, int arg4, int arg5)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.TimeSpan.get_Days()")]
        [JSIsPure]
        public int Days { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.TimeSpan.get_Hours()")]
        [JSIsPure]
        public int Hours { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.TimeSpan.get_Milliseconds()")]
        [JSIsPure]
        public int Milliseconds { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.TimeSpan.get_Minutes()")]
        [JSIsPure]
        public int Minutes { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.TimeSpan.get_Seconds()")]
        [JSIsPure]
        public int Seconds { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.TimeSpan.get_Ticks()")]
        [JSIsPure]
        public long Ticks { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.TimeSpan.get_TotalMilliseconds()")]
        [JSIsPure]
        public double TotalMilliseconds { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.TimeSpan.get_TotalSeconds()")]
        [JSIsPure]
        public double TotalSeconds { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.TimeSpan.get_TotalMinutes()")]
        [JSIsPure]
        public double TotalMinutes { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.TimeSpan.get_TotalHours()")]
        [JSIsPure]
        public double TotalHours { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.TimeSpan.get_TotalDays()")]
        [JSIsPure]
        public double TotalDays { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.TimeSpan.Parse($s)")]
        [JSIsPure]
        public static TimeSpan Parse(string s)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.TimeSpan.toString()")]
        [JSIsPure]
        public static string ToString()
        {
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

    [JSProxy(typeof(DateTime), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_DateTime
    {
        [JSExternal]
        public System_DateTime(long ticks)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        private System_DateTime(ulong dateData)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public System_DateTime(int year, int month, int day)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public System_DateTime(int year, int month, int day, Calendar calendar)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static System.DateTime Now
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public static System.DateTime UtcNow
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public string ToLongTimeString()
        {
            throw new NotImplementedException();
        }
    }
}
