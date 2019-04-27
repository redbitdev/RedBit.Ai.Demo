using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

// TODO : this is hardcoded to work for demo, might want to refactor if you want to make generic or something else

namespace RedBit.Ai.Core
{
    public class ImageAnalyzer : CognativeService<string>
    {
        private readonly string BASE_URL = "https://canadacentral.api.cognitive.microsoft.com/vision/v2.0/analyze?";

        public ImageAnalyzer(ImageEntity imageEntity, BlobManager blobManager, TableManager tableManager, string subscriptionKey) 
            : base(imageEntity, blobManager, tableManager, subscriptionKey)
        {

        }

        public async Task<ImageEntity> Analyze()
        {
            // create the query string
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["visualFeatures"] = $"Description,Objects,Categories";
            queryString["language"] = "en";

            // make the request and get the resposne content
            var responseContent = await MakeRequest(queryString);

            // now that we have the response we can save to table
            _imageEntity.ImageAnalyzerResults = responseContent;

            return _imageEntity;
        }

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

        internal override Task<string> OnRequestComplete(HttpResponseMessage response) => response.Content.ReadAsStringAsync();
    }
}
