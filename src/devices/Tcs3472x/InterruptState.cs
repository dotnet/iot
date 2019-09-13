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
        /// <summary>Every RGBC cycle generates an interrupt</summary>
        All = 0b0000,
        /// <summary>1 clear channel value outside of threshold range</summary>
        Percistence01Cycle = 0b0001,
        /// <summary>2 clear channel consecutive values out of range </summary>
        Percistence02Cycle = 0b0010,
        /// <summary>3 clear channel consecutive values out of range </summary>
        Percistence03Cycle = 0b0011,
        /// <summary>5 clear channel consecutive values out of range</summary>
        Percistence05Cycle = 0b0100,
        /// <summary>10 clear channel consecutive values out of range</summary>
        Percistence10Cycle = 0b0101,
        /// <summary>15 clear channel consecutive values out of range</summary>
        Percistence15Cycle = 0b0110,
        /// <summary>20 clear channel consecutive values out of range</summary>
        Percistence20Cycle = 0b0111,
        /// <summary>25 clear channel consecutive values out of range</summary>
        Percistence25Cycle = 0b1000,
        /// <summary>30 clear channel consecutive values out of range</summary>
        Percistence30Cycle = 0b1001,
        /// <summary>35 clear channel consecutive values out of range</summary>
        Percistence35Cycle = 0b1010,
        /// <summary>40 clear channel consecutive values out of range</summary>
        Percistence40Cycle = 0b1011,
        /// <summary>45 clear channel consecutive values out of range</summary>
        Percistence45Cycle = 0b1100,
        /// <summary>50 clear channel consecutive values out of range</summary>
        Percistence50Cycle = 0b1101,
        /// <summary>55 clear channel consecutive values out of range</summary>
        Percistence55Cycle = 0b1110,
        /// <summary>60 clear channel consecutive values out of range</summary>
        Percistence60Cycle = 0b1111,
    }
}
