using System;
using Twm.Core.DataProviders.Enums;

namespace Twm.Core.DataProviders.Common
{
    public class EndpointData
    {
        public Uri Uri;
        public EndpointSecurityType SecurityType;
        public bool UseCache { get; }

        public string QueryString { get; }

        public EndpointData(Uri uri, EndpointSecurityType securityType, bool useCache = false, string queryString = null)
        {
            Uri = uri;
            SecurityType = securityType;
            UseCache = useCache;
            QueryString = queryString;
        }

        public override string ToString()
        {
            return Uri.AbsoluteUri;
        }
    }
}