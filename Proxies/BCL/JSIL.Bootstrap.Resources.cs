using System;
using System.Globalization;
using System.Resources;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies.Bcl
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
        [JSExternal]
        [JSReplaceConstructor]
        public System_Globalization_CultureInfo(string cultureId)
        {
            
        }

        [JSExternal]
        [JSReplaceConstructor]
        public System_Globalization_CultureInfo(string str, bool boolean)
        {

        }

        [JSExternal]
        public virtual object Clone()
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public string TwoLetterISOLanguageName
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool UseUserOverride
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        [JSExternal]
        private static CultureInfo GetCultureByName(string str, bool boolean)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static CultureInfo GetCultureInfo(string str)
        {
            throw new NotImplementedException();
        }

        [JSExternal]
        public static CultureInfo GetCultureInfoByIetfLanguageTag(string str)
        {
            throw new NotImplementedException();
        }

        public static CultureInfo CurrentUICulture
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public static CultureInfo InvariantCulture
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }

        public static CultureInfo CurrentCulture
        {
            [JSExternal]
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
