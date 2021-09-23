using System.Threading.Tasks;
using Guetta.App.RabbitMQ;
using Guetta.Queue.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Guetta.Queue
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var playerEventSubscriberService = host.Services.GetService<PlayerEventSubscriberService>();
            await playerEventSubscriberService!.Subscribe();
            
            var rabbitQueueReader = host.Services.GetService<RabbitQueueReader>(); 
            rabbitQueueReader!.Initialize();
            
            var rabbitQueueManagerReader = host.Services.GetService<RabbitQueueCommandReader>(); 
            rabbitQueueManagerReader!.Initialize();
            
            host.Services.InitializeRabbitQueues();
            await host.RunAsync();
        }


        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}