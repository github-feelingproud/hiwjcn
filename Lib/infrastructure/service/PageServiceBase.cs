using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.data;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;
using Lib.core;
using Lib.infrastructure.extension;
using Lib.data.ef;

namespace Lib.infrastructure.service
{
    public abstract class PageServiceBase<PageBase>
        where PageBase : PageEntityBase
    {
        private readonly IEFRepository<PageBase> _pageRepo;

        public PageServiceBase(
            IEFRepository<PageBase> _pageRepo)
        {
            this._pageRepo = _pageRepo;
        }

        public virtual async Task<bool> CheckPageOwner(string user_uid, string page_uid) =>
            await this.CheckPageOwner(user_uid, new List<string>() { page_uid });

        public virtual async Task<bool> CheckPageOwner(string user_uid, List<string> page_uids)
        {
            if (!ValidateHelper.IsPlumpString(user_uid) || !ValidateHelper.IsPlumpList(page_uids))
            {
                throw new Exception("无法检查页面所有者，参数错误");
            }
            var pages = await this._pageRepo.GetListAsync(x => page_uids.Contains(x.UID));
            return pages.All(x => x.UserUID == user_uid);
        }

        public virtual async Task<_<string>> AddPage(PageBase page)
        {
            var data = new _<string>();
            if (!page.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }

            if (await this._pageRepo.ExistAsync(x => x.PostName == page.PostName))
            {
                data.SetErrorMsg("页面名称已存在");
                return data;
            }

            if (await this._pageRepo.AddAsync(page) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }
            throw new Exception("保存页面失败");
        }

        public abstract void UpdatePageEntity(ref PageBase old_page, ref PageBase new_page);

        public virtual async Task<_<string>> UpdatePage(PageBase model)
        {
            var data = new _<string>();
            var page = await this._pageRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(page, "页面不存在");
            this.UpdatePageEntity(ref page, ref model);
            page.Update();
            if (!page.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._pageRepo.UpdateAsync(page) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("更新页面失败");
        }

        public virtual async Task<_<string>> DeletePage(params string[] page_uids)
        {
            var data = new _<string>();
            if (!ValidateHelper.IsPlumpList(page_uids)) { throw new Exception("参数错误"); }
            if (await this._pageRepo.DeleteWhereAsync(x => page_uids.Contains(x.UID)) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }
            throw new Exception("删除页面错误");
        }

        public virtual async Task<List<PageBase>> GetPagesByGroup(string group)
        {
            if (!ValidateHelper.IsPlumpString(group)) { throw new Exception("参数错误"); }
            return await this._pageRepo.GetListAsync(x => x.PostGroup == group && x.IsRemove <= 0);
        }


    }
}
