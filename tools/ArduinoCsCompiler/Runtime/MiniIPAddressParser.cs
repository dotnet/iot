// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement("System.Net.IPAddressParser", "System.Net.Primitives.dll", false, typeof(IPAddress), IncludingPrivates = true)]
    internal class MiniIPAddressParser
    {
        [ArduinoImplementation]
        public static IPAddress? Parse(ReadOnlySpan<char> ipSpan, bool tryParse)
        {
            if (ipSpan.Contains(':'))
            {
                throw new NotSupportedException("IPv6 not supported");
            }
            else if (Ipv4StringToAddress(ipSpan, out long address))
            {
                return new IPAddress(address);
            }

            if (tryParse)
            {
                return null;
            }

            throw new FormatException("Cannot parse address");
        }

        public static unsafe bool Ipv4StringToAddress(ReadOnlySpan<char> ipSpan, out long address)
        {
            int index = 0;
            int byte1 = ParseFrom(ipSpan, ref index);
            int byte2 = ParseFrom(ipSpan, ref index);
            int byte3 = ParseFrom(ipSpan, ref index);
            int byte4 = ParseFrom(ipSpan, ref index);

            address = byte1 << 24 | (byte2 << 16) | (byte3 << 8) | byte4;
            return true;
        }

        private static byte ParseFrom(ReadOnlySpan<char> ipSpan, ref int index)
        {
            // We're parsing a value from 0 - 255 from left to right. If something other than a digit (that would hopefully be a dot) is encountered,
            // stop the group.
            int result = 0;
            char b = ipSpan[index++];
            if (Char.IsDigit(b))
            {
                result = b;
            }
            else
            {
                // Cannot have no digits in a group
                throw new ArgumentOutOfRangeException(nameof(ipSpan));
            }

            b = ipSpan[index++];
            if (char.IsDigit(b))
            {
                result = result * 10 + b;
            }
            else
            {
                return (byte)result;
            }

            b = ipSpan[index++];
            if (char.IsDigit(b))
            {
                result = result * 10 + b;
            }

            return (byte)result;
        }
    }
}
