namespace Client.Core;

public static class ClientIdentity
{
    private static string FileId = Guid.NewGuid().ToString();
    private static string FilePath = $"client_id_{FileId}.txt";
    private static string? _nickname;

    static ClientIdentity()
    {
        if (File.Exists(FilePath))
            Id = File.ReadAllText(FilePath);
        else
        {
            Id = Guid.NewGuid().ToString();
            File.WriteAllText(FilePath, Id);
        }
    }

    public static string Id { get; }

    public static string Nickname => _nickname ?? "Not logged in";
    public static readonly Action<string> SetNickname = nickname => _nickname = nickname;
}

