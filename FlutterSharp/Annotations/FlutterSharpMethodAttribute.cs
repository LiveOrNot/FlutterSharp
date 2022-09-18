using System;

namespace FlutterSharp.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class FlutterMethodAttribute : Attribute
    {
        public string Method { get; private set; }

        public FlutterMethodAttribute(string method) => Method = method;
    }
}