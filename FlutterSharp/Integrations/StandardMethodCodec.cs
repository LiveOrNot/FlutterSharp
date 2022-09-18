using System;
using System.IO;
using FlutterSharp.Exceptions;

namespace FlutterSharp.Integrations
{
    public sealed class StandardMethodCodec : IMethodCodec
    {
        private StandardMessageCodec _messageCodec;

        public static StandardMethodCodec Instance { get; private set; } = new StandardMethodCodec(StandardMessageCodec.Instance);

        public StandardMethodCodec(StandardMessageCodec messageCodec)
        {
            _messageCodec = messageCodec;
        }

        public byte[] EncodeMethodCall(MethodCall methodCall)
        {
            var stream = new MemoryStream();
            _messageCodec.WriteValue(stream, methodCall.Method);
            _messageCodec.WriteValue(stream, methodCall.Arguments);
            return stream.ToArray();
        }

        public MethodCall DecodeMethodCall(byte[] methodCall)
        {
            using (var reader = new BinaryReader(new MemoryStream(methodCall)))
            {
                object method = _messageCodec.ReadValue(reader);
                object arguments = _messageCodec.ReadValue(reader);
                if (method is string && reader.BaseStream.Position == reader.BaseStream.Length)
                {
                    return new MethodCall(((string)(method)), arguments);
                }
                throw new ArgumentException("Method call corrupted");
            }
        }

        public byte[] EncodeSuccessEnvelope(object result)
        {
            var stream = new MemoryStream();
            stream.WriteByte(0);
            _messageCodec.WriteValue(stream, result);
            return stream.ToArray();
        }

        public byte[] EncodeErrorEnvelope(string errorCode, string errorMessage, object errorDetails)
        {
            var stream = new MemoryStream();
            stream.WriteByte(1);
            _messageCodec.WriteValue(stream, errorCode);
            _messageCodec.WriteValue(stream, errorMessage);
            if ((errorDetails is Exception ex)) _messageCodec.WriteValue(stream, ex.StackTrace);
            else _messageCodec.WriteValue(stream, errorDetails);
            return stream.ToArray();
        }

        public object DecodeEnvelope(byte[] envelope)
        {
            using (var reader = new BinaryReader(new MemoryStream(envelope)))
            {
                byte flag = reader.ReadByte();
                if (flag > 1) throw new ArgumentException("Envelope corrupted");
                if (flag == 0)
                {
                    object result = _messageCodec.ReadValue(reader);
                    if (reader.BaseStream.Position == reader.BaseStream.Length) return result;
                }
                object code = _messageCodec.ReadValue(reader);
                object message = _messageCodec.ReadValue(reader);
                object details = _messageCodec.ReadValue(reader);
                if (code is string && (message == null || message is string) && reader.BaseStream.Position >= reader.BaseStream.Length)
                {
                    throw new FlutterSharpException((string)code, (string)message, details);
                }
                throw new ArgumentException("Envelope corrupted");
            }
        }
    }
}