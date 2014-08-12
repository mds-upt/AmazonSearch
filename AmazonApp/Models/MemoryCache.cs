using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime;
using System.Web.Script.Serialization;
using System.Web.Configuration;

namespace AmazonApp.Models
{
    public static class MemoryCache
    {
        private static long cacheLimitInBytes = CacheSizeLimit();
        private static List<Tuple<ResponseContainer, int, int, String, int>> cache = new List<Tuple<ResponseContainer, int, int, String, int>>();

        public static void Cache(ResponseContainer response, int page, int perPage, String keywords, int catID)
        {
            if (!cache.Exists(x => Match(x, page, perPage, keywords, catID)))
            {
                cache.RemoveAll(x => Match(x, page, perPage, keywords, catID));
            }
            Tuple<ResponseContainer, int, int, String, int> tuple = new Tuple<ResponseContainer, int, int, String, int>(response, page, perPage, keywords, catID);
            cache.Add(tuple);

            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            while (jsSerializer.Serialize(cache).Length * sizeof(Char) >= cacheLimitInBytes)
            {
                // JSSerializer abuse to limit cache size
                cache.RemoveAt(1);
            }
        }

        public static ResponseContainer Acquire(int page, int perPage, String keywords, int catID)
        {
            if (cache.Exists(x => Match(x, page, perPage, keywords, catID)))
            {
                return cache.Find(x => Match(x, page, perPage, keywords, catID)).Item1;
            }
            return null;
        }

        private static bool Match(Tuple<ResponseContainer, int, int, String, int> entry, int page, int perPage, String keywords, int catID)
        {
            if (entry.Item5 == catID)
                if (entry.Item4 == keywords)
                    if (entry.Item2 == page)
                        if (entry.Item3 == perPage)
                            return true;
            return false;
        }

        private static long CacheSizeLimit()
        {
            long returnVal = 2000000000;
            long.TryParse(WebConfigurationManager.AppSettings["cacheSizeLimitInBytes"].ToString(), out returnVal);
            return returnVal;
        }
    }
}