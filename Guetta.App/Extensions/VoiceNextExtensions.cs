using System;
using System.Reflection;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext;

namespace Guetta.App.Extensions;

public static class VoiceNextExtensions
{
    private static Type Type { get; } = typeof(VoiceNextConnection);

    private static PropertyInfo IsDisposedProperty { get; } = Type.GetProperty("IsDisposed", BindingFlags.Instance | BindingFlags.NonPublic);

    private static PropertyInfo WebsocketProperty { get; } = Type.GetProperty("VoiceWs", BindingFlags.Instance | BindingFlags.NonPublic);

    public static bool IsDisposed(this VoiceNextConnection voiceNextConnection)
    {
        return (bool)IsDisposedProperty!.GetValue(voiceNextConnection)!;
    }

    public static IWebSocketClient GetWebsocket(this VoiceNextConnection voiceNextConnection) => WebsocketProperty.GetValue(voiceNextConnection) as IWebSocketClient;
}