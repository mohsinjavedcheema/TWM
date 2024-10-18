using System;

namespace Twm.Core.DataProviders.Common
{
    public class ClientConfiguration
    {
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string ServerName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string Reference { get; set; }

        public int ConnectionId { get; set; }
        public bool EnableRateLimiting { get; set; }
        public TimeSpan CacheTime { get; set; } = TimeSpan.FromMinutes(30);
        public TimeSpan TimestampOffset { get; set; } = TimeSpan.FromMilliseconds(0);
        public int DefaultReceiveWindow { get; set; } = 10000;
    }
}