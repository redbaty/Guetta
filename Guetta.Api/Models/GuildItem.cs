namespace Guetta.Api.Models;

public record GuildItem
{
    public string Id { get; init; } = null!;
    
    public string? Name { get; init; }
}