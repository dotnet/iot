// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp25xxx.Register.ErrorDetection
{
    /// <summary>
    /// Transmit Error Counter Register.
    /// </summary>
    public class Tec : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the Tec class.
        /// </summary>
        /// <param name="transmitErrorCount">TEC[7:0]: Transmit Error Count bits.</param>
        public Tec(byte transmitErrorCount)
        {
            TransmitErrorCount = transmitErrorCount;
        }

        /// <summary>
        /// TEC[7:0]: Transmit Error Count bits.
        /// </summary>
        public byte TransmitErrorCount { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.Tec;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte() => TransmitErrorCount;
    }
}
