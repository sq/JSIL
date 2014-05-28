using System;
using System.Globalization;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies.Bcl
{
    [JSProxy(typeof(TimeSpan), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_TimeSpan
    {
        [JSExternal]
        public static TimeSpan FromMilliseconds(double value)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TimeSpan FromSeconds(double value)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TimeSpan FromMinutes(double value)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TimeSpan FromHours(double value)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TimeSpan FromDays(double value)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static TimeSpan FromTicks(long value)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static System_TimeSpan operator +(System_TimeSpan t1, System_TimeSpan t2)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static bool operator ==(System_TimeSpan t1, System_TimeSpan t2)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static bool operator >(System_TimeSpan t1, System_TimeSpan t2)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static bool operator >=(System_TimeSpan t1, System_TimeSpan t2)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static bool operator !=(System_TimeSpan t1, System_TimeSpan t2)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static bool operator <(System_TimeSpan t1, System_TimeSpan t2)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static bool operator <=(System_TimeSpan t1, System_TimeSpan t2)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static System_TimeSpan operator -(System_TimeSpan t1, System_TimeSpan t2)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static System_TimeSpan operator -(System_TimeSpan t)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        [JSReplaceConstructor]
        public System_TimeSpan(long ticks)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        [JSReplaceConstructor]
        public System_TimeSpan(int hours, int minutes, int seconds)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        [JSReplaceConstructor]
        public System_TimeSpan(int days, int hours, int minutes, int seconds)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        [JSReplaceConstructor]
        public System_TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            throw new NotImplementedException();
        }

        public int Days
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Hours 
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Milliseconds 
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Minutes 
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Seconds 
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public long Ticks
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public double TotalMilliseconds
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public double TotalSeconds  
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public double TotalMinutes 
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public double TotalHours 
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public double TotalDays 
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public static System.TimeSpan Parse(string s)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }

    [JSProxy(typeof(DateTime), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_DateTime
    {
        [JSExternal]
        [JSReplaceConstructor]
        public System_DateTime(long ticks)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        [JSReplaceConstructor]
        private System_DateTime(ulong dateData)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        [JSReplaceConstructor]
        public System_DateTime(int year, int month, int day)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        [JSReplaceConstructor]
        public System_DateTime(int year, int month, int day, Calendar calendar)
        {
            throw new NotImplementedException();
        }

        public static DateTime Now
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public static DateTime UtcNow
        {
            [JSExternal]
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
