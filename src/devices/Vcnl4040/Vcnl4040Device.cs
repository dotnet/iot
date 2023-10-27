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
        private AlsHighInterruptThresholdRegister _alsHighInterruptThresholdRegister;
        private AlsLowInterruptThresholdRegister _alsLowInterruptThresholdRegister;
        private PsConf1Register _psConf1Register;
        private PsConf2Register _psConf2Register;
        private PsConf3Register _psConf3Register;
        private PsMsRegister _psMsRegister;
        private PsCancellationLevelRegister _psCancellationLevelRegister;
        private PsLowInterruptThresholdRegister _psLowInterruptThresholdRegister;
        private PsHighInterruptThresholdRegister _psHighInterruptThresholdRegister;
        private PsDataRegister _psDataRegister;
        private AlsDataRegister _alsDataRegister;
        private WhiteDataRegister _whiteDataRegister;
        private IntFlagRegister _intFlagRegister;
        private IdRegister _idRegister;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vcnl4040"/> binding.
        /// </summary>
        public Vcnl4040Device(I2cDevice i2cDevice)
        {
            I2cDevice dev = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _i2cBus = new I2cInterface(dev);

            _alsConfRegister = new AlsConfRegister(_i2cBus);
            _alsHighInterruptThresholdRegister = new AlsHighInterruptThresholdRegister(_i2cBus);
            _alsLowInterruptThresholdRegister = new AlsLowInterruptThresholdRegister(_i2cBus);
            _psConf1Register = new PsConf1Register(_i2cBus);
            _psConf2Register = new PsConf2Register(_i2cBus);
            _psConf3Register = new PsConf3Register(_i2cBus);
            _psMsRegister = new PsMsRegister(_i2cBus);
            _psCancellationLevelRegister = new PsCancellationLevelRegister(_i2cBus);
            _psLowInterruptThresholdRegister = new PsLowInterruptThresholdRegister(_i2cBus);
            _psHighInterruptThresholdRegister = new PsHighInterruptThresholdRegister(_i2cBus);
            _psDataRegister = new PsDataRegister(_i2cBus);
            _alsDataRegister = new AlsDataRegister(_i2cBus);
            _whiteDataRegister = new WhiteDataRegister(_i2cBus);
            _intFlagRegister = new IntFlagRegister(_i2cBus);
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
        public int GetDeviceId()
        {
            _idRegister.Read();
            return _idRegister.Id;
        }

        /// <summary>
        /// BLA BLA
        /// </summary>
        /// <returns></returns>
        public int GetAlsReading()
        {
            _alsDataRegister.Read();
            return _alsDataRegister.Data;
        }

        /// <summary>
        /// BLA BLA
        /// </summary>
        /// <returns></returns>
        public int GetPsReading()
        {
            _psDataRegister.Read();
            return _psDataRegister.Data;
        }
    }
}
