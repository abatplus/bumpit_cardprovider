using CardExchangeService.Redis;
using CardExchangeService.Services;
using CardExchangeService.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace CardExchangeService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IImageFileService, ImageFileService>();
            services.AddSingleton<IRedisClient, RedisClient>();
            services.AddControllers();
            services.AddSignalR(conf =>
            {
                conf.MaximumReceiveMessageSize = null;
            });
            services.AddTransient<ISubscriptionDataRepository, SubscriptionDataRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true).AllowCredentials());

            app.UseRouting();
            app.UseStaticFiles();

            app.UseAuthorization();


            string thumbUrlPathPrefix = Configuration["THUMBNAILS_URL_PATH_PREFIX"] ?? Configuration["ImageFileSettings:ThumbUrlPathPrefix"];
            string thumbPath = Configuration["THUMBNAILS_PATH"] ?? Configuration["ImageFileSettings:ThumbFolder"];
            string imgsUrlPathPrefix = Configuration["IMAGES_URL_PATH_PREFIX"] ?? Configuration["ImageFileSettings:ImgUrlPathPrefix"];
            string imgPath = Configuration["IMAGES_PATH"] ?? Configuration["ImageFileSettings:ImagesFolder"];

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CardExchangeHub>("/swaphub");
                endpoints.MapGet(thumbUrlPathPrefix + "/{name}", async context =>
                 {
                     try
                     {
                         var name = context.Request.RouteValues["name"];
                         if (name.ToString().Contains(".."))
                         {
                             throw new Exception();
                         }

                         String filename = Path.GetFullPath(thumbPath) + $"\\{name}";
                         await context.Response.SendFileAsync(filename);
                     }
                     catch (Exception e)
                     {
                         Console.Write(DateTime.Now.ToString("MM/dd/yyyy") + ": " + e.ToString());
                         await context.Response.WriteAsync("Error");
                     }
                 });
                endpoints.MapGet(imgsUrlPathPrefix + "/{name}", async context =>
                  {
                      try
                      {
                          var name = context.Request.RouteValues["name"];
                          if (name.ToString().Contains(".."))
                          {
                              throw new Exception();
                          }
                         
                          String filename = Path.GetFullPath(imgPath) + $"\\{name}";
                          await context.Response.SendFileAsync(filename);
                      }
                      catch (Exception e)
                      {
                          Console.Write(DateTime.Now.ToString("MM/dd/yyyy") + ": " + e.ToString());
                          await context.Response.WriteAsync("Error");
                      }
                  });
            });
        }
    }
}