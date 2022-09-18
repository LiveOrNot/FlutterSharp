using System.Collections.Generic;
using System.Reflection;

namespace FlutterSharp.Services.Implements
{
    public sealed class FlutterSharpConfiguration : IFlutterSharpConfiguration
    {
        public string ProjectPath { get; set; }

        public IEnumerable<string> StartupArguments { get; set; }

        public IEnumerable<Assembly> PluginAssemblies { get; set; }
    }
}