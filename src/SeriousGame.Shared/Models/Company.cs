using Shared.Models.Base;

namespace Shared.Models;

public class Company : BaseModel
{
    public required string Name { get; set; }
    public required Player PlayerOwner { get; set; }
    public int Treasury { get; private set; } = 1000000;
    public ICollection<Consultant> Staff { get; } = [];
    
    // TODO : Ajouter des méthodes pour augmenter/diminuer la trésorerie et impliquer le staff
}