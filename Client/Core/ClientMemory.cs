using Shared.Models;

namespace Client.Core;

public static class ClientMemory
{
    public static List<Game> Games = [];
    public static Game? CurrentGame;
}