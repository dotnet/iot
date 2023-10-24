// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vncl4040.Infrastructure;

namespace Iot.Device.Vncl4040
{
    /// <summary>
    /// Represents a VNCL4040 device
    /// </summary>
    public class Vcnl4040Device : IDisposable
    {
        /// <summary>
        /// Default I2C bus address
        /// </summary>
        public static int DefaultI2cAddress = 0x60;
        private I2cInterface _i2cBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vncl4040"/> binding.
        /// </summary>
        public Vcnl4040Device(I2cDevice i2cDevice)
        {
            I2cDevice dev = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _i2cBus = new I2cInterface(dev);

        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_i2cBus != null)
            {
                _i2cBus?.Dispose();
                _i2cBus = null!;
            }
        }
    }
}
