using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Z_Apps.Models.SystemBase;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Hosting;

namespace Z_Apps
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration
        {
            get;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddSingleton(new SiteMapService());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SiteMapService siteMapService)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            var options = new RewriteOptions().AddRedirect("(.*)/$", "$1");
            app.UseRewriter(options);


            app.Use(async (context, next) =>
            {
                var ua = context.Request.Headers["User-Agent"].ToString();
                string url = context.Request.Path.Value;

                if (url.EndsWith("sitemap.xml"))
                {
                    context.Response.Headers.Add("Content-Type", "application/xml");

                    var hostName = SiteMapService
                                    .hostNames
                                    .FirstOrDefault(
                                        h => context
                                            .Request
                                            .Host
                                            .Host
                                            .Contains(h.Value));

                    await context.Response.WriteAsync(
                        siteMapService.GetSiteMapText(hostName.Value)
                    );
                }
                else if (ua.StartsWith("facebookexternalhit") || ua.StartsWith("Twitterbot"))
                {
                    var hostName = SiteMapService
                                    .hostNames
                                    .FirstOrDefault(
                                        h => context
                                            .Request
                                            .Host
                                            .Host
                                            .Contains(h.Value));

                    if (url == null)
                    {
                        await next.Invoke();
                    }
                    else
                    {
                        string resultHTML = "";
                        if (url == "/")
                        {
                            resultHTML = "<!DOCTYPE html><html>" +
                                "<head>" +
                                "<meta name='twitter:card' content='summary'>" + Environment.NewLine +
                                "<meta name='twitter:site' content='@LingualNinja'>" + Environment.NewLine +
                                "<meta property='og:image' content='https://" + hostName + "/ogp-img.png'>" + Environment.NewLine +
                                "<meta property='og:url' content='https://" + hostName + "'>" + Environment.NewLine +
                                "<meta property='og:type' content='website'>" + Environment.NewLine +
                                "<meta property='og:title' content='Lingual Ninja'>" + Environment.NewLine +
                                "<meta property='og:image:alt' content='Lingual Ninja'>" + Environment.NewLine +
                                "<meta property='og:description' content='Free website to learn the meaning of Japanese words! You can learn a lot of Japanese words!'>" + Environment.NewLine +
                                "<meta property='og:site_name' content='Lingual Ninja'>" + Environment.NewLine +
                                "<meta property='fb:app_id' content='217853132566874'>" + Environment.NewLine +
                                "<meta property='fb:page_id' content='491712431290062'>" + Environment.NewLine +
                                "</head>" + Environment.NewLine +
                                "<body>Content for SNS bot</body></html>";

                        }
                        else
                        {
                            resultHTML = "<!DOCTYPE html><html>" +
                                    "<head>" +
                                    "<meta name='twitter:card' content='summary'>" + Environment.NewLine +
                                    "<meta name='twitter:site' content='@LingualNinja'>" + Environment.NewLine +
                                    "<meta property='og:image' content='https://" + hostName + "/ogp-img.png'>" + Environment.NewLine +
                                    "<meta property='og:url' content='https://" + hostName + url + "'>" + Environment.NewLine +
                                    "<meta property='og:type' content='article'>" + Environment.NewLine +
                                    "<meta property='og:title' content='Lingual Ninja'>" + Environment.NewLine +
                                    "<meta property='og:image:alt' content='Lingual Ninja'>" + Environment.NewLine +
                                    "<meta property='og:description' content='Free website to learn the meaning of Japanese words! You can learn a lot of Japanese words!'>" + Environment.NewLine +
                                    "<meta property='og:site_name' content='Lingual Ninja'>" + Environment.NewLine +
                                    "<meta property='fb:app_id' content='217853132566874'>" + Environment.NewLine +
                                    "<meta property='fb:page_id' content='491712431290062'>" + Environment.NewLine +
                                    "</head>" + Environment.NewLine +
                                    "<body>Content for SNS bot</body></html>";
                        }

                        var clientLogService = new ClientLogService();
                        clientLogService.RegisterLog(new ClientOpeLog()
                        {
                            url = url,
                            operationName = "get OGP setting",
                            userId = "SNS Bot",
                            parameters = "ua: " + ua

                        });

                        context.Response.Headers.Add("Content-Type", "text/html");
                        await context.Response.WriteAsync(resultHTML);
                    }
                }
                else
                {
                    await next.Invoke();
                }
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                spa.Options.DefaultPage = $"/index.html";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
