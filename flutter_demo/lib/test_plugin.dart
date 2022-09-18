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