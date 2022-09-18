using System.ComponentModel.Composition.Hosting;
using System.Windows;
using FlutterSharp.Integrations;

namespace FlutterSharp.Services
{
    public interface IFlutterSharp
    {
        FrameworkElement Host { get; }

        bool Initialized { get; }

        CompositionContainer Container { get; }

        IFlutterSharpConfiguration Configuration { get; }

        FlutterDesktopEngineRef Engine { get; }

        FlutterDesktopViewControllerRef Controller { get; }

        void Initialize();

        IFlutterSharp UseConfiguration(IFlutterSharpConfiguration configuration);
    }
}