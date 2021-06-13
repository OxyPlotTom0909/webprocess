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
using static WebProcess.Helper.AESCryptography;
using static WebProcess.Helper.HelperUtilities;
using WebProcess.Models;

namespace WebProcess
{
    public static class WebProcessUpdateClientOrder
    {
        [FunctionName("WebProcessUpdateClientOrder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/UpdateClientOrder")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var list = new List<JsonReplyDataModel>();

            string requestBody = await new StreamReader(req.Body, Encoding.UTF8).ReadToEndAsync();

            dynamic jsonData = JsonConvert.DeserializeObject(requestBody);

            try
            {

                var connectingString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                //var container = Environment.GetEnvironmentVariable("AzureWebJobsStorage").
                //                     Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).
                //                     ToDictionary(x => x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[0],
                //                                  x => x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                var filePath = Environment.GetEnvironmentVariable("FilePath").
                                     Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).
                                     ToDictionary(x => x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[0],
                                                  x => x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1]);

                //log.LogInformation("https://" + "linebackend.blob.core.windows.net/file-container");
                //log.LogInformation("Protocol: " + container["DefaultEndpointsProtocol"] + "; " +
                //                   "AccountName: " + container["AccountName"] + ";" +
                //                   "Suffix" + container["EndpointSuffix"]);
                //var containerPath = container["DefaultEndpointsProtocol"] + "://" + container["AccountName"] + ".blob." +
                //                    container["EndpointSuffix"] + "/file-container";

                /**
                 * json format
                 * {
                 *   key=... , (email) --> Necessary
                 *   item=..., (item number) -> Necessary
                 *   feeUpdate=..., (true, false)
                 *   fee:...,(feeUpdate=true)
                 *   paymentStatus:..., (0 = not pay, 1 = pay)
                 *   path:..., (string, 0 = default, 1 = use client Unit)
                 *   clientUnit:..., (string, clientCompanyUnit)
                 *   remark:...
                 * }
                 **/

                string key = jsonData?.key == null ? string.Empty : Convert.ToString(jsonData?.key);
                string item = jsonData?.item == null ? string.Empty : Convert.ToString(jsonData?.item);
                bool feeUpdate = jsonData?.feeUpdate == null ? false : Convert.ToBoolean(jsonData?.feeUpdate);
                double? fee = jsonData?.fee == null ? null : Convert.ToDouble(jsonData?.fee);
                int? paymentStatus = jsonData?.paymentStatus == null ? null : Convert.ToInt16(jsonData?.paymentStatus);
                string servicePlatform = jsonData?.servicePlatform == null ? string.Empty : Convert.ToString(jsonData?.servicePlatform);
                string path = jsonData?.path == null ? string.Empty : Convert.ToString(jsonData?.path);
                string clientUnit = jsonData?.clientUnit == null ? string.Empty : Convert.ToString(jsonData?.clientUnit);
                string remark = jsonData?.remark == null ? string.Empty : Convert.ToString(jsonData?.remark);

                log.LogInformation($"clientUnit: {clientUnit}, email: {key}, item: {item}" +
                                   $"feeUpdate: {feeUpdate}, fee: {fee}, paymentStatus: {paymentStatus}" +
                                   $"servicePlatfomr: {servicePlatform}, path: {path}, remark: {remark}");
                string errorMessage = string.Empty;
                var dateTime = DateTime.Now;

                if (key == string.Empty || key.Length < 1)
                {
                    errorMessage = "未提供key";
                    log.LogError($"Error! {errorMessage}");
                    list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = errorMessage });
                    return new OkObjectResult(list);
                }

                if (item == string.Empty)
                {
                    errorMessage = "未提供item號碼";
                    log.LogError($"Error! {errorMessage}");
                    list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = errorMessage });
                    return new OkObjectResult(list);
                }

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
                var clientOrder = purchaseHistoryTable.FindAsync(decryptMail, item).Result;
                
                if (clientOrder == null)
                {
                    errorMessage = "未找到訂單";
                    log.LogError($"Error! {errorMessage}");
                    list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = errorMessage });
                    return new OkObjectResult(list);
                }

                if (feeUpdate && fee != null)
                    clientOrder.ServiceFee = (double)fee;

                if (paymentStatus != null)
                {
                    if (paymentStatus > 0)
                    {
                        clientOrder.ServicePaymentStatus = (int)paymentStatus;
                        clientOrder.ServicePaymentDate = dateTime;
                    }
                }

                if (servicePlatform != string.Empty)
                    clientOrder.ServicePlatform = servicePlatform;

                if (path != string.Empty && path == "1" && clientUnit != string.Empty)
                    clientOrder.FilePath = filePath[path] + "/" + clientUnit;
                else if (path != string.Empty && path == "0")
                    clientOrder.FilePath = filePath[path];

                if (clientUnit != string.Empty)
                    clientOrder.ClientUnit = clientUnit;

                if (remark != string.Empty)
                    clientOrder.Remark = remark;

                await purchaseHistoryTable.AddAsync(clientOrder);

                errorMessage = "訂單已修改";
                log.LogInformation($"Message! {errorMessage}");
                list.Add(new JsonReplyDataModel { Result = "Add Success", ResultMessage = errorMessage });
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
