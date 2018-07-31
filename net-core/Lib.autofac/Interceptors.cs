﻿using Lib.extension;
using Polly;
using System;
using System.Linq;
using System.Transactions;

namespace Lib.ioc
{
    /*

    http://docs.autofac.org/en/latest/advanced/interceptors.html

    1注册aop类
    2目标类设置 EnableClassInterceptors
    3然后目标类的目标方法必须要是public virtual才可以实现aop

    */

    /// <summary>
    /// 捕获异常
    /// </summary>
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
                e.AddLog("错误");
            }

        }
    }

    /// <summary>
    /// 重试
    /// </summary>
    public class Retry : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            Policy.Handle<Exception>().Retry(3).Execute(() => invocation.Proceed());
        }
    }

    /// <summary>
    /// 分布事务
    /// </summary>
    public class DistributeTransaction : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var transactionOption = new TransactionOptions();
            //设置事务隔离级别
            transactionOption.IsolationLevel = IsolationLevel.ReadCommitted;
            // 设置事务超时时间为60秒
            transactionOption.Timeout = TimeSpan.FromSeconds(3);

            using (var tran = new TransactionScope(TransactionScopeOption.Required, transactionOption))
            {
                try
                {
                    invocation.Proceed();

                    //没有错误,提交事务
                    tran.Complete();
                }
                catch (Exception e)
                {
                    e.AddErrorLog();
                }
            }
        }
    }
}
