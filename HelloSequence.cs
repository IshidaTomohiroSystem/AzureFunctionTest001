using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Company.Function
{
    public static class HelloSequence
    {
        [FunctionName("HelloSequence")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            int num = context.GetInput<int>();
            log.LogInformation("num:" + num);
            int result1 = await context.CallActivityAsync<int>("PlusTwo", num);
            log.LogInformation("result:" + result1);
        }

        [FunctionName("PlusTwo")]
        public static int PlusTwo([ActivityTrigger] int num, ILogger log)
        {
            return num + 2;
        }

        [FunctionName("HelloSequence_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string queryString = req.RequestUri.Query;
            NameValueCollection query = HttpUtility.ParseQueryString(queryString, Encoding.UTF8);
            string num = query["num"];
            log.LogInformation("query_num:" + num);

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("HelloSequence", num);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}