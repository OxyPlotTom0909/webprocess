using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using WebProcess.CloudStorage;
using static WebProcess.Helper.AESCryptography;
using WebProcess.Models;

namespace WebProcess
{
    public static class WebProcessGetEmailKey
    {
        [FunctionName("WebProcessGetEmailKey")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/GetEmailKey")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var content = req.QueryString.Value.Remove(req.QueryString.Value.IndexOf('?'), 1).
                                     Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries).
                                     ToDictionary(x => x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[0],
                                                  x => x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1]);
            var list = new List<JsonReplyDataModel>();

            try
            {
                var connectingString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                var agentTable = await TableStorage<AgentInfoModel>.CreateTable(connectingString, "AgentInfo");
                var agentInfo = agentTable.FindAsync(content["email"]).Result.FirstOrDefault();

                if (agentInfo != null)
                {
                    var key = GetRandomString(16, false);
                    var decryptMail = AESEncryptString(content["email"], key);
                    var keyResult = decryptMail + key;

                    list.Add(new JsonReplyDataModel { Result = "Success", ResultMessage = keyResult });
                }
                else
                    list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = "No Register" });
            }
            catch (Exception ex)
            {
                list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = ex.ToString() });
            }

            var result = JsonConvert.SerializeObject(list);

            return new OkObjectResult(result);
        }
    }
}
