namespace Client.UI;

public sealed class ConsoleAnimator
{
    private readonly string[] _frames;
    private readonly int _intervalMs;
    private readonly string _message;
    private readonly object _lifecycleLock = new();
    private CancellationTokenSource? _cts;
    private Task? _animationTask;

    public ConsoleAnimator(string message, string[] frames, int intervalMs = 100)
    {
        _frames = frames;
        _intervalMs = intervalMs;
        _message = message;
    }

    public void Start()
    {
        // Verrouillé : Start peut être appelé depuis un thread SignalR (broadcast) et le flux
        // principal en même temps - sans lock, deux boucles pourraient démarrer et l'une
        // deviendrait orpheline (plus aucun token pour l'arrêter).
        lock (_lifecycleLock)
        {
            // Redémarrable : un cycle Stop/Start recrée le token, seule une animation
            // encore en cours rend l'appel sans effet.
            if (_animationTask is { IsCompleted: false }) return;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _animationTask = Task.Run(async () =>
            {
                int i = 0;
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        Console.Write($"\r{_message} {_frames[i++ % _frames.Length]}");
                        await Task.Delay(_intervalMs, token);
                    }
                }
                catch (TaskCanceledException) { }
                finally
                {
                    Console.Write("\r" + new string(' ', _message.Length + 10) + "\r");
                }
            });
        }
    }

    public void Stop()
    {
        lock (_lifecycleLock)
        {
            _cts?.Cancel();
        }
    }
}
