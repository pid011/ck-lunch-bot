using System;

namespace CKLunchBot.Twitter
{
    [Serializable]
    public class CreateConfigException : Exception
    {
        public CreateConfigException()
        {
        }

        public CreateConfigException(string message) : base(message)
        {
        }

        public CreateConfigException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CreateConfigException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}