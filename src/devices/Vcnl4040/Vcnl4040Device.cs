// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;
using Iot.Device.Vcnl4040.Infrastructure;
using Iot.Device.Vcnl4040.Internal;

namespace Iot.Device.Vcnl4040
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

        private AlsConfRegister _alsConfRegister;
        private IdRegister _idRegister;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vcnl4040"/> binding.
        /// </summary>
        public Vcnl4040Device(I2cDevice i2cDevice)
        {
            I2cDevice dev = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _i2cBus = new I2cInterface(dev);

            _alsConfRegister = new AlsConfRegister(_i2cBus);
            _idRegister = new IdRegister(_i2cBus);
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

        /// <summary>
        /// Gets the current ALS integration time.
        /// ADD MORE DETAILS
        /// </summary>
        public AlsIntegrationTime GetIntegrationTime()
        {
            _alsConfRegister.Read();
            return _alsConfRegister.AlsIt;
        }

        /// <summary>
        /// Sets the ALS integration time.
        /// ADD MORE DETAILS
        /// </summary>
        /// <param name="integrationTime">ALS integration time</param>
        public void SetIntegrationTime(AlsIntegrationTime integrationTime)
        {
            _alsConfRegister.Read();
            _alsConfRegister.AlsIt = integrationTime;
            _alsConfRegister.Write();
        }

        /// <summary>
        /// Gets the current ALS power state.
        /// ADD MORE DETAILS
        /// </summary>
        public PowerState GetPowerState()
        {
            _alsConfRegister.Read();
            return _alsConfRegister.AlsSd;
        }

        /// <summary>
        /// Switches the ALS on.
        /// ADD MORE DETAILS
        /// </summary>
        public void SetPowerOn()
        {
            _alsConfRegister.Read();
            _alsConfRegister.AlsSd = PowerState.PowerOn;
            _alsConfRegister.Write();
        }

        /// <summary>
        /// Switches the ALS off.
        /// ADD MORE DETAILS
        /// </summary>
        public void SetPowerOff()
        {
            _alsConfRegister.Read();
            _alsConfRegister.AlsSd = PowerState.Shutdown;
            _alsConfRegister.Write();
        }

        /// <summary>
        /// Gets the device version code.
        /// </summary>
        public byte GetDeviceVersion()
        {
            _idRegister.Read();
            Console.WriteLine(_idRegister.IdLsb);
            return _idRegister.VersionCode;
        }
    }
}
