using JSIL.Meta;
using JSIL.Proxy;
using System;
using System.Globalization;
using System.Resources;
using System.Text;

namespace JSIL.Proxies
{
    [JSProxy(typeof(ResourceManager), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Resources_ResourceManager
    {
    }

    [JSProxy(typeof(ResourceSet), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Resources_ResourceSet
    {
    }

    [JSProxy(typeof(CultureInfo), JSProxyMemberPolicy.ReplaceDeclared, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_Globalization_CultureInfo
    {
        private NumberFormatInfo _numberFormat;
        private DateTimeFormatInfo _dateTimeFormat;

        [JSExternal]
        public System_Globalization_CultureInfo(string cultureId)
        {

        }

        [JSExternal]
        public System_Globalization_CultureInfo(string str, bool boolean)
        {

        }

        [JSExternal]
        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public string TwoLetterISOLanguageName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public bool UseUserOverride
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public CultureInfo CurrentCulture
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        public CultureInfo CurrentUICulture
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Calendar Calendar
        {
            get { return DateTimeFormat.Calendar; }
        }

        public NumberFormatInfo NumberFormat
        {
            get
            {
                if (_numberFormat == null)
                {
                    _numberFormat = new NumberFormatInfo
                    {
                        CurrencyDecimalDigits = 2,
                        CurrencyDecimalSeparator = ".",
                        CurrencyGroupSeparator = ",",
                        CurrencyGroupSizes = new[] { 3 },
                        CurrencyNegativePattern = 0,
                        CurrencyPositivePattern = 0,
                        CurrencySymbol = "$",
                        DigitSubstitution = DigitShapes.None,
                        NaNSymbol = "NaN",
                        NativeDigits = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" },
                        NegativeInfinitySymbol = "-Infinity",
                        NegativeSign = "-",
                        NumberDecimalDigits = 2,
                        NumberDecimalSeparator = ".",
                        NumberGroupSeparator = ",",
                        NumberGroupSizes = new[] { 3 },
                        NumberNegativePattern = 1,
                        PerMilleSymbol = "‰",
                        PercentDecimalDigits = 2,
                        PercentDecimalSeparator = ".",
                        PercentGroupSeparator = ",",
                        PercentGroupSizes = new[] { 3 },
                        PercentNegativePattern = 0,
                        PercentPositivePattern = 0,
                        PercentSymbol = "%",
                        PositiveInfinitySymbol = "Infinity",
                        PositiveSign = "+"
                    };
                }
                return _numberFormat;
            }
        }

        public DateTimeFormatInfo DateTimeFormat
        {
            get
            {
                if (_dateTimeFormat == null)
                {
                    _dateTimeFormat = new DateTimeFormatInfo
                    {
                        AbbreviatedDayNames = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" },
                        AbbreviatedMonthGenitiveNames =
                            new[]
                        {
                            "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
                            string.Empty
                        },
                        AbbreviatedMonthNames =
                            new[]
                        {
                            "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
                            string.Empty
                        },
                        AMDesignator = "AM",
                        Calendar = new GregorianCalendar(GregorianCalendarTypes.USEnglish),
                        CalendarWeekRule = CalendarWeekRule.FirstDay,
                        DateSeparator = "/",
                        DayNames = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" },

                        FirstDayOfWeek = DayOfWeek.Sunday,
                        FullDateTimePattern = "dddd, MMMM dd, yyyy h:mm:ss tt",
                        LongDatePattern = "dddd, MMMM dd, yyyy",
                        LongTimePattern = "h:mm:ss tt",
                        MonthDayPattern = "MMMM dd",
                        MonthGenitiveNames =
                            new[]
                        {
                            "January", "February", "March", "April", "May", "June", "July", "August", "September",
                            "October", "November", "December", string.Empty
                        },
                        MonthNames =
                            new[]
                        {
                            "January", "February", "March", "April", "May", "June", "July", "August", "September",
                            "October", "November", "December", string.Empty
                        },
                        PMDesignator = "PM",
                        ShortDatePattern = "M/d/yyyy",
                        ShortestDayNames = new[] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" },
                        ShortTimePattern = "h:mm tt",
                        TimeSeparator = ":",
                        YearMonthPattern = "MMMM, yyyy"
                    };
                }
                return _dateTimeFormat;
            }
        }

        [JSExternal]
        public virtual object Clone()
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public CultureInfo GetCultureByName(string str, bool boolean)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public CultureInfo GetCultureInfo(string str)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public CultureInfo GetCultureInfoByIetfLanguageTag(string str)
        {
            throw new NotImplementedException();
        }

        private static string GetDefaultLocaleName(int localeType)
        {
            return "en-us";
        }

        private static string GetUserDefaultUILanguage()
        {
            return "en-us";
        }

        private static string GetSystemDefaultUILanguage()
        {
            return "en-us";
        }
    }

    [JSProxy(typeof(DateTimeFormatInfo), JSProxyMemberPolicy.ReplaceDeclared, JSProxyAttributePolicy.ReplaceDeclared,
        JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System_Globalization_DateTimeFormatInfo
    {
        internal Calendar calendar;
        public Calendar Calendar { set { calendar = value; } get { return calendar; } }
    }



    [JSProxy("System.DateTimeFormat", JSProxyMemberPolicy.ReplaceDeclared, JSProxyAttributePolicy.ReplaceDeclared,
        JSProxyInterfacePolicy.ReplaceNone, false)]
    public class System__DateTimeFormat
    {
        internal static void FormatDigits(StringBuilder outputBuffer, int value, int len, bool overrideLengthLimit)
        {
            if (!overrideLengthLimit && len > 2)
            {
                len = 2;
            }

            var chars = new char[16];

            var ptr2 = 16;
            var num = value;
            do
            {
                chars[ptr2] = (char)(num % 10 + 48);
                ptr2--;
                num /= 10;
            }
            while (num != 0 && ptr2 != 0);
            var num2 = (16 - ptr2);
            while (num2 < len && ptr2 != 0)
            {
                chars[ptr2] = '0';
                ptr2--;
                num2++;
            }

            for (var i = 0; i < num2; i++)
            {
                outputBuffer.Append(chars[ptr2 + i]);
            }
        }
    }
}
