using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Company.Function
{

    public static class httpABIExpense
    {
        public static HttpClient httpClient = new HttpClient();

        [FunctionName("httpABIExpense")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var reqString = await new StreamReader(req.Body).ReadToEndAsync(); //Read HTTP request to a string. Where 'req' is the request from HTTP Trigger of Azure Function. 

            var reqJson = JObject.Parse(reqString); //Convert HTTP request string to a JSON format for procceing easily. 

            var reqUri = reqJson["Uri"].ToString(); //Read RestAPI URI from HTTP request. 
            
            var reqBodyJson = reqJson["Body"]; //Read RestAPI required body from HTTP request.

            var reqPage = int.Parse(reqBodyJson["page"].ToString()); //Extract 'page' from HTTP request body for next pagination rule.

            var reqAuthorizationToken = req.Headers["Authorization"].ToString(); //Get Authorization token from HTTP Header.

            httpClient.DefaultRequestHeaders.Clear(); //In order to inject authorization HTTP header.
            httpClient.DefaultRequestHeaders.Add("Authorization", reqAuthorizationToken);

            using var httpResponse = await httpClient.PostAsJsonAsync(reqUri, reqBodyJson); //Get response after send HTTP request to the Restful API

            httpResponse.EnsureSuccessStatusCode();

            var responseBody = await httpResponse.Content.ReadAsStringAsync(); //Read response.

            var responseJson = JObject.Parse(responseBody); //Convert response to JSON format.

            var newPage = reqPage + 1; //Auto increment page number.

            if (responseJson["data"].First != null) //Make sure that there is any data from restful API. Otherwise return HTTP 204 for ADF Pagination rules. 
            {
                responseJson["Page"] = newPage; //Inject newPage number to the response of Azure Function

                var response = new OkObjectResult(responseJson);

                response.ContentTypes.Add("application/json"); //Format response to application/json for HTTP transaction.

                return response;
            }

            return null; //Return HTTP 204 by default.

        }
    }
}
