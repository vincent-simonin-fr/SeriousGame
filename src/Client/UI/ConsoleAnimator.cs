namespace Client.UI;

public sealed class ConsoleAnimator
{
    private readonly string[] _frames;
    private readonly int _intervalMs;
    private readonly string _message;
    private readonly CancellationTokenSource _cts = new();
    private Task? _animationTask;

    public ConsoleAnimator(string message, string[] frames, int intervalMs = 100)
    {
        _frames = frames;
        _intervalMs = intervalMs;
        _message = message;
    }

    public void Start()
    {
        if (_animationTask != null) return;

        _animationTask = Task.Run(async () =>
        {
            int i = 0;
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    Console.Write($"\r{_message} {_frames[i++ % _frames.Length]}");
                    await Task.Delay(_intervalMs, _cts.Token);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                Console.Write("\r" + new string(' ', _message.Length + 10) + "\r");
            }
        });
    }

    public void Stop()
    {
        _cts.Cancel();
    }
}
