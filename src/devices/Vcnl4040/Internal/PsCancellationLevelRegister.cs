// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Common.Defnitions;
using Iot.Device.Vcnl4040.Infrastructure;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// PS cancellation level setting register
    /// Command code / address: 0x05
    /// Documentation: datasheet (Rev. 1.7, 04-Nov-2020 9 Document Number: 84274).
    /// </summary>
    internal class PsCancellationLevelRegister : Register
    {
        /// <summary>
        /// Cancellation level
        /// </summary>
        public int Level { get; set; } = 0;

        public PsCancellationLevelRegister(I2cInterface bus)
            : base(CommandCode.PS_CANC, bus)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte dataHigh) = ReadData();
            Level = dataHigh << 8 | dataLow;
        }

        /// <inheritdoc/>>
        public override void Write()
        {
            WriteData((byte)(Level & 0xff), (byte)((Level >> 8) & 0xff));
        }
    }
}
