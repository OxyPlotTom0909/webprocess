using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebProcess.Models
{
    public class ClientOrderPurchasedHistoryModel : TableEntity
    {
        /// <summary>
        /// Line User ID, it is user mail
        /// </summary>
        [IgnoreProperty]
        public string ClientEmail { get { return PartitionKey; } set { PartitionKey = value; } }

        /// <summary>
        /// This service item number
        /// </summary>
        [IgnoreProperty]
        public string ServiceItemNumber { get { return RowKey; } set { RowKey = value; } }

        /// <summary>
        /// Use in Client Unit to save in path
        /// </summary>
        public string ClientUnit { get; set; }

        /// <summary>
        /// Order Id, Create by system
        /// </summary>
        public string PurchasedId { get; set; }

        /// <summary>
        /// Purchased service order date, Create by system
        /// </summary>
        public DateTime ServiceOrderDate { get; set; }

        /// <summary>
        /// Purchased service fee
        /// </summary>
        public double ServiceFee { get; set; }

        /// <summary>
        /// Already payment or not, 3 status. -1 = initial, 0 = not pay, 1 = pay
        /// </summary>
        public int ServicePaymentStatus { get; set; }

        /// <summary>
        /// Recieved service payment date
        /// </summary>
        public DateTime ServicePaymentDate { get; set; } 

        /// <summary>
        /// For this service, message will send with which platform
        /// </summary>
        public string ServicePlatform { get; set; }

        /// <summary>
        /// File path, Setting in Environment
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Record other condition for this purchase
        /// </summary>
        public string Remark { get; set; }

        public ClientOrderPurchasedHistoryModel(string ClientEmail, string ServiceItemNumber)
            : base(ClientEmail, ServiceItemNumber)
        {
            ClientUnit = string.Empty;
            PurchasedId = string.Empty;
            ServiceOrderDate = DateTime.Now;
            ServiceFee = 0;
            ServicePaymentStatus = -1;
            ServicePaymentDate = DateTime.Now;
            ServicePlatform = string.Empty;
            FilePath = string.Empty;
            FileName = string.Empty;
           
            Remark = string.Empty;
        }

        public ClientOrderPurchasedHistoryModel()
        {
        }
    }
}
