using Shared.Models.Dtos;

namespace Client.Core;

public static class ClientMemory
{
    public static List<GameDto> Games = [];
    public static GameDto? CurrentGame;
}
