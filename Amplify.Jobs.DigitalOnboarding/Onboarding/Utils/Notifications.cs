using Amplify.Notifications;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Utils
{
    public class Notifications
    {
        public static void SendSingleNotification(string to, string subject, string message, List<System.Net.Mail.Attachment> attachments = null)
        {
            var config = AppConfig.Configuration;

            try
            {
                Mail.MailProvider mail = new Mail.MailProvider(
                     config["AppSettings:mail:user"],
                     config["AppSettings:mail:password"],
                     config["AppSettings:mail:fromaddress"],
                     config["AppSettings:mail:bccaddress"]
                );

                var msg = mail.CreateMessage(subject, to);
                msg.Body = Mail.MailProvider.GetBasicTemplate_NoButton(subject, message);


                if (attachments != null)
                    foreach (var a in attachments)
                        msg.Attachments.Add(a);

#if TEST
				msg.Subject = "[TEST] " + msg.Subject;
#endif

                mail.SendMessage(msg);

            }
            catch { }

        }
    }
}
