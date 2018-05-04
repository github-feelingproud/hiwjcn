using EPC.Core;
using EPC.Core.Entity;
using Lib.data.ef;
using Lib.extension;
using Lib.helper;
using Lib.ioc;
using Lib.mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Hiwjcn.Service.Epc
{
    public interface IPageService : IAutoRegistered
    {
        Task<_<PageEntity>> AddPage(PageEntity page);

        Task<_<PageEntity>> UpdatePage(PageEntity model);

        Task<_<int>> DeletePage(params string[] page_uids);

        Task<List<PageEntity>> GetPagesByGroup(string group);

        Task<PageEntity> GetPageByName(string name);

        Task<PageEntity> GetPageByUID(string uid);

        Task<List<PageEntity>> QueryList(string org_uid, string q, string device_uid, int? max_id, int pagesize);
    }

    public class PageService : IPageService
    {
        private readonly IEpcRepository<PageEntity> _pageRepo;
        private readonly IEpcRepository<DeviceEntity> _deviceRepo;

        public PageService(
            IEpcRepository<PageEntity> _pageRepo,
            IEpcRepository<DeviceEntity> _deviceRepo)
        {
            this._pageRepo = _pageRepo;
            this._deviceRepo = _deviceRepo;
        }

        public virtual async Task<_<PageEntity>> AddPage(PageEntity page)
        {
            var data = new _<PageEntity>();
            page.Init("page");
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
                data.SetSuccessData(page);
                return data;
            }
            throw new Exception("保存页面失败");
        }

        public virtual async Task<_<PageEntity>> UpdatePage(PageEntity model)
        {
            var data = new _<PageEntity>();
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
                data.SetSuccessData(page);
                return data;
            }

            throw new Exception("更新页面失败");
        }

        public virtual async Task<_<int>> DeletePage(params string[] page_uids)
        {
            var data = new _<int>();
            if (!ValidateHelper.IsPlumpList(page_uids)) { throw new Exception("参数错误"); }
            var count = await this._pageRepo.DeleteWhereAsync(x => page_uids.Contains(x.UID));
            if (count > 0)
            {
                data.SetSuccessData(count);
                return data;
            }
            throw new Exception("删除页面错误");
        }

        public virtual async Task<List<PageEntity>> GetPagesByGroup(string group)
        {
            if (!ValidateHelper.IsPlumpString(group)) { throw new Exception("参数错误"); }
            return await this._pageRepo.PrepareIQueryableAsync(async query =>
            {
                query = query.Where(x => x.IsRemove <= 0);
                query = query.OrderByDescending(x => x.Sort).OrderByDescending(x => x.UpdateTime);
                return await query.Take(1000).ToListAsync();
            });
        }

        public virtual async Task<PageEntity> GetPageByName(string name)
        {
            if (!ValidateHelper.IsPlumpString(name)) { return null; }
            var page = await this._pageRepo.GetFirstAsync(x => x.PageName == name && x.IsRemove <= 0);
            return page;
        }

        public virtual async Task<PageEntity> GetPageByUID(string uid)
        {
            if (!ValidateHelper.IsPlumpString(uid)) { return null; }
            var page = await this._pageRepo.GetFirstAsync(x => x.UID == uid && x.IsRemove <= 0);
            if (page != null && ValidateHelper.IsPlumpString(page.DeviceUID))
            {
                page.DeviceModel = await this._deviceRepo.GetFirstAsync(x => x.UID == page.DeviceUID);
            }
            return page;
        }

        public virtual async Task<List<PageEntity>> QueryList(string org_uid, string q, string device_uid,
            int? max_id, int pagesize)
        {
            return await this._pageRepo.PrepareIQueryableAsync(async query =>
            {
                query = query.Where(x => x.OrgUID == org_uid);
                if (ValidateHelper.IsPlumpString(q))
                {
                    query = query.Where(x => x.PageTitle.Contains(q));
                }
                if (ValidateHelper.IsPlumpString(device_uid))
                {
                    query = query.Where(x => x.DeviceUID == device_uid);
                }
                if (max_id != null && max_id.Value >= 0)
                {
                    query = query.Where(x => x.IID < max_id);
                }

                var data = await query.OrderByDescending(x => x.IID).Take(pagesize).ToListAsync();

                return data;
            });
        }

        private void UpdatePageEntity(ref PageEntity old_page, ref PageEntity new_page)
        {
            old_page.CommentStatus = new_page.CommentStatus;
            old_page.IsRemove = new_page.IsRemove;
            old_page.PageContent = new_page.PageContent;
            old_page.PageContentMarkdown = new_page.PageContentMarkdown;
            old_page.PageTitle = new_page.PageTitle;
            old_page.PageDescription = new_page.PageDescription;
            old_page.PageEndTime = new_page.PageEndTime;
            old_page.PageStartTime = new_page.PageStartTime;
            old_page.PageStatus = new_page.PageStatus;
            old_page.PageGroup = new_page.PageGroup;
            old_page.PageLanguage = new_page.PageLanguage;
            old_page.PageMetaJson = new_page.PageMetaJson;
            old_page.DeviceUID = new_page.DeviceUID;
        }

    }
}
