namespace FlutterSharp.Services
{
    public interface IFlutterSharpBuilder
    {
        IFlutterSharp Build();

        T Build<T>() where T : IFlutterSharp, new();

        IFlutterSharpBuilder UseConfiguration(IFlutterSharpConfiguration configuration);
    }
}