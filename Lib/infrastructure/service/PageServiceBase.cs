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
        protected readonly IEFRepository<PageBase> _pageRepo;

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

            if (await this._pageRepo.ExistAsync(x => x.PageName == page.PageName))
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

        public virtual async Task<_<string>> HideOrShowPages(bool show, params string[] page_uids)
        {
            if (!ValidateHelper.IsPlumpList(page_uids)) { throw new Exception("参数错误"); }
            var data = new _<string>();
            var pages = show ?
                await this._pageRepo.GetListAsync(x => page_uids.Contains(x.UID) && x.IsRemove > 0) :
                await this._pageRepo.GetListAsync(x => page_uids.Contains(x.UID) && x.IsRemove <= 0);

            if (ValidateHelper.IsPlumpList(pages))
            {
                foreach (var m in pages)
                {
                    m.IsRemove = show.ToBoolInt();
                }
                await this._pageRepo.UpdateAsync(pages.ToArray());
            }

            data.SetSuccessData(string.Empty);
            return data;
        }

        public virtual async Task<List<PageBase>> GetPagesByGroup(string group)
        {
            if (!ValidateHelper.IsPlumpString(group)) { throw new Exception("参数错误"); }
            return await this._pageRepo.PrepareIQueryableAsync_(async query =>
            {
                query = query.Where(x => x.IsRemove <= 0);
                query = query.OrderByDescending(x => x.Sort).OrderByDescending(x => x.UpdateTime);
                return await query.Take(1000).ToListAsync();
            });
        }

        public virtual async Task<PageBase> GetPageByName(string name)
        {
            if (!ValidateHelper.IsPlumpString(name)) { return null; }
            var page = await this._pageRepo.GetFirstAsync(x => x.PageName == name && x.IsRemove <= 0);
            return page;
        }

        public virtual async Task<PageBase> GetPageByUID(string uid)
        {
            if (!ValidateHelper.IsPlumpString(uid)) { return null; }
            var page = await this._pageRepo.GetFirstAsync(x => x.UID == uid && x.IsRemove <= 0);
            return page;
        }

        public virtual async Task<PagerData<PageBase>> RemovedPages(
            string user_uid, int page = 1, int pagesize = 10)
        {
            return await this._pageRepo.PrepareIQueryableAsync_(async query =>
            {
                var data = new PagerData<PageBase>();
                var now = DateTime.Now;

                query = query.Where(x => x.IsRemove > 0);
                query = query.Where(x => x.UserUID == user_uid);

                data.ItemCount = await query.CountAsync();
                data.DataList = await query.OrderByDescending(x => x.CreateTime).QueryPage(page, pagesize).ToListAsync();
                return data;
            });
        }

        public virtual async Task<PagerData<PageBase>> QueryPages(
            string user_uid = null, bool enforce_date_range = true,
            int page = 1, int pagesize = 10)
        {
            return await this._pageRepo.PrepareIQueryableAsync_(async query =>
            {
                var data = new PagerData<PageBase>();
                var now = DateTime.Now;

                query = query.Where(x => x.IsRemove <= 0);
                if (enforce_date_range)
                {
                    query = query.Where(x => x.PageStartTime == null || x.PageStartTime < now);
                    query = query.Where(x => x.PageEndTime == null || x.PageEndTime > now);
                }

                if (ValidateHelper.IsPlumpString(user_uid))
                {
                    query = query.Where(x => x.UserUID == user_uid);
                }

                data.ItemCount = await query.CountAsync();
                data.DataList = await query.OrderByDescending(x => x.CreateTime).QueryPage(page, pagesize).ToListAsync();
                return data;
            });
        }

    }
}
