using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

// TODO : this is hardcoded to work for demo, might want to refactor if you want to make generic or something else

namespace RedBit.Ai.Core
{
    public class ImageResizer : CognativeService<Stream>
    {
        private readonly string BASE_URL = "https://canadacentral.api.cognitive.microsoft.com/vision/v2.0/generateThumbnail?";

        public ImageResizer(ImageEntity imageEntity, BlobManager blobManager, TableManager tableManager, string subscriptionKey)
            : base(imageEntity, blobManager, tableManager, subscriptionKey) { }

        internal override string Url => BASE_URL;
        internal override HttpContent Content
        {
            get
            {
                var ret = new StringContent(JsonConvert.SerializeObject(new { url = _imageEntity.OriginalImageUrl }), Encoding.UTF8, "application/json");
                ret.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return ret;
            }
        }

        internal async override Task<Stream> OnRequestComplete(HttpResponseMessage response)
        {
            var resp = await response.Content.ReadAsStreamAsync();
            var streamCopy = new MemoryStream((int)resp.Length);
            resp.CopyTo(streamCopy);
            return streamCopy;
        }

        public async Task Resize(ImageSize imageSize)
        {
            // create the query string
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            var (width, height) = imageDimensionsTable[imageSize];
            queryString["width"] = $"{width}";
            queryString["height"] = $"{height}";
            queryString["smartCropping"] = "true";

            // make the request and get the resposne content
            var responseContent = await MakeRequest(queryString);

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
