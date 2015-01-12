using ServiceCenter.Framework.Notification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager
{
    public class TimeLogEmailNotificationListener : EmailNotificationListener
    {
        public override void Notify(NotificationContext context)
        {
            var Context = context as TimeLogNotificationContext;

            if (Context != null)
            {
                var managerEmailList = Context.Data.ServiceContext.TeamProfiles.Where(p => p.Role == "Manager").Select(p => p.Email).ToList();
                var ccEmails = string.Empty;
                if (managerEmailList != null && managerEmailList.Count > 0)
                {
                    ccEmails = string.Join(",", managerEmailList);
                }

                var devEmail = Context.Data.TeamMember.Email;
                var formatter = Context.DataFormatter as TimeLogDataFormatter;

                if (formatter != null)
                {
                    base.Notify(new EmailContext
                    {
                        To = devEmail,
                        CC = ccEmails,
                        Subject = formatter.FormatTitle(Context.Data),
                        Body = formatter.FormatData(Context.Data)
                    });

                }
            }
        }
    }
}
