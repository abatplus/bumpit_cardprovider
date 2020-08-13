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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CardExchangeHub>("/swaphub");
                endpoints.MapGet("/thumbnails/{name}", async context =>
                {
                    try
                    {
                        var name = context.Request.RouteValues["name"];
                        if (name.ToString().Contains(".."))
                        {
                            throw new Exception();
                        }
                        String filename = $"thumbnails/{name}";
                        await context.Response.SendFileAsync(filename);
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.ToString());
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync("Error");
                    }
                });
                endpoints.MapGet("/images/{name}", async context =>
                {
                    try
                    {
                        var name = context.Request.RouteValues["name"];
                        if (name.ToString().Contains(".."))
                        {
                            throw new Exception();
                        }
                        String filename = $"images/{name}";
                        await context.Response.SendFileAsync(filename);
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.ToString());
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync("Error");
                    }
                });
            });
        }
    }
}