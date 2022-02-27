using System;
using System.Runtime.Serialization;

namespace CKLunchBot.Core;

[Serializable]
public class NoProvidedMenuException : Exception
{
    public NoProvidedMenuException() { }
    public NoProvidedMenuException(string message) : base(message) { }
    public NoProvidedMenuException(string message, Exception inner) : base(message, inner) { }
    protected NoProvidedMenuException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
