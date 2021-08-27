using FluentValidation;
using FluentValidation.AspNetCore;
using Guetta.App.Extensions;
using Guetta.Player.Requests;
using Guetta.Player.Services;
using Guetta.Player.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Guetta.Player
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddFluentValidation();
            services.AddLogging(o => o.AddSerilog());
            services.AddScoped<PlayingService>();
            services.AddSingleton<PlayingServiceTokens>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Guetta.Player", Version = "v1" });
            });
            services.AddTransient<IValidator<PlayRequest>, PlayRequestValidator>();
            services.AddRedisConnection();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Guetta.Player v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}