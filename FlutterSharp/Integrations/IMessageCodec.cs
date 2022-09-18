namespace FlutterSharp.Integrations
{
    public interface IMessageCodec<T>
    {
        byte[] EncodeMessage(T message);

        T DecodeMessage(byte[] message);
    }
}