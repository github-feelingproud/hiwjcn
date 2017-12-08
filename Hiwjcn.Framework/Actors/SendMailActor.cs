using Akka.Actor;
using Lib.extension;
using Lib.net;
using System;

namespace Hiwjcn.Framework.Actors
{
    /// <summary>
    /// 异步记录请求记录
    /// </summary>
    public class SendMailActor : ReceiveActor
    {
        public SendMailActor()
        {
            this.Receive<EmailModel>(x =>
            {
                try
                {
                    if (!EmailSender.SendMail(x))
                    {
                        $"发送邮件失败，邮件内容：{x?.ToJson()}".AddBusinessInfoLog();
                    }
                }
                catch (Exception e)
                {
                    e.DebugInfo();
                }
            });
        }
    }
}
