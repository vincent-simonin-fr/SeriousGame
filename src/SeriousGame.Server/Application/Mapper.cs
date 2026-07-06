using Server.Domain;
using Shared.Models.Dtos;

namespace Server.Application;

public static class Mapper
{
    public static PlayerDto ToDto(Player player) => new()
    {
        Id = player.Id,
        Nickname = player.Nickname,
        IsActive = player.IsActive
    };

    public static GameDto ToDto(Game game) => new()
    {
        Id = game.Id,
        Name = game.Name,
        MinimumPlayers = game.MinimumPlayers,
        MaximumPlayers = game.MaximumPlayers,
        RoundsNumber = game.RoundsNumber,
        IsInProgress = game.IsInProgress,
        Owner = ToDto(game.Owner),
        Players = game.Players.Select(ToDto).ToList()
    };
}
