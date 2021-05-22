using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebProcess.Models
{
    public class UserServicePurchasedHistoryModel : TableEntity
    {
        /// <summary>
        /// Line user ID
        /// </summary>
        [IgnoreProperty]
        public string UserId { get { return PartitionKey; } set { PartitionKey = value; } }

        /// <summary>
        /// User purchased ID
        /// </summary>
        [IgnoreProperty]
        public string PurchasedID { get { return RowKey; } set { RowKey = value; } }

        /// <summary>
        /// User purchased service
        /// </summary>
        public string Service { get; set; } 

        /// <summary>
        /// Coordinated product of purchased service
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// Purchased service start date
        /// </summary>
        public DateTime ServiceStartDate { get; set; }

        /// <summary>
        /// Purchased service expired date
        /// </summary>
        public DateTime ServiceExpiredDate { get; set; }

        /// <summary>
        /// Purchased service term
        /// </summary>
        public string ServiceTerm { get; set; }

        /// <summary>
        /// Purchased service status for now
        /// </summary>
        public string ServiceStatus { get; set; }

        /// <summary>
        /// Purchased service fee
        /// </summary>
        public double ServiceFee { get; set; }

        /// <summary>
        /// For this service, message will send with which platform
        /// </summary>
        public string ServicePlatform { get; set; }

        /// <summary>
        /// Record other condition for this purchase
        /// </summary>
        public string Remark { get; set; }

        public UserServicePurchasedHistoryModel(string userId, string purchasedId)
            : base(userId, purchasedId)
        {
            Service = string.Empty;
            Product = string.Empty;
            ServiceStartDate = DateTime.Now;
            ServiceExpiredDate = DateTime.Now;
            ServiceTerm = string.Empty;
            ServiceStatus = string.Empty;
            ServiceFee = double.MinValue;
            ServicePlatform = string.Empty;
        }

        public UserServicePurchasedHistoryModel()
        {
        }
    }
}
