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
        [ArduinoImplementation(CompareByParameterNames = true)]
        public static IPAddress? Parse<TChar>(ReadOnlySpan<TChar> ipSpan, bool tryParse)
            where TChar : unmanaged
        {
            if (typeof(TChar) == typeof(char))
            {
                return ParseIpv4(MemoryMarshal.Cast<TChar, char>(ipSpan), tryParse);
            }

            if (typeof(TChar) == typeof(byte))
            {
                var bytes = MemoryMarshal.Cast<TChar, byte>(ipSpan);
                if (bytes.Contains((byte)':'))
                {
                    throw new NotSupportedException("IPv6 not supported");
                }

                if (bytes.Length > 15)
                {
                    if (tryParse)
                    {
                        return null;
                    }

                    throw new FormatException("Cannot parse address");
                }

                Span<char> chars = stackalloc char[15];
                for (int i = 0; i < bytes.Length; i++)
                {
                    chars[i] = (char)bytes[i];
                }

                return ParseIpv4(chars.Slice(0, bytes.Length), tryParse);
            }

            throw new NotSupportedException("Unsupported IP address character type");
        }

        private static IPAddress? ParseIpv4(ReadOnlySpan<char> ipSpan, bool tryParse)
        {
            if (ipSpan.Contains(':'))
            {
                throw new NotSupportedException("IPv6 not supported");
            }

            int index = 0;
            uint address = 0;
            for (int part = 0; part < 4; part++)
            {
                int value = 0;
                int digits = 0;
                while (index < ipSpan.Length && ipSpan[index] >= '0' && ipSpan[index] <= '9')
                {
                    value = (value * 10) + (ipSpan[index++] - '0');
                    digits++;
                    if (value > 255 || digits > 3)
                    {
                        break;
                    }
                }

                if (digits == 0 || digits > 3 || value > 255 ||
                    (part < 3 ? index >= ipSpan.Length || ipSpan[index++] != '.' : index != ipSpan.Length))
                {
                    if (tryParse)
                    {
                        return null;
                    }

                    throw new FormatException("Cannot parse address");
                }

                address |= (uint)value << (part * 8);
            }

            return new IPAddress(address);
        }
    }
}
