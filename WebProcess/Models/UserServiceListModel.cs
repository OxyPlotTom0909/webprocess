using Microsoft.WindowsAzure.Storage.Table;

namespace WebProcess.Models
{
    public class UserServiceListModel : TableEntity
    {
        [IgnoreProperty]
        public string ServiceGroup { get { return PartitionKey; } set { PartitionKey = value; } }
        [IgnoreProperty]
        public string ServiceName { get { return RowKey; } set { RowKey = value; } }

        public string ServiceRemark1 { get; set; }

        public string ServiceRemark2 { get; set; }

        public string ServiceRemark3 { get; set; }

        public string ServiceRemark4 { get; set; }

        public string ServiceRemark5 { get; set; }

        public UserServiceListModel(string group, string name)
            : base(group, name)
        {
            ServiceRemark1 = string.Empty;
            ServiceRemark2 = string.Empty;
            ServiceRemark3 = string.Empty;
            ServiceRemark4 = string.Empty;
            ServiceRemark5 = string.Empty;
        }

        public UserServiceListModel()
        {
        }
    }
}
