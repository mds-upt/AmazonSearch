using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using System.Net;
using System.Collections.Specialized;
using System.IO;

namespace AmazonApp.Models
{
    public static class FinanceData
    {
        //static String yahooQuery = "http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.xchange%20where%20pair%20in%20%28%22USDEUR%22,%20%22USDJPY%22,%20%22USDBGN%22,%20%22USDCZK%22,%20%22USDDKK%22,%20%22USDGBP%22,%20%22USDHUF%22,%20%22USDLTL%22,%20%22USDLVL%22,%20%22USDPLN%22,%20%22USDRON%22,%20%22USDSEK%22,%20%22USDCHF%22,%20%22USDNOK%22,%20%22USDHRK%22,%20%22USDRUB%22,%20%22USDTRY%22,%20%22USDAUD%22,%20%22USDBRL%22,%20%22USDCAD%22,%20%22USDCNY%22,%20%22USDHKD%22,%20%22USDIDR%22,%20%22USDILS%22,%20%22USDINR%22,%20%22USDKRW%22,%20%22USDMXN%22,%20%22USDMYR%22,%20%22USDNZD%22,%20%22USDPHP%22,%20%22USDSGD%22,%20%22USDTHB%22,%20%22USDZAR%22,%20%22USDISK%22%29&format=json&env=store://datatables.org/alltableswithkeys";
        public static readonly Dictionary<String, String> LangCodes = new Dictionary<String, String>
        { 
            { "EUR", "Euro" },
            { "USD", "United States Dollar" }, 
            { "AED", "Arab Emirates Dirham" }, 
            { "AUD", "Australian Dollar" }, 
            { "AZN", "Azerbaijani Manat" }, 
            { "BHD", "Bahraini Dinar" }, 
            { "BRL", "Brazilian Real" }, 
            { "CAD", "Canadian Dollar" }, 
            { "CHF", "Swiss Franc" }, 
            { "CNY", "Chinese Yuan Renminbi" }, 
            { "DKK", "Danish Krone" }, 
            { "GBP", "British Pound Sterling" }, 
            { "HKD", "Hong Kong Dollar" }, 
            { "INR", "Indian Rupee" }, 
            { "JOD", "Jordanian Dinar" }, 
            { "JPY", "Japanese Yen" }, 
            { "KPW", "North Korean Won" }, 
            { "KRW", "South Korean Won" }, 
            { "KWD", "Kuwaiti Dinar" }, 
            { "KYD", "Caymanian Dollar " }, 
            { "LVL", "Latvian Lats" }, 
            { "MXN", "Mexican Peso" }, 
            { "NZD", "New Zealand Dollar" }, 
            { "OMR", "Omani Rial" }, 
            { "NOK", "Norwegian Krone" }, 
            { "PHP", "Philippine Peso" }, 
            { "RUB", "Russian Ruble" }, 
            { "SEK", "Swedish Krona" }, 
            { "SGD", "Singapore Dollar" }, 
            { "THB", "Thailand Baht" }, 
            { "TRY", "Turkish Lira" }, 
            { "ZAR", "South African Rand" }
        };

        public static Dictionary<String, String> GetCurrencies()
        {
            return LangCodes;
        }

        public static String QueryExchangeRate(String from, String to)
        {
            NameValueCollection queryParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryParams["from"] = from;
            queryParams["to"] = to;
            WebRequest request = HttpWebRequest.Create("http://rate-exchange.herokuapp.com/fetchRate?" + queryParams.ToString());
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseBody = reader.ReadToEnd();
            reader.Close();
            response.Close();
            ////JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            ////object json = jsSerializer.Serialize();
            return responseBody;
        }
    }
}