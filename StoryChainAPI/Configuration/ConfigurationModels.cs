using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.Configuration
{
    public class JwtConfig
    {
        public string Secret { get; set; }

    }

    public class WebsiteConfig
    {
        public string Host { get; set; }
    }
}
