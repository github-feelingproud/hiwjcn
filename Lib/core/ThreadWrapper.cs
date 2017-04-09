using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.core
{
    /// <summary>
    /// 来自nop的方法
    /// </summary>
    public class WriteLockDisposable : IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteLockDisposable"/> class.
        /// </summary>
        /// <param name="rwLock">The rw lock.</param>
        public WriteLockDisposable(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
            _rwLock.EnterWriteLock();
        }

        void IDisposable.Dispose()
        {
            _rwLock.ExitWriteLock();
        }
    }
    /// <summary>
    /// 对线程的一个封装，方便安全结束线程
    /// </summary>
    public class ThreadWrapper
    {
        /// <summary>
        /// 停止线程并从列表中移除
        /// </summary>
        /// <param name="list"></param>
        public static void CloseAndRemove(ref List<ThreadWrapper> list)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return; }
            for (int i = list.Count - 1; i >= 0; --i)
            {
                try
                {
                    var tw = list[i];
                    tw.Close();
                    list.RemoveAt(i);
                }
                catch { }
            }
            if (list.Count > 0)
            {
                throw new Exception("至少一个线程无法安全关闭");
            }
        }

        private Thread tt { get; set; }
        private bool KeepRunning { get; set; }

        #region 各种事件

        public Func<bool> OnClosed { get; set; }
        public Func<bool> OnFinished { get; set; }
        public Func<bool> OnStarted { get; set; }
        public Func<Exception, bool> OnExceptionOccured { get; set; }

        private void CallCloseFunc()
        {
            bool? s = OnClosed?.Invoke();
        }

        private void CallFinishFunc()
        {
            bool? s = OnFinished?.Invoke();
        }

        private void CallStartFunc()
        {
            bool? s = OnStarted?.Invoke();
        }

        private void CallExceptionFunc(Exception e)
        {
            bool? s = OnExceptionOccured?.Invoke(e);
        }

        #endregion

        /// <summary>
        /// 请捕捉异常
        /// </summary>
        /// <param name="func"></param>
        /// <param name="background"></param>
        public void Run(Func<Func<bool>, bool> func, string ThreadName = null, bool background = false)
        {
            if (tt != null) { throw new Exception("当前有线程在执行"); }

            KeepRunning = true;
            tt = new Thread(() =>
            {
                try
                {
                    //这里使用func，直接引用标识对象，在标识发生改变的时候无法拿到最新状态
                    func.Invoke(() => KeepRunning);
                }
                catch (Exception e)
                {
                    CallExceptionFunc(e);
                }
                //设置空引用，标记线程执行结束
                tt = null;
                CallFinishFunc();
            });
            if (!ValidateHelper.IsPlumpString(ThreadName)) { ThreadName = $"默认线程名{Com.GetUUID()}"; }
            tt.Name = ThreadName;
            tt.IsBackground = background;
            tt.Start();
            CallStartFunc();
        }

        /// <summary>
        /// 线程是否已经结束
        /// </summary>
        /// <returns></returns>
        public bool ThreadIsStoped()
        {
            return tt == null;
        }

        /// <summary>
        /// 关闭线程
        /// </summary>
        public void Close()
        {
            KeepRunning = false;
            while (tt != null)
            {
                try
                {
                    tt.Interrupt();
                }
                catch { }
                Thread.Sleep(100);
            }
            CallCloseFunc();
        }
    }

    /// <summary>
    /// 同步调用异步async防止死锁方案
    /// 
    /// 当前代码地址
    /// https://github.com/tejacques/AsyncBridge/blob/master/src/AsyncBridge/AsyncHelper.cs
    /// 
    /// https://github.com/tejacques/AsyncBridge
    /// http://stackoverflow.com/questions/5095183/how-would-i-run-an-async-taskt-method-synchronously
    /// https://social.msdn.microsoft.com/Forums/en-US/163ef755-ff7b-4ea5-b226-bbe8ef5f4796/is-there-a-pattern-for-calling-an-async-method-synchronously?forum=async
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// Execute's an async Task<T> method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task<T> method to execute</param>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException($"AsyncHelpers.Run Exception.{InnerException.Message}", InnerException);
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}
