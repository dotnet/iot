// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device;
using System.Device.I2c;

namespace Iot.Device.Seesaw
{
    /// <summary>
    /// Represents Seesaw device
    /// </summary>
    public partial class Seesaw : IDisposable
    {
        private const byte SessawHardwareId = 0x55;

        /// <summary>
        /// I2C device used for communication
        /// </summary>
        /// <value></value>
        public I2cDevice I2cDevice { get; private set; }

        private uint _options;

        /// <summary>
        /// Initializes new instance of Seesaw.
        /// </summary>
        /// <param name="i2cDevice">The I2cDevice to be used to communicate with the SeeSaw module. Note that the I2cDevice
        /// will be disposed when the along with the SeeSaw device</param>
        public Seesaw(I2cDevice i2cDevice)
        {
            I2cDevice = i2cDevice;
            Initialize(i2cDevice);
        }

        /// <summary>
        /// Version of the SeeSaw module.
        /// </summary>
        public uint Version { get; private set; }

        /// <summary>
        /// Reads the temperature of the SeeSaw device.
        /// </summary>
        /// <returns>A float that represents the temperature in degrees celcius.</returns>
        public float GetTemperature() => (1.0F / (1UL << 16)) * BinaryPrimitives.ReadUInt32BigEndian(Read(SeesawModule.Status, SeesawFunction.StatusTemp, 4, 1000));

        /// <summary>
        /// Tests to see if a module has been compiled into the SeeSaw firmware.
        /// </summary>
        /// <param name="moduleAddress">>An Seesaw_Module enum that represents the mdule to test for.</param>
        /// <returns>Returns true if the functionality associated with the module is available.</returns>
        public bool HasModule(SeesawModule moduleAddress) => (_options & (1 << (byte)moduleAddress)) != 0;

        /// <summary>
        /// Performs a soft reset of the SeeSaw module.
        /// </summary>
        public void SoftwareReset()
        {
            WriteByte(SeesawModule.Status, SeesawFunction.StatusSwrst, 0xFF);
        }

        /// <summary>
        /// Initializes the Seesaw device.
        /// <param name="i2cDevice">The I2cDevice to initialize the Seesaw device with.</param>
        /// </summary>
        protected void Initialize(I2cDevice i2cDevice)
        {
            SoftwareReset();
            DelayHelper.DelayMilliseconds(10, true);

            if (ReadByte(SeesawModule.Status, SeesawFunction.StatusHwId) != SessawHardwareId)
            {
                throw new NotSupportedException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not appear to be an Adafruit SeeSaw module");
            }

            _options = GetOptions();
            Version = GetVersion();
        }

        /// <summary>
        /// Get the firmware version of the Seesaw board.
        /// </summary>
        /// <returns>Returns the seesaw version.</returns>
        protected uint GetVersion() => BinaryPrimitives.ReadUInt32BigEndian(Read(SeesawModule.Status, SeesawFunction.StatusVersion, 4));

        /// <summary>
        /// Gets the options/modules present on the Seesaw board
        /// </summary>
        /// <returns>Returns 32 bit integer where the bits represent the options/modules present on the Seesaw board.</returns>
        protected uint GetOptions() => BinaryPrimitives.ReadUInt32BigEndian(Read(SeesawModule.Status, SeesawFunction.StatusOptions, 4));

        /// <summary>
        /// Write a byte to the I2cDevice connected to the Seesaw board.
        /// </summary>
        /// <param name="moduleAddress">An Seesaw_Module enum that represents the module that we are writing to.</param>
        /// <param name="functionAddress">An Seesaw_Function enum that represents the Seesaw function to be called.</param>
        /// <param name="value">The byte value ro be send as a parameter to the Seesaw device.</param>
        protected void WriteByte(SeesawModule moduleAddress, SeesawFunction functionAddress, byte value)
        {
            Write(moduleAddress, functionAddress, new byte[] { value });
        }

        /// <summary>
        /// Write a series of bytes to the I2cDevice connected to the Seesaw board.
        /// </summary>
        /// <param name="moduleAddress">An Seesaw_Module enum that represents the module that we are writing to.</param>
        /// <param name="functionAddress">An Seesaw_Function enum that represents the Seesaw function to be called.</param>
        /// <param name="data">The Span containg data to be send as a parameter or parameters to the Seesaw device.</param>
        protected void Write(SeesawModule moduleAddress, SeesawFunction functionAddress, ReadOnlySpan<byte> data)
        {
            const int StackThreshold = 512;

            Span<byte> buffer = data.Length < StackThreshold ? stackalloc byte[data.Length + 2] : new byte[data.Length + 2];

            buffer[0] = (byte)moduleAddress;
            buffer[1] = (byte)functionAddress;
            data.CopyTo(buffer.Slice(2));

            I2cDevice.Write(buffer);
        }

        /// <summary>
        /// Read a byte from the I2cDevice connected to the Seesaw board.
        /// </summary>
        /// <param name="moduleAddress">A Seesaw_Module enum that represents the module that we are reading from.</param>
        /// <param name="functionAddress">A Seesaw_Function enum that represents the Seesaw function to be called.</param>
        /// <param name="readDelayMicroSeconds">The delay between sending the function and readinng the data in microseconds.</param>
        /// <returns>Returns the byte value from the Seesaw device.</returns>
        protected byte ReadByte(SeesawModule moduleAddress, SeesawFunction functionAddress, short readDelayMicroSeconds = 0) => Read(moduleAddress, functionAddress, 1, readDelayMicroSeconds)[0];

        /// <summary>
        /// Read a byte array from the I2cDevice connected to the Seesaw board.
        /// </summary>
        /// <param name="moduleAddress">A Seesaw_Module enum that represents the module that we are reading from.</param>
        /// <param name="functionAddress">A Seesaw_Function enum that represents the Seesaw function to be called.</param>
        /// <param name="length">The number of bytes that are expected to be returned from the Seesaw device.</param>
        /// <param name="readDelayMicroSeconds">The delay between sending the function and readinng the data in microseconds.</param>
        /// <returns>Returns the byte array representaing values from the Seesaw device.</returns>
        protected byte[] Read(SeesawModule moduleAddress, SeesawFunction functionAddress, int length, short readDelayMicroSeconds = 0)
        {
            byte[] retval = new byte[length];

            Span<byte> bytesToWrite = stackalloc byte[2]
            {
                (byte)moduleAddress, (byte)functionAddress
            };

            I2cDevice.Write(bytesToWrite);

            if (readDelayMicroSeconds > 0)
            {
                DelayHelper.DelayMicroseconds(readDelayMicroSeconds, true);
            }

            I2cDevice.Read(retval);

            return retval;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            I2cDevice?.Dispose();
            I2cDevice = null!;
        }
    }
}
