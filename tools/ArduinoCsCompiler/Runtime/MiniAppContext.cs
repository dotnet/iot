// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(AppContext), true, IncludingPrivates = true)]
    internal class MiniAppContext
    {
        public static bool TryGetSwitch(string switchName, out bool isEnabled)
        {
            if (switchName == "System.Globalization.Invariant")
            {
                isEnabled = true;
                return true;
            }

            isEnabled = false;
            return false;
        }

        public static void SetSwitch(string switchName, bool isEnabled)
        {
            // Ignore?
        }

        public static void SetData(string name, object data)
        {
            // Ignore?
        }

        public static object? GetData(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return null;
        }
    }
}
