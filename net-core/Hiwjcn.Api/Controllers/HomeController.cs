using Hiwjcn.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using Lib.extension;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hiwjcn.Api.Controllers
{
    public class EntityDB : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("");

            this.Database.EnsureCreated();

            base.OnConfiguring(optionsBuilder);
        }
    }

    public class LoginFilterAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (new Random((int)DateTime.Now.Ticks).Choice(new bool[] { true, false }))
            {
                context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
                await context.HttpContext.Response.WriteAsync("验证前");
            }
            else
                await next.Invoke();
        }
    }

    public class HomeController : Controller
    {
        public HomeController(IHttpContextAccessor _context)
        {
            var context = _context.HttpContext;
            //HttpContext.RequestServices.GetService(null);
        }

        [LoginFilter]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
