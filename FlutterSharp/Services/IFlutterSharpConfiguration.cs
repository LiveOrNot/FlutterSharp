using System.Collections.Generic;
using System.Reflection;

namespace FlutterSharp.Services
{
    public interface IFlutterSharpConfiguration
    {
        string ProjectPath { get; }

        IEnumerable<string> StartupArguments { get; }

        IEnumerable<Assembly> PluginAssemblies { get; }
    }
}