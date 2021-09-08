using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Guetta.App.Redis;
using Guetta.Player.Client;
using Guetta.Queue.Extensions;
using Guetta.Queue.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Guetta.Queue
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Guetta.Queue", Version = "v1" });
            });
            services.AddRedisConnection();
            services.AddRedLock();
            services.AddPlayerClient();
            services.AddScoped<QueueService>();
            services.AddScoped<QueueStatusService>();
            services.AddSingleton<PlayerEventSubscriberService>();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Guetta.Queue v1"));
            }
            
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}