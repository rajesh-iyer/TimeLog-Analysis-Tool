
using System;
using System.Net.Mail;
using System.Threading;
using System.Linq;

namespace ServiceCenter.Framework.Notification
{
    public class EmailNotificationListener : NotificationListener
    {
        public string SMTPServer { get; set; }
        public string SMTPPort { get; set; }
        public string SMTPUser { get; set; }
        public string SMTPPassword { get; set; }

        public override void Notify(NotificationContext context)
        {
            SendEmail(context as EmailContext);
        }

        protected void SendEmail(EmailContext context)
        {
            using (MailMessage message = new MailMessage())
            {
                message.To.Add(context.To);

                if (string.IsNullOrEmpty(context.CC) == false)
                {
                    var list = context.CC.Split(',');
                    foreach (string s in list)
                    {
                        message.CC.Add(s);
                    }
                }
                if (string.IsNullOrEmpty(context.BCC) == false) message.Bcc.Add(context.BCC);

                message.From = new MailAddress(SMTPUser);

                message.Subject = context.Subject;
                message.Body = context.Body;
                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient(SMTPServer))
                {
                    smtpClient.Port = Convert.ToInt32(SMTPPort);
                    smtpClient.Credentials = new System.Net.NetworkCredential(SMTPUser, SMTPPassword);
                    //smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    try
                    {
                        smtpClient.Send(message);
                    }
                    catch (SmtpException)
                    {
                        Thread.Sleep(60 * 1000);
                        smtpClient.Send(message);
                    }
                }
            }
        }
    }
}
