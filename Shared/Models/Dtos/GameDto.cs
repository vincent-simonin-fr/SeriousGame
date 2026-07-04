using System.Linq.Expressions;

namespace Shared.Models.Dtos;

public class GameDto
{


    public static Expression<Func<Game, GameDto>> Projection = game => new GameDto
    {
        
    };

    public static GameDto FromEntity(Game game)
    {
        return Projection.Compile().Invoke(game);
    }
}