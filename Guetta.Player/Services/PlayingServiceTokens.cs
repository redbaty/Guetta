using System.Collections.Generic;
using System.Threading;

namespace Guetta.Player.Services
{
    public class PlayingServiceTokens
    {
        private Dictionary<ulong, CancellationTokenSource> CancellationTokenSources { get; } = new();

        public CancellationToken GetCancellationToken(ulong channel)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            CancellationTokenSources.Add(channel, cancellationTokenSource);
            return cancellationTokenSource.Token;
        }

        internal void Remove(ulong channel)
        {
            if (!CancellationTokenSources.ContainsKey(channel))
                return;

            CancellationTokenSources.Remove(channel);
        }

        public bool Cancel(ulong channel)
        {
            if (!CancellationTokenSources.ContainsKey(channel))
                return false;

            var cancellationTokenSource = CancellationTokenSources[channel];
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            CancellationTokenSources.Remove(channel);

            return true;
        }
    }
}