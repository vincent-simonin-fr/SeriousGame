using Shared.Models.Dtos;

namespace Client.State;

public static class ClientMemory
{
    public static List<GameDto> Games = [];
    public static GameDto? CurrentGame;
}
