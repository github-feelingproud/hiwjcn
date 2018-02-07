using Lib.task;
using Quartz;
using System.Threading.Tasks;

namespace Hiwjcn.Framework.Tasks
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class UpdateEsIndexTask : QuartzJobBase
    {
        public override string Name => "更新ES索引";

        public override bool AutoStart => true;

        public override ITrigger Trigger => TriggerDaily(1, 1);

        public override async Task Execute(IJobExecutionContext context)
        {
            //do nothing
        }
    }
}
