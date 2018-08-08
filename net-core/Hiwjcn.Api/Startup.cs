using Hiwjcn.Api.Controllers;
using Lib.entityframework;
using Lib.mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Hiwjcn.Api
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(env.ContentRootPath, "appsettings.json"));
            this.Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //new ServiceCollection();
            //解决中文被编码
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
            //use httpcontext
            services.AddHttpContextAccessor_();
            //use ef
            services.UseEF<EntityDB>().UseEFRepositoryFromIoc();
            //use mvc
            services.AddMvc();

            //finally,build service provider
            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //new LoggerFactory();

            loggerFactory.AddConsole();
            //loggerFactory.AddLog4Net(Path.Combine(env.ContentRootPath, "log4net.config"), watch: true);
            //loggerFactory.CreateLogger("").LogInformation("");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
