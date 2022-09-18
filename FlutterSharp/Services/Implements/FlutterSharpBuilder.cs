using System;
using System.IO;

namespace FlutterSharp.Services.Implements
{
    public sealed class FlutterSharpBuilder : IFlutterSharpBuilder
    {
        private readonly object _syncLock = new object();
        private IFlutterSharpConfiguration _configuration;

        public IFlutterSharp Build()
        {
            lock (_syncLock)
            {
                if (_configuration == null) throw new ArgumentNullException($"'{nameof(_configuration)}' cannot be null");
                if (!Directory.Exists(_configuration.ProjectPath)) throw new DirectoryNotFoundException($"'{nameof(_configuration.ProjectPath)}' cannot be found");
                FlutterSharp flutterSharp = new FlutterSharp();
                flutterSharp.UseConfiguration(_configuration);
                return flutterSharp;
            }
        }

        public T Build<T>() where T : IFlutterSharp, new()
        {
            lock (_syncLock)
            {
                if (_configuration == null) throw new ArgumentNullException($"'{nameof(_configuration)}' cannot be null");
                if (!File.Exists(_configuration.ProjectPath)) throw new DirectoryNotFoundException($"'{nameof(_configuration.ProjectPath)}' cannot be found");
                T flutterSharp = new T();
                flutterSharp.UseConfiguration(_configuration);
                return flutterSharp;
            }
        }

        public IFlutterSharpBuilder UseConfiguration(IFlutterSharpConfiguration configuration)
        {
            lock (_syncLock)
            {
                _configuration = configuration;
                return this;
            }
        }
    }
}