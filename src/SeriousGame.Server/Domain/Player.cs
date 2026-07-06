using Server.Domain.Base;

namespace Server.Domain;

public class Player
{
    public required string Id { get; init; }
    public required string Nickname { get; init; }
    public required string ConnectionId { get; set; }
    
    public bool IsActive { get; set; } =  true;
}