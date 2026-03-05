namespace CKLunchBot;

public readonly struct Account(string id, string name, string description)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public string Description { get; } = description;
}

public readonly struct Post(string id, string text)
{
    public string Id { get; } = id;
    public string Text { get; } = text;
}

public interface IPostService
{
    ValueTask<bool> IsValidAsync(CancellationToken cancellationToken = default);
    ValueTask<Account> GetAccountInfoAsync(CancellationToken cancellationToken = default);
    ValueTask<Post> PostMessageAsync(string message, CancellationToken cancellationToken = default);
}
