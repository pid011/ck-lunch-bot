namespace CKLunchBot.Menu;

[Serializable]
public class MenuParseException : Exception
{
    public MenuParseException() { }
    public MenuParseException(string message) : base(message) { }
    public MenuParseException(string message, Exception inner) : base(message, inner) { }
}
