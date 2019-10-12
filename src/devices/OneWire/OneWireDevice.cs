// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.OneWire
{
    /// <summary>
    /// Represents a 1-wire device.
    /// </summary>
    public class OneWireDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OneWireDevice"/> class
        /// </summary>
        /// <param name="bus">The 1-wire bus the device is found on</param>
        /// <param name="deviceId">The id of the device</param>
        /// <param name="family">The 1-wire fmily id</param>
        protected internal OneWireDevice(OneWireBus bus, string deviceId, OneWireBus.DeviceFamily family)
        {
            if (family <= 0 || (int)family > 0xff)
                throw new ArgumentException(nameof(family));
            Bus = bus;
            DeviceId = deviceId;
            Family = family;
        }

        /// <summary>
        /// The bus where this device is attached.
        /// </summary>
        public OneWireBus Bus { get; }
        /// <summary>
        /// The 1-wire id of this device.
        /// </summary>
        public string DeviceId { get; }
        /// <summary>
        /// The device family id of this device.
        /// </summary>
        public OneWireBus.DeviceFamily Family { get; }
    }
}
