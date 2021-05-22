using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebProcess.Models
{
    public class AgentServiceDataModel : TableEntity
    {
        [IgnoreProperty]
        public string DomainName { get { return PartitionKey; } set { PartitionKey = value; } }
        [IgnoreProperty]
        public string ServiceName { get { return RowKey; } set { RowKey = value; } }

        public string ApiName { get; set; }

        public string Code { get; set; }

        public AgentServiceDataModel(string DomainName, string ServiceName)
            : base(DomainName, ServiceName)
        {
            ApiName = string.Empty;
            Code =  string.Empty;
        }

        public AgentServiceDataModel()
        {
        }
    }
}
