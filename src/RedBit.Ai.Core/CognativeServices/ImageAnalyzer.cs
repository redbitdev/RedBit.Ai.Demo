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
        // see here for visual feature props https://westcentralus.dev.cognitive.microsoft.com/docs/services/5adf991815e1060e6355ad44/operations/56f91f2e778daf14a499e1fa

        // visual features to include
        public bool DetectBrands { get; set; }
        public bool ShouldCategorize { get; set; } 
        public bool DetectColors { get; set; }
        public bool IncludeDescription { get; set; } 
        public bool DetectFaces { get; set; }
        public bool DetectImageType { get; set; }
        public bool DetectObjects { get; set; }
        public bool IncludeTags { get; set; }

        // details options
        public bool DetectCelebrities { get; set; }
        public bool DetectLandmarks { get; set; }


        public async Task<ImageEntity> Analyze()
        {
            // create the query string
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            var visualFeatures = VisualFeatures;
            if (!string.IsNullOrEmpty(visualFeatures))
                queryString["visualFeatures"] = visualFeatures;
            var details = Details;
            if (!string.IsNullOrEmpty(details))
                queryString["details"] = details;
            queryString["language"] = "en";

            // make the request and get the resposne content
            var responseContent = await MakeRequest(queryString);

            // now that we have the response we can save to table
            _imageEntity.ImageAnalyzerResults = responseContent;

            return _imageEntity;
        }
        private string Details
        {
            get
            {
                var sb = new StringBuilder();
                if (DetectCelebrities) sb.Append("Celebrities,");
                if (DetectLandmarks) sb.Append("Landmarks,");
                var ret = sb.ToString();
                if (ret.Length > 0)
                    ret = ret.Remove(ret.Length - 1, 1);
                return ret;
            }
        }
        private string VisualFeatures
        {
            get
            {
                var sb = new StringBuilder();
                if (DetectBrands) sb.Append("Brands,");
                if (ShouldCategorize) sb.Append("Categories,");
                if (DetectColors) sb.Append("Color,");
                if (IncludeDescription) sb.Append("Description,");
                if (DetectFaces) sb.Append("Faces,");
                if (DetectImageType) sb.Append("ImageType,");
                if (DetectObjects) sb.Append("Objects,");
                if (IncludeTags) sb.Append("Tags,");
                var ret = sb.ToString();
                if (ret.Length > 0)
                    ret = ret.Remove(ret.Length - 1, 1);
                return ret;
            }
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
