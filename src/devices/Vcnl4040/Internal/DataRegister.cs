// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040.Internal
{
    /// <summary>
    /// Base class for read-only 12/16-bit data registers of ALS and PS.
    /// </summary>
    internal class DataRegister : Register
    {
        public int Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRegister"/> class.
        /// </summary>
        public DataRegister(CommandCode commandCode, I2cDevice device)
            : base(commandCode, device)
        {
        }

        /// <inheritdoc/>>
        public override void Read()
        {
            (byte dataLow, byte dataHigh) = ReadData();
            Data = dataHigh << 8 | dataLow;
        }

        /// <summary>
        /// This register is read-only. The write-operation is not implemented.
        /// </summary>
        /// <exception cref="System.NotImplementedException">This register is read-only. The write-operation is not implemented.</exception>
        public override void Write()
        {
            throw new System.NotImplementedException();
        }
    }
}
