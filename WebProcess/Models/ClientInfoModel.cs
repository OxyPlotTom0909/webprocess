using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebProcess.Models
{
    public class ClientInfoModel : TableEntity
    {
        /// <summary>
        /// Line User ID, it is user mail
        /// </summary>
        [IgnoreProperty]
        public string ClientMail { get { return PartitionKey; } set { PartitionKey = value; } }

        /// <summary>
        /// User Name
        /// </summary>
        [IgnoreProperty]
        public string ClientName { get { return RowKey; } set { RowKey = value; } }

        /// <summary>
        /// Line Name of Client, Not necessary
        /// </summary>
        public string ClientLineName { get; set; }

        /// <summary>
        /// First Add client time, system create automatically
        /// </summary>
        public DateTime AddServiceTime { get; set; }


        public ClientInfoModel(string ClientMail, string ClientName)
            : base(ClientMail, ClientName)
        { 
            AddServiceTime = DateTime.Now;
        }

        public ClientInfoModel()
        {
        }
    }
}
