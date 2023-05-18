// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BuildHat.Models
{
    /// <summary>
    /// Class containing the board information.
    /// </summary>
    public class BuildHatInformation
    {
        /// <summary>
        /// Gets or sets the Version information.
        /// </summary>
        public string Version { get; internal set; }

        /// <summary>
        /// Gets or sets the signature of the firmawre.
        /// </summary>
        public byte[] Signature { get; internal set; }

        /// <summary>
        /// Gets or sets the Firmware date.
        /// </summary>
        public DateTimeOffset FirmwareDate { get; internal set; }

        /// <summary>
        /// Create a BuildHat information class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="signature">The signature.</param>
        /// <param name="firmwareDate">The firmware date.</param>
        public BuildHatInformation(string version, byte[] signature, DateTimeOffset firmwareDate)
        {
            Version = version;
            Signature = signature;
            FirmwareDate = firmwareDate;
        }
    }
}
