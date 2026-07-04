using Shared.Models.Base;

namespace Shared.Models;

public class Round :  BaseModel
{
    public required Game Game { get; init; }
    public int Order { get; init; }
    public bool IsCompleted { get; set; } =  false;
    public ICollection<Tender> Tenders { get; init; } = [];
    public ICollection<Training> Traininigs { get; init; } = [];
}