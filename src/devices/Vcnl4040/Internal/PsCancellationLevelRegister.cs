// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS cancellation level setting register
    /// Command code / address: 0x05 (LSB, MSB)
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsCancellationLevelRegister : LevelRegister
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PsCancellationLevelRegister"/> class.
        /// </summary>
        public PsCancellationLevelRegister(I2cDevice device)
            : base(CommandCode.PS_CANC, device)
        {
        }
    }
}
