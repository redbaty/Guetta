using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Guetta.App.Extensions;

internal static class AsyncDiscordExtensions
{
    public static async IAsyncEnumerable<byte[]> ChunkAndMerge(this IAsyncEnumerable<byte[]> toChunk, int chunkSize, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        var chunksWritten = 0;
            
        await foreach (var bytes in toChunk.WithCancellation(cancellationToken))
        {
            await stream.WriteAsync(bytes, cancellationToken);
            chunksWritten++;
                
            if (chunksWritten == chunkSize)
            {
                yield return stream.ToArray();
                stream.SetLength(0);
                chunksWritten = 0;
            }
        }
    }
}