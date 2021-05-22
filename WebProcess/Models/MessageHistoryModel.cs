using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebProcess.Models
{
    public class MessageHistoryModel : TableEntity
    {
        /// <summary>
        /// Which platform need to send message.
        /// </summary>
        [IgnoreProperty]
        public string TradePlatform { get { return PartitionKey; } set { PartitionKey = value; } }

        /// <summary>
        /// The message record ID. Correspond with SendMsgToClientHistoryModel
        /// </summary>
        [IgnoreProperty]
        public string MessageHistoryRecordId { get { return RowKey; } set { RowKey = value; } }

        /// <summary>
        /// Send message date.
        /// </summary>
        public string MsgDate { get; set; }

        /// <summary>
        /// Send message time.
        /// </summary>
        public string MsgTime { get; set; }

        /// <summary>
        /// The message sent.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// How many will receive message.
        /// </summary>
        public int SendPeople { get; set; }

        public MessageHistoryModel(string platform, string recordId)
            : base(platform, recordId)
        {
            MsgDate = string.Empty;
            MsgTime = string.Empty;
            Message = string.Empty;
            SendPeople = -1;
        }

        public MessageHistoryModel()
        {
        }
    }
}
