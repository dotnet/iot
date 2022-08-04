// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Buffers.Binary;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using Iot.Device.DistanceSensor.Models.LidarLiteV3;
using UnitsNet;

namespace Iot.Device.DistanceSensor
{
    /// <summary>
    /// Lidar Lite v3 is a long-range fixed position distance sensor by Garmin.
    /// </summary>
    public class LidarLiteV3 : IDisposable
    {
        /// <summary>
        /// Default address for LidarLiteV3
        /// </summary>
        public const byte DefaultI2cAddress = 0x62;

        private GpioController? _gpioController = null;
        private bool _shouldDispose;
        private I2cDevice _i2cDevice;
        private int _powerEnablePin;

        /// <summary>
        /// Initialize the LidarLiteV3
        /// </summary>
        /// <param name="i2cDevice">I2C device</param>
        /// <param name="gpioController">GPIO controller</param>
        /// <param name="powerEnablePin">The pin number used to control power to the device</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance.</param>
        public LidarLiteV3(I2cDevice i2cDevice, GpioController? gpioController = null, int powerEnablePin = -1, bool shouldDispose = true)
        {
            _gpioController = gpioController ?? new GpioController();
            _shouldDispose = shouldDispose;
            _i2cDevice = i2cDevice;
            _powerEnablePin = powerEnablePin;

            if (_powerEnablePin != -1)
            {
                _gpioController.OpenPin(_powerEnablePin, PinMode.Output);
                PowerOn();
            }

            Reset();
        }

        /// <summary>
        /// Power off the device if GPIO controller and power enable pin is provided.
        /// </summary>
        public void PowerOff()
        {
            if (_powerEnablePin == -1)
            {
                throw new InvalidOperationException("Cannot power off without providing GPIO controller and power enable pin.");
            }

            _gpioController?.Write(_powerEnablePin, PinValue.Low);
        }

        /// <summary>
        /// Power on the device if GPIO controller and power enable pin is provided.
        /// </summary>
        public void PowerOn()
        {
            if (_powerEnablePin == -1)
            {
                throw new InvalidOperationException("Cannot power off without providing GPIO controller and power enable pin.");
            }

            _gpioController?.Write(_powerEnablePin, PinValue.High);
        }

        /// <summary>
        /// Reset FPGA, all registers return to default values
        /// </summary>
        public void Reset()
        {
            try
            {
                WriteRegister(Register.ACQ_COMMAND, 0x00);
            }
            catch (IOException ex)
            {
                // `IOException: Error 121 performing I2C data transfer.` is thrown every time
                // the device is reset on the Raspberry PI.  I think the reset signal causes
                // a disruption in the I2C connection.  Without shallowing this exception,
                // Reset() would always throw the IOException.
                if (ex.Message != "Error 121 performing I2C data transfer.")
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Measure distance.
        /// </summary>
        /// <remarks>
        /// Note: Do not call if running while in repetition mode.  It will block until
        /// repetition finishes (forever if infinite).
        /// </remarks>
        /// <param name="withReceiverBiasCorrection">Faster without bias correction, but more prone to errors if condition changes.</param>
        /// <returns>Distance as a length unit.</returns>
        public Length MeasureDistance(bool withReceiverBiasCorrection = true)
        {
            WriteRegister(Register.ACQ_COMMAND, withReceiverBiasCorrection ? (byte)0x04 : (byte)0x03);

            while (Status.HasFlag(SystemStatus.BusyFlag))
            {
                DelayHelper.DelayMicroseconds(4, allowThreadYield: true);
            }

            return LastDistance;
        }

        /// <summary>
        /// Set the repetition mode to enable automatic measurement.
        /// </summary>
        /// <param name="measurementRepetition">Repetition mode, either Off, Repeat, or RepeatInfinitely.</param>
        /// <param name="count">If Repeat, the number of times to repeat the measurement.</param>
        /// <param name="delay">The delay between each measurements. Note the unit does not directly to hz, a value of 20 maps to about 100 hz.</param>
        public void SetMeasurementRepetitionMode(MeasurementRepetition measurementRepetition, int count = -1, int delay = -1)
        {
            if (count < 2 || count > 254)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 2 and 254");
            }

            switch (measurementRepetition)
            {
                case MeasurementRepetition.Repeat:
                    WriteRegister(Register.OUTER_LOOP_COUNT, (byte)count);
                    break;
                case MeasurementRepetition.RepeatIndefinitely:
                    WriteRegister(Register.OUTER_LOOP_COUNT, 0xff);
                    break;
                case MeasurementRepetition.Off:
                    WriteRegister(Register.OUTER_LOOP_COUNT, 0x00);
                    break;
            }

            if (delay != -1)
            {
                WriteRegister(Register.MEASURE_DELAY, (byte)delay);

                // Set mode to use custom delay.
                var currentAcqSettings = AcquisitionSettings;
                currentAcqSettings |= AcquisitionSettings.UseCustomDelay;
                AcquisitionSettings = currentAcqSettings;
            }
            else
            {
                // Set mode to use default delay.
                var currentAcqSettings = AcquisitionSettings;
                currentAcqSettings &= ~AcquisitionSettings.UseCustomDelay;
                AcquisitionSettings = currentAcqSettings;
            }

            // Kick it off with a single acquire command.
            WriteRegister(Register.ACQ_COMMAND, 0x04);
        }

        /// <summary>
        /// Set a new I2C address and dispose the device.
        /// </summary>
        /// <remarks>
        /// Note, if the device is powered off or reset, the IC2 address will reset to the default address.
        /// </remarks>
        /// <param name="address">new address, valid values are 7-bit values with 0 in the LSB.</param>
        public void SetI2cAddressAndDispose(byte address)
        {
            if ((address & 1) == 1)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address must have 0-bit in the LSB.");
            }

            // Read in the unit's serial number.
            Span<byte> rawData = stackalloc byte[2];

            ReadBytes(Register.UNIT_ID, rawData);

            // Write serial number to I2C_ID.
            WriteRegister(Register.I2C_ID_HIGH, rawData[1]);
            WriteRegister(Register.I2C_ID_LOW, rawData[0]);

            // Write the new address.
            WriteRegister(Register.I2C_SEC_ADDR, address);

            // Disable the default address
            WriteRegister(Register.I2C_CONFIG, 0x08);

            // Dispose
            Dispose();
        }

        #region Device Registers

        /// <summary>
        /// Get the last distance measurement.
        /// </summary>
        public Length LastDistance
        {
            get
            {
                Span<byte> rawData = stackalloc byte[2];
                ReadBytes(Register.FULL_DELAY, rawData);
                return Length.FromCentimeters(BinaryPrimitives.ReadUInt16BigEndian(rawData));
            }
        }

        /// <summary>
        /// Get the difference between the current and last measurement resulting
        /// in a signed (2's complement) 8-bit number.
        /// Positive is away from the device.
        /// </summary>
        public Length DifferenceBetweenLastTwoDistances
        {
            get
            {
                Span<byte> rawData = stackalloc byte[1];
                ReadBytes(Register.VELOCITY, rawData);
                return Length.FromCentimeters((int)(sbyte)rawData[0]);
            }
        }

        /// <summary>
        /// Get or set the various settings to control the acquistion behavior.
        /// </summary>
        public AcquisitionSettings AcquisitionSettings
        {
            get
            {
                Span<byte> rawData = stackalloc byte[1];
                ReadBytes(Register.ACQ_CONFIG_REG, rawData);
                return (AcquisitionSettings)rawData[0];
            }
            set
            {
                WriteRegister(Register.ACQ_CONFIG_REG, (byte)value);
            }
        }

        /// <summary>
        /// Get or set the maximum aquisition count limits the number of times
        /// the device will integrate acquistions to find a correlation
        /// record peak.
        ///
        /// Roughly correlates to: acq rate = 1/count and max
        /// range = count^(1/4)
        /// </summary>
        private int MaximumAcquisitionCount
        {
            get
            {
                Span<byte> rawData = stackalloc byte[1];
                ReadBytes(Register.SIG_COUNT_VAL, rawData);
                return rawData[0];
            }
            set
            {
                WriteRegister(Register.SIG_COUNT_VAL, (byte)value);
            }
        }

        /// <summary>
        /// Get or set the threshold of peak value that bypasses the internal algorithm.
        ///
        /// Recommended non-default values are 32 for higher sensitivity
        /// but higher erronenous measurement and 96 for reduced
        /// sensitivity and fewer erroneous measurements.
        /// </summary>
        public int AlgorithmBypassThreshold
        {
            get
            {
                Span<byte> rawData = stackalloc byte[1];
                ReadBytes(Register.THRESHOLD_BYPASS, rawData);
                return rawData[0];
            }
            set
            {
                WriteRegister(Register.THRESHOLD_BYPASS, (byte)value);
            }
        }

        /// <summary>
        /// Get or set the power control option.
        /// </summary>
        public PowerMode PowerMode
        {
            get
            {
                Span<byte> rawData = stackalloc byte[1];
                ReadBytes(Register.POWER_CONTROL, rawData);
                return (PowerMode)rawData[0];
            }
            set
            {
                // Bit 0 disables receiver circuit
                WriteRegister(Register.POWER_CONTROL, (byte)value);
            }
        }

        /// <summary>
        /// Get the system status
        /// </summary>
        public SystemStatus Status
        {
            get
            {
                Span<byte> rawData = stackalloc byte[1];
                ReadBytes(Register.STATUS, rawData);
                return (SystemStatus)rawData[0];
            }
        }

        #endregion

        #region I2C

        private void WriteRegister(Register reg, byte data)
        {
            Span<byte> dataout = stackalloc byte[]
            {
                (byte)reg,
                data
            };
            _i2cDevice.Write(dataout);
        }

        private byte ReadByte(Register reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            return _i2cDevice.ReadByte();
        }

        private void ReadBytes(Register reg, Span<byte> readBytes)
        {
            _i2cDevice.WriteByte((byte)reg);
            _i2cDevice.Read(readBytes);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _gpioController?.Dispose();
                _gpioController = null;
            }

            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        #endregion
    }
}
