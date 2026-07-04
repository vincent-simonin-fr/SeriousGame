namespace Shared.Models.Dtos;

public record CreatePlayerCommand
{
    public required string PlayerId { get; init; }
    public required string Nickname { get; init; }
}