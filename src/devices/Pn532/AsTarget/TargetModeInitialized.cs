// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532.AsTarget
{
    /// <summary>
    /// The information regarding the mode initialized
    /// </summary>
    public class TargetModeInitialized
    {
        /// <summary>
        /// The target baud rate between the PN532 and the reader
        /// </summary>
        public TargetBaudRateInialized TargetBaudRate { get; set; }

        /// <summary>
        /// True if we have a PICC emulation
        /// </summary>
        public bool IsISO14443_4Picc { get; set; }

        /// <summary>
        /// True if it's DEP emulation
        /// </summary>
        public bool IsDep { get; set; }

        /// <summary>
        /// The target framing type
        /// </summary>
        public TargetFramingType TargetFramingType { get; set; }
    }
}
