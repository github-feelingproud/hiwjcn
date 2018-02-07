using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;
using Lib.task;

namespace Hiwjcn.Framework.Tasks
{
    public static class TaskManager
    {
        public static readonly Lazy_<TaskContainer> Jobs = new Lazy_<TaskContainer>(() =>
        {
            var con = new TaskContainer();
            Task.Run(async () =>
            {
                await con.AddJobFromAssembly(typeof(TaskManager).Assembly);
                await con.Start();
            }).Wait();
            return con;
        }).WhenDispose((ref TaskContainer x) => x.Dispose(false));

        public static void Start()
        {
            var con = Jobs.Value;
        }

        public static void Dispose() => Jobs.Dispose();
    }
}
