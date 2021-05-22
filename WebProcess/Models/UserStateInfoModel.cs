using Microsoft.WindowsAzure.Storage.Table;

namespace WebProcess.Models
{
    public class UserStateInfoModel : TableEntity
    {
        public string IdType { get; set; }

        public string UserName { get; set; }

        public string UserStatus { get; set; }

        public string FollowTime { get; set; }

        public string RefollowTime { get; set; }

        public string UnFollowTime { get; set; }

        public string LastSpeakTime { get; set; }

        public UserStateInfoModel(string adminId, string userId)
            : base(adminId, userId)
        {
            IdType = string.Empty;
            UserName = string.Empty;
            UserStatus = string.Empty;
            FollowTime = string.Empty;
            UnFollowTime = string.Empty;
            LastSpeakTime = string.Empty;
        }

        public UserStateInfoModel() { }
    }
}
