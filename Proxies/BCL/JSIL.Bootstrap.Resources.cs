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

        [JSExternal]
        public CultureInfo CurrentUICulture
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
    }
}
