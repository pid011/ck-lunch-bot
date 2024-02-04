namespace CKLunchBot;

[Serializable]
public class HtmlLoadException : Exception
{
    public HtmlLoadException() { }
    public HtmlLoadException(string message) : base(message) { }
    public HtmlLoadException(string message, Exception inner) : base(message, inner) { }
}
