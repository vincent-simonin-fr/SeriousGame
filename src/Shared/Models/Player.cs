using Shared.Models.Base;

namespace Shared.Models;

public class Player
{
    public required string Id { get; init; }
    public required string Nickname { get; init; }
    public required string ConnectionId { get; set; }
    
    public bool IsActive { get; set; } =  true;
}