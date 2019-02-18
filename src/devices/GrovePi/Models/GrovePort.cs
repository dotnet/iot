using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.GrovePiDevice.Models
{
    /// <summary>
    /// The supported Grove Ports
    /// See README.md files for the exact location of each Port
    /// </summary>
    public enum GrovePort
    {
        AnalogPin0 = 0,
        AnalogPin1 = 1,
        AnalogPin2 = 2,
        DigitalPin2 = 2,
        DigitalPin3 = 3,
        DigitalPin4 = 4,
        DigitalPin5 = 5,
        DigitalPin6 = 6,
        DigitalPin7 = 7,
        DigitalPin8 = 8,
        // This is Analogic Pin A0 used as digital Pin
        DigitalPin14 = 14,
        // This is Analogic Pin A1 used as digital Pin
        DigitalPin15 = 15,
        // This is Analogic Pin A2 used as digital Pin
        DigitalPin16 = 16
    }
}
