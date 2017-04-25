using Lib.helper;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Lib.net
{
    public class EmailModel
    {
        public EmailModel()
        {
            this.EnableSSL = false;
            this.SendPort = 25;
            this.TimeOut = 1000 * 10;
        }

        //发送设置
        public string SmtpServer { get; set; }

        public string PopServer { get; set; }

        public string UserName { get; set; }

        public string SenderName { get; set; }

        public string Address { get; set; }

        public string Password { get; set; }

        public bool EnableSSL { get; set; }

        public int SendPort { get; set; }

        public int TimeOut { get; set; }

        public List<string> ToList { get; set; }

        public List<string> CcList { get; set; }

        public string Subject { get; set; }

        public string MailBody { get; set; }

        public string[] File_attachments { get; set; }
    }

    /// <summary>
    ///Send_Emails 的摘要说明
    /// </summary>
    public static class EmailSender
    {
        private static System.Net.Mail.MailMessage BuildMail(EmailModel model)
        {
            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
            //收件人
            if (ValidateHelper.IsPlumpList(model.ToList))
            {
                model.ToList.ForEach(delegate (string to)
                {
                    if (ValidateHelper.IsEmail(to))
                    {
                        mail.To.Add(to);
                    }
                });
            }
            //抄送人
            if (ValidateHelper.IsPlumpList(model.CcList))
            {
                model.CcList.ForEach(delegate (string cc)
                {
                    if (ValidateHelper.IsEmail(cc))
                    {
                        mail.CC.Add(cc);
                    }
                });
            }
            mail.From = new MailAddress(model.Address, model.SenderName, Encoding.UTF8);
            mail.Subject = model.Subject;
            mail.SubjectEncoding = Encoding.UTF8;
            mail.Body = model.MailBody;
            mail.BodyEncoding = Encoding.UTF8;
            mail.IsBodyHtml = true;//发送html
            mail.Priority = System.Net.Mail.MailPriority.Normal;
            return mail;
        }

        private static SmtpClient BuildSmtp(EmailModel model)
        {
            SmtpClient smtp = new SmtpClient();
            smtp.Credentials = new NetworkCredential(model.UserName, model.Password);
            smtp.Host = model.SmtpServer;
            smtp.EnableSsl = model.EnableSSL;
            smtp.Port = model.SendPort;
            smtp.Timeout = model.TimeOut;
            return smtp;
        }

        public static bool SendMail(EmailModel model)
        {
            using (var mail = BuildMail(model))
            {
                using (var smtp = BuildSmtp(model))
                {
                    smtp.Send(mail);
                    return true;
                }
            }
        }

        public static async Task<bool> SendMailAsync(EmailModel model)
        {
            using (var mail = BuildMail(model))
            {
                using (var smtp = BuildSmtp(model))
                {
                    await smtp.SendMailAsync(mail);
                    return true;
                }
            }
        }
    }
}