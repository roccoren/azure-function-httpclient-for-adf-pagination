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
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Company.Function
{
    public class Expense
    {
        public string language { get; set; }
        public int page { get; set; }
        public int size { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string useLastModifiedDate { get; set; }
    }


    public static class httpABIExpense
    {
        public static HttpClient httpClient = new HttpClient();

        [FunctionName("httpABIExpense")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            log.LogInformation(string.Format("Dump request:\n {0}", req.ToString()));
            // parse query parameter


            // var reqPage = req.Query["page"];
            var reqPage = int.Parse(req.Headers["Page"]);

            string uri = "http://stage.huilianyi.com/report/api/open/report/searchReimbursement";

            var postExpense = new Expense
            {
                language = "zh_cn",
                page = reqPage,
                size = 20,
                startDate = "2020-01-01",
                endDate = "2020-12-31",
                useLastModifiedDate = "Y"
            };

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", String.Format("Bearer ac9caae1-785c-43e3-a18a-4c08a96930f9"));

            using var httpResponse = await httpClient.PostAsJsonAsync(uri, postExpense);

            httpResponse.EnsureSuccessStatusCode();

            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            //var responseObj = JsonSerializer.Deserialize(responseBody);
            var responseJson = JObject.Parse(responseBody);

            var newPage = reqPage + 1;

            if (responseJson["data"].First!=null)
            {
                responseJson["Page"] = newPage;
            }

            var response = new OkObjectResult(responseJson);
            response.ContentTypes.Add("application/json");

            return response;
        }
    }
}
