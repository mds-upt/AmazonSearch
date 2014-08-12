using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using AmazonApp.Models;

namespace AmazonApp.Controllers
{
    /// <summary>
    /// Main controller.
    /// </summary>
    public class HomeController : Controller
    {
        //[RequireHttps] TODO: uncomment
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Controller action for product query
        /// </summary>
        /// <returns>Query results as json.</returns>
        /// <param name="catID">Category ID</param>
        /// <param name="searchString">Search Keywords</param>
        /// <param name="page">Starting page of query</param>
        /// <param name="perPage">Items per page</param>
        public String Json(int catID, String searchString, int page = 1, int perPage = 10)
        {
            // Sanitize user inputs
            String category = ProductCategories.ResolveCategory(catID);
            page = Math.Max(1, page);
            page = Math.Min(10, page);
            perPage = Math.Max(1, perPage);
            perPage = Math.Min(70, perPage);
            searchString = Regex.Replace(searchString, "[^a-z A-Z|0-9]", "");
            
            // Check if is cached
            ResponseContainer products = MemoryCache.Acquire(page, perPage, searchString, catID);
            if (products == null)
            {
                products = AmazonData.AWSQuery(searchString, category, page, perPage);

                // Try to cache current page
                if (products.error == null)
                {
                    MemoryCache.Cache(products, page, perPage, searchString, catID);
                }
            }
            else 
            {
                Debug.WriteLine("Retrieved page {0} from cache", page);
            }

            // Start new async thread to cache next page
            Action<String, String, int, int, int> cacheDelegate = PreemptiveCache;
            cacheDelegate.BeginInvoke(category, searchString, page, perPage, catID, cacheDelegate.EndInvoke, null);

            // Return response
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            Response.AddHeader("Content-Type", "text/plain");
            Debug.WriteLine("query returned.");
            return jsSerializer.Serialize(products);
        }

        /// <summary> Controller action for currency exchange rate conversion </summary>
        /// <param name="from">Currecy to convert from</param>
        /// <param name="to">Currency to convert to</param>
        public String ExchangeRate(String from, String to)
        {
            return FinanceData.QueryExchangeRate(from, to);
        }

        [ChildActionOnly]
        private void PreemptiveCache(String category, String searchString, int page, int perPage, int catID)
        {
            // Try to cache next page
            // TODO: If cacher is already in progress when page is requested, wait for completion
            Debug.WriteLine("Async cache started " + DateTime.Now.ToLongTimeString());
            ResponseContainer cacheThese = AmazonData.AWSQuery(searchString, category, page + 1, perPage);
            if (cacheThese.error == null)
            {
                MemoryCache.Cache(cacheThese, page + 1, perPage, searchString, catID);
            }

            Debug.WriteLine("Async cache completed " + DateTime.Now.ToLongTimeString());
        }
    }
}
