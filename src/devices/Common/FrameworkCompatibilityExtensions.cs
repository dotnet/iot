using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing documentation (these methods are mostly hidden from the user)
#if NETFRAMEWORK
namespace Iot.Device
{
    /// <summary>
    /// .NET Core compatibility helper functions (methods that do not exist in .NET Framework)
    /// </summary>
    public static class FrameworkCompatibilityExtensions
    {
        public static bool StartsWith(this Span<char> span, string value)
        {
            return span.StartsWith(new ReadOnlySpan<char>(value.ToCharArray()));
        }

        public static bool StartsWith(this Span<char> span, string value, StringComparison comparison)
        {
            return span.ToString().StartsWith(value, comparison);
        }

        public static bool StartsWith(this ReadOnlySpan<char> span, string value, StringComparison comparison)
        {
            return span.ToString().StartsWith(value, comparison);
        }

        public static int CompareTo(this ReadOnlySpan<char> span, string value, StringComparison comparison)
        {
            return string.Compare(span.ToString(), value, comparison);
        }

        public static string GetString(this Encoding encoding, Span<byte> input)
        {
            return encoding.GetString(input.ToArray());
        }

        public static void NextBytes(this Random random, Span<byte> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)random.Next();
            }
        }

        /// <summary>
        /// Attempts to add the specified key and value to the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <typeparam name="TValue">Type of value</typeparam>
        /// <param name="dictionary">Dictionary</param>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        /// <returns>True if the value was added, false otherwise. No exception is thrown</returns>
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }

            dictionary.Add(key, value);
            return true;
        }
    }
}

#pragma warning disable SA1403 // File may only contain a single namespace
namespace System.Runtime.CompilerServices
#pragma warning restore SA1403 // File may only contain a single namespace
{
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IsExternalInit
    {
    }
}
#endif
