// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
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

        private PsConf1Register _psConf1Register;
        private PsConf2Register _psConf2Register;
        private PsConf3Register _psConf3Register;
        private PsMsRegister _psMsRegister;
        private PsCancellationLevelRegister _psCancellationLevelRegister;
        private PsLowInterruptThresholdRegister _psLowInterruptThresholdRegister;
        private PsHighInterruptThresholdRegister _psHighInterruptThresholdRegister;
        private PsDataRegister _psDataRegister;
        private WhiteDataRegister _whiteDataRegister;
        private InterruptFlagRegister _interruptFlagRegister;
        private IdRegister _idRegister;

        /// <summary>
        /// Ambient Light Sensor of the VCNL4040 device
        /// </summary>
        public AmbientLightSensor AmbientLightSensor { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vcnl4040"/> binding.
        /// </summary>
        public Vcnl4040Device(I2cDevice i2cDevice)
        {
            I2cDevice dev = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _i2cBus = new I2cInterface(dev);

            AmbientLightSensor = new AmbientLightSensor(_i2cBus);

            _psConf1Register = new PsConf1Register(_i2cBus);
            _psConf2Register = new PsConf2Register(_i2cBus);
            _psConf3Register = new PsConf3Register(_i2cBus);
            _psMsRegister = new PsMsRegister(_i2cBus);
            _psCancellationLevelRegister = new PsCancellationLevelRegister(_i2cBus);
            _psLowInterruptThresholdRegister = new PsLowInterruptThresholdRegister(_i2cBus);
            _psHighInterruptThresholdRegister = new PsHighInterruptThresholdRegister(_i2cBus);
            _psDataRegister = new PsDataRegister(_i2cBus);
            _whiteDataRegister = new WhiteDataRegister(_i2cBus);
            _interruptFlagRegister = new InterruptFlagRegister(_i2cBus);
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
        /// Gets the device version code.
        /// </summary>
        public int GetDeviceId()
        {
            _idRegister.Read();
            return _idRegister.Id;
        }

        /// <summary>
        /// Gets and clears (by reading) the interrupt flags.
        /// </summary>
        public InterruptFlags GetAndClearInterruptFlags()
        {
            _interruptFlagRegister.Read();
            InterruptFlags flags = new InterruptFlags(_interruptFlagRegister.PsSpFlag,
                                                      _interruptFlagRegister.AlsIfL,
                                                      _interruptFlagRegister.AlsIfH,
                                                      _interruptFlagRegister.PsIfClose,
                                                      _interruptFlagRegister.PsIfAway);
            return flags;
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
