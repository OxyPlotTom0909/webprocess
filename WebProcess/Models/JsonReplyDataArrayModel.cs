using System.Collections.Generic;

namespace WebProcess.Models
{
    public class JsonReplyDataArrayModel
    {
        public string Result { get; set; }
        public List<string> ResultMessage { get; set; }

        public JsonReplyDataArrayModel()
        {
        }
    }
}
