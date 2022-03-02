using System;
using System.Runtime.Serialization;

namespace CKLunchBot.Functions;

[Serializable]
public class ProcessFaildException : Exception
{
    public ProcessFaildException() { }
    public ProcessFaildException(string message) : base(message) { }
    public ProcessFaildException(string message, Exception inner) : base(message, inner) { }
    protected ProcessFaildException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
