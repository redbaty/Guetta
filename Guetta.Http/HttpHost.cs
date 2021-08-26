using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Guetta.Http
{
    public static class HttpHost
    {
        public static IHostBuilder CreateHostBuilder(string[] args, int port,
            Action<IServiceCollection> configureServices = null!, Action<IWebHostBuilder> configureHostBuilder = null!)
        {
            return Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureServices(services => { configureServices?.Invoke(services); })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    configureHostBuilder?.Invoke(webBuilder);
                    webBuilder.UseUrls($"http://*:{port}");
                    webBuilder.Configure(builder => { });
                })
                .ConfigureLogging(builder => builder.AddSerilog())
                .UseSerilog();
        }
    }
}