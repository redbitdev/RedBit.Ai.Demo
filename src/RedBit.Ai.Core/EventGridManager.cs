// TODO : this is hardcoded to work for demo, might want to refactor if you want to make generic or something else

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RedBit.Ai.Core
{
    /// <summary>
    /// Manages sending messages to the event grid
    /// </summary>
    public class EventGridManager
    {
        public EventGridManager(string topicEndpoint, string key)
        {
            TopicEndpoint = topicEndpoint;
            Key = key;
        }

        public string TopicEndpoint { get; }
        public string Key { get; }

        public async Task SendNewImageEvent(ImageEntity imageEntity)
        {
            // should be sending an array but we only have one item to send at a time
            var items = new List<GridEvent<ImageEntity>>
            {
                new GridEvent<ImageEntity>
                {
                    Data = imageEntity,
                    Subject = "newImage",
                    EventType = "newImage"
                }
            };

            using (HttpClient client = new HttpClient())
            {
                // set the headers
                client.DefaultRequestHeaders.Add("aeg-sas-key", Key);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("xai");

                // create the request
                using (var msg = new HttpRequestMessage(HttpMethod.Post, TopicEndpoint))
                {
                    msg.Headers.Add("Accept", "application/json");

                    // set the body for the POST
                    msg.Content = new StringContent(JsonConvert.SerializeObject(items), Encoding.UTF8, "application/json");
                    msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    // send the response
                    using (var response = await client.SendAsync(msg, HttpCompletionOption.ResponseContentRead))
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                    }
                }
            }
        }
    }

    public class GridEvent<T> where T : class
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Subject { get; set; }
        public string EventType { get; set; }
        public T Data { get; set; }
        public DateTime EventTime { get; set; } = DateTime.UtcNow;
    }
}
