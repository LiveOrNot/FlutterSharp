using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using FlutterSharp.Controls;
using FlutterSharp.Integrations;
using FlutterSharp.Utilities;

namespace FlutterSharp.Services.Implements
{
    public sealed class FlutterSharp : IFlutterSharp, IDisposable
    {
        private readonly object _syncLock = new object();

        public FrameworkElement Host { get; private set; } = null;

        public bool Initialized { get; private set; } = false;

        public CompositionContainer Container { get; private set; }

        public IFlutterSharpConfiguration Configuration { get; private set; }

        public FlutterDesktopEngineRef Engine { get; private set; }

        public FlutterDesktopViewControllerRef Controller { get; private set; }

        public void Dispose()
        {
            lock (_syncLock)
            {
                if (Initialized && Engine != null && !Engine.IsClosed)
                {
                    try
                    {
                        Host = null;
                        CompositionTarget.Rendering -= OnRendering;
                        Process.GetCurrentProcess().Exited -= OnExited;
                        FlutterInterop.FlutterDesktopEngineDestroy(Engine);
                        Engine.Dispose();
                        Engine = null;
                        Controller = null;
                        Container.Dispose();
                        Container = null;
                        GC.Collect();
                    }
                    catch
                    {
                    }

                    Initialized = false;
                }
            }
        }

        public void Initialize()
        {
            lock (_syncLock)
            {
                if (!Initialized)
                {
                    if (Configuration == null) throw new ArgumentNullException($"'{nameof(Configuration)}' cannot be null");
                    if (!Directory.Exists(Configuration.ProjectPath)) throw new DirectoryNotFoundException($"'{nameof(Configuration.ProjectPath)}' cannot be found");

                    var aggregateCatalog = new AggregateCatalog();
                    aggregateCatalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
                    if (Configuration?.PluginAssemblies?.Any() ?? false)
                    {
                        Configuration.PluginAssemblies.ToList().ForEach(assembly =>
                        {
                            aggregateCatalog.Catalogs.Add(new AssemblyCatalog(assembly));
                        });
                    }
                    Container = new CompositionContainer(aggregateCatalog);
                    Container.ComposeParts(this);

                    FlutterInterop.SetProjectPath(Configuration.ProjectPath);
                    var properties = new FlutterDesktopEngineProperties
                    {
                        AssetsPath = Path.Combine(FlutterInterop.ProjectPath, @"data\flutter_assets"),
                        IcuDataPath = Path.Combine(FlutterInterop.ProjectPath, @"data\icudtl.dat"),
                        AotLibraryPath = Path.Combine(FlutterInterop.ProjectPath, @"data\app.so"),
                    };
                    var switches = Configuration?.StartupArguments?.ToArray() ?? new string[0];
                    properties.Switches = FlutterInterop.CreateSwitches(switches);
                    properties.SwitchesCount = new IntPtr(switches.Length);
#if DEBUG
                    NativeMethods.AllocConsole();
                    FlutterInterop.FlutterDesktopResyncOutputStreams();
#endif

                    Engine = FlutterInterop.FlutterDesktopEngineCreate(properties);
                    var plugins = Container.GetExportedValues<FlutterPlugin>();
                    if (Engine != null)
                    {
                        if (plugins?.Any() ?? false)
                        {
                            foreach (var plugin in plugins)
                            {
                                var registrar = FlutterInterop.FlutterDesktopEngineGetPluginRegistrar(Engine, plugin.Name);
                                var messenger = FlutterInterop.FlutterDesktopEngineGetMessenger(Engine);
                                plugin.SetMessenger(messenger);
                            }
                        }

                        Process.GetCurrentProcess().Exited += OnExited;
                        CompositionTarget.Rendering += OnRendering;
                        Controller = FlutterInterop.FlutterDesktopViewControllerCreate(1024, 768, Engine);

                        if (Controller != null)
                        {
                            var view = FlutterInterop.FlutterDesktopViewControllerGetView(Controller);
                            var host = new WindowsFormsHost();
                            host.Child = new HwndHostControl(FlutterInterop.FlutterDesktopViewGetHWND(view));
                            Host = host;
                        }
                    }

                    Initialized = true;
                }
            }
        }

        private void OnExited(object sender, EventArgs e)
        {
            lock (_syncLock)
            {
                if (Initialized) Dispose();
            }
        }

        private void OnRendering(object sender, EventArgs e)
        {
            lock (_syncLock)
            {
                if (Initialized && Engine != null && !Engine.IsClosed)
                {
                    FlutterInterop.FlutterDesktopEngineProcessMessages(Engine);
                }
            }
        }

        public IFlutterSharp UseConfiguration(IFlutterSharpConfiguration configuration)
        {
            lock (_syncLock)
            {
                Configuration = configuration;
                return this;
            }
        }
    }
}