// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace Iot.Device.Nrf24l01
{
    /// <summary>
    /// Single chip 2.4 GHz Transceiver nRF24L01
    /// </summary>
    public class Nrf24l01 : IDisposable
    {
        private readonly int _ce;
        private readonly int _irq;

        private readonly byte[] _empty = Array.Empty<byte>();

        private GpioController _gpio;
        private SpiDevice _sensor;
        private bool _shouldDispose;

        #region prop

        private byte _packetSize;

        /// <summary>
        /// nRF24L01 Receive Packet Size
        /// </summary>
        public byte PacketSize
        {
            get { return _packetSize; }
            set { _packetSize = value < 0 || value > 32 ? throw new ArgumentOutOfRangeException("PacketSize needs to be in the range of 0 to 32.") : value; }
        }

        /// <summary>
        /// nRF24L01 Send Address
        /// </summary>
        public byte[] Address
        {
            get => ReadTxAddress();
            set => SetTxAddress(value);
        }

        /// <summary>
        /// nRF24L01 Working Channel
        /// </summary>
        public byte Channel
        {
            get => ReadChannel();
            set => SetChannel(value);
        }

        /// <summary>
        /// nRF24L01 Output Power
        /// </summary>
        public OutputPower OutputPower
        {
            get => ReadOutputPower();
            set => SetOutputPower(value);
        }

        /// <summary>
        /// nRF24L01 Send Rate
        /// </summary>
        public DataRate DataRate
        {
            get => ReadDataRate();
            set => SetDataRate(value);
        }

        /// <summary>
        /// nRF24L01 Power Mode
        /// </summary>
        public PowerMode PowerMode
        {
            get => ReadPowerMode();
            set => SetPowerMode(value);
        }

        /// <summary>
        /// nRF24L01 Working Mode
        /// </summary>
        public WorkingMode WorkingMode
        {
            get => ReadWorkwingMode();
            set => SetWorkingMode(value);
        }

        /// <summary>
        /// nRF24L01 Pipe 0
        /// </summary>
        public Nrf24l01Pipe Pipe0 { get; private set; }

        /// <summary>
        /// nRF24L01 Pipe 1
        /// </summary>
        public Nrf24l01Pipe Pipe1 { get; private set; }

        /// <summary>
        /// nRF24L01 Pipe 2
        /// </summary>
        public Nrf24l01Pipe Pipe2 { get; private set; }

        /// <summary>
        /// nRF24L01 Pipe 3
        /// </summary>
        public Nrf24l01Pipe Pipe3 { get; private set; }

        /// <summary>
        /// nRF24L01 Pipe 4
        /// </summary>
        public Nrf24l01Pipe Pipe4 { get; private set; }

        /// <summary>
        /// nRF24L01 Pipe 5
        /// </summary>
        public Nrf24l01Pipe Pipe5 { get; private set; }

        #endregion

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
        /// <param name="gpioController"><see cref="GpioController"/> related with operations on pins</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Nrf24l01(SpiDevice sensor, int ce, int irq, byte packetSize, byte channel = 2,
            OutputPower outputPower = OutputPower.N00dBm, DataRate dataRate = DataRate.Rate2Mbps, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _sensor = sensor ?? throw new ArgumentException(nameof(sensor));
            _ce = ce;
            _irq = irq;
            PacketSize = packetSize;
            _shouldDispose = shouldDispose || gpioController is null;

            Initialize(pinNumberingScheme, outputPower, dataRate, channel, gpioController);
            InitializePipe();
#if NETCOREAPP2_1 || NETCOREAPP3_1
            if (_gpio is null ||
                Pipe0 is null ||
                Pipe1 is null ||
                Pipe2 is null ||
                Pipe3 is null ||
                Pipe4 is null ||
                Pipe5 is null)
                {
                    throw new Exception($"{nameof(Nrf24l01)} is incorrectly configuted");
                }
#endif
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="data">Send Data</param>
        public void Send(byte[] data)
        {
            // Details in the Datasheet P65
            SetWorkingMode(WorkingMode.Transmit);
            // wait for some time so that it doesn't go too fast
            Thread.Sleep(1);

            Write(Command.NRF_W_TX_PAYLOAD, Register.NRF_NOOP, data);

            _gpio.Write(_ce, PinValue.High);
            Thread.Sleep(1);
            _gpio.Write(_ce, PinValue.Low);
            Thread.Sleep(1);

            SetWorkingMode(WorkingMode.Receive);
            Thread.Sleep(1);
        }

        /// <summary>
        /// Receive
        /// </summary>
        /// <param name="length">Packet Size</param>
        /// <returns>Data</returns>
        public Span<byte> Receive(byte length)
        {
            // Details in the Datasheet P65
            _gpio.Write(_ce, PinValue.Low);

            Span<byte> writeData = stackalloc byte[length];
            Span<byte> readData = WriteRead(Command.NRF_R_RX_PAYLOAD, Register.NRF_NOOP, writeData);

            Span<byte> ret = new byte[readData.Length];
            readData.CopyTo(ret);

            _gpio.Write(_ce, PinValue.Low);
            // clear RX FIFO interrupt
            Write(Command.NRF_W_REGISTER, Register.NRF_STATUS, 0b_0100_1110);
            _gpio.Write(_ce, PinValue.High);

            return ret;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _sensor?.Dispose();
            _sensor = null!;

            if (_shouldDispose)
            {
                _gpio?.Dispose();
                _gpio = null!;
            }
        }

        /// <summary>
        /// Delegate representing event that data was received
        /// </summary>
        /// <param name="sender">Object sending the event</param>
        /// <param name="e">Arguments for the event</param>
        public delegate void DataReceivedHandle(object sender, DataReceivedEventArgs e);

        /// <summary>
        /// Triggering when data was received
        /// </summary>
        public event DataReceivedHandle? DataReceived;

        private void Irq_ValueChanged(object sender, PinValueChangedEventArgs args)
        {
            if (DataReceived is object)
            {
                DataReceived(sender, new DataReceivedEventArgs(Receive(_packetSize).ToArray()));
            }

        }

        #region private and internal

        /// <summary>
        /// Initialize
        /// </summary>
#if !NETCOREAPP2_1 && !NETCOREAPP3_1
        [MemberNotNull(nameof(_gpio))]
#endif
        private void Initialize(PinNumberingScheme pinNumberingScheme, OutputPower outputPower, DataRate dataRate, byte channel, GpioController? gpioController)
        {
            // open pins
            _gpio = gpioController ?? new GpioController(pinNumberingScheme);
            _gpio.OpenPin(_ce, PinMode.Output);
            _gpio.OpenPin(_irq, PinMode.Input);
            _gpio.RegisterCallbackForPinValueChangedEvent(_irq, PinEventTypes.Falling, Irq_ValueChanged);

            // wait for some time
            Thread.Sleep(10);

            // Details in the datasheet P53
            _gpio.Write(_ce, PinValue.Low);
            Write(Command.NRF_FLUSH_TX, Register.NRF_NOOP, _empty);
            Write(Command.NRF_FLUSH_RX, Register.NRF_NOOP, _empty);
            // reflect RX_DR interrupt, TX_DS interrupt not reflected, power up, receive mode
            Write(Command.NRF_W_REGISTER, Register.NRF_CONFIG, 0b_0011_1011);
            _gpio.Write(_ce, PinValue.High);

            SetAutoAck(false);
            SetChannel(channel);
            SetOutputPower(outputPower);
            SetDataRate(dataRate);
            SetRxPayload(_packetSize);
        }

        /// <summary>
        /// Initialize nRF24L01 Pipe
        /// </summary>
#if !NETCOREAPP2_1 && !NETCOREAPP3_1
        [MemberNotNull(nameof(Pipe0), nameof(Pipe0), nameof(Pipe1), nameof(Pipe2), nameof(Pipe3), nameof(Pipe4), nameof(Pipe5))]
#endif
        private void InitializePipe()
        {
            Pipe0 = new Nrf24l01Pipe(this, 0);
            Pipe1 = new Nrf24l01Pipe(this, 1);
            Pipe2 = new Nrf24l01Pipe(this, 2);
            Pipe3 = new Nrf24l01Pipe(this, 3);
            Pipe4 = new Nrf24l01Pipe(this, 4);
            Pipe5 = new Nrf24l01Pipe(this, 5);
        }

        /// <summary>
        /// Set nRF24L01 Receive Packet Size (All Pipe)
        /// </summary>
        /// <param name="payload">Size, from 0 to 32</param>
        internal void SetRxPayload(byte payload)
        {
            // Details in the Datasheet P56
            if (payload > 32 || payload < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(payload), $"{nameof(payload)} needs to be in the range of 0 to 32.");
            }

            _gpio.Write(_ce, PinValue.Low);

            Span<byte> writeData = stackalloc byte[1]
            {
                payload
            };

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
        internal void SetRxPayload(byte pipe, byte payload)
        {
            // Details in the Datasheet P56
            if (payload > 32 || payload < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(payload), $"{nameof(payload)} needs to be in the range of 0 to 32.");
            }

            if (pipe > 5 || pipe < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pipe), $"{nameof(pipe)} needs to be in the range of 0 to 5.");
            }

            Span<byte> writeData = stackalloc byte[]
            {
                (byte)((byte)Command.NRF_W_REGISTER + (byte)Register.NRF_RX_PW_P0 + pipe),
                payload
            };

            _gpio.Write(_ce, PinValue.Low);

            Write(writeData);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Read nRF24L01 Receive Packet Size
        /// </summary>
        /// <param name="pipe">Pipe, form 0 to 5</param>
        /// <returns>Size</returns>
        internal byte ReadRxPayload(byte pipe)
        {
            // Details in the Datasheet P56
            if (pipe > 5 || pipe < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pipe), $"{nameof(pipe)} needs to be in the range of 0 to 5.");
            }

            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, (Register)((byte)Register.NRF_RX_PW_P0 + pipe), 1);

            return readData[0];
        }

        /// <summary>
        /// Set nRF24L01 Auto Acknowledgment (All Pipe)
        /// </summary>
        /// <param name="isAutoAck">Is Enable</param>
        internal void SetAutoAck(bool isAutoAck)
        {
            // Details in the Datasheet P53
            _gpio.Write(_ce, PinValue.Low);

            if (isAutoAck)
            {
                // all pipe
                Write(Command.NRF_W_REGISTER, Register.NRF_EN_AA, 0b_0011_1111);
            }
            else
            {
                Write(Command.NRF_W_REGISTER, Register.NRF_EN_AA, 0b_0000_0000);
            }

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Auto Acknowledgment
        /// </summary>
        /// <param name="pipe">Pipe, form 0 to 5</param>
        /// <param name="isAutoAck">Is Enable</param>
        internal void SetAutoAck(byte pipe, bool isAutoAck)
        {
            // Details in the Datasheet P53
            if (pipe > 5 || pipe < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pipe), $"{nameof(pipe)} needs to be in the range of 0 to 5.");
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
        /// Read nRF24L01 Auto Acknowledgment
        /// </summary>
        /// <param name="pipe">Pipe, form 0 to 5</param>
        /// <returns>Is Enable</returns>
        internal bool ReadAutoAck(byte pipe)
        {
            // Details in the Datasheet P53
            if (pipe > 5 || pipe < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pipe), $"{nameof(pipe)} needs to be in the range of 0 to 5.");
            }

            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_EN_AA, 1);

            byte ret = (byte)(readData[0] >> pipe & 0b_0000_0001);
            if (ret == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Set nRF24L01 Receive Pipe (All Pipe)
        /// </summary>
        /// <param name="isEnable">Is Enable Receive</param>
        internal void SetRxPipe(bool isEnable)
        {
            // Details in the Datasheet P54
            _gpio.Write(_ce, PinValue.Low);

            if (isEnable)
            {
                // all pipe
                Write(Command.NRF_W_REGISTER, Register.NRF_EN_RXADDR, 0b_0011_1111);
            }
            else
            {
                Write(Command.NRF_W_REGISTER, Register.NRF_EN_RXADDR, 0b_0000_0000);
            }

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Set nRF24L01 Receive Pipe is Enable
        /// </summary>
        /// <param name="pipe">Pipe, form 0 to 5</param>
        /// <param name="isEnable">Is Enable the Pipe Receive</param>
        internal void SetRxPipe(byte pipe, bool isEnable)
        {
            // Details in the Datasheet P54
            if (pipe > 5 || pipe < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pipe), $"{nameof(pipe)} needs to be in the range of 0 to 5.");
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
        /// Read nRF24L01 Receive Pipe is Enable
        /// </summary>
        /// <param name="pipe">Pipe, form 0 to 5</param>
        /// <returns>Is Enable the Pipe Receive</returns>
        internal bool ReadRxPipe(byte pipe)
        {
            // Details in the Datasheet P54
            if (pipe > 5 || pipe < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pipe), $"{nameof(pipe)} needs to be in the range of 0 to 5.");
            }

            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_EN_RXADDR, 1);

            byte ret = (byte)(readData[0] >> pipe & 0x01);
            if (ret == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Set nRF24L01 Power Mode
        /// </summary>
        /// <param name="mode">Power Mode</param>
        internal void SetPowerMode(PowerMode mode)
        {
            // Details in the Datasheet P53
            // Register:0x00, bit[1]
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
        /// Read nRF24L01 Power Mode
        /// </summary>
        /// <returns>Power Mode</returns>
        internal PowerMode ReadPowerMode()
        {
            // Details in the Datasheet P53
            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_CONFIG, 1);

            return (PowerMode)((readData[0] >> 1) & 0b_0000_0001);
        }

        /// <summary>
        /// Set nRF24L01 Working Mode
        /// </summary>
        /// <param name="mode">Working Mode</param>
        internal void SetWorkingMode(WorkingMode mode)
        {
            // Details in the Datasheet P53
            // Register:0x00, bit[0]
            _gpio.Write(_ce, PinValue.Low);

            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_CONFIG, 1);

            byte setting;
            switch (mode)
            {
                case WorkingMode.Receive:
                    setting = (byte)(readData[0] | 1);
                    break;
                case WorkingMode.Transmit:
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
        /// Read nRF24L01 Working Mode
        /// </summary>
        /// <returns>Working Mode</returns>
        internal WorkingMode ReadWorkwingMode()
        {
            // Details in the Datasheet P53
            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_CONFIG, 1);

            return (WorkingMode)(readData[0] & 0b_0000_0001);
        }

        /// <summary>
        /// Set nRF24L01 Output Power
        /// </summary>
        /// <param name="power">Output Power</param>
        internal void SetOutputPower(OutputPower power)
        {
            // Details in the Datasheet P54
            // Register: 0x06, bit[2:1]
            _gpio.Write(_ce, PinValue.Low);

            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_RF_SETUP, 1);

            byte setting = (byte)(readData[0] & (~0b_0000_0110) | ((byte)power << 1));

            Write(Command.NRF_W_REGISTER, Register.NRF_RF_SETUP, setting);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Read nRF24L01 Output Power
        /// </summary>
        /// <returns>Output Power</returns>
        internal OutputPower ReadOutputPower()
        {
            // Details in the Datasheet P54
            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_RF_SETUP, 1);

            return (OutputPower)((readData[0] & 0b_0000_0110) >> 1);
        }

        /// <summary>
        /// Set nRF24L01 Send Rate
        /// </summary>
        /// <param name="rate">Send Rate</param>
        internal void SetDataRate(DataRate rate)
        {
            // Details in the Datasheet P54
            // Register: 0x06, bit[3]
            _gpio.Write(_ce, PinValue.Low);

            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_RF_SETUP, 1);

            byte setting = (byte)(readData[0] & (~0b_0000_1000) | ((byte)rate << 1));

            Write(Command.NRF_W_REGISTER, Register.NRF_RF_SETUP, setting);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Read nRF24L01 Send Rate
        /// </summary>
        /// <returns>Send Rate</returns>
        internal DataRate ReadDataRate()
        {
            // Details in the Datasheet P54
            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_RF_SETUP, 1);

            return (DataRate)((readData[0] & 0b_0000_1000) >> 3);
        }

        /// <summary>
        /// Set nRF24L01 Receive Address
        /// </summary>
        /// <param name="pipe">Pipe, form 0 to 5</param>
        /// <param name="address">Address, if (pipe > 1) then (address.Length = 1), else if (pipe = 1 || pipe = 0) then (address.Length ≤ 5)</param>
        internal void SetRxAddress(byte pipe, ReadOnlySpan<byte> address)
        {
            // Details in the Datasheet P55
            if (pipe > 5 || pipe < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pipe), $"{nameof(pipe)} needs to be in the range of 0 to 5.");
            }

            if (address.Length > 5 || address.Length < 1)
            {
                throw new ArgumentOutOfRangeException("Array Length must less than 6.");
            }

            if (pipe > 1 && address.Length > 1)
            {
                throw new ArgumentOutOfRangeException("Array Length must equal 1 when pipe more than 1. Address equal pipe1's address the first 4 byte + one byte your custom.");
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
        /// Read nRF24L01 Receive Address
        /// </summary>
        /// <param name="pipe">Pipe, form 0 to 5</param>
        /// <returns>Address</returns>
        internal byte[] ReadRxAddress(byte pipe)
        {
            // Details in the Datasheet P55
            if (pipe > 5 || pipe < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pipe), $"{nameof(pipe)} needs to be in the range of 0 to 5.");
            }

            int length = 5;
            if (pipe > 1)
            {
                length = 1;
            }

            byte[] readData = WriteRead(Command.NRF_R_REGISTER, (Register)((byte)Register.NRF_RX_ADDR_P0 + pipe), length);

            return readData;
        }

        /// <summary>
        /// Set nRF24L01 Send Address
        /// </summary>
        /// <param name="address">Address, address.Length = 5</param>
        internal void SetTxAddress(ReadOnlySpan<byte> address)
        {
            // Details in the Datasheet P56
            if (address.Length > 5 || address.Length < 1)
            {
                throw new ArgumentOutOfRangeException("Array Length must less than 6.");
            }

            _gpio.Write(_ce, PinValue.Low);

            Write(Command.NRF_W_REGISTER, Register.NRF_TX_ADDR, address);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Read nRF24L01 Send Address
        /// </summary>
        /// <returns>Address</returns>
        internal byte[] ReadTxAddress()
        {
            // Details in the Datasheet P56
            return WriteRead(Command.NRF_R_REGISTER, Register.NRF_TX_ADDR, 5);
        }

        /// <summary>
        /// Set Working Channel
        /// </summary>
        /// <param name="channel">From 0 to 127</param>
        internal void SetChannel(byte channel)
        {
            // Details in the Datasheet P54
            // Register: 0x05, bit[6:0]
            if (channel < 0 || channel > 127)
            {
                throw new ArgumentOutOfRangeException("Channel needs to be in the range of 0 to 127.");
            }

            _gpio.Write(_ce, PinValue.Low);

            Write(Command.NRF_W_REGISTER, Register.NRF_RF_CH, channel);

            _gpio.Write(_ce, PinValue.High);
        }

        /// <summary>
        /// Read Working Channel
        /// </summary>
        /// <returns>Channel</returns>
        internal byte ReadChannel()
        {
            // Details in the Datasheet P54
            Span<byte> readData = WriteRead(Command.NRF_R_REGISTER, Register.NRF_RF_CH, 1);

            return readData[0];
        }
        #endregion

        #region sensor operation
        internal void Write(ReadOnlySpan<byte> writeData)
        {
            Span<byte> readBuf = stackalloc byte[writeData.Length];

            _sensor.TransferFullDuplex(writeData, readBuf);
        }

        internal void Write(Command command, Register register, byte writeByte)
        {
            Span<byte> writeBuf = stackalloc byte[2]
            {
                (byte)((byte)command + (byte)register),
                writeByte
            };
            Span<byte> readBuf = stackalloc byte[2];

            _sensor.TransferFullDuplex(writeBuf, readBuf);
        }

        internal void Write(Command command, Register register, ReadOnlySpan<byte> writeData)
        {
            Span<byte> writeBuf = stackalloc byte[1 + writeData.Length];
            Span<byte> readBuf = stackalloc byte[1 + writeData.Length];

            writeBuf[0] = (byte)((byte)command + (byte)register);
            writeData.CopyTo(writeBuf.Slice(1));

            _sensor.TransferFullDuplex(writeBuf, readBuf);
        }

        internal byte[] WriteRead(Command command, Register register, int dataLength)
        {
            Span<byte> writeBuf = stackalloc byte[1 + dataLength];
            Span<byte> readBuf = stackalloc byte[1 + dataLength];

            writeBuf[0] = (byte)((byte)command + (byte)register);

            _sensor.TransferFullDuplex(writeBuf, readBuf);

            return readBuf.Slice(1).ToArray();
        }

        internal byte[] WriteRead(Command command, Register register, ReadOnlySpan<byte> writeData)
        {
            Span<byte> writeBuf = stackalloc byte[1 + writeData.Length];
            Span<byte> readBuf = stackalloc byte[1 + writeData.Length];

            writeBuf[0] = (byte)((byte)command + (byte)register);
            writeData.CopyTo(writeBuf.Slice(1));

            _sensor.TransferFullDuplex(writeBuf, readBuf);

            return readBuf.Slice(1).ToArray();
        }
        #endregion
    }
}
