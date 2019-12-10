// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Native library name")]
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
