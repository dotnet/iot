// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        /// <param name="data">Transmit Error Count bits.</param>
        public Tec(byte data)
        {
            Data = data;
        }

        /// <summary>
        /// Transmit Error Count bits.
        /// </summary>
        public byte Data { get; set; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address GetAddress() => Address.Tec;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte()
        {
            return Data;
        }
    }
}
