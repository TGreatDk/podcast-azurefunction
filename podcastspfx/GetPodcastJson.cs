using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Xml;
using System.Text.RegularExpressions;

namespace podcastspfx
{
    public static class Function1
    {
        [FunctionName("GetPodcastJson")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string podUrl = (string)req.Query["podUrl"] ?? data?.podUrl;

            
            Response.ReqStatus status = Response.ReqStatus.OK;

            object results = string.Empty;

            if (string.IsNullOrEmpty(podUrl))
            {                                
                return new BadRequestObjectResult(new Response() { Status=Response.ReqStatus.Fail, Results="Please provide 'podUrl' in querystring or request body" });
            }
            

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Regex regex = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
                    if (!regex.IsMatch(podUrl))
                        throw new ArgumentException();

                    var podStr = await client.GetStringAsync(podUrl);

                    if (string.IsNullOrEmpty(podStr))
                    {
                        throw new ArgumentNullException();
                    }

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(podStr);
                    results = doc.SelectSingleNode("/rss/*");
                }
                catch (ArgumentNullException)
                {
                    status = Response.ReqStatus.ApiEmpty;
                }
                catch(ArgumentException)
                {
                    status = Response.ReqStatus.ApiBadUrl;
                }
                catch (Exception ex)
                {
                    status = Response.ReqStatus.ApiFail;
                    if (req.Query.ContainsKey("debug"))
                        results = ex.Message;
                }
            }

            var response = new Response() { Status = status, Results = results };

            return (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(response));
        }
       
        public class Response
        {
            [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
            public ReqStatus Status { get; set; }
            public object Results { get; set; }

            public enum ReqStatus
            {
                OK,
                ApiFail,
                ApiBadUrl,
                ApiEmpty,
                KeyExpire,
                KeyFull,
                Fail,
            }
        }
    }
}
