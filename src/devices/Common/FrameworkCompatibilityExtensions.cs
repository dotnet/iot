// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#pragma warning disable CS1591 // Missing documentation (these methods are mostly hidden from the user)
namespace Iot.Device
{
    /// <summary>
    /// .NET Core compatibility helper functions (methods that do not exist in .NET Framework)
    /// </summary>
#if BUILDING_IOT_DEVICE_BINDINGS
    internal
#else
    public
#endif
    static class FrameworkCompatibilityExtensions
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

        public static void Write(this Stream stream, Span<byte> data)
        {
            stream.Write(data.ToArray(), 0, data.Length);
        }

        public static void Write(this Stream stream, ReadOnlySpan<byte> data)
        {
            stream.Write(data.ToArray(), 0, data.Length);
        }

        public static int Read(this Stream stream, Span<byte> data)
        {
            byte[] rawData = new byte[data.Length];
            int result = stream.Read(rawData, 0, rawData.Length);
            rawData.CopyTo(data);
            return result;
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
