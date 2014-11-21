using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceCenter.Framework
{
    public class NotificationManager
    {
        public void SendEmail(EmailContext context)
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
                
                message.From = new MailAddress(ConfigurationManager.AppSettings["SMTPUser"]);
                
                message.Subject = context.Subject;
                message.Body = context.Body;
                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"]))
                {
                    smtpClient.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                    smtpClient.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SMTPUser"],
                        ConfigurationManager.AppSettings["SMTPPassword"]);
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

        void smtpClient_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            
        }
    }
}