// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.I2c;
using System.IO;
using System.Linq;
using Iot.Device.PiJuiceDevice.Models;

namespace Iot.Device.PiJuiceDevice
{
    /// <summary>
    /// Create a PiJuice class
    /// </summary>
    public class PiJuice : IDisposable
    {
        private const byte MaxRetries = 4;
        private const int ReadRetryDelay = 50;

        private readonly bool _shouldDispose;

        private I2cDevice _i2cDevice;

        /// <summary>
        /// The default PiJuice I2C address is 0x14
        /// Other addresses can be uses, see PiJuice documentation
        /// </summary>
        public const byte DefaultI2cAddress = 0x14;

        /// <summary>
        /// Contains the PiJuice key information
        /// </summary>
        public PiJuiceInfo PiJuiceInfo { get; internal set; }

        /// <summary>Creates a new instance of the PiJuice.</summary>
        /// <param name="i2cDevice">The I2C device. Device address is 0x14</param>
        /// <param name="shouldDispose">True to dispose the I2C device</param>
        public PiJuice(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _shouldDispose = shouldDispose;
            PiJuiceInfo = new PiJuiceInfo(GetFirmwareVerion());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null!;
            }
        }

        /// <summary>
        /// Get the firmware version
        /// </summary>
        /// <returns>PiJuice firmware version</returns>
        public Version GetFirmwareVerion()
        {
            var response = ReadCommand(PiJuiceCommand.FirmwareVersion, 2);

            var major_version = response[0] >> 4;
            var minor_version = response[0] & 0x0F;

            return new Version(major_version, minor_version);
        }

        /// <summary>
        /// (https://github.com/PiSupply/PiJuice/blob/02c129c3aa884934c368936c9363df90690148a8/Software/Source/pijuice.py#L1531)
        /// </summary>
        public void SetDefaultConfiguration()
        {
            WriteCommand(PiJuiceCommand.ResetToDefault, new byte[] { 0xAA, 0x55, 0x0A, 0xA3 });
        }

        /// <summary>
        /// The write protection of the EEPROM
        /// </summary>
        /// <value>Whether the EEPROM is write protected</value>
        public bool IdEepromWriteProtect
        {
            get
            {
                var response = ReadCommand(PiJuiceCommand.IdEepromWriteProtect, 1);

                return response[0] == 1;
            }
            set
            {
                WriteCommandVerify(PiJuiceCommand.IdEepromWriteProtect, new byte[] { (byte)(value ? 1 : 0) });
            }
        }

        /// <summary>
        /// The physical I2C address of the EEPROM
        /// </summary>
        /// <value>The I2C Address of the EEPROM</value>
        public IdEepromAddress IdEepromAddress
        {
            get
            {
                var response = ReadCommand(PiJuiceCommand.IdEepromAddress, 1);

                return (IdEepromAddress)response[0];
            }
            set
            {
                WriteCommandVerify(PiJuiceCommand.IdEepromAddress, new byte[] { (byte)value });
            }
        }

        /// <summary>
        /// Write a PiJuice command
        /// </summary>
        /// <param name="command">The PiJuice command</param>
        /// <param name="data">The data to write</param>
        internal void WriteCommand(PiJuiceCommand command, ReadOnlySpan<byte> data)
        {
            byte tries = 0;
            Span<byte> buffer = stackalloc byte[data.Length + 2];

            data.CopyTo(buffer.Slice(1));

            buffer[0] = (byte)command;
            buffer[buffer.Length - 1] = GetCheckSum(data, checkLastByte: true);

            // When writing/reading to the I2C port, PiJuice doesn't respond on time in some cases
            // So we wait a little bit before retrying
            // In most cases, the I2C read/write can go thru without waiting
            while (tries < MaxRetries)
            {
                try
                {
                    _i2cDevice.Write(buffer);
                    return;
                }
                catch (IOException ex)
                {
                    tries++;

                    if (tries >= MaxRetries)
                    {
                        throw new IOException($"{nameof(WriteCommand)}: Failed to write command {command}", ex);
                    }

                    DelayHelper.DelayMilliseconds(ReadRetryDelay, false);
                }
            }
        }

        /// <summary>
        /// Write a PiJuice command and verify
        /// </summary>
        /// <param name="command">The PiJuice command</param>
        /// <param name="data">The data to write</param>
        /// <param name="delay">The delay before reading the data</param>
        internal void WriteCommandVerify(PiJuiceCommand command, ReadOnlySpan<byte> data, int delay = 0)
        {
            WriteCommand(command, data);

            if (delay > 0)
            {
                DelayHelper.DelayMilliseconds(delay, false);
            }

            var response = ReadCommand(command, (byte)data.Length);

            if (!data.SequenceEqual(response))
            {
                throw new IOException($"{nameof(WriteCommandVerify)}: Failed to write command {command}");
            }
        }

        /// <summary>
        /// Read data from PiJuice
        /// </summary>
        /// <param name="command">The PiJuice command</param>
        /// <param name="length">The length of the data to be read</param>
        /// <returns>Returns an array of the bytes read</returns>
        internal byte[] ReadCommand(PiJuiceCommand command, byte length)
        {
            byte tries = 0;
            Span<byte> outArray = stackalloc byte[length + 1];
            outArray.Clear();

            // When writing/reading the I2C port, PiJuice doesn't respond on time in some cases
            // So we wait a little bit before retrying
            // In most cases, the I2C read/write can go thru without waiting
            while (tries < MaxRetries)
            {
                try
                {
                    _i2cDevice.Write(new byte[] { (byte)command });
                    _i2cDevice.Read(outArray);

                    var checksum = GetCheckSum(outArray);
                    if (checksum != outArray[length])
                    {
                        outArray[0] |= 0x80;
                        checksum = GetCheckSum(outArray);
                        if (checksum != outArray[length])
                        {
                            tries++;

                            if (tries >= MaxRetries)
                            {
                                throw new InvalidDataException($"{nameof(ReadCommand)}: Invalid checksum read command {command}, checksum {checksum}");
                            }

                            DelayHelper.DelayMilliseconds(ReadRetryDelay, false);
                            continue;
                        }
                    }

                    break;
                }
                catch (IOException ex)
                {
                    tries++;

                    if (tries >= MaxRetries - 1)
                    {
                        throw new IOException($"{nameof(ReadCommand)}: Failed to read command {command}", ex);
                    }

                    DelayHelper.DelayMilliseconds(ReadRetryDelay, false);
                }
            }

            return outArray.Slice(0, length).ToArray();
        }

        /// <summary>Gets the check sum.</summary>
        /// <param name="data">The data.</param>
        /// <param name="checkLastByte">Whether the last byte in the data is included in the checksum</param>
        /// <returns>Checksum</returns>
        private byte GetCheckSum(ReadOnlySpan<byte> data, bool checkLastByte = false)
        {
            byte fcs = 0xff;

            for (int i = 0; i < data.Length - (checkLastByte ? 0 : 1); i++)
            {
                fcs = (byte)(fcs ^ data[i]);
            }

            return fcs;
        }
    }
}
