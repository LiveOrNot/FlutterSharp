using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FlutterSharp.Integrations
{
    public class StandardMessageCodec : IMessageCodec<object>
    {
        private const byte NULL = 0;
        private const byte TRUE = 1;
        private const byte FALSE = 2;
        private const byte INT = 3;
        private const byte LONG = 4;
        private const byte BIGINT = 5;
        private const byte DOUBLE = 6;
        private const byte STRING = 7;
        private const byte BYTE_ARRAY = 8;
        private const byte INT_ARRAY = 9;
        private const byte LONG_ARRAY = 10;
        private const byte DOUBLE_ARRAY = 11;
        private const byte LIST = 12;
        private const byte MAP = 13;
        private static bool LITTLE_ENDIAN = BitConverter.IsLittleEndian;

        public static StandardMessageCodec Instance { get; private set; } = new StandardMessageCodec();

        public byte[] EncodeMessage(object message)
        {
            if (message == null) return null;
            var stream = new MemoryStream();
            WriteValue(stream, message);
            return stream.ToArray();
        }

        public object DecodeMessage(byte[] message)
        {
            if (message == null) return null;
            using (var reader = new BinaryReader(new MemoryStream(message)))
            {
                object value = this.ReadValue(reader);
                if (reader.BaseStream.Position < reader.BaseStream.Length) throw new ArgumentException("Message corrupted");
                return value;
            }
        }

        protected static void WriteSize(Stream stream, int value)
        {
            if (value < 254)
            {
                stream.WriteByte((byte)value);
            }
            else if (value <= 65535)
            {
                stream.WriteByte(254);
                WriteChar(stream, value);
            }
            else
            {
                stream.WriteByte(255);
                WriteInt(stream, value);
            }
        }

        protected static void WriteChar(Stream stream, int value)
        {
            if (LITTLE_ENDIAN)
            {
                stream.WriteByte((byte)value);
                stream.WriteByte((byte)(value >> 8));
            }
            else
            {
                stream.WriteByte((byte)(value >> 8));
                stream.WriteByte((byte)value);
            }
        }

        protected static void WriteInt(Stream stream, int value)
        {
            if (LITTLE_ENDIAN)
            {
                stream.WriteByte((byte)value);
                stream.WriteByte((byte)(value >> 8));
                stream.WriteByte((byte)(value >> 16));
                stream.WriteByte((byte)(value >> 24));
            }
            else
            {
                stream.WriteByte((byte)(value >> 24));
                stream.WriteByte((byte)(value >> 16));
                stream.WriteByte((byte)(value >> 8));
                stream.WriteByte((byte)value);
            }
        }

        protected static void WriteLong(Stream stream, long value)
        {
            if (LITTLE_ENDIAN)
            {
                stream.WriteByte((byte)value);
                stream.WriteByte((byte)(value >> 8));
                stream.WriteByte((byte)(value >> 16));
                stream.WriteByte((byte)(value >> 24));
                stream.WriteByte((byte)(value >> 32));
                stream.WriteByte((byte)(value >> 40));
                stream.WriteByte((byte)(value >> 48));
                stream.WriteByte((byte)(value >> 56));
            }
            else
            {
                stream.WriteByte((byte)(value >> 8));
                stream.WriteByte((byte)(value >> 16));
                stream.WriteByte((byte)(value >> 24));
                stream.WriteByte((byte)(value >> 32));
                stream.WriteByte((byte)(value >> 40));
                stream.WriteByte((byte)(value >> 48));
                stream.WriteByte((byte)(value >> 56));
                stream.WriteByte((byte)value);
            }
        }

        protected static void WriteDouble(Stream stream, double value)
        {
            WriteLong(stream, BitConverter.DoubleToInt64Bits(value));
        }

        protected static void WriteBytes(Stream stream, byte[] bytes)
        {
            WriteSize(stream, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void WriteAlignment(Stream stream, int alignment)
        {
            int mod = (int)stream.Length % alignment;
            if (mod != 0)
            {
                for (int i = 0; i < (alignment - mod); i++)
                {
                    stream.WriteByte(0);
                }
            }
        }

        internal void WriteValue(Stream stream, object value)
        {
            if ((value == null) || value.Equals(null))
            {
                stream.WriteByte(NULL);
            }
            else if (value.Equals(true))
            {
                stream.WriteByte(TRUE);
            }
            else if (value.Equals(false))
            {
                stream.WriteByte(FALSE);
            }
            else if (value is int || value is short || value is byte)
            {
                stream.WriteByte(INT);
                WriteInt(stream, (int)value);
            }
            else if (value is long)
            {
                stream.WriteByte(LONG);
                WriteLong(stream, (long)value);
            }
            else if (value is float || value is double)
            {
                stream.WriteByte(DOUBLE);
                WriteAlignment(stream, 8);
                WriteDouble(stream, (double)value);
            }
            else if (value is string str)
            {
                stream.WriteByte(STRING);
                WriteBytes(stream, Encoding.UTF8.GetBytes(str));
            }
            else if (value is byte[])
            {
                stream.WriteByte(BYTE_ARRAY);
                WriteBytes(stream, (byte[])value);
            }
            else if (value is int[])
            {
                stream.WriteByte(INT_ARRAY);
                int[] array = (int[])value;
                WriteSize(stream, array.Length);
                WriteAlignment(stream, 4);
                foreach (int n in array)
                {
                    WriteInt(stream, n);
                }
            }
            else if (value is long[])
            {
                stream.WriteByte(LONG_ARRAY);
                long[] array = (long[])value;
                WriteSize(stream, array.Length);
                WriteAlignment(stream, 8);
                foreach (long n in array)
                {
                    WriteLong(stream, n);
                }
            }
            else if (value is double[] doubleArray)
            {
                stream.WriteByte(DOUBLE_ARRAY);
                double[] array = doubleArray;
                WriteSize(stream, array.Length);
                WriteAlignment(stream, 8);
                foreach (double d in array)
                {
                    WriteDouble(stream, d);
                }
            }
            else if (value is IList list1)
            {
                stream.WriteByte(LIST);
                IList list = list1;
                WriteSize(stream, list.Count);
                foreach (object o in list)
                {
                    WriteValue(stream, o);
                }
            }
            else if (value is IDictionary dictionary)
            {
                stream.WriteByte(MAP);
                IDictionary map = dictionary;
                WriteSize(stream, map.Count);
                var enumerator = map.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    WriteValue(stream, enumerator.Key);
                    WriteValue(stream, enumerator.Value);
                }
            }
            else
            {
                throw new ArgumentException("Unsupported value: " + value);
            }
        }

        private static int ReadSize(BinaryReader buffer)
        {
            if (buffer.BaseStream.Position >= buffer.BaseStream.Length) throw new ArgumentException("Message corrupted");
            int value = buffer.ReadByte() & 255;
            if (value < 254)
            {
                return value;
            }
            else if (value == 254)
            {
                return buffer.ReadChar();
            }
            else
            {
                return buffer.ReadInt32();
            }
        }

        protected static byte[] ReadBytes(BinaryReader buffer)
        {
            int length = ReadSize(buffer);
            byte[] bytes = new byte[length];
            buffer.Read(bytes, 0, length);
            return bytes;
        }

        protected static void ReadAlignment(BinaryReader buffer, int alignment)
        {
            int mod = (int)buffer.BaseStream.Position % alignment;
            if (mod != 0) buffer.BaseStream.Seek(buffer.BaseStream.Position + (alignment - mod), SeekOrigin.Begin);
        }

        internal object ReadValue(BinaryReader buffer)
        {
            if (buffer.BaseStream.Position >= buffer.BaseStream.Length) throw new ArgumentException("Message corrupted");
            byte type = (byte)buffer.ReadByte();
            return ReadValueOfType(type, buffer);
        }

        protected object ReadValueOfType(byte type, BinaryReader buffer)
        {
            object result;
            switch (type)
            {
                case NULL:
                    result = null;
                    break;

                case TRUE:
                    result = true;
                    break;

                case FALSE:
                    result = false;
                    break;

                case INT:
                    result = buffer.ReadInt32();
                    break;

                case LONG:
                    result = buffer.ReadInt64();
                    break;

                case BIGINT:
                    throw new NotImplementedException();
                case DOUBLE:
                    ReadAlignment(buffer, 8);
                    result = buffer.ReadDouble();
                    break;

                case STRING:
                    byte[] bytes = ReadBytes(buffer);
                    result = Encoding.UTF8.GetString(bytes);
                    break;

                case BYTE_ARRAY:
                    result = ReadBytes(buffer);
                    break;

                case INT_ARRAY:
                    {
                        int length = ReadSize(buffer);
                        int[] array = new int[length];
                        ReadAlignment(buffer, 4);
                        for (int i = 0; i < length; i++)
                        {
                            array[i] = buffer.ReadInt32();
                        }
                        result = array;
                    }
                    break;

                case LONG_ARRAY:
                    {
                        int llength = ReadSize(buffer);
                        long[] larray = new long[llength];
                        ReadAlignment(buffer, 8);
                        for (int i = 0; i < llength; i++)
                        {
                            larray[i] = buffer.ReadInt64();
                        }
                        result = larray;
                    }
                    break;

                case DOUBLE_ARRAY:
                    {
                        int length = ReadSize(buffer);
                        double[] array = new double[length];
                        ReadAlignment(buffer, 8);
                        for (int i = 0; i < length; i++)
                        {
                            array[i] = buffer.ReadDouble();
                        }
                        result = array;
                    }
                    break;

                case LIST:
                    {
                        int size = ReadSize(buffer);
                        List<object> list = new List<object>(size);
                        for (int i = 0; i < size; i++)
                        {
                            list.Add(ReadValue(buffer));
                        }
                        result = list;
                    }
                    break;

                case MAP:
                    {
                        int size = ReadSize(buffer);
                        var map = new Dictionary<object, object>();
                        for (int i = 0; i < size; i++)
                        {
                            map.Add(ReadValue(buffer), ReadValue(buffer));
                        }
                        result = map;
                    }
                    break;

                default:
                    throw new ArgumentException("Message corrupted");
            }
            return result;
        }
    }
}