using Hiwjcn.Framework;
using Hiwjcn.Framework.Tasks;
using Hiwjcn.Service.MemberShip;
using Lib.mvc;
using System.Threading.Tasks;
using System.Web.Mvc;
using Lib.task;

namespace Hiwjcn.Web.Controllers
{
    public class SysManageController : EpcBaseController
    {
        private readonly ISystemService _sysService;

        public SysManageController(
            ISystemService _sysService)
        {
            this._sysService = _sysService;
        }

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Query()
        {
            return await RunActionAsync(async () =>
            {
                var data = await this._sysService.QueryAll();
                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> QueryJobs()
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                var data = await TaskManager.Jobs.Value.TaskScheduler.GetAllTasks_();
                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }
    }
}