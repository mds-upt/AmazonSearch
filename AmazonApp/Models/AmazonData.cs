using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Security.Cryptography;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Web.Configuration;
using AmazonApp.Amazon;

namespace AmazonApp.Models
{
    public static class AmazonData
    {
        private static String secretAccessKey = WebConfigurationManager.AppSettings["SecretAccessKey"].ToString();
        private static String awsAccessKeyId = WebConfigurationManager.AppSettings["AWSAccessKeyID"].ToString();
        private static String associateTag = WebConfigurationManager.AppSettings["AssociateTag"].ToString();
        private static AWSECommerceServicePortTypeClient client = ConstructAWSProvider();

        private static AWSECommerceServicePortTypeClient ConstructAWSProvider()
        {
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            binding.MaxReceivedMessageSize = Int32.MaxValue;
            EndpointAddress endpoint = new EndpointAddress("https://ecs.amazonaws.com/onca/soap?Service=AWSECommerceService");
            //// https://webservices.amazon.com/onca/soap?Service=AWSECommerceService
            AWSECommerceServicePortTypeClient aws = new AWSECommerceServicePortTypeClient(binding, endpoint);
            aws.ChannelFactory.Endpoint.EndpointBehaviors.Add(new AmazonSigningEndpointBehavior(awsAccessKeyId, secretAccessKey));
            return aws;
        }

        // TODO: maybe return tuples instead
        public static ResponseContainer AWSQuery(String keywords, String group = "All", int page = 1, int perPage = 10, bool fetchAll = false)
        {
            ItemSearch search0 = new ItemSearch();
            search0.AWSAccessKeyId = awsAccessKeyId;
            search0.AssociateTag = associateTag;

            // Search request
            ItemSearchRequest request0 = new ItemSearchRequest();
            request0.ResponseGroup = new string[] { "ItemAttributes, Images, OfferSummary, Offers, OfferFull" };
            request0.SearchIndex = group;
            request0.Keywords = keywords;

            ResponseContainer returnData = new ResponseContainer();
            int p = page;
            int skip = 0;
            int nulls = 0; // Send error if more than 6 null responses
            if (p > 1)
            { // Api always returns 10 items
                p--;
                string s = perPage.ToString();
                string s1 = s.Substring(s.Length - 1, 1);
                int.TryParse(s1, out skip);
            }
            while (true)
            {
                Debug.WriteLine("So far {0} items", returnData.products.Count);
                Debug.WriteLine("Sleeping thread for 8 seconds.");
                Thread.Sleep(8000); // Necessary, app wont work without this
                Debug.WriteLine("Resuming thread.");
                request0.ItemPage = p.ToString();
                search0.Request = new ItemSearchRequest[] { request0 };
                ItemSearchResponse response0 = new ItemSearchResponse();
                try
                {
                    response0 = client.ItemSearch(search0);
                }
                catch (ServerTooBusyException)
                {
                    returnData.error = "Amazon server busy.";
                    break;
                }
                if (nulls >= 6)
                {
                    returnData.error = "Amazon server returned null.";
                    break;
                }

                if (response0.Items == null) // sometimes is null
                {
                    Debug.WriteLine("ITEMS property was null - skipped loop");
                    nulls++;
                    continue;
                }
                foreach (Items items in response0.Items)
                {
                    if (items.Item == null) // sometimes is null
                    {
                        Debug.WriteLine("ITEM property was null - skipped loop");
                        nulls++;
                        continue;
                    }
                    foreach (Item item in items.Item)
                    {
                        string title = item.ItemAttributes.Title;
                        string listPrice = "Failed to fetch";
                        try
                        {
                            listPrice = TrimDollarSign(item.OfferSummary.LowestNewPrice.FormattedPrice);
                        }
                        catch (NullReferenceException)
                        {
                            Debug.WriteLine("NullReferenceException at Listprice");
                        }
                        string imageURL = "";
                        try
                        {
                            imageURL = item.LargeImage.URL;
                        }
                        catch (NullReferenceException)
                        {
                            Debug.WriteLine("NullReferenceException at ImageURL");
                        }
                        string pageURL = item.DetailPageURL;

                        if (skip <= 0)
                        {
                            returnData.products.Add(new Product(title, listPrice, imageURL, pageURL));
                            Debug.WriteLine("Reached adding part");
                        }
                        else
                        {
                            skip--;
                        }

                        if (returnData.products.Count >= perPage)
                            break;
                    }
                    if (returnData.products.Count >= perPage)
                        break;
                }
                p += 1;

                if (returnData.products.Count >= perPage)
                {
                    returnData.totalResults = (int.Parse(response0.Items[0].TotalResults) / perPage) - 1;
                    break;
                }
            }
            return returnData;
        }

        private static String TrimDollarSign(String inputString)
        {
            return Regex.Replace(inputString, @"[\$]", "");
        }

        private class AmazonSigningEndpointBehavior : IEndpointBehavior
        {
            private string accessKeyId = "";
            private string secretKey = "";

            public AmazonSigningEndpointBehavior(string accessKeyId, string secretKey)
            {
                this.accessKeyId = accessKeyId;
                this.secretKey = secretKey;
            }

            public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime clientRuntime)
            {
                clientRuntime.ClientMessageInspectors.Add(new AmazonSigningMessageInspector(accessKeyId, secretKey));
            }

            public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatched)
            {
            }

            public void Validate(ServiceEndpoint serviceEndpoint)
            {
            }

            public void AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParemeters)
            {
            }
        }

        private class AmazonSigningMessageInspector : IClientMessageInspector
        {
            private string accessKeyId = "";
            private string secretKey = "";

            public AmazonSigningMessageInspector(string accessKeyId, string secretKey)
            {
                this.accessKeyId = accessKeyId;
                this.secretKey = secretKey;
            }

            public Object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
            {
                string operation = Regex.Match(request.Headers.Action, "[^/]+$").ToString();
                DateTime now = DateTime.UtcNow;
                String timestamp = now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                String signMe = operation + timestamp;
                Byte[] bytesToSign = Encoding.UTF8.GetBytes(signMe);

                Byte[] secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
                HMAC hmacSha256 = new HMACSHA256(secretKeyBytes);
                Byte[] hashBytes = hmacSha256.ComputeHash(bytesToSign);
                String signature = Convert.ToBase64String(hashBytes);

                request.Headers.Add(new AmazonHeader("AWSAccessKeyId", accessKeyId));
                request.Headers.Add(new AmazonHeader("Timestamp", timestamp));
                request.Headers.Add(new AmazonHeader("Signature", signature));
                return null;
            }

            void IClientMessageInspector.AfterReceiveReply(ref System.ServiceModel.Channels.Message message, Object correlationState)
            {
            }
        }

        private class AmazonHeader : MessageHeader
        {
            private string m_name;
            private string value;

            public AmazonHeader(string name, string value)
            {
                this.m_name = name;
                this.value = value;
            }

            public override string Name
            {
                get { return this.m_name; }
            }

            public override string Namespace
            {
                get { return "http://security.amazonaws.com/doc/2007-01-01/"; }
            }

            protected override void OnWriteHeaderContents(System.Xml.XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteString(value);
            }
        }
    }
}