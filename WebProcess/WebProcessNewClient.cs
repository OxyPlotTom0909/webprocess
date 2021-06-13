using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using WebProcess.CloudStorage;
using static WebProcess.Helper.HelperUtilities;
using WebProcess.Models;

namespace WebProcess
{
    public static class WebProcessNewClient
    {

        [FunctionName("WebProcessNewClient")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/NewClient")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var list = new List<JsonReplyDataModel>();

            string requestBody = await new StreamReader(req.Body, Encoding.UTF8).ReadToEndAsync();

            dynamic jsonData = JsonConvert.DeserializeObject(requestBody);

            try
            {
                var connectingString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                var clientTable = await TableStorage<ClientInfoModel>.CreateTable(connectingString, "ClientInfo");

                /**
                 * Step
                 * 1. New Client
                 * 2. Get Email key
                 * 3. New Client Order
                 * 4. Update Client Order (Or not)
                 * 5. Get Flex Message
                 */

                /**
                 * json format
                 * {
                 *   email:... ,
                 *   name(real name):...,
                 *   lineName:...,
                 * }
                 **/
                string email = Convert.ToString(jsonData?.email);
                string name = Convert.ToString(jsonData?.name);
                string lineName = Convert.ToString(jsonData?.lineName);

                var clientData = clientTable.FindAsync(email, name).Result;

                if (clientData == null)
                {
                    clientData = new ClientInfoModel(email, name);
                    clientData.ClientLineName = lineName;
                    await clientTable.AddAsync(clientData);

                    list.Add(new JsonReplyDataModel { Result = "Add Success", ResultMessage = "使用者新增完成" });
                }
                else
                {
                    list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = "已有使用者" });
                }

            }
            catch (Exception ex)
            {
                list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = ex.ToString() });
                log.LogError($"Error: {ex.ToString()}");
            }

            return new OkObjectResult(list);
        }
    }
}
