using Hiwjcn.Bll;
using Hiwjcn.Bll.User;
using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Domain.User;
using Hiwjcn.Framework;
using Lib.data.ef;
using Lib.data.elasticsearch;
using Lib.distributed.redis;
using Lib.events;
using Lib.extension;
using Lib.extra.api;
using Lib.extra.log;
using Lib.helper;
using Lib.mvc;
using Lib.mvc.attr;
using Lib.mvc.user;
using Nest;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebCore.MvcLib.Controller;

namespace Hiwjcn.Web.Controllers
{
    public class TBColumns
    {
        public virtual string Field { get; set; }
        public virtual string Type { get; set; }
        public virtual string Key { get; set; }
    }

    public class TestController : EpcBaseController
    {
        private readonly IEventPublisher _IEventPublisher;
        private readonly IMSRepository<AuthClient> _clientRepo;
        private readonly IUserService _IUserService;

        public TestController(
            IEventPublisher pub,
            IUserService _IUserService,
            IMSRepository<AuthClient> _clientRepo)
        {
            this._IEventPublisher = pub;
            this._IUserService = _IUserService;
            this._clientRepo = _clientRepo;

            this._IEventPublisher.Publish("发布一个垃圾消息");
        }

        /// <summary>
        /// 表结构
        /// </summary>
        /// <returns></returns>
        public ActionResult EntityJson() =>
            GetJson(typeof(Hiwjcn.Core.CacheKeyManager).Assembly.FindEntityDefaultInstance());
        
        /// <summary>
        /// 不会卡死
        /// </summary>
        /// <returns></returns>
        public ActionResult mimimi1()
        {
            var data = AsyncHelper_.RunSync(() => this._clientRepo.GetListAsync(null));
            return GetJson(data);
        }

        /// <summary>
        /// 不会卡死
        /// </summary>
        /// <returns></returns>
        public ActionResult mimimi2()
        {
            var data = Lib.helper.AsyncHelper.RunSync(() => this._clientRepo.GetListAsync(null));
            return GetJson(data);
        }

        /// <summary>
        /// 卡死
        /// </summary>
        /// <returns></returns>
        public ActionResult mimimi3()
        {
            var data = this._clientRepo.GetListAsync(null).Result;
            return GetJson(data);
        }

        public ActionResult excel()
        {
            return RunAction(() =>
            {
                var list = this._clientRepo.GetList(x => x.IID > 0);
                var data = Lib.io.ExcelHelper.ObjectToExcel(list, "用户列表");

                return File(data, Lib.io.ExcelHelper.ContentType, $"用户列表导出-{DateTime.Now.ToDateTimeString()}.xls");
            });
        }

        public ActionResult es_log()
        {
            foreach (var i in Com.Range(50))
            {
                $"业务日志{i}".AddBusinessInfoLog();
                $"警告日志{i}".AddBusinessWarnLog();
                //new Exception().AddErrorLog();
            }
            return Content("ok");
        }

        [ValidateSign]
        public ActionResult sign()
        {
            return Content("");
        }

        public ActionResult qiniu()
        {
            //
            return Content("");
        }

        public ActionResult UView()
        {
            return View();
        }

        public ActionResult P()
        {
            return Content(this.X.context.PostAndGet().ToUrlParam());
        }

        public class ppp
        {
            public string name { get; set; }
            public int age { get; set; }
        }

        public ActionResult pj(ppp pp)
        {
            if (pp != null)
            {
                return GetJson(pp);
            }
            return Content("no data");
        }

        public ActionResult err()
        {
            return RunAction(() =>
            {
                int? page = null;

                return Content((page ?? throw new Exception("fasdfasd")).ToString());
            });
        }

        public async Task<ActionResult> es_log_list(string q)
        {
            return await RunActionAsync(async () =>
            {
                var data = await ESLogHelper.Search(keyword: q);
                return GetJson(new _() { success = true, data = data });
            });
        }

        [AntiReSubmit]
        public ActionResult MM()
        {
            return Content("ok");
        }

        [ValidateSign]
        public ActionResult KK()
        {
            return Content("ok");
        }

        public ActionResult DLL()
        {
            return RunAction(() =>
            {
                var ass = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.StartsWith("Hiwjcn.Plugin."));
                var s = string.Join("\n", ass.Select(x => x.FullName));
                foreach (var a in ass)
                {
                    var controller = a.GetTypes().Where(x => x.IsAssignableTo_<BaseController>() && !x.IsAbstract && !x.IsInterface);
                    if (!controller.Any()) { continue; }

                }
                return Content("");
            });
        }

        private static CircuitBreakerPolicy p = Policy.Handle<Exception>().CircuitBreaker(5, TimeSpan.FromSeconds(30));

        public ActionResult breaker()
        {
            return RunAction(() =>
            {
                new Action(() =>
                {
                    Thread.Sleep(1000 * 3);
                    bool i = true;
                    if (i) { throw new Exception("测试执行错误"); }
                }).InvokeWithCircuitBreaker(ref p);

                return Content("");
            });
        }

        private static readonly CircuitBreakerPolicy pp = Policy.Handle<Exception>().CircuitBreaker(5, TimeSpan.FromSeconds(30));

        public ActionResult breaker_1()
        {
            return RunAction(() =>
            {
                new Action(() =>
                {
                    Thread.Sleep(1000 * 3);
                    bool i = true;
                    if (i) { throw new Exception("测试执行错误"); }
                }).InvokeWithCircuitBreaker(pp);

                return Content("");
            });
        }

        public ActionResult timeout()
        {
            return RunAction(() =>
            {
                new Action(() =>
                {
                    Thread.Sleep(1000 * 10);
                }).InvokeWithTimeOut(2);
                return Content("");
            });
        }

        public ActionResult retry()
        {
            return RunAction(() =>
            {
                int retryCount = 0;
                new Action(() =>
                {
                    ++retryCount;
                    if (retryCount < 3)
                    {
                        throw new Exception("出错，再试");
                    }
                }).InvokeWithRetry<Exception>(5);
                return Content($"retrycount:{retryCount}");
            });
        }

        public ActionResult Log()
        {
            DateTime.Now.ToString().AddBusinessInfoLog();
            DateTime.Now.ToString().AddBusinessWarnLog();
            new Exception("第一层错误", new Exception(DateTime.Now.ToString())).AddErrorLog();
            return Content(DateTime.Now.ToString());
        }

        //public ActionResult set_cookie()
        //{
        //    AccountHelper.Admin.SetUserLogin(this.X.context, new LoginUserInfo() { });
        //    return Content("");
        //}

        //public ActionResult remove_cookie()
        //{
        //    AccountHelper.Admin.DeleteCookie(this.X.context, true);
        //    AccountHelper.Admin.DeleteCookie(this.X.context, false);
        //    return Content("");
        //}

        public async Task<ActionResult> tt()
        {
            var list = new List<string>();

            list.Add($"{Thread.CurrentThread.Name}-{Thread.CurrentThread.ManagedThreadId}");

            Func<string> func = () => { return $"{Thread.CurrentThread.Name}-{Thread.CurrentThread.ManagedThreadId}"; };
            list.Add(await Task.FromResult(func()));

            list.Add($"{Thread.CurrentThread.Name}-{Thread.CurrentThread.ManagedThreadId}");

            return Content(string.Join(",", list));
        }

        public ActionResult Lang()
        {
            return View();
        }

        public ActionResult redis_list()
        {
            var db = new RedisHelper(dbNum: 15, readWriteHosts: "172.16.42.28:6379,password=1q2w3e4r5T");

            var model = db.ListRightPop<TBColumns>("UserOperationLog");

            return Content("");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Task_Http()
        {
            var url = await Task.Run(() =>
            {
                object obj = new { me = new UserEntity() };
                var dy = obj as dynamic;

                return this.X.context.Request.Url.ToString();
            });
            return Content(url);
        }

        public async Task<ActionResult> Baidu(string q, string from = "auto", string to = "en")
        {
            var data = await BaiduTranslateHelper.BaiduTranslate(q, from, to);
            return Content(data);
        }

        public ActionResult Pinyin(string str)
        {
            var html = "";
            html += Com.GetSpell(str);
            html += "================";
            html += Com.Pinyin(str);
            return Content(html);
        }

        public async Task<ActionResult> AsyncAction()
        {
            var start = DateTime.Now;
            var countlist = new List<Task<int>>();
            for (int i = 0; i < 10; ++i)
            {
                countlist.Add(Task<int>.Run(() =>
                {
                    Thread.Sleep(1000);
                    return 1;
                }));
            }
            var count = 0;
            foreach (var t in countlist)
            {
                count += await t;
            }
            return Content($"耗时：{(DateTime.Now - start).TotalSeconds}，结果：{count}");
        }

        public ActionResult FindAllActions()
        {
            try
            {
                foreach (var t in this.GetType().Assembly.GetTypes())
                {
                    //
                    if (t.BaseType != null && t.BaseType == typeof(WebCore.MvcLib.Controller.UserBaseController))
                    {
                        //
                    }
                }
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
            return View();
        }

        public async Task<ActionResult> yibu()
        {
            var count = await Task<int>.Run(() =>
            {
                Thread.Sleep(5000);
                return 0;
            });
            return Content(count.ToString());
        }

        [PageAuth(Permission = "auth_manage.order,auth_manage.user", Scope = "order")]
        public ActionResult WJ()
        {
            var str = Com.GetUUID();
            var key = Com.GetPassKey(str);
            return Content(string.Format("{0}==============={1}", str, key));
        }

        public ActionResult session()
        {
            var key = Com.GetUUID();
            Session[key] = key;
            return Content(key);
        }

        [ElasticsearchType(IdProperty = nameof(UID), Name = nameof(SuggestTest))]
        public class SuggestTest : CompletionSuggestIndexBase
        {
            [String(Name = nameof(UID), Index = FieldIndexOption.NotAnalyzed)]
            public virtual string UID { get; set; }

            [String(Name = nameof(SearchTitle), Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
            public string SearchTitle { get; set; }
        }

        public async Task<ActionResult> suggest(string q)
        {
            return await RunActionAsync(async () =>
            {
                var client = ElasticsearchClientManager.Instance.DefaultClient.CreateClient();
                var index = "a_suggest_test_index";

                var list = await client.SimpleCompletionSuggest<SuggestTest>(index, q);

                return GetJson(list);
            });
        }
    }
}
