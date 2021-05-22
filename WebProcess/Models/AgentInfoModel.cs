using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebProcess.Models
{
    public class AgentInfoModel : TableEntity
    {
        [IgnoreProperty]
        public string AgentMail { get { return PartitionKey; } set { PartitionKey = value; } }
        [IgnoreProperty]
        public string AgentName { get { return RowKey; } set { RowKey = value; } }

        public string AgentLineName { get; set; }

        public DateTime AddServiceTime { get; set; } 

        public DateTime ServiceStartDate { get; set; }

        public DateTime ServiceExpiredDate { get; set; }

        public string ServiceTerm { get; set; }

        public string ServiceStatus { get; set; }

        public string PurchasedID { get; set; }

        public int RemainedDays { get; set; }

        public string DomainName { get; set; }

        public string TestDomainName { get; set; }

        public AgentInfoModel(string AgentMail, string AgentName)
            : base(AgentMail, AgentName)
        {
            AgentLineName = string.Empty;
            AddServiceTime = DateTime.Now;
            ServiceStartDate = DateTime.Now;
            ServiceExpiredDate = DateTime.Now;
            ServiceTerm = string.Empty;
            ServiceStatus = string.Empty;
            PurchasedID = string.Empty;
            RemainedDays = int.MinValue;
            DomainName = string.Empty;
            TestDomainName = string.Empty;
        }

        public AgentInfoModel()
        {
        }
    }
}
