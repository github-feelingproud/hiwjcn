using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;
using Lib.extension;
using System.ServiceModel.Channels;
using System.Configuration;
using System.IO;
using System.Web;
using System.Reflection;
using Castle.DynamicProxy;
using System.ServiceModel.Description;

namespace Lib.rpc
{
    [ServiceContract]
    public interface IWcfIntercept
    {
        [OperationContract(Action = "*", ReplyAction = "*")]
        Message Intercept(Message request);
    }

    [ServiceBehavior(UseSynchronizationContext = false, AddressFilterMode = AddressFilterMode.Any)]
    public class WcfInterceptionService : IWcfIntercept
    {
        public Message Intercept(Message request)
        {
            using (var channel = new ChannelFactory<IWcfIntercept>())
            {
                var interceptor = channel.CreateChannel();
                using (interceptor as IDisposable)
                {
                    var reqBuffer = request.CreateBufferedCopy(int.MaxValue);
                    var response = interceptor.Intercept(reqBuffer.CreateMessage());
                    var resBuffer = response.CreateBufferedCopy(int.MaxValue);
                    return resBuffer.CreateMessage();
                }
            }
        }
    }
}
