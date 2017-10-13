using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Lib.ioc;
using Lib.helper;
using Lib.core;

namespace Lib.mq
{
    public static class MqExtension
    {
        public static void UseMessageQueue<T>() where T : class, IMessageProducer
        { }
    }
}
