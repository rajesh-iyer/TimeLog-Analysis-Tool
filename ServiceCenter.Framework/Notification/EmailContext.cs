
namespace ServiceCenter.Framework.Notification
{
    public class EmailContext : NotificationContext
    {        
        public string To { get; set; }
        public string From { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
