using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Framework;
using Hiwjcn.Service.Epc.InputsType;
using Hiwjcn.Service.MemberShip;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;
using Lib.data.elasticsearch;
using Lib.distributed.redis;
using Lib.events;
using Lib.extension;
using Lib.extra.log;
using Lib.helper;
using Lib.mvc;
using Lib.mvc.attr;
using Nest;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

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
        private readonly IMSRepository<AuthToken> _clientRepo;
        private readonly IUserService _IUserService;

        public TestController(
            IEventPublisher pub,
            IUserService _IUserService,
            IMSRepository<AuthToken> _clientRepo)
        {
            this._IEventPublisher = pub;
            this._IUserService = _IUserService;
            this._clientRepo = _clientRepo;

            this._IEventPublisher.Publish("发布一个垃圾消息");
            //new ArgumentNullException("发布一个垃圾消息").AddLog_("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
        }

        public ActionResult InitUser()
        {
            return RunAction(() =>
            {
                return Content("ok");
            });
        }

        /// <summary>
        /// 表结构
        /// </summary>
        /// <returns></returns>
        public ActionResult EntityJson() =>
            GetJson(typeof(Hiwjcn.Core.CacheKeyManager).Assembly.FindEntityDefaultInstance());

        public ActionResult Rule()
        {
            var rrule = new RecurrencePattern(FrequencyType.Weekly, 2);
            var serializer = new RecurrencePatternSerializer();
            var s = serializer.SerializeToString(rrule);

            return Content(s);
        }

        public ActionResult InputParamJson()
        {
            var dict = new Dictionary<string, object>();
            var tps = typeof(InputExpressionValidateable).Assembly.GetTypes().Where(x => x.IsNormalClass() && x.IsAssignableTo_<InputExpressionValidateable>());

            foreach (var t in tps)
            {
                dict[t.Name] = Activator.CreateInstance(t);
            }

            return GetJson(dict);
        }

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

        public ActionResult P()
        {
            return Content(this.X.context.PostAndGet().ToUrlParam());
        }

        public ActionResult Log()
        {
            DateTime.Now.ToString().AddBusinessInfoLog();
            DateTime.Now.ToString().AddBusinessWarnLog();
            $"业务日志{DateTime.Now}".AddBusinessInfoLog();
            $"警告日志{DateTime.Now}".AddBusinessWarnLog();
            new Exception("第一层错误", new Exception(DateTime.Now.ToString())).AddErrorLog();
            return Content(DateTime.Now.ToString());
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

        public ActionResult redis_list()
        {
            var db = new RedisHelper(dbNum: 15, readWriteHosts: "172.16.42.28:6379,password=1q2w3e4r5T");

            var model = db.ListRightPop<TBColumns>("UserOperationLog");

            return Content("");
        }

        public ActionResult Pinyin(string str)
        {
            var html = "";
            html += Com.GetSpell(str);
            html += "================";
            html += Com.Pinyin(str);
            return Content(html);
        }

        public ActionResult FindAllActions()
        {
            this.GetType().Assembly.ScanAllAssignedPermissionOnThisAssembly();
            return View();
        }

        [ElasticsearchType(IdProperty = nameof(UID), Name = nameof(SuggestTest))]
        public class SuggestTest : CompletionSuggestIndexBase
        {
            [Text(Name = nameof(UID), Index = false)]
            public virtual string UID { get; set; }

            [Text(Name = nameof(SearchTitle), Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
            public string SearchTitle { get; set; }
        }
    }
}
