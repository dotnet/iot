// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.IO;

namespace Iot.Device.Cap1xxx
{
    /// <summary>
    /// CAP1208: 8-channel capacitive touch sensor from Microchip.
    /// </summary>
    /// <remarks>
    /// The touch status is read by polling: the ALERT# pin of the chip is not used.
    /// </remarks>
    public class Cap1208 : IDisposable
    {
        /// <summary>
        /// Default I2C address of the CAP1208.
        /// </summary>
        public const byte DefaultI2cAddress = 0x28;

        private const byte Cap1208ProductId = 0x6B;
        private const byte MicrochipManufacturerId = 0x5D;
        private const byte InterruptBit = 0x01;
        private const byte MultipleTouchBlockingBit = 0x80;
        private const byte DeltaSenseMask = 0x70;
        private const int DeltaSenseShift = 4;

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Creates a new instance of the CAP1208.
        /// </summary>
        /// <param name="i2cDevice">The I2C device of the chip. It is disposed with this instance.</param>
        /// <remarks>
        /// The constructor does not change the configuration of the chip.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="i2cDevice"/> is null.</exception>
        /// <exception cref="IOException">The device is not a CAP1208.</exception>
        public Cap1208(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            byte productId = ReadRegister(Register.ProductId);
            byte manufacturerId = ReadRegister(Register.ManufacturerId);
            if (productId != Cap1208ProductId || manufacturerId != MicrochipManufacturerId)
            {
                throw new IOException($"The device is not a CAP1208: product ID 0x{productId:X2} (expected 0x{Cap1208ProductId:X2}), " +
                    $"manufacturer ID 0x{manufacturerId:X2} (expected 0x{MicrochipManufacturerId:X2}).");
            }
        }

        /// <summary>
        /// Gets or sets whether the chip blocks multiple touches (MULT_BLK_EN).
        /// </summary>
        /// <remarks>
        /// The power-on default is true: the chip reports one touch at a time and blocks the other inputs.
        /// Set it to false to read several touches at the same time.
        /// </remarks>
        public bool MultipleTouchBlocking
        {
            get => (ReadRegister(Register.MultipleTouchConfiguration) & MultipleTouchBlockingBit) != 0;
            set
            {
                byte configuration = ReadRegister(Register.MultipleTouchConfiguration);
                configuration = value
                    ? (byte)(configuration | MultipleTouchBlockingBit)
                    : (byte)(configuration & ~MultipleTouchBlockingBit);
                WriteRegister(Register.MultipleTouchConfiguration, configuration);
            }
        }

        /// <summary>
        /// Gets or sets the sensitivity of the touch detection.
        /// </summary>
        /// <remarks>
        /// The power-on default is <see cref="Sensitivity.Multiplier32"/>.
        /// </remarks>
        public Sensitivity Sensitivity
        {
            get => (Sensitivity)((ReadRegister(Register.SensitivityControl) & DeltaSenseMask) >> DeltaSenseShift);
            set
            {
                if (value < Sensitivity.Multiplier128 || value > Sensitivity.Multiplier1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                byte sensitivityControl = ReadRegister(Register.SensitivityControl);
                sensitivityControl = (byte)((sensitivityControl & ~DeltaSenseMask) | ((byte)value << DeltaSenseShift));
                WriteRegister(Register.SensitivityControl, sensitivityControl);
            }
        }

        /// <summary>
        /// Reads the inputs touched since the previous call, including the inputs that are still touched.
        /// </summary>
        /// <returns>The touched inputs.</returns>
        /// <remarks>
        /// The chip keeps a touch in its status register until the INT bit is cleared, so a short touch
        /// between two calls is not lost. This method clears the INT bit after reading the status:
        /// the chip then clears the inputs that are no longer touched.
        /// </remarks>
        public SensorInputs ReadTouchedInputs()
        {
            SensorInputs touched = (SensorInputs)ReadRegister(Register.SensorInputStatus);

            byte mainControl = ReadRegister(Register.MainControl);
            if ((mainControl & InterruptBit) != 0)
            {
                WriteRegister(Register.MainControl, (byte)(mainControl & ~InterruptBit));
            }

            return touched;
        }

        /// <summary>
        /// Forces the calibration of the given inputs.
        /// </summary>
        /// <param name="inputs">The inputs to calibrate. By default, all of them.</param>
        /// <remarks>
        /// Calibrate the inputs again when the surroundings of the sensor pads change. The inputs do not
        /// detect touches while they are calibrated.
        /// </remarks>
        public void Recalibrate(SensorInputs inputs = SensorInputs.All)
        {
            WriteRegister(Register.CalibrationActivate, (byte)inputs);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        private byte ReadRegister(Register register)
        {
            Span<byte> readBuffer = stackalloc byte[1];
            _i2cDevice.WriteRead(stackalloc byte[] { (byte)register }, readBuffer);
            return readBuffer[0];
        }

        private void WriteRegister(Register register, byte value)
        {
            _i2cDevice.Write(stackalloc byte[] { (byte)register, value });
        }
    }
}
