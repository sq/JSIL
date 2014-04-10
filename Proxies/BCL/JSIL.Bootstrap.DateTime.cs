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
        public static System.TimeSpan FromTicks(long value)
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
        public System_TimeSpan(long ticks)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public System_TimeSpan(int hours, int minutes, int seconds)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public System_TimeSpan(int days, int hours, int minutes, int seconds)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public System_TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public int Days
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public int Hours 
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public int Milliseconds 
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public int Minutes 
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public int Seconds 
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public long Ticks
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public double TotalMilliseconds
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public double TotalSeconds  
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public double TotalMinutes 
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public double TotalHours 
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public double TotalDays 
        {
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
        public static System.DateTime UtcNow { 
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
