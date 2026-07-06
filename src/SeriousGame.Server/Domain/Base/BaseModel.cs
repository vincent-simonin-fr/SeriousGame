namespace Server.Domain.Base;

public abstract class BaseModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
}