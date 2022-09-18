using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FlutterSharp.Annotations;
using FlutterSharp.Extensions;
using FlutterSharp.Integrations;

namespace FlutterSharp.Services
{
    public abstract class FlutterPlugin
    {
        private FlutterDesktopMessengerRef _messenger;
        private FlutterDesktopMessageCallback _callback;

        public abstract string Name { get; }

        protected abstract string ChannelName { get; }

        internal void SetMessenger(FlutterDesktopMessengerRef messenger)
        {
            _messenger = messenger;
            _callback = new FlutterDesktopMessageCallback(OnFlutterMessageReceived);
            FlutterInterop.FlutterDesktopMessengerSetCallback(_messenger, ChannelName, _callback, IntPtr.Zero);
        }

        protected bool SendMessage(byte[] message)
        {
            return FlutterInterop.FlutterDesktopMessengerSend(_messenger, ChannelName, message, new IntPtr(message.Length));
        }

        protected Task<(bool success, byte[] reply)> SendMessageWithReplyAsync(byte[] message)
        {
            var tcs = new TaskCompletionSource<(bool success, byte[] reply)>();
            bool result = FlutterInterop.FlutterDesktopMessengerSendWithReply(
                _messenger, ChannelName, message, new IntPtr(message.Length),
                (data, size, _) => tcs.SetResult((true, data)), IntPtr.Zero);
            return tcs.Task;
        }

        protected virtual void CallFlutterMethod(object[] arguments, [CallerMemberName] string name = null)
        {
            var args = new Dictionary<object, object>();
            var m = GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var parameters = m.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                args[parameters[i].Name] = arguments[i];
            }

            var methodCall = new MethodCall(m.Name, args);
            var message = StandardMethodCodec.Instance.EncodeMethodCall(methodCall);
            if (!SendMessage(message)) throw new MethodAccessException();
        }

        private void OnFlutterMessageReceived(IntPtr messenger, FlutterDesktopMessage message, IntPtr userData)
        {
            byte[] data = new byte[message.MessageSize.ToInt32()];
            Marshal.Copy(message.Message, data, 0, data.Length);
            byte[] resultData;

            try
            {
                var methodCall = StandardMethodCodec.Instance.DecodeMethodCall(data);
                var methodInfo = FindFlutterMethod(methodCall);
                var result = methodInfo.Invoke(this, MapFlutterParameters(methodInfo, methodCall.CastedArguments<IDictionary<object, object>>()));
                resultData = StandardMethodCodec.Instance.EncodeSuccessEnvelope(result);
            }
            catch (Exception ex)
            {
                resultData = StandardMethodCodec.Instance.EncodeErrorEnvelope("", ex.Message, ex);
            }

            FlutterInterop.FlutterDesktopMessengerSendResponse(_messenger, message.ResponseHandle, resultData, new IntPtr(resultData.Length));
        }

        protected virtual MethodInfo FindFlutterMethod(MethodCall methodCall)
        {
            return GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<FlutterMethodAttribute>()?.Method == methodCall.Method)
                .FirstOrDefault() ?? throw new MissingMethodException(GetType().Name, methodCall.Method);
        }

        protected virtual object[] MapFlutterParameters(MethodInfo targetMethod, IDictionary<object, object> arguments)
        {
            var parameters = targetMethod.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (arguments.TryGetValue(parameters[i].Name, out var value)) args[i] = value;
            }

            return args;
        }
    }
}