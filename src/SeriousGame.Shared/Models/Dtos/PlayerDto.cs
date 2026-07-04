namespace Shared.Models.Dtos;

public class PlayerDto
{
    public required string Id { get; init; }
    public required string Nickname { get; init; }
    public bool IsActive { get; set; } = true;
}
