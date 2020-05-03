using System;

namespace CKLunchBot.Twitter
{
    [Serializable]
    public class ConfigCreatedException : Exception
    {
        public Reasons Reason { get; }

        public ConfigCreatedException(Reasons reason)
        {
            Reason = reason;
        }

        public ConfigCreatedException(string message) : base(message)
        {
        }

        public ConfigCreatedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ConfigCreatedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public enum Reasons
        {
            ConfigDoesNotExist
        }
    }
}