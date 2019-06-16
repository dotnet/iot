// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal static class OperatingSystem
    {
        public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsOsx() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static OSPlatform GetOsPlatform()
        {
            if (IsLinux())
            {
                return OSPlatform.Linux;
            }
            else if (IsWindows())
            {
                return OSPlatform.Windows;
            }
            else if (IsOsx())
            {
                return OSPlatform.OSX;
            }
            else
            {
                throw new PlatformNotSupportedException($"OS platform is not supported.");
            }
        }
    }
}
