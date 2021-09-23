using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Guetta.Queue.Abstractions;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Guetta.Player.Client
{
    public class PlayerService
    {
        public PlayerService(IModel rabbitModel, ISubscriber subscriber, IDatabase database)
        {
            RabbitModel = rabbitModel;
            Subscriber = subscriber;
            Database = database;
        }

        private IModel RabbitModel { get; }
        
        private ISubscriber Subscriber { get; }
        
        private IDatabase Database { get; }

        private static byte[] Encode(PlayRequest playRequest) =>
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(playRequest));

        public void EnqueueToPlay(PlayRequest playRequest)
        {
            RabbitModel.BasicPublish(string.Empty, "player", true, null, Encode(playRequest));
        }
        
        public async Task EnqueueVolumeChange(ulong voiceChannel, double newVolume)
        {
            await Database.HashSetAsync(voiceChannel.ToString(), "volume", newVolume);
            await Subscriber.PublishAsync($"{voiceChannel}:volume", newVolume.ToString(CultureInfo.InvariantCulture));
        }
        
        public async Task EnqueueStop(string voiceChannel)
        {
            await Subscriber.PublishAsync($"{voiceChannel}:stop", RedisValue.EmptyString);
        }
    }
}