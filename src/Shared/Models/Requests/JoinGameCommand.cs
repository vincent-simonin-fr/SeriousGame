namespace Shared.Models.Dtos;

public record JoinGameCommand
{
    public required string PlayerId { get; init; }
    public required string GameId { get; init; }
}