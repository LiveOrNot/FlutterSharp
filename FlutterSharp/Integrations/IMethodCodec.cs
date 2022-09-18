namespace FlutterSharp.Integrations
{
    public interface IMethodCodec
    {
        byte[] EncodeMethodCall(MethodCall methodCall);

        MethodCall DecodeMethodCall(byte[] methodCall);

        byte[] EncodeSuccessEnvelope(object result);

        byte[] EncodeErrorEnvelope(string errorCode, string errorMessage, object errorDetails);

        object DecodeEnvelope(byte[] envelope);
    }
}