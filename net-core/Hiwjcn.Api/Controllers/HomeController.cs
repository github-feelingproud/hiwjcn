using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hiwjcn.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.ServiceModel;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Pens;
using SixLabors.ImageSharp.Drawing.Brushes;
using SixLabors.ImageSharp;
using Microsoft.AspNetCore.Http;
using Autofac;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hiwjcn.Api.Controllers
{
    public class ffasdfasdgdsafa : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("");
            optionsBuilder.UseNpgsql("");

            this.Database.EnsureCreated();

            base.OnConfiguring(optionsBuilder);
        }

    }
    public class ServiceClient<T> : ClientBase<T>, IDisposable where T : class
    {

    }

    public class xx : ActionFilterAttribute
    {
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            return base.OnActionExecutionAsync(context, next);
        }
    }

    public class HomeController : Controller
    {
        public HomeController(IHttpContextAccessor _context)
        {
            var context = _context.HttpContext;

            using (var s = Startup.ioc_container.BeginLifetimeScope())
            {
                var data = s.Resolve<IHttpContextAccessor>();
            }

            //HttpContext.RequestServices.GetService(null);
        }

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
