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
    public static class WebProcessNewClientOrder
    {
        [FunctionName("WebProcessNewClientOrder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/NewClientBusinessCardOrder")] HttpRequest req,
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

                var clientTable = await TableStorage<ClientInfoModel>.CreateTable(connectingString, "ClientInfo");

                //log.LogInformation("https://" + "linebackend.blob.core.windows.net/file-container");
                //log.LogInformation("Protocol: " + container["DefaultEndpointsProtocol"] + "; " +
                //                   "AccountName: " + container["AccountName"] + ";" +
                //                   "Suffix" + container["EndpointSuffix"]);
                //var containerPath = container["DefaultEndpointsProtocol"] + "://" + container["AccountName"] + ".blob." +
                //                    container["EndpointSuffix"] + "/file-container";

                /**
                 * json format
                 * {
                 *   key:... , (Cryptemail)
                 *   item:..., (only new)
                 *   fee:...,
                 *   paymentStatus:..., (0 = not pay, 1 = pay)
                 *   servicePlatform, (web, app)
                 *   path:..., (string, 0 = default, 1 = use client Unit)
                 *   clientUnit:..., (string, clientCompanyUnit)
                 *   remark:...
                 * }
                 **/

                string key = jsonData?.key == null ? string.Empty : Convert.ToString(jsonData?.key);
                string item =  jsonData?.item == null ? string.Empty : Convert.ToString(jsonData?.item);
                double? fee = jsonData?.fee == null ? null : Convert.ToDouble(jsonData?.fee);
                int? paymentStatus = jsonData?.paymentStatus == null ? null : Convert.ToInt16(jsonData?.paymentStatus);
                string servicePlatform = jsonData?.servicePlatform == null ? string.Empty : Convert.ToString(jsonData?.servicePlatform);
                string path = jsonData?.path == null ? string.Empty : Convert.ToString(jsonData?.path);
                string clientUnit = jsonData?.clientUnit == null ? string.Empty : Convert.ToString(jsonData?.clientUnit);
                string remark = jsonData?.remark == null ? string.Empty : Convert.ToString(jsonData?.remark);
                string errorMessage = string.Empty;

                var purchaseId = string.Empty;
                var dateTime = DateTime.Now;

                if (key == string.Empty || key.Length < 1)
                {
                    errorMessage = "未提供key";
                    log.LogError($"Error! {errorMessage}");
                    list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = errorMessage });
                    return new OkObjectResult(list);
                }


                if (item != "new")
                {
                    errorMessage = "item設定錯誤";
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

                var clientData = clientTable.FindAsync(email).Result;

                if (clientData == null)
                {
                    errorMessage = "未找到使用者";
                    log.LogError($"Error! {errorMessage}");
                    list.Add(new JsonReplyDataModel { Result = "Error", ResultMessage = errorMessage });
                    return new OkObjectResult(list);
                }

                var purchaseHistoryTable = await TableStorage<ClientOrderPurchasedHistoryModel>.CreateTable(connectingString, "ClientOrderPurchaseHistory");
                var clientOrder = purchaseHistoryTable.FindAsync(decryptMail).Result.OrderByDescending(x => x.ServiceItemNumber);
                
                int itemNumber = 0;
                if (clientOrder != null)
                    itemNumber = clientOrder.Count();

                var newClientOrder = new ClientOrderPurchasedHistoryModel(decryptMail, Convert.ToString(itemNumber));

                newClientOrder.ServiceOrderDate = dateTime;  // First Add Time
                                                             // Add Purchase
                                                             // Product purchase Id
                var servicePurchase = dateTime.ToString("HHmmssfff") +
                                      email +
                                      dateTime.ToString("yyyyMMdd") + itemNumber.ToString();
                purchaseId = GenerateBase64String(servicePurchase);

                newClientOrder.PurchasedId = purchaseId;

                if (fee != null)
                    newClientOrder.ServiceFee = (double)fee;

                if (paymentStatus != null)
                {
                    newClientOrder.ServicePaymentStatus = (int)paymentStatus;

                    if (paymentStatus > 0)
                        newClientOrder.ServicePaymentDate = dateTime;
                }

                if (clientUnit != string.Empty)
                    newClientOrder.ClientUnit = clientUnit;

                if (servicePlatform != string.Empty)
                    newClientOrder.ServicePlatform = servicePlatform;
                else
                    newClientOrder.ServicePlatform = "Web";

                if (path != string.Empty && path == "1")
                    newClientOrder.FilePath = filePath[path] + "/" + clientUnit;
                else if (path == string.Empty || (path != string.Empty && path == "0"))
                    newClientOrder.FilePath = filePath["0"];

                var emailSuffix = decryptMail.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                newClientOrder.FileName = emailSuffix[0] + "_" + Convert.ToString(itemNumber) + ".txt";

                if (remark != string.Empty)
                    newClientOrder.Remark = remark;

                await purchaseHistoryTable.AddAsync(newClientOrder);

                list.Add(new JsonReplyDataModel { Result = "Add Success", ResultMessage = "新訂單已加入" });
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
