using SilentAuction.Caches;
using SilentAuction.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SilentAuction
{
    public static class CacheStartup
    {
        public static void StartCaches()
        {
            FontCacheManager fm = new FontCacheManager();
            fm.RefreshFonts();

            SystemCacheManager systemCache = new SystemCacheManager();
            systemCache.Initialize();


        }
    }
}