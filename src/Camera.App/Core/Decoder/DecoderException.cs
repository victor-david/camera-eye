using System;
using System.Runtime.Serialization;

namespace Restless.App.Camera.Core
{
    [Serializable]
    public class DecoderException : Exception
    {
        public DecoderException()
        {
        }

        public DecoderException(string message) : base(message)
        {
        }

        public DecoderException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DecoderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}