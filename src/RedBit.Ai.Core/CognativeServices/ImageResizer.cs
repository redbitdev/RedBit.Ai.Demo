using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

// TODO : this is hardcoded to work for demo, might want to refactor if you want to make generic or something else

namespace RedBit.Ai.Core
{
    public class ImageResizer
    {
        private readonly string BASE_URL = "https://canadacentral.api.cognitive.microsoft.com/vision/v2.0/generateThumbnail?";
        private readonly ImageEntity _imageEntity;
        public ImageResizer(ImageEntity imageEntity, BlobManager blobManager, TableManager tableManager, string subscriptionKey)
        {
            _imageEntity = imageEntity;
            BlobManager = blobManager;
            TableManager = tableManager;
            SubscriptionKey = subscriptionKey;
        }
        public BlobManager BlobManager { get; }
        public TableManager TableManager { get; }
        public string SubscriptionKey { get; }

        public async Task Resize(ImageSize imageSize)
        {
            // upload using HttpClient
            using (HttpClient client = new HttpClient())
            {
                // create the query string and params
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                var (width, height) = imageDimensionsTable[imageSize];
                queryString["width"] = $"{width}";
                queryString["height"] = $"{height}";
                queryString["smartCropping"] = "true";

                // create the url
                var url = $"{BASE_URL}{queryString.ToString()}";

                // Add the subscription key
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

                // create the request
                using (var msg = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    msg.Headers.Add("Accept", "application/json");

                    // set the body for the POST
                    msg.Content = new StringContent(JsonConvert.SerializeObject(new { url = _imageEntity.OriginalImageUrl }), Encoding.UTF8, "application/json");
                    msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    // send the response
                    using (var response = await client.SendAsync(msg, HttpCompletionOption.ResponseContentRead))
                    {
                        var responseContent = await response.Content.ReadAsStreamAsync();

                        // take the stream and add it to blob
                        var blobUrl = "";
                        switch (imageSize)
                        {
                            case ImageSize.ExtraSmall:
                                blobUrl = await BlobManager.AddExtraSmallImage(responseContent, _imageEntity.OriginalImageUrl);
                                _imageEntity.ExtraSmallImageUrl = blobUrl;
                                break;
                            case ImageSize.Small:
                                blobUrl = await BlobManager.AddSmallImage(responseContent, _imageEntity.OriginalImageUrl);
                                _imageEntity.SmallImageUrl = blobUrl;
                                break;
                            case ImageSize.Medium:
                                blobUrl = await BlobManager.AddMediumImage(responseContent, _imageEntity.OriginalImageUrl);
                                _imageEntity.MediumImageUrl = blobUrl;
                                break;
                            default:
                                break;
                        }

                        // if we have a url then lets add to table
                        if (!string.IsNullOrEmpty(blobUrl))
                        {
                            await TableManager.UpdateRecord(_imageEntity);
                        }

                    }
                }
            }
        }

        public enum ImageSize
        {
            ExtraSmall, Small, Medium
        }

        private readonly Dictionary<ImageSize, (int width, int height)> imageDimensionsTable = new Dictionary<ImageSize, (int width, int height)>()
        {
            { ImageSize.ExtraSmall, (320, 200) },
            { ImageSize.Small,      (640, 400) },
            { ImageSize.Medium,     (800, 600) }
        };

    }
}
