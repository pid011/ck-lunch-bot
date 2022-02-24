using System;
using System.Runtime.Serialization;

namespace CKLunchBot.Core
{
    [Serializable]
    public class MenuImageGenerateException : Exception
    {
        public MenuImageGenerateException() { }
        public MenuImageGenerateException(string message) : base(message) { }
        public MenuImageGenerateException(string message, Exception inner) : base(message, inner) { }
        protected MenuImageGenerateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
