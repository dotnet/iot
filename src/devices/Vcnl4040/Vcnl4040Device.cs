// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.IO;
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

        /// <summary>
        /// This is the version code this binding implementation is compatible with.
        /// Is
        /// </summary>
        private const int CompatibleDeviceId = 0x0186;

        private readonly InterruptFlagRegister _interruptFlagRegister;
        private readonly IdRegister _idRegister;
        private I2cInterface _i2cBus;

        /// <summary>
        /// Ambient light sensor of the VCNL4040 device
        /// </summary>
        public AmbientLightSensor AmbientLightSensor { get; private init; }

        /// <summary>
        /// Proximity sensor of the VCNL4040 device
        /// </summary>
        public ProximitySensor ProximitySensor { get; private init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vcnl4040"/> binding.
        /// </summary>
        public Vcnl4040Device(I2cDevice i2cDevice)
        {
            I2cDevice dev = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _i2cBus = new I2cInterface(dev);

            AmbientLightSensor = new AmbientLightSensor(_i2cBus);
            ProximitySensor = new ProximitySensor(_i2cBus);

            _interruptFlagRegister = new InterruptFlagRegister(_i2cBus);
            _idRegister = new IdRegister(_i2cBus);
        }

        /// <summary>
        /// Resets the device to defaults
        /// </summary>
        public void Reset()
        {
            new AlsConfRegister(_i2cBus).Write();
            new AlsHighInterruptThresholdRegister(_i2cBus).Write();
            new AlsLowInterruptThresholdRegister(_i2cBus).Write();
            new PsConf1Register(_i2cBus).Write();
            new PsConf2Register(_i2cBus).Write();
            new PsConf3Register(_i2cBus).Write();
            new PsMsRegister(_i2cBus).Write();
            new PsCancellationLevelRegister(_i2cBus).Write();
            new PsLowInterruptThresholdRegister(_i2cBus).Write();
            new PsHighInterruptThresholdRegister(_i2cBus).Write();
        }

        /// <summary>
        /// Verifies whether a functional I2C connection to the device exists and checks the identification for
        /// device recognition. If the communication doesn't work or the identification is incorrect, an exception is raised.
        /// </summary>
        /// <exception cref="IOException">Non-functional I2C communication</exception>
        /// <exception cref="IncompatibleDeviceException">Incompatible device detected</exception>
        public void VerifyDevice()
        {
            _idRegister.Read();
            if (_idRegister.Id != CompatibleDeviceId)
            {
                throw new IncompatibleDeviceException(CompatibleDeviceId, _idRegister.Id);
            }
        }

        /// <summary>
        /// Attaches the binding instance to an already operating device.
        /// This synchronizes the binding with the following parameters configured in the device:
        ///     - Ambient light sensor: integration time for load reduction mode
        ///     - Proximity sensor: state of active force mode
        /// </summary>
        public void Attach()
        {
            AmbientLightSensor.Attach();
            ProximitySensor.Attach();
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
        public int DeviceId
        {
            get
            {
                _idRegister.Read();
                return _idRegister.Id;
            }
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
    }
}
