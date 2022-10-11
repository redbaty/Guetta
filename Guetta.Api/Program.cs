using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using Guetta.Api.Exceptions;
using Guetta.Api.HealthChecks;
using Guetta.Api.HostedServices;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Commands.Extensions;
using Guetta.Localisation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddSingleton(s =>
{
    var discordSocketClient = new DiscordClient(new DiscordConfiguration
    {
        Token =  Environment.GetEnvironmentVariable("TOKEN") ?? throw new MissingEnvironmentVariableException("TOKEN"),
        TokenType = TokenType.Bot,
        LoggerFactory = s.GetRequiredService<ILoggerFactory>()
    });
    discordSocketClient.UseVoiceNext();
    discordSocketClient.UseInteractivity();
    return discordSocketClient;
});
builder.Services.AddGuettaServices();
builder.Services.AddGuettaCommands();
builder.Services.AddGuettaLocalisation();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck<DiscordCheck>("discord");
builder.Services.AddHostedService<YoutubeDlUpdater>();

var app = builder.Build();

var discordSocketClient = app.Services.GetRequiredService<DiscordClient>();
var socketClientEventsService = app.Services.GetRequiredService<SocketClientEventsService>();
socketClientEventsService.Subscribe();
await discordSocketClient.ConnectAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/health/startup", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/ready", new HealthCheckOptions { Predicate = _ => false });
await app.RunAsync();