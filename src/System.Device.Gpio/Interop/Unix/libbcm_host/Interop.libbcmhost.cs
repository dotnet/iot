// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class libbcmhost
    {
        private const string LibbcmhostLibrary = "libbcm_host";

        /// <summary>
        /// Get the peripheral base address of a RaspberryPi.
        /// </summary>
        [DllImport(LibbcmhostLibrary, SetLastError = true)]
        internal static extern uint bcm_host_get_peripheral_address();
    }
}
