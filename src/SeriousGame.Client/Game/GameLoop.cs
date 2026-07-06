using Client.Resources;
using Client.State;
using Client.UI;

namespace Client.Game;

/// <summary>
/// Squelette de la boucle de partie côté client : parcourt les tours de la partie en cours et,
/// pour chacun, les phases de TurnPhase avec un écran placeholder - aucune donnée de marché
/// réelle, aucune décision collectée, aucun appel serveur. Instanciée par App pour la durée
/// d'une seule partie, comme ConsoleAnimator, pas résolue depuis le conteneur DI.
/// </summary>
public class GameLoop
{
    private static readonly TurnPhase[] Phases = Enum.GetValues<TurnPhase>();

    private readonly ClientSession _session;

    public GameLoop(ClientSession session)
    {
        _session = session;
    }

    public Task RunAsync()
    {
        var roundsNumber = _session.CurrentGame!.RoundsNumber;

        for (var round = 1; round <= roundsNumber; round++)
        {
            ConsoleUI.WriteHeader(string.Format(ClientResources.RoundHeaderFormat, round, roundsNumber));

            foreach (var phase in Phases)
                RenderPhase(phase);
        }

        ConsoleUI.WriteHeader(ClientResources.GameOverHeader);
        ConsoleUI.WriteInfo(ClientResources.GameOverMessage);

        return Task.CompletedTask;
    }

    private static void RenderPhase(TurnPhase phase)
    {
        ConsoleUI.WriteInfo(PlaceholderFor(phase));
        ConsoleUI.WritePrompt(ClientResources.PressEnterToContinuePrompt);
        Console.ReadLine();
    }

    private static string PlaceholderFor(TurnPhase phase) => phase switch
    {
        TurnPhase.MarketAnalysis => ClientResources.MarketAnalysisPlaceholder,
        TurnPhase.Simulation => ClientResources.SimulationPlaceholder,
        TurnPhase.Decision => ClientResources.DecisionPlaceholder,
        TurnPhase.Submission => ClientResources.SubmissionPlaceholder,
        TurnPhase.Resolution => ClientResources.ResolutionPlaceholder,
        _ => throw new ArgumentOutOfRangeException(nameof(phase))
    };
}
