using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;

namespace Hiwjcn.Bll
{
    /*
    
        http://docs.autofac.org/en/latest/advanced/interceptors.html
        
        1注册aop类
        2目标类设置 EnableClassInterceptors
        3然后目标类的目标方法必须要是public virtual才可以实现aop
         
    */

    public class AopLogError : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {

            try
            {
                invocation.Proceed();
            }
            catch (Exception e)
            {
                var name = invocation.Method.Name;
                var type = invocation.Method.ReturnType;
                //log error
                e.SaveLog("错误");
            }

        }
    }
}
