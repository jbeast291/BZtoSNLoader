using System.Runtime.InteropServices;
using System.Text;

//This namespace is imported but never used???
namespace System.Reflection.PortableExecutable { }

namespace FModBankParser
{
    public static class HashCode
    {
        public static int Combine(uint h1, uint h2,  uint h3, uint h4)
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (int)h1;
                hash = (hash * 7) + (int)h2;
                hash = (hash * 7) + (int)h3;
                hash = (hash * 7) + (int)h4;
                return hash;
            }
        }
    }
    
    public static class BackportFixes
    {
        extension<T>(Stack<T> stack)
        {
            public bool TryPeek(out T value)
            {
                if (stack.Count > 0)
                {
                    value = stack.Peek();
                    return true;
                }
                value = default;
                return false;
            }
        }

        extension(ArgumentNullException)
        {
            public static void ThrowIfNull(object value)
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
            }
        }

        extension<TKey, TValue>(KeyValuePair<TKey, TValue> kvp)
        {
            public void Deconstruct(out TKey key, out TValue value)
            {
                key = kvp.Key;
                value = kvp.Value;
            }
        }

        extension(Encoding encoding)
        {
            public string GetString(Span<byte> span) 
                => encoding.GetString(span.ToArray());
        }

        extension(Stream stream)
        {
            public void ReadExactly(Span<byte> buffer)
            {
                int bytesRead = 0;
                while (bytesRead < buffer.Length)
                {
                    byte[] temp = new byte[buffer.Length - bytesRead];
                    int read = stream.Read(temp, 0, temp.Length);

                    if (read == 0) throw new EndOfStreamException();

                    temp.AsSpan(0, read).CopyTo(buffer[bytesRead..]);
                    bytesRead += read;
                }
            }
        }

        extension(BitConverter)
        {
            public static uint ToUInt32(ReadOnlySpan<byte> span) 
                => MemoryMarshal.Read<uint>(span);

            public static void TryWriteBytes(Span<byte> destination, uint value) 
                => BitConverter.GetBytes(value).AsSpan().CopyTo(destination);
        }

        extension(Guid)
        {
            public static Guid ToGuid(Span<byte> span) => new(span.ToArray());
        }
    }
}
