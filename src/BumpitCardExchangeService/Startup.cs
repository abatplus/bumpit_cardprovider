using BumpitCardExchangeService.Redis;
using BumpitCardExchangeService.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BumpitCardExchangeService
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
            services.AddSingleton<IRedisClient, RedisClient>();
            services.AddControllers();
            services.AddSignalR();
            services.AddTransient<ISubscriptionDataRepository, SubscriptionDataRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
            {
                builder
                    .WithOrigins(Configuration["ConnectionInfo:AllowedCoreOrigins:0"])//TODO: make as environment
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                .AllowCredentials();
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CardExchangeHub>("/swaphub");
            });
        }
    }
}
