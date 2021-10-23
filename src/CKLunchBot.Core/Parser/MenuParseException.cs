using System;

namespace CKLunchBot.Core.Parser
{

    [Serializable]
    public class MenuParseException : Exception
    {
        public MenuParseException() { }
        public MenuParseException(string message) : base(message) { }
        public MenuParseException(string message, Exception inner) : base(message, inner) { }
        protected MenuParseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
