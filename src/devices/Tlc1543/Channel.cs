// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Tlc1543
{
    /// <summary>
    /// Available Channels to poll from on Tlc1543
    /// <remarks>
    /// <br>For more information see Table 2 and 3 in datasheet</br>
    /// </remarks>
    /// </summary>
    public enum Channel
    {
        /// <summary>
        /// Channel A0
        /// </summary>
        A0 = 0,

        /// <summary>
        /// Channel A1
        /// </summary>
        A1 = 1,

        /// <summary>
        /// Channel A2
        /// </summary>
        A2 = 2,

        /// <summary>
        /// Channel A3
        /// </summary>
        A3 = 3,

        /// <summary>
        /// Channel A4
        /// </summary>
        A4 = 4,

        /// <summary>
        /// Channel A5
        /// </summary>
        A5 = 5,

        /// <summary>
        /// Channel A6
        /// </summary>
        A6 = 6,

        /// <summary>
        /// Channel A7
        /// </summary>
        A7 = 7,

        /// <summary>
        /// Channel A8
        /// </summary>
        A8 = 8,

        /// <summary>
        /// Channel A9
        /// </summary>
        A9 = 9,

        /// <summary>
        /// Channel A10
        /// </summary>
        A10 = 10,

        /// <summary>
        /// Self Test channel that sets charge capacitors to (Vref+ - Vref-)/2. Expected output is 512.
        /// </summary>
        SelfTestHalf = 11,

        /// <summary>
        /// Self Test channel that sets charge capacitors to Vref-. Expected output is 0.
        /// </summary>
        SelfTestMin = 12,

        /// <summary>
        /// Self Test channel that sets charge capacitors to Vref+. Expected output is 1023.
        /// </summary>
        SelfTestMax = 13,
    }
}
