using Shared.Models.Base;

namespace Shared.Models;

public class Company : BaseModel
{
    public required string Name { get; set; }
    public required Player PlayerOwner { get; set; }
    public int Treasury { get; private set; } = 1000000;
    public ICollection<Consultant> Staff { get; } = [];
    
    // TODO: Add methods to increase or decrease treasury and involve the staff
}