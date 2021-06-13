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
using static WebProcess.Helper.HelperUtilities;
using WebProcess.Models;

namespace WebProcess
{
    public static class WebProcessGetFlexMessage
    {
        [FunctionName("WebProcessGetFlexMessage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/GetFlexMessage")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var list = new List<JsonReplyDataModel>();

            /**
             * querystring: 
             * key=...., (email & key consolidate string)
             * fileItem=... (item=0, 1, 2...) 
             **/

            var content = req.QueryString.Value.Remove(req.QueryString.Value.IndexOf('?'), 1).
                                      Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries).
                                      ToDictionary(x => x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[0],
                                                   x => x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1]);

            try
            {
                var connectingString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                
                // split email and key, and AES decrypt the mail
                var key = content["key"];
                string errorMessage = string.Empty;

                if (key.Length < 10)
                {
                    errorMessage = "key值提供錯誤";
                    log.LogError($"Error! {errorMessage}");
                    list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = errorMessage });
                    return new OkObjectResult(list);
                }

                var email = key.Substring(0, key.Length - 16);
                var decryptKey = key.Substring(key.Length - 16, 16);
                var decryptMail = AESDecrypt(email, decryptKey);
                var purchaseHistoryTable = await TableStorage<ClientOrderPurchasedHistoryModel>.CreateTable(connectingString, "ClientOrderPurchaseHistory");
                var clientOrder = purchaseHistoryTable.FindAsync(decryptMail, content["fileItem"]).Result;
                
                if (clientOrder == null)
                {
                    errorMessage = "未找到訂單";
                    log.LogError($"Error! {errorMessage}");
                    list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = errorMessage });
                    return new OkObjectResult(list);
                }

                if (clientOrder.ServicePaymentStatus > 0)
                {
                    var blobStorage = await BlobStorage.CreateAsync(connectingString, "file-container");
                    var jsonpath = blobStorage.GetContainerUri(clientOrder.FilePath, clientOrder.FileName);
                    var jsonbub = await JsonTextFromUriFile(jsonpath.ToString());

                    list.Add(new JsonReplyDataModel { Result = "OK", ResultMessage = jsonbub });
                }
                else
                {
                    errorMessage = "未完成付費流程";
                    log.LogError($"Error! {errorMessage}");
                    list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = errorMessage });
                }

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
