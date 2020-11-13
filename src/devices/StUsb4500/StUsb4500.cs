// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Iot.Device.Usb.Enumerations;
using Iot.Device.Usb.Objects;
using UnitsNet;

namespace Iot.Device.Usb
{
    /// <summary>USB PD sink controller STUSB4500.</summary>
    /// <remarks>This is based on code from the official STUSB4500 repo (https://github.com/usb-c/STUSB4500/).</remarks>
    public class StUsb4500 : IDisposable
    {
        /// <summary>STUSB4500 default I2C address.</summary>
        public const byte DefaultI2cAddress = 0x28;

        private I2cDevice _i2cDevice;

        /// <summary>Gets or sets the device Id.</summary>
        public byte DeviceId => ReadDeviceId();

        /// <summary>Gets the cable connection state.</summary>
        public UsbCCableConnection CableConnection => ReadCableConnection();

        /// <summary>Gets the power delivery objects from the sink.</summary>
        public PowerDeliveryObject[] SinkPowerDeliveryObjects { get => ReadSinkPdo(); set => WriteSinkPdo(value); }

        /// <summary>Gets the source power delivery objects.
        /// ATTENTION: This triggers a new USB PD contract negotiation and can cause a short power-disruption.</summary>
        public PowerDeliveryObject[] SourcePowerDeliveryObjects => ReadSourcePdo();

        /// <summary>Gets the request data object.</summary>
        public RequestDataObject RequestDataObject => ReadRdo();

        /// <summary>Gets the requested voltage.</summary>
        public ElectricPotentialDc RequestedVoltage => ReadRequestedVoltage();

        /// <summary>Gets or sets the NVM data.</summary>
        public byte[] NvmData { get => ReadNvm(); set => WriteNvm(value); }

        /// <summary>Initializes a new instance of the <see cref="StUsb4500"/> class.</summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public StUsb4500(I2cDevice i2cDevice) => _i2cDevice = i2cDevice;

        /// <summary>Finalizes an instance of the <see cref="StUsb4500"/> class.</summary>
        ~StUsb4500() => Dispose(false);

        /// <summary>Performs a USB PD software reset.</summary>
        public void PerformUsbPdSoftwareReset()
        {
            Span<byte> buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.TX_HEADER_LOW, StUsb4500Constants.SOFT_RESET
                };
            _i2cDevice.Write(buffer);
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.PD_COMMAND_CTRL, StUsb4500Constants.SEND_COMMAND
                };
            _i2cDevice.Write(buffer);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            _i2cDevice?.Dispose();
        }

        /// <summary>Reads the device Id.</summary>
        /// <returns>Device Id</returns>
        private byte ReadDeviceId()
        {
            _i2cDevice.WriteByte((byte)StUsb4500Register.Device_ID);
            return _i2cDevice.ReadByte();
        }

        /// <summary>Reads the cable connection.</summary>
        /// <returns>Cable status</returns>
        private UsbCCableConnection ReadCableConnection()
        {
            _i2cDevice.WriteByte((byte)StUsb4500Register.PORT_STATUS_1);
            byte status = _i2cDevice.ReadByte();
            if ((status & StUsb4500Constants.STUSBMASK_ATTACHED_STATUS) != StUsb4500Constants.VALUE_ATTACHED)
            {
                return UsbCCableConnection.Disconnected;
            }

            _i2cDevice.WriteByte((byte)StUsb4500Register.TYPEC_STATUS);
            status = _i2cDevice.ReadByte();
            return (status & StUsb4500Constants.MASK_REVERSE) == StUsb4500Constants.VALUE_NOT_ATTACHED ? UsbCCableConnection.CC1 : UsbCCableConnection.CC2;
        }

        /// <summary>Reads the sink's PDOs.</summary>
        /// <returns>The PDOs from the sink.</returns>
        private PowerDeliveryObject[] ReadSinkPdo()
        {
            _i2cDevice.WriteByte((byte)StUsb4500Register.DPM_PDO_NUMB);
            byte status = _i2cDevice.ReadByte();
            int localPdoCount = status & 0x03;

            Span<byte> buffer = stackalloc byte[localPdoCount * 4];
            uint[] pdoConfigurations = new uint[localPdoCount];
            _i2cDevice.WriteByte((byte)StUsb4500Register.DPM_SNK_PDO1_0);
            _i2cDevice.Read(buffer);
            for (int i = 0; i < localPdoCount; ++i)
            {
                int j = i * 4;
                pdoConfigurations[i] = (uint)(buffer[j] + (buffer[j + 1] << 8) + (buffer[j + 2] << 16) + (buffer[j + 3] << 24));
            }

            return pdoConfigurations.Select(PowerDeliveryObject.CreateFromValue).ToArray();
        }

        /// <summary>Writes the sink's PDOs.</summary>
        /// <param name="objects">The PDOs to write.</param>
        private void WriteSinkPdo(PowerDeliveryObject[] objects)
        {
            if (objects.Length <= 0 || objects.Length > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(objects));
            }

            // first byte is the address + 4 byte for each PDO
            Span<byte> buffer = stackalloc byte[1 + (objects.Length * 4)];
            buffer[0] = (byte)StUsb4500Register.DPM_SNK_PDO1_0;
            for (int i = 0; i < objects.Length; ++i)
            {
                int j = i * 4;
                uint pdoValue = objects[i].Value;
                buffer[j + 1] = (byte)pdoValue;
                buffer[j + 2] = (byte)(pdoValue >> 8);
                buffer[j + 3] = (byte)(pdoValue >> 16);
                buffer[j + 4] = (byte)(pdoValue >> 24);
            }

            _i2cDevice.Write(buffer);

            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.DPM_PDO_NUMB, (byte)objects.Length
                };
            _i2cDevice.Write(buffer);
        }

        /// <summary>Reads the source PDOs.</summary>
        /// <param name="retryCount">The retry count. If 0 no more retries are performed.</param>
        /// <returns>The PDOs from the source.</returns>
        private PowerDeliveryObject[] ReadSourcePdo(int retryCount = 3)
        {
            try
            {
                return GetSourcePdo();
            }
            catch (ArgumentException exception) when (exception.Message.Contains("readBuffer"))
            {
                // sometimes we get a "readBuffer cannot be empty." exception
                if (retryCount <= 0)
                {
                    // avoid endless tries in case of other problems
                    throw;
                }

                Trace.WriteLine(exception.Message);
                // try again, second try almost always works
                return ReadSourcePdo(retryCount - 1);
            }
        }

        /// <summary>Reads the source PDOs.</summary>
        /// <returns>The PDOs from the source.</returns>
        private PowerDeliveryObject[] GetSourcePdo()
        {
            int objectCount;
            UsbPdDataMessageType messageType;

            Span<byte> prtStatusBuffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.PRT_STATUS
                };
            Span<byte> rxHeaderBuffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.RX_HEADER_LOW
                };
            Span<byte> rxDataBuffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.RX_DATA_OBJ1_0
                };

            Span<byte> statusBuffer = stackalloc byte[1];
            Span<byte> headerBuffer = stackalloc byte[2];

            // to avoid endless-loop
            var cts = new CancellationTokenSource(500);
            // Source PDOs are only readable during negotiation, which happens after a reset
            PerformUsbPdSoftwareReset();

            do
            {
                bool msgReceived;
                do
                {
                    _i2cDevice.WriteRead(prtStatusBuffer, statusBuffer);
                    msgReceived = (statusBuffer[0] >> 2 & 0b1) == 1;
                }
                while (!msgReceived && !cts.IsCancellationRequested);

                _i2cDevice.WriteRead(rxHeaderBuffer, headerBuffer);
                var messageHeader = new UsbPdMessageHeader((ushort)(headerBuffer[0] + (headerBuffer[1] << 8)));
                messageType = messageHeader.DataMessageType;
                objectCount = messageHeader.NumberOfDataObjects;
            }
            while (objectCount <= 0 && messageType != UsbPdDataMessageType.USBPD_DATAMSG_Source_Capabilities && !cts.IsCancellationRequested);

            if (cts.IsCancellationRequested)
            {
                return Array.Empty<PowerDeliveryObject>();
            }

            // The original code does a check for the object count at this stage which is not used here as it can not really prevent the error mentioned below,
            // as the buffer-change can happen after the check and before the read.
            // It actually increases the probability of the error as it increases the time between message detection and reading the buffer.

            // Warning: Short Timing
            // There is ~3 ms time-frame to read the source capabilities, before the next Message ("Accept") arrives and overwrites the first bytes of RX_DATA_OBJ register
            Span<byte> dataBuffer = stackalloc byte[objectCount * 4];
            _i2cDevice.WriteRead(rxDataBuffer, dataBuffer);

            uint[] pdoConfigurations = new uint[objectCount];
            for (int i = 0; i < objectCount; ++i)
            {
                int j = i * 4;
                pdoConfigurations[i] = (uint)(dataBuffer[j] + (dataBuffer[j + 1] << 8) + (dataBuffer[j + 2] << 16) + (dataBuffer[j + 3] << 24));
            }

            return pdoConfigurations.Select(PowerDeliveryObject.CreateFromValue).ToArray();
        }

        /// <summary>Reads the RDO.</summary>
        /// <returns>The current RDO.</returns>
        private RequestDataObject ReadRdo()
        {
            Span<byte> buffer = stackalloc byte[4];
            _i2cDevice.WriteByte((byte)StUsb4500Register.RDO_REG_STATUS_0);
            _i2cDevice.Read(buffer);
            return new RequestDataObject((uint)(buffer[0] + (buffer[1] << 8) + (buffer[2] << 16) + (buffer[3] << 24)));
        }

        /// <summary>Reads the requested voltage.</summary>
        /// <returns>The requested voltage.</returns>
        private ElectricPotentialDc ReadRequestedVoltage()
        {
            _i2cDevice.WriteByte((byte)StUsb4500Register.MONITORING_CTRL_1);
            byte voltage = _i2cDevice.ReadByte();
            return ElectricPotentialDc.FromVoltsDc(voltage / 10.0);
        }

        /// <summary>Reads the NVM.</summary>
        /// <returns>Content of the NVM.</returns>
        private byte[] ReadNvm()
        {
            EnterNvmReadMode();

            byte[] nvmData = new byte[40];
            Span<byte> dataSpan = nvmData.AsSpan();
            ReadNvmSector(0, dataSpan.Slice(0, 8));
            ReadNvmSector(1, dataSpan.Slice(8, 8));
            ReadNvmSector(2, dataSpan.Slice(16, 8));
            ReadNvmSector(3, dataSpan.Slice(24, 8));
            ReadNvmSector(4, dataSpan.Slice(32, 8));

            ExitNvmMode();
            return nvmData;
        }

        /// <summary>Enters the NVM read mode.</summary>
        private void EnterNvmReadMode()
        {
            // set password
            Span<byte> buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CUST_PASSWORD_REG,
                    StUsb4500Constants.FTP_CUST_PASSWORD
                };
            _i2cDevice.Write(buffer);

            // NVM internal controller reset
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_0;
            buffer[1] = 0x00;
            _i2cDevice.Write(buffer);

            // Set PWR and RST_N bits
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_0;
            buffer[1] = StUsb4500Constants.FTP_CUST_PWR | StUsb4500Constants.FTP_CUST_RST_N;
            _i2cDevice.Write(buffer);
        }

        /// <summary>Reads a NVM sector.</summary>
        /// <param name="sectorNumber">The sector number. This must be a value between 0 - 4.</param>
        /// <param name="sectorDataBuffer">The sector data buffer.</param>
        private void ReadNvmSector(byte sectorNumber, Span<byte> sectorDataBuffer)
        {
            if (sectorNumber > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(sectorNumber));
            }

            // Set PWR and RST_N bits
            Span<byte> buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_0,
                    StUsb4500Constants.FTP_CUST_PWR | StUsb4500Constants.FTP_CUST_RST_N
                };
            _i2cDevice.Write(buffer);

            // Set Read Sectors Opcode
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_1;
            buffer[1] = StUsb4500Constants.READ & StUsb4500Constants.FTP_CUST_OPCODE_MASK;
            _i2cDevice.Write(buffer);

            // Load Read Sectors Opcode
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_0;
            buffer[1] = (byte)((sectorNumber & StUsb4500Constants.FTP_CUST_SECT) | StUsb4500Constants.FTP_CUST_PWR | StUsb4500Constants.FTP_CUST_RST_N | StUsb4500Constants.FTP_CUST_REQ);
            _i2cDevice.Write(buffer);

            // The FTP_CUST_REQ is cleared by NVM controller when the operation is finished.
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_0
                };
            Span<byte> readBuffer = stackalloc byte[1];
            do
            {
                _i2cDevice.WriteRead(buffer, readBuffer);
            }
            while ((readBuffer[0] & StUsb4500Constants.FTP_CUST_REQ) > 0);

            // Sectors Data are available in RW-BUFFER
            buffer[0] = (byte)StUsb4500Register.RW_BUFFER;
            _i2cDevice.WriteRead(buffer, sectorDataBuffer);

            // NVM internal controller reset
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_0,
                    0x00
                };
            _i2cDevice.Write(buffer);
        }

        /// <summary>Writes the NVM.</summary>
        /// <param name="nvmData">The NVM data.</param>
        public void WriteNvm(byte[] nvmData)
        {
            if (nvmData.Length != 40)
            {
                throw new ArgumentOutOfRangeException(nameof(nvmData));
            }

            EnterNvmWriteMode(StUsb4500Constants.SECTOR_0 | StUsb4500Constants.SECTOR_1 | StUsb4500Constants.SECTOR_2 | StUsb4500Constants.SECTOR_3 | StUsb4500Constants.SECTOR_4);

            Span<byte> dataSpan = nvmData.AsSpan();
            WriteNvmSector(0, dataSpan.Slice(0, 8));
            WriteNvmSector(1, dataSpan.Slice(8, 8));
            WriteNvmSector(2, dataSpan.Slice(16, 8));
            WriteNvmSector(3, dataSpan.Slice(24, 8));
            WriteNvmSector(4, dataSpan.Slice(32, 8));

            ExitNvmMode();
        }

        /// <summary>Enters the NVM write mode.</summary>
        private void EnterNvmWriteMode(byte erasedSectors)
        {
            // set password
            Span<byte> buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CUST_PASSWORD_REG,
                    StUsb4500Constants.FTP_CUST_PASSWORD
                };
            _i2cDevice.Write(buffer);

            // this register must be NULL for Partial Erase feature
            buffer[0] = (byte)StUsb4500Register.RW_BUFFER;
            buffer[1] = 0x00;
            _i2cDevice.Write(buffer);

            // NVM internal controller reset
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_0;
            _i2cDevice.Write(buffer);

            // Set PWR and RST_N bits
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_0;
            buffer[1] = StUsb4500Constants.FTP_CUST_PWR | StUsb4500Constants.FTP_CUST_RST_N;
            _i2cDevice.Write(buffer);

            // Set Write SER Opcode - Load 0xF1 to erase all sectors of FTP and Write SER Opcode
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_1;
            buffer[1] = (byte)(((byte)(erasedSectors << 3) & StUsb4500Constants.FTP_CUST_SER_MASK) | (StUsb4500Constants.WRITE_SER & StUsb4500Constants.FTP_CUST_OPCODE_MASK));
            _i2cDevice.Write(buffer);

            // Load Write SER Opcode
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_0;
            buffer[1] = StUsb4500Constants.FTP_CUST_PWR | StUsb4500Constants.FTP_CUST_RST_N | StUsb4500Constants.FTP_CUST_REQ;
            _i2cDevice.Write(buffer);

            // Wait for execution
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_0
                };
            Span<byte> readBuffer = stackalloc byte[1];
            do
            {
                _i2cDevice.WriteRead(buffer, readBuffer);
            }
            while ((readBuffer[0] & StUsb4500Constants.FTP_CUST_REQ) > 0);

            // Set Soft Prog Opcode
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_1,
                    StUsb4500Constants.SOFT_PROG_SECTOR & StUsb4500Constants.FTP_CUST_OPCODE_MASK
                };
            _i2cDevice.Write(buffer);

            // Load Soft Prog Opcode
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_0;
            buffer[1] = StUsb4500Constants.FTP_CUST_PWR | StUsb4500Constants.FTP_CUST_RST_N | StUsb4500Constants.FTP_CUST_REQ;
            _i2cDevice.Write(buffer);

            // Wait for execution
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_0
                };
            do
            {
                _i2cDevice.WriteRead(buffer, readBuffer);
            }
            while ((readBuffer[0] & StUsb4500Constants.FTP_CUST_REQ) > 0);

            // Set Erase Sectors Opcode
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_1,
                    StUsb4500Constants.ERASE_SECTOR & StUsb4500Constants.FTP_CUST_OPCODE_MASK
                };
            _i2cDevice.Write(buffer);

            // Load Erase Sectors Opcode
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_0;
            buffer[1] = StUsb4500Constants.FTP_CUST_PWR | StUsb4500Constants.FTP_CUST_RST_N | StUsb4500Constants.FTP_CUST_REQ;
            _i2cDevice.Write(buffer);

            // Wait for execution
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_0
                };
            do
            {
                _i2cDevice.WriteRead(buffer, readBuffer);
            }
            while ((readBuffer[0] & StUsb4500Constants.FTP_CUST_REQ) > 0);
        }

        /// <summary>Writes a NVM sector.</summary>
        /// <param name="sectorNumber">The sector.</param>
        /// <param name="data">The data.</param>
        private void WriteNvmSector(byte sectorNumber, Span<byte> data)
        {
            // Write the 64-bit data to be written in the sector
            Span<byte> buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.RW_BUFFER,
                    data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7]
                };
            _i2cDevice.Write(buffer);

            // Set PWR and RST_N bits
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_0,
                    StUsb4500Constants.FTP_CUST_PWR | StUsb4500Constants.FTP_CUST_RST_N
                };
            _i2cDevice.Write(buffer);

            // NVM Program Load Register to write with the 64-bit data to be written in sector
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_1;
            buffer[1] = StUsb4500Constants.WRITE_PL & StUsb4500Constants.FTP_CUST_OPCODE_MASK;
            _i2cDevice.Write(buffer);

            // Load Write to PL Sectors Opcode
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_0;
            buffer[1] = StUsb4500Constants.FTP_CUST_PWR | StUsb4500Constants.FTP_CUST_RST_N | StUsb4500Constants.FTP_CUST_REQ;
            _i2cDevice.Write(buffer);

            // Wait for execution
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_0
                };
            Span<byte> readBuffer = stackalloc byte[1];
            do
            {
                _i2cDevice.WriteRead(buffer, readBuffer);
            }
            while ((readBuffer[0] & StUsb4500Constants.FTP_CUST_REQ) > 0);

            // NVM Program Load Register to write with the 64-bit data to be written in sector
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_1,
                    StUsb4500Constants.PROG_SECTOR & StUsb4500Constants.FTP_CUST_OPCODE_MASK
                };
            _i2cDevice.Write(buffer);

            // Load Prog Sectors Opcode
            buffer[0] = (byte)StUsb4500Register.FTP_CTRL_0;
            buffer[1] = (byte)((sectorNumber & StUsb4500Constants.FTP_CUST_SECT) | StUsb4500Constants.FTP_CUST_PWR | StUsb4500Constants.FTP_CUST_RST_N | StUsb4500Constants.FTP_CUST_REQ);
            _i2cDevice.Write(buffer);

            // Wait for execution
            buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_0
                };
            do
            {
                _i2cDevice.WriteRead(buffer, readBuffer);
            }
            while ((readBuffer[0] & StUsb4500Constants.FTP_CUST_REQ) > 0);
        }

        /// <summary>Exits the test mode.</summary>
        private void ExitNvmMode()
        {
            // clear register
            Span<byte> buffer = stackalloc byte[]
                {
                    (byte)StUsb4500Register.FTP_CTRL_0,
                    StUsb4500Constants.FTP_CUST_RST_N, 0x00
                };
            _i2cDevice.Write(buffer);

            // clear password
            buffer[0] = (byte)StUsb4500Register.FTP_CUST_PASSWORD_REG;
            buffer[1] = 0x00;
            _i2cDevice.Write(buffer);
        }
    }
}
