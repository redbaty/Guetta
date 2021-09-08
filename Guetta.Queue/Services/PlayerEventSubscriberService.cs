using System;
using System.Text.Json;
using System.Threading.Tasks;
using Guetta.Abstractions;
using Guetta.Queue.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedLockNet.SERedis;
using StackExchange.Redis;

namespace Guetta.Queue.Services
{
    public class PlayerEventSubscriberService
    {
        public PlayerEventSubscriberService(IServiceProvider serviceProvider, ILogger<PlayerEventSubscriberService> logger)
        {
            ServiceProvider = serviceProvider;
            Logger = logger;
        }

        private bool IsSubscribed { get; set; }
        
        private ILogger<PlayerEventSubscriberService> Logger { get; }
        
        private IServiceProvider ServiceProvider { get; }

        public async Task Subscribe()
        {
            if (IsSubscribed)
            {
                Logger.LogError("Trying to subscribe again");
                return;
            }

            using var serviceScope = ServiceProvider.CreateScope();
            var subscriber = serviceScope.ServiceProvider.GetService<ISubscriber>();
            var subscription = await subscriber!.SubscribeAsync("player_events");
            subscription.OnMessage(OnPlayerEvent);
            IsSubscribed = true;
        }

        private async Task OnPlayerEvent(ChannelMessage channelMessage)
        {
            var message = channelMessage.Message;
            if (message.HasValue)
            {
                try
                {
                    var playerEventMessage = JsonSerializer.Deserialize<PlayerEventMessage>(message);
                    
                    using var serviceScope = ServiceProvider.CreateScope();
                    var redLockFactory = serviceScope.ServiceProvider.GetService<RedLockFactory>();
                    await using var @lock = await redLockFactory!.CreateLockAsync(playerEventMessage!.Id, TimeSpan.FromSeconds(10));
                    
                    var queueStatusService = serviceScope.ServiceProvider.GetService<QueueStatusService>();
                    var queueStatus = await queueStatusService!.GetQueueStatus(playerEventMessage.Channel);

                    if (queueStatus.PlayingId == playerEventMessage.Id)
                    {
                        if (playerEventMessage!.Event == PlayerEvent.EndedPlaying)
                        {
                            await queueStatusService!.UpdateQueueStatus(playerEventMessage.Channel,
                                QueueStatusEnum.Stopped, null, null);

                            var queueService = serviceScope.ServiceProvider.GetService<QueueService>();
                            await queueService!.CheckQueueStatus(playerEventMessage.Channel);
                        }
                    }
                }
                catch
                {
                    Logger.LogWarning("An invalid player message was received. {@MessageContent}", message.ToString());
                }
            }
        }
    }
}