using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebProcess.Models
{
    public class SendMsgToClientHistoryModel : TableEntity
    {
        /// <summary>
        /// Send Message Date.
        /// </summary>
        [IgnoreProperty]
        public string Date { get { return PartitionKey; } set { PartitionKey = value; } }

        /// <summary>
        /// Send Message Time.
        /// </summary>
        [IgnoreProperty]
        public string Time { get { return RowKey; } set { RowKey = value; } }

        /// <summary>
        /// Client ID.
        /// </summary>
        public string ClientID { get; set; }

        /// <summary>
        /// Record unique client id
        /// </summary>
        public string SMSClientID { get; set; }

        /// <summary>
        /// MsgID after send message.
        /// </summary>
        public string MsgID { get; set; }

        /// <summary>
        /// Record message history Id.
        /// </summary>
        public string MessageHistoryRecordId { get; set; }

        /// <summary>
        /// The Platform need to send.
        /// </summary>
        public string MsgPlatform { get; set; }

        /// <summary>
        /// The software send this message.
        /// </summary>
        public string SendMsgSoftware { get; set; } 

        /// <summary>
        /// Record send message result. -1 is not define. 0 is sent success.
        /// </summary>
        public int SendResult { get; set; }


        public SendMsgToClientHistoryModel(string sDate, string sTime)
            : base(sDate, sTime)
        {
            ClientID = string.Empty;
            SMSClientID = string.Empty;
            MsgID = string.Empty;
            MsgPlatform = string.Empty;
            MessageHistoryRecordId = string.Empty;
            SendMsgSoftware = string.Empty;
            SendResult = -1;
        }

        public SendMsgToClientHistoryModel()
        {
        }
    }
}
