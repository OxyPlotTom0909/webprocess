using System;

using Microsoft.WindowsAzure.Storage.Table;

namespace WebProcess.Models
{
    public class SoftwareRecordModel : TableEntity
    {
        [IgnoreProperty]
        public string RecordTitle { get { return PartitionKey; } set { PartitionKey = value; } }
        [IgnoreProperty]
        public string Date { get { return RowKey; } set { RowKey = value; } }

        public string ContentString1 { get; set; }

        public string ContentString2 { get; set; }

        public double ContentDouble1 { get; set; }

        public double ContentDouble2 { get; set; }

        public long ContentLong1 { get; set; }

        public long ContentLong2 { get; set; }

        public int ContentInt1 { get; set; }

        public int ContentInt2 { get; set; }

        public DateTime ContentDateTime1 { get; set; }

        public DateTime ContentDateTime2 { get; set; }

        public bool ContentBool1 { get; set; }

        public bool ContentBool2 { get; set; }


        public SoftwareRecordModel(string sRecordTitle, string sDate)
            : base(sRecordTitle, sDate)
        {
            ContentString1 = string.Empty;
            ContentString2 = string.Empty;
            ContentDouble1 = double.MinValue;
            ContentDouble2 = double.MinValue;
            ContentLong1 = long.MinValue;
            ContentLong2 = long.MinValue;
            ContentInt1 = 0;
            ContentInt2 = -1;
            ContentBool1 = false;
            ContentBool2 = false;
        }

        public SoftwareRecordModel()
        {
        }
    }
}
