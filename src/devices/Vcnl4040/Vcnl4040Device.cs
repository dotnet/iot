// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.IO;
using Iot.Device.Vcnl4040.Common.Definitions;
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
        private I2cDevice _i2cDevice;

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
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            AmbientLightSensor = new AmbientLightSensor(_i2cDevice);
            ProximitySensor = new ProximitySensor(_i2cDevice);

            _interruptFlagRegister = new InterruptFlagRegister(_i2cDevice);
            _idRegister = new IdRegister(_i2cDevice);
        }

        /// <summary>
        /// Resets the device to defaults
        /// </summary>
        public void Reset()
        {
            Span<byte> data = stackalloc byte[3];

            data[1] = 0x00;
            data[2] = 0x00;

            data[0] = (byte)CommandCode.ALS_THDL;
            _i2cDevice.Write(data);
            data[0] = (byte)CommandCode.ALS_THDH;
            _i2cDevice.Write(data);
            data[0] = (byte)CommandCode.PS_THDL;
            _i2cDevice.Write(data);
            data[0] = (byte)CommandCode.PS_THDH;
            _i2cDevice.Write(data);
            data[0] = (byte)CommandCode.PS_CANC;
            _i2cDevice.Write(data);
            data[0] = (byte)CommandCode.PS_CONF_3_MS;
            _i2cDevice.Write(data);

            data[1] = 0x01;
            data[0] = (byte)CommandCode.ALS_CONF;
            _i2cDevice.Write(data);
            data[0] = (byte)CommandCode.PS_CONF_1_2;
            _i2cDevice.Write(data);

            // clear interrupt flags by reading
            new InterruptFlagRegister(_i2cDevice).Read();
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

        /// <inheritdoc />
        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null!;
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
            InterruptFlags flags = new(PsProtectionMode: _interruptFlagRegister.PsSpFlag,
                                       AlsLow: _interruptFlagRegister.AlsIfL,
                                       AlsHigh: _interruptFlagRegister.AlsIfH,
                                       PsClose: _interruptFlagRegister.PsIfClose,
                                       PsAway: _interruptFlagRegister.PsIfAway);
            return flags;
        }
    }
}
