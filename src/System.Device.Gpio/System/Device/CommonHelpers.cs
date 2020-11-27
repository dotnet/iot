// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device
{
    internal static class CommonHelpers
    {
        private const string WindowsPlatformTargetingFormat = "In order to use {0} on Windows with .NET 5.0 it is required for your application to target net5.0-windows10.0.17763.0 or higher. Please add that to your target frameworks in your project file.";

        public static string GetFormattedWindowsPlatformTargetingErrorMessage(string className)
         => string.Format(WindowsPlatformTargetingFormat, className);
    }
}
