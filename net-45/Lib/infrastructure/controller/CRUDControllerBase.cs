using Lib.data;
using Lib.mvc;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lib.infrastructure.controller
{
    public abstract class CRUDControllerBase<T> : BaseController
        where T : class, IDBTable, new()
    {
        private readonly IRepository<T> _repo;

        public CRUDControllerBase(
            IRepository<T> _repo)
        {
            this._repo = _repo;
        }

        public virtual async Task<ActionResult> PageList(int? page, int? pagesize)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }

        public virtual async Task<ActionResult> Edit(int? page, int? pagesize)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }

        [HttpPost]
        public virtual async Task<ActionResult> SaveAction(int? page, int? pagesize)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }

        [HttpPost]
        public virtual async Task<ActionResult> DeleteAction(int? page, int? pagesize)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }
    }
}
