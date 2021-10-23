using System;

namespace CKLunchBot.Core.ImageProcess
{
    [Serializable]
    public class MenuImageGenerateException : Exception
    {
        public MenuImageGenerateException() { }
        public MenuImageGenerateException(string message) : base(message) { }
        public MenuImageGenerateException(string message, Exception inner) : base(message, inner) { }
        protected MenuImageGenerateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
