using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lib.mvc.plugin
{
    /// <summary>
    /// 支付插件要实现的控制器
    /// </summary>
    public abstract class BasePaymentController : BasePluginController
    {
        //在系统启动的时候把插件加载到autofac
        //然后再支付的页面获取所有支付插件的controller
        //把controller中的plugininfo展示出来
        public abstract string PluginInfo { get; }

        public abstract List<string> ValidatePaymentForm(FormCollection form);
        public abstract ProcessPaymentRequest GetPaymentInfo(FormCollection form);
    }

    /// <summary>
    /// 这里包含卖家/买家/订单/促销等信息
    /// </summary>
    [Serializable]
    public partial class ProcessPaymentRequest
    {
        //
    }
}
