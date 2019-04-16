// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Tcs3472x
{
    /// <summary>
    /// This enum allows to select how many cycles cill be done measuring before
    /// raising an interupt.
    /// </summary>
    public enum InterruptState
    {
        // Every RGBC cycle generates an interrupt
        All = 0b0000,
        // 1 clear channel value outside of threshold range
        Percistence01Cycle = 0b0001,
        // 2 clear channel consecutive values out of range 
        Percistence02Cycle = 0b0010,
        // 3 clear channel consecutive values out of range 
        Percistence03Cycle = 0b0011,
        // 5 clear channel consecutive values out of range
        Percistence05Cycle = 0b0100,
        // 10 clear channel consecutive values out of range
        Percistence10Cycle = 0b0101,
        // 15 clear channel consecutive values out of range
        Percistence15Cycle = 0b0110,
        // 20 clear channel consecutive values out of range
        Percistence20Cycle = 0b0111,
        // 25 clear channel consecutive values out of range
        Percistence25Cycle = 0b1000,
        // 30 clear channel consecutive values out of range
        Percistence30Cycle = 0b1001,
        // 35 clear channel consecutive values out of range
        Percistence35Cycle = 0b1010,
        // 40 clear channel consecutive values out of range
        Percistence40Cycle = 0b1011,
        // 45 clear channel consecutive values out of range
        Percistence45Cycle = 0b1100,
        // 50 clear channel consecutive values out of range
        Percistence50Cycle = 0b1101,
        // 55 clear channel consecutive values out of range
        Percistence55Cycle = 0b1110,
        // 60 clear channel consecutive values out of range
        Percistence60Cycle = 0b1111,
    }
}
