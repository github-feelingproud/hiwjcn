using Lib.mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    /// <summary>
    /// 不要继承basecontroller
    /// </summary>
    [ValidateInput(false)]
    public class ErrorController : Controller
    {

        public ActionResult Http404()
        {
            return new Http404();
        }

        public ActionResult Http403()
        {
            return new Http403();
        }

        public ActionResult Http500()
        {
            return new Http500();
        }

        /// <summary>
        /// 测试异步处理
        /// </summary>
        /// <returns></returns>
        public ActionResult ThreadTest()
        {
            var task1 = Task<string>.Factory.StartNew(() =>
            {
                Thread.Sleep(10000);
                return DateTime.Now.ToString();
            });
            var task2 = Task<string>.Factory.StartNew(() =>
            {
                Thread.Sleep(10000);
                return DateTime.Now.ToString();
            });
            var task3 = Task<string>.Factory.StartNew(() =>
            {
                Thread.Sleep(10000);
                return DateTime.Now.ToString();
            });
            var task4 = Task<string>.Factory.StartNew(() =>
            {
                Thread.Sleep(10000);
                return DateTime.Now.ToString();
            });
            DateTime start = DateTime.Now;
            string re = task1.Result + task2.Result + task3.Result + task4.Result;
            double sec = (DateTime.Now - start).TotalSeconds;
            return Content(sec.ToString());
        }

    }
}
