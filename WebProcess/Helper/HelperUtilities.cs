using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using WebProcess.CloudStorage;
using WebProcess.Models;

namespace WebProcess.Helper
{
    public class HelperUtilities
    {
        /// <summary>
        /// Find table when the column value correspond the given data
        /// </summary>
        public enum SelectCondition { Equal, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, NotEqual };

        /// <summary>
        /// UserStatus - User status 
        /// </summary>
        public enum UserStatus { Follow, Refollow, Unfollow, Active, suspended };

        /// <summary>
        /// UserType - User Type in Line
        /// </summary>
        public enum UserType { User, Room, Group };

        /// <summary>
        /// UserClass - User Class for admin
        /// </summary>
        public enum UserClass { NewClient, AccountClient, DeletedClient };

        public static async Task<string> RespondFileStreamToString(Stream contentStream)
        {
            string strStream = string.Empty;

            const int bufferSize = 1024;
            var bytes = new byte[bufferSize];
            var chars = new char[System.Text.Encoding.UTF8.GetMaxCharCount(bufferSize)];
            var decoder = System.Text.Encoding.UTF8.GetDecoder();
            // We don't know how long the result will be in chars, but one byte per char is a
            // reasonable first approximation. This will expand as necessary.
            var stringBuilder = new System.Text.StringBuilder();
            int totalReadBytes = 0;

            while (totalReadBytes <= contentStream.Length)
            {
                int readBytes = await contentStream.ReadAsync(bytes, 0, bufferSize);

                // We reached the end of the stream
                if (readBytes == 0)
                    break;

                totalReadBytes += readBytes;

                int readChars = decoder.GetChars(bytes, 0, readBytes, chars, 0);

                strStream += new String(chars);
            }

            return strStream;
        }

        /// <summary>
        /// Change string of Term discription to days
        /// </summary>
        /// <param name="strTerm"></param>
        /// <returns></returns>
        public static int TermToDays(string strTerm)
        {
            int days = 1;
            strTerm = strTerm.ToLower();
            int strlength = -1;

            if (strTerm.Contains("day"))
            {
                strlength = "day".Length;
            }
            else if (strTerm.Contains("month"))
            {
                strlength = "month".Length;
                days *= 30;
            }
            else if (strTerm.Contains("year"))
            {
                strlength = "year".Length;
                days *= 365;
            }

            var terms = Convert.ToDouble(strTerm.Substring(0, strTerm.Length - strlength).TrimEnd());

            return Convert.ToInt32(terms * days);
        }


        /// <summary>
        /// Generate PurchaseId for trade software
        /// </summary>
        /// <param name="connectingString"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static async Task<string> GeneratePurchaseId(string connectingString, string date)
        {
            string purchaseId = string.Empty;
            string recordTitle = "PurchaseId";

            var recordTable = await TableStorage<SoftwareRecordModel>.CreateTable(connectingString, "SoftwareRecord");
            var serviceList = await TableStorage<UserServiceListModel>.CreateTable(connectingString, "ServiceList");
            var record = recordTable.FindAsync(recordTitle, date).Result;
            var recordSuffix = serviceList.FindAsync("SystemTradeSuffix").Result.FirstOrDefault();

            if (record == null)
            {
                var newRecord = new SoftwareRecordModel(recordTitle, date);
                newRecord.ContentString1 = recordSuffix.ServiceName;
                newRecord.ContentInt1++;
                purchaseId = $"{date}{newRecord.ContentInt1}{newRecord.ContentString1}";
            }
            else
            {
                purchaseId = $"{date}{record.ContentInt1}{record.ContentString1}";
            }

            return purchaseId;
        }

        public static string GenerateBase64String(string strSource)
        {
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(strSource);

            string result = Convert.ToBase64String(bytes);

            return result;
        }

        /// <summary>
        /// Call Restful API
        /// </summary>
        /// <param name="api"></param>
        /// <param name="contentType"></param>
        /// <param name="header"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static async Task<string> CallAPI(string api,
                                                 string contentType,
                                                 Dictionary<string, string> header,
                                                 Dictionary<string, string> body)
        {
            string returnMessage = "Error result";
            HttpResponseMessage response;

            using (var _client = new HttpClient())
            {
                //var contentHeader = _client.DefaultRequestHeaders.Contains("Content-Type");
                //if (!contentHeader)
                //    _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                //_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //_client.DefaultRequestHeaders.Add("Content-Type", contentType);

                foreach (var item in header)
                {
                    var hadHeader = _client.DefaultRequestHeaders.Contains(item.Key);
                    if (!hadHeader)
                        _client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }

                var fromdata = new FormUrlEncodedContent(body);
                //HttpContent content = new StringContent(bady, Encoding.UTF8, contentType);

                response = await _client.PostAsync(api, fromdata).ConfigureAwait(false);
                var returnString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                returnMessage = (returnString.Length > 0) ? returnString : string.Empty;
            }
            
            return returnMessage;
        }
    }
}
