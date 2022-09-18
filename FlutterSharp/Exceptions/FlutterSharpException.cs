using System;
using System.Runtime.Serialization;

namespace FlutterSharp.Exceptions
{
    internal class FlutterSharpException : Exception
    {
        private string _code;
        private string _message;
        private object _details;

        public FlutterSharpException()
        { }

        public FlutterSharpException(string message) : base(message)
        {
        }

        public FlutterSharpException(string code, string message, object details)
        {
            _code = code;
            _message = message;
            _details = details;
        }

        public FlutterSharpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FlutterSharpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}