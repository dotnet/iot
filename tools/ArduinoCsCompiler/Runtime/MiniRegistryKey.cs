// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

#pragma warning disable CA1416 // Code is reachable on all platforms

namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// Replaces only the minimalistic implementation in Corelib, not (yet) the variant in Microsoft.Win32.Registry.dll
    /// </summary>
    [ArduinoReplacement("Internal.Win32.RegistryKey", "System.Private.Corelib.dll", true, typeof(System.String), IncludingPrivates = true)]
    internal sealed class MiniRegistryKey : IDisposable
    {
        private string _name;
        private MiniRegistryKey(string name)
        {
            _name = name;
        }

        [ArduinoCompileTimeConstant]
        public static byte[] GetTziForCurrentTimeZone()
        {
            string keyName = GetRegistryKeyNameForLocalTz();
            using var key = Registry.LocalMachine.OpenSubKey(keyName, false);
            if (key == null)
            {
                throw new InvalidOperationException($"Could not locate registry key for local time zone at {keyName}");
            }

            var value = key.GetValue("TZI", null);
            if (value == null)
            {
                throw new InvalidOperationException("Unable to query time zone data for local time zone");
            }

            return (byte[])value;
        }

        [ArduinoCompileTimeConstant]
        public static string GetRegistryKeyNameForLocalTz()
        {
            TimeZoneInfo tz = TimeZoneInfo.Local;
            string keyName = $"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones\\{tz.Id}";
            return keyName;
        }

        public MiniRegistryKey OpenSubKey(string name, bool writable)
        {
            return new MiniRegistryKey(name);
        }

        internal static MiniRegistryKey OpenBaseKey(IntPtr hKey)
        {
            if (hKey == new IntPtr(0x80000001))
            {
                return new MiniRegistryKey("HKCU");
            }
            else
            {
                return new MiniRegistryKey("HKLM");
            }
        }

        public object GetValue(string value, object defaultValue)
        {
            Debug.WriteLine($"Reading registry key {_name} value {value}");
            if (_name == GetRegistryKeyNameForLocalTz() && value == "TZI")
            {
                return GetTziForCurrentTimeZone();
            }

            return defaultValue;
        }

        public string[] GetSubKeyNames()
        {
            Debug.WriteLine($"Reading subkeys of {_name}");
            return new string[0];
        }

        public void Dispose()
        {
        }
    }
}
