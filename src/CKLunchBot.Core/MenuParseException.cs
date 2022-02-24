using System;
using System.Runtime.Serialization;

namespace CKLunchBot.Core
{

    [Serializable]
    public class MenuParseException : Exception
    {
        public MenuParseException() { }
        public MenuParseException(string message) : base(message) { }
        public MenuParseException(string message, Exception inner) : base(message, inner) { }
        protected MenuParseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
