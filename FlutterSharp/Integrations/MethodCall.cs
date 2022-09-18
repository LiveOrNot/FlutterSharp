using System;
using System.Collections;

namespace FlutterSharp.Integrations
{
    public sealed class MethodCall
    {
        public MethodCall(string method, object arguments)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Arguments = arguments;
        }

        public string Method { get; private set; }

        public object Arguments { get; private set; }

        public T CastedArguments<T>()
        {
            return ((T)(Arguments));
        }

        public T Argument<T>(string key)
        {
            if ((this.Arguments == null))
            {
                return default;
            }
            else if ((this.Arguments is IDictionary))
            {
                return ((T)(((IDictionary)(this.Arguments))[key]));
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public bool HasArgument(string key)
        {
            if ((this.Arguments == null))
            {
                return false;
            }
            else if ((this.Arguments is IDictionary))
            {
                return ((IDictionary)(this.Arguments)).Contains(key);
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }
}