using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Serializers
{
    public class LogInHistoryDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string? IpAdress { get; set; }
        public string? LogInType { get; set; }
        public DateTime LogInTime { get; set; }
        public string? UserAgent { get; set; }

        public LogInHistoryDto(LogInHistory logInHistory)
        {
            Id = logInHistory.Id.ToString();
            UserId = logInHistory.UserId.ToString();
            IpAdress = logInHistory.IpAdress;
            LogInType = logInHistory.LogInType;
            LogInTime = logInHistory.LogInTime;
            UserAgent = logInHistory.UserAgent;
        }
    }
}
