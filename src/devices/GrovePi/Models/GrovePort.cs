// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.GrovePiDevice.Models
{
    /// <summary>
    /// The supported Digital Grove Ports
    /// See README.md files for the exact location of each Port
    /// </summary>
    public enum GrovePort
    {
        /// <summary>Analog pin 0 (A0)</summary>
        AnalogPin0 = 0,

        /// <summary>Analog pin (A1)</summary>
        AnalogPin1 = 1,

        /// <summary>Analog pin (A2)</summary>
        AnalogPin2 = 2,

        /// <summary>Digital pin 2</summary>
        DigitalPin2 = 2,

        /// <summary>Digital pin 3</summary>
        DigitalPin3 = 3,

        /// <summary>Digital pin 4</summary>
        DigitalPin4 = 4,

        /// <summary>Digital pin 5</summary>
        DigitalPin5 = 5,

        /// <summary>Digital pin 6</summary>
        DigitalPin6 = 6,

        /// <summary>Digital pin 7</summary>
        DigitalPin7 = 7,

        /// <summary>Digital pin 8</summary>
        DigitalPin8 = 8,

        /// <summary>Analog Pin A0 used as Digital Pin</summary>
        DigitalPin14 = 14,

        /// <summary>Analog Pin A1 used as Digital Pin</summary>
        DigitalPin15 = 15,

        /// <summary>This is Analog Pin A2 used as Digital Pin</summary>
        DigitalPin16 = 16
    }
}
