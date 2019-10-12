using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace podcastspfx
{
    public static class Hello
    {
        [FunctionName("Hello")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var objs = new string[] { "Hello", "Bounjour", "Nihao", "Gutentag", "Hola", DateTime.Now.ToLongTimeString() };
            return (ActionResult)new ObjectResult(JsonConvert.SerializeObject(objs));
        }
    }
}
