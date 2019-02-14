// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

namespace Iot.Device.Nrf24l01
{
    /// <summary>
    /// Single chip 2.4 GHz Transceiver nRF24L01
    /// </summary>
    public class Nrf24l01 : IDisposable
    {
        private GpioController _gpio = null;
        private SpiDevice _sensor = null;

        private readonly int _ce;
        private readonly int _irq;

        private readonly byte[] _empty = new byte[0];

        private byte _packetSize;
        /// <summary>
        /// nRF24L01 Receive Packet Size
        /// </summary>
        public byte PacketSize
        {
            get { return _packetSize; }
            set { _packetSize = value < 0 || value > 32 ? throw new ArgumentOutOfRangeException("PacketSize needs to be in the range of 0 to 32!") : value; }
        }

        #region SpiSettings
        /// <summary>
        /// NRF24L01 SPI Clock Frequency
        /// </summary>
        public const int SpiClockFrequency = 2000000;

        /// <summary>
        /// NRF24L01 SPI Mode
        /// </summary>
        public const SpiMode SpiMode = System.Device.Spi.SpiMode.Mode0;
        #endregion

        /// <summary>
        /// Creates a new instance of the nRF24L01.
        /// </summary>
        /// <param name="sensor">The communications channel to a device on a SPI bus</param>
        /// <param name="ce">CE Pin</param>
        /// <param name="irq">IRQ Pin</param>
        /// <param name="packetSize">Receive Packet Size</param>
        /// <param name="channel">Working Channel</param>
        /// <param name="outputPower">Output Power</param>
        /// <param name="dataRate">Send Data Rate</param>
        /// <param name="pinNumberingScheme">Pin Numbering Scheme</param>
        public Nrf24l01(SpiDevice sensor, int ce, int irq, byte packetSize, byte channel = 2,
            OutputPower outputPower = OutputPower.N00dBm, DataRate dataRate = DataRate.Rate2Mbps, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical)
        {
            _sensor = sensor;
            _ce = ce;
            _irq = irq;
            PacketSize = packetSize;

            Initialize(pinNumberingScheme, outputPower, dataRate, channel);
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void Initialize(PinNumberingScheme pinNumberingScheme, OutputPower outputPower, DataRate dataRate, byte channel)
        {
            _gpio = new GpioController(pinNumberingScheme);
            _gpio.OpenPin(_ce, PinMode.Output);
            _gpio.OpenPin(_irq, PinMode.Input);
            _gpio.RegisterCallbackForPinValueChangedEvent(_irq, PinEventTypes.Falling, Irq_ValueChanged);

            Thread.Sleep(50);

            // Details in the datasheet P53
            _gpio.Write(_ce, PinValue.Low);
            Write(Command.NRF_FLUSH_TX, Register.NRF_NOOP, _empty);
            Write(Command.NRF_FLUSH_RX, Register.NRF_NOOP, _empty);
            Write(Command.NRF_W_REGISTER, Register.NRF_CONFIG, 0x3B);
            _gpio.Write(_ce, PinValue.High);

            SetAutoAck(false);
            SetChannel(channel);
            SetOutputPower(outputPower);
            SetDataRate(dataRate);
            SetRxPayload(_packetSize);
        }

        /// <summary>
        /// Set nRF24L01 Receive Packet Size (All Pipe)
        /// </summary>
        /// <param name="payload">Size, from 0 to 32</param>
        public void SetRxPayload(byte payload)
        {
            if (payload > 32 || payload < 0)
            {
                throw new ArgumentOutOfRangeException("payload", "payload needs to be in the range of 0 to 32!");
            }

            _gpio.Write(_ce, PinValue.Low);

            Span<byte> writeData = stackalloc byte[1] { payload };

            Write(Command.NRF_W_REGISTER, Register.NRF_RX_PW_P0, writeData);
            Write(Command.NRF_W_REGISTER, Register.NRF_RX_PW_P1, writeData);
            Write(Command.NRF_W_REGISTER, Register.NRF_RX_PW_P2, writeData);
            Write(Command.NRF_W_REGISTER, Register.NRF_RX_PW_P3, writeData);
            Write(Command.NRF_W_REGISTER, Register.NRF_RX_PW_P4, writeData);
            Write(Command.NRF_W_REGISTER, Register.NRF_RX_PW_P5, writeData);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Receive Packet Size
        /// </summary>
        /// <param name="pipe">Pipe, form 0 to 5</param>
        /// <param name="payload">Size, from 0 to 32</param>
        public void SetRxPayload(byte pipe, byte payload)
        {
            if (payload > 32 || payload < 0)
            {
                throw new ArgumentOutOfRangeException("payload", "payload needs to be in the range of 0 to 32!");
            }

            if (pipe > 5 || pipe < 0)
            {
                throw new ArgumentOutOfRangeException("pipe", "pipe needs to be in the range of 0 to 5!");
            }

            Span<byte> writeData = stackalloc byte[] { (byte)((byte)Command.NRF_W_REGISTER + (byte)Register.NRF_RX_PW_P0 + pipe), payload };

            _gpio.Write(_ce, PinValue.Low);

            Write(writeData);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Auto Acknowledgment (All Pipe)
        /// </summary>
        /// <param name="isAutoAck">Is Enable</param>
        public void SetAutoAck(bool isAutoAck)
        {
            _gpio.Write(_ce, PinValue.Low);

            if (isAutoAck)
            {
                Write(Command.NRF_W_REGISTER, Register.NRF_EN_AA, 0x3F);
            }
            else
            {
                Write(Command.NRF_W_REGISTER, Register.NRF_EN_AA, 0x00);
            }

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Auto Acknowledgment
        /// </summary>
        /// <param name="pipe">Pipe, form 0 to 5</param>
        /// <param name="isAutoAck">Is Enable</param>
        public void SetAutoAck(byte pipe, bool isAutoAck)
        {
            if (pipe > 5 || pipe < 0)
            {
                throw new ArgumentOutOfRangeException("pipe", "pipe needs to be in the range of 0 to 5!");
            }

            _gpio.Write(_ce, PinValue.Low);

            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_EN_AA, 1);

            byte setting;
            if (isAutoAck)
            {
                setting = (byte)(readData[0] | (1 << pipe));
            }
            else
            {
                setting = (byte)(readData[0] & ~(1 << pipe));
            }

            Write(Command.NRF_W_REGISTER, Register.NRF_EN_AA, setting);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Receive Pipe (All Pipe)
        /// </summary>
        /// <param name="isEnable">Is Enable Receive</param>
        public void SetRxPipe(bool isEnable)
        {
            _gpio.Write(_ce, PinValue.Low);

            if (isEnable)
            {
                Write(Command.NRF_W_REGISTER, Register.NRF_EN_RXADDR, 0x3F);
            }
            else
            {
                Write(Command.NRF_W_REGISTER, Register.NRF_EN_RXADDR, 0x00);
            }

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Receive Pipe
        /// </summary>
        /// <param name="pipe">Pipe, form 0 to 5</param>
        /// <param name="isEnable">Is Enable the Pipe Receive</param>
        public void SetRxPipe(byte pipe, bool isEnable)
        {
            if (pipe > 5 || pipe < 0)
            {
                throw new ArgumentOutOfRangeException("pipe", "pipe needs to be in the range of 0 to 5!");
            }

            _gpio.Write(_ce, PinValue.Low);

            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_EN_RXADDR, 1);

            byte setting;
            if (isEnable)
            {
                setting = (byte)(readData[0] | (1 << pipe));
            }
            else
            {
                setting = (byte)(readData[0] & ~(1 << pipe));
            }

            Write(Command.NRF_W_REGISTER, Register.NRF_EN_RXADDR, setting);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Power Mode
        /// </summary>
        /// <param name="mode">Power Mode</param>
        public void SetPowerMode(PowerMode mode)
        {
            _gpio.Write(_ce, PinValue.Low);

            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_CONFIG, 1);

            byte setting;
            switch (mode)
            {
                case PowerMode.UP:
                    setting = (byte)(readData[0] | (1 << 1));
                    break;
                case PowerMode.DOWN:
                    setting = (byte)(readData[0] & ~(1 << 1));
                    break;
                default:
                    setting = (byte)(readData[0] | (1 << 1));
                    break;
            }

            Write(Command.NRF_W_REGISTER, Register.NRF_CONFIG, setting);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Working Mode
        /// </summary>
        /// <param name="mode">Working Mode</param>
        public void SetWorkingMode(WorkingMode mode)
        {
            _gpio.Write(_ce, PinValue.Low);

            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_CONFIG, 1);

            byte setting;
            switch (mode)
            {
                case WorkingMode.RX:
                    setting = (byte)(readData[0] | 1);
                    break;
                case WorkingMode.TX:
                    setting = (byte)(readData[0] & ~1);
                    break;
                default:
                    setting = (byte)(readData[0] | 1);
                    break;
            }

            Write(Command.NRF_W_REGISTER, Register.NRF_CONFIG, setting);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Output Power
        /// </summary>
        /// <param name="power">Output Power</param>
        public void SetOutputPower(OutputPower power)
        {
            _gpio.Write(_ce, PinValue.Low);

            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_RF_SETUP, 1);

            byte setting = (byte)(readData[0] & (~0x06) | ((byte)power << 1));

            Write(Command.NRF_W_REGISTER, Register.NRF_RF_SETUP, setting);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Send Rate
        /// </summary>
        /// <param name="rate">Send Rate</param>
        public void SetDataRate(DataRate rate)
        {
            _gpio.Write(_ce, PinValue.Low);

            Span<byte> readData = stackalloc byte[1];

            WriteRead(Command.NRF_R_REGISTER, Register.NRF_RF_SETUP, _empty);

            byte setting = (byte)(readData[0] & (~0x08) | ((byte)rate << 1));

            Write(Command.NRF_W_REGISTER, Register.NRF_RF_SETUP, setting);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Receive Address
        /// </summary>
        /// <param name="pipe">Pipe, form 0 to 5</param>
        /// <param name="address">Address, if (pipe > 1) then (address.Length = 1), else if (pipe = 1 || pipe = 0) then (address.Length ≤ 5)</param>
        public void SetRxAddress(byte pipe, Span<byte> address)
        {
            if (address.Length > 5)
            {
                throw new ArgumentOutOfRangeException("Array Length must less than 6!");
            }

            if (pipe > 1 && address.Length > 1)
            {
                throw new ArgumentOutOfRangeException("Array Length must equal 1 when pipe more than 1. Address equal pipe1's address the first 4 byte + one byte your custom!");
            }

            Span<byte> writeData = stackalloc byte[1 + address.Length];
            writeData[0] = (byte)((byte)Command.NRF_W_REGISTER + (byte)Register.NRF_RX_ADDR_P0 + pipe);
            for (int i = 0; i < address.Length; i++)
            {
                writeData[i + 1] = address[i];
            }

            _gpio.Write(_ce, PinValue.Low);

            Write(writeData);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Send Address
        /// </summary>
        /// <param name="address">Address, address.Length = 5</param>
        public void SetTxAddress(Span<byte> address)
        {
            if (address.Length > 5)
            {
                throw new ArgumentOutOfRangeException("Array Length must less than 6!");
            }

            _gpio.Write(_ce, PinValue.Low);

            Write(Command.NRF_W_REGISTER, Register.NRF_TX_ADDR, address);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set Working Channel
        /// </summary>
        /// <param name="channel">From 0 to 127</param>
        public void SetChannel(byte channel)
        {
            if (channel < 0 || channel > 127)
            {
                throw new ArgumentOutOfRangeException("Channel needs to be in the range of 0 to 127!");
            }

            _gpio.Write(_ce, PinValue.Low);

            Write(Command.NRF_W_REGISTER, Register.NRF_RF_CH, channel);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="data">Send Data</param>
        public void Send(byte[] data)
        {
            SetWorkingMode(WorkingMode.TX);
            Thread.Sleep(4);

            Write(Command.NRF_W_TX_PAYLOAD, Register.NRF_NOOP, data);

            _gpio.Write(_ce, PinValue.High);
            Thread.Sleep(1);
            _gpio.Write(_ce, PinValue.Low);
            Thread.Sleep(10);

            SetWorkingMode(WorkingMode.RX);
            Thread.Sleep(1);
        }

        /// <summary>
        /// Receive
        /// </summary>
        /// <param name="length">Packet Size</param>
        /// <returns>Data</returns>
        public Span<byte> Receive(byte length)
        {
            _gpio.Write(_ce, PinValue.Low);

            Span<byte> writeData = stackalloc byte[length];
            Span<byte> readData = WriteRead(Command.NRF_R_RX_PAYLOAD, Register.NRF_NOOP, writeData);

            Span<byte> ret = new byte[readData.Length];
            readData.CopyTo(ret);

            _gpio.Write(_ce, PinValue.Low);
            Write(Command.NRF_W_REGISTER, Register.NRF_STATUS, 0x4E);
            _gpio.Write(_ce, PinValue.High);

            return ret;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            if (_sensor != null)
            {
                _sensor.Dispose();
            }
        }

        public delegate void DataReceivedHandle(object sender, DataReceivedEventArgs e);

        /// <summary>
        /// Triggering when data was received
        /// </summary>
        public event DataReceivedHandle DataReceived;

        private void Irq_ValueChanged(object sender, PinValueChangedEventArgs args)
        {
            DataReceived(sender, new DataReceivedEventArgs(Receive(_packetSize).ToArray()));
        }

        #region sensor operation
        private void Write(Span<byte> writeData)
        {
            Span<byte> readBuf = stackalloc byte[writeData.Length];

            _sensor.TransferFullDuplex(writeData, readBuf);
        }

        private void Write(Command command, Register register, byte writeByte)
        {
            Span<byte> writeBuf = stackalloc byte[2] { (byte)((byte)command + (byte)register), writeByte };
            Span<byte> readBuf = stackalloc byte[2];

            _sensor.TransferFullDuplex(writeBuf, readBuf);
        }

        private void Write(Command command, Register register, Span<byte> writeData)
        {
            Span<byte> writeBuf = stackalloc byte[1 + writeData.Length];
            Span<byte> readBuf = stackalloc byte[1 + writeData.Length];

            writeBuf[0] = (byte)((byte)command + (byte)register);
            for (int i = 0; i < writeData.Length; i++)
            {
                writeBuf[1 + i] = writeData[i];
            }

            _sensor.TransferFullDuplex(writeBuf, readBuf);
        }

        private Span<byte> WriteRead(Command command, Register register, int dataLength)
        {
            Span<byte> writeBuf = stackalloc byte[1 + dataLength];
            Span<byte> readBuf = new byte[1 + dataLength];

            writeBuf[0] = (byte)((byte)command + (byte)register);

            _sensor.TransferFullDuplex(writeBuf, readBuf);

            return readBuf.Slice(1);
        }

        private Span<byte> WriteRead(Command command, Register register, Span<byte> writeData)
        {
            Span<byte> writeBuf = stackalloc byte[1 + writeData.Length];
            Span<byte> readBuf = new byte[1 + writeData.Length];

            writeBuf[0] = (byte)((byte)command + (byte)register);
            for (int i = 0; i < writeData.Length; i++)
            {
                writeBuf[1 + i] = writeData[i];
            }

            _sensor.TransferFullDuplex(writeBuf, readBuf);

            return readBuf.Slice(1);
        }
        #endregion
    }
}
