namespace Client.UI;

public static class ConsoleUI
{
    public static void WriteHeader(string text)
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n {text} \n");
        Console.ResetColor();
    }

    public static void WriteInfo(string text)
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void WriteError(string text)
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($@"❌ {text}");
        Console.ResetColor();
    }

    public static void WritePrompt(string text)
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static string? ReadPrompt()
    {
        Console.BackgroundColor = ConsoleColor.Black;
        var input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? null : input;
    }
}