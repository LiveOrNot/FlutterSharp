# FlutterSharp
## Tutorials
``` csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using FlutterSharp.Annotations;
using FlutterSharp.Services;
using FlutterSharp.Services.Implements;

namespace FlutterSharp.Wpf
{
    /// <summary>
    /// MainWindow.xaml �Ľ����߼�
    /// </summary>
    public partial class MainWindow : Window
    {
        private IFlutterSharp _flutterSharp;

        public MainWindow()
        {
            InitializeComponent();

            _flutterSharp = new FlutterSharpBuilder()
                .UseConfiguration(new FlutterSharpConfiguration()
                {
                    PluginAssemblies = new List<Assembly>() { Assembly.GetEntryAssembly() },
                    ProjectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\flutter_demo\build\windows\runner\Debug")
                }).Build();

            _flutterSharp.Initialize();

            Host.Children.Add(_flutterSharp.Host);
        }

        [Export(typeof(FlutterPlugin))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        public class TestPlugin : FlutterPlugin
        {
            public override string Name => nameof(TestPlugin);

            protected override string ChannelName => "fluttersharp.plugins/test_plugin";

            [FlutterMethod("callCsharp")]
            public string CallCsharp(string message)
            {
                return $"Flutter ���� C# �ɹ�: {message}";
            }

            public void CallFlutter(string message)
            {
                CallFlutterMethod(new object[] { message });
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var testPlugin = _flutterSharp.Container.GetExportedValues<FlutterPlugin>().FirstOrDefault(p => p.Name == "TestPlugin");
            (testPlugin as TestPlugin)?.CallFlutter(TbMessage?.Text ?? string.Empty);
        }
    }
}
```

``` dart
import 'package:flutter/services.dart';

const MethodChannel _channel = MethodChannel('fluttersharp.plugins/test_plugin');


class TestPlugin {
  Function? _onCallback;

  TestPlugin() {
    _channel.setMethodCallHandler(callbackHandler);
  }

  Future<dynamic> callbackHandler(dynamic arg) {    
    _onCallback!(arg.arguments["message"]);
    return Future.value(null);
  }

  void setCallbackHandler({Function? onCallback}) {
    _onCallback = onCallback;
  }

  Future<String?> callCsharp(String message) async {
    final Map<String, Object> args = <String, Object>{
      'message': message
    };
    return await _channel.invokeMethod<String>("callCsharp", args);
  }

}
```
## Showcase
![Flutter ���� C#](20220918192407.png)
![C# �ص� Flutter](20220918192345.png)