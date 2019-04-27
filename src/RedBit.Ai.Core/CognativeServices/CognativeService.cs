using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;

// TODO : this is hardcoded to work for demo, might want to refactor if you want to make generic or something else

namespace RedBit.Ai.Core
{
    public abstract class CognativeService<ResponseType>
    {
        public CognativeService(ImageEntity imageEntity, BlobManager blobManager, TableManager tableManager, string subscriptionKey)
        {
            _imageEntity = imageEntity;
            BlobManager = blobManager;
            TableManager = tableManager;
            SubscriptionKey = subscriptionKey;
        }

        public BlobManager BlobManager { get; }
        public TableManager TableManager { get; }
        public string SubscriptionKey { get; }

        internal async Task<ResponseType> MakeRequest(NameValueCollection queryString)
        {
            // upload using HttpClient
            using (HttpClient client = new HttpClient())
            {
                // create the url
                var url = $"{Url}{queryString.ToString()}";

                // Add the subscription key
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

                // create the request
                using (var msg = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    msg.Headers.Add("Accept", "application/json");

                    // set the body for the POST
                    msg.Content = Content;

                    // send the message
                    using (var response = await client.SendAsync(msg, HttpCompletionOption.ResponseContentRead))
                    {
                        return await OnRequestComplete(response);
                    }
                }
            }
        }

        internal abstract string Url { get; }

        internal abstract HttpContent Content { get; }

        internal abstract Task<ResponseType> OnRequestComplete(HttpResponseMessage response);

        internal readonly ImageEntity _imageEntity;
    }
}
