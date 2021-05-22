using Microsoft.WindowsAzure.Storage.Table;

namespace WebProcess.Models
{
    public class RichMenuState : TableEntity
    {
        [IgnoreProperty]
        public string ProviderID { get { return PartitionKey; } set { PartitionKey = value; } }

        [IgnoreProperty]
        public string RichMenuName { get { return RowKey; } set { RowKey = value; } }

        public string RichMenuID { get; set; }

        public RichMenuState() { }

    }
}
