// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.IO;
using System.Threading;
using Iot.Device.Mcp25xxx.Models;
using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.CanControl;
using Iot.Device.Mcp25xxx.Register.MessageReceive;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Iot.Device.Mcp25xxx.Tests.Register.CanControl;

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// A general purpose driver for the Microchip MCP25 CAN controller device family.
    /// </summary>
    public abstract class Mcp25xxx : IDisposable
    {
        /// The MCP2515 implements three transmit buffers. Each of these buffers occupies 14 bytes of SRAM
        private const int TransmitBufferMaxSize = 14;
        private readonly int _reset;
        private readonly int _tx0rts;
        private readonly int _tx1rts;
        private readonly int _tx2rts;
        private readonly int _interrupt;
        private readonly int _rx0bf;
        private readonly int _rx1bf;
        private readonly int _clkout;
        internal GpioController? _gpioController;
        private bool _shouldDispose;
        private SpiDevice _spiDevice;

        /// <summary>
        /// A general purpose driver for the Microchip MCP25 CAN controller device family.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="reset">The output pin number that is connected to Reset.</param>
        /// <param name="tx0rts">The output pin number that is connected to Tx0RTS.</param>
        /// <param name="tx1rts">The output pin number that is connected to Tx1RTS.</param>
        /// <param name="tx2rts">The output pin number that is connected to Tx2RTS.</param>
        /// <param name="interrupt">The input pin number that is connected to INT.</param>
        /// <param name="rx0bf">The input pin number that is connected to Rx0BF.</param>
        /// <param name="rx1bf">The input pin number that is connected to Rx1BF.</param>
        /// <param name="clkout">The input pin number that is connected to CLKOUT.</param>
        /// <param name="gpioController">
        /// The GPIO controller for defined external pins. If not specified, the default controller will be used.
        /// </param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Mcp25xxx(
            SpiDevice spiDevice,
            int reset = -1,
            int tx0rts = -1,
            int tx1rts = -1,
            int tx2rts = -1,
            int interrupt = -1,
            int rx0bf = -1,
            int rx1bf = -1,
            int clkout = -1,
            GpioController? gpioController = null,
            bool shouldDispose = true)
        {
            _spiDevice = spiDevice;
            _shouldDispose = shouldDispose || gpioController is null;

            _reset = reset;
            _tx0rts = tx0rts;
            _tx1rts = tx1rts;
            _tx2rts = tx2rts;
            _interrupt = interrupt;
            _rx0bf = rx0bf;
            _rx1bf = rx1bf;
            _clkout = clkout;

            // Only need controller if there are external pins provided.
            if (_reset != -1 ||
                _tx0rts != -1 ||
                _tx1rts != -1 ||
                _tx2rts != -1 ||
                _interrupt != -1 ||
                _rx0bf != -1 ||
                _rx1bf != -1 ||
                _clkout != -1)
            {
                _gpioController = gpioController ?? new GpioController();

                if (_reset != -1)
                {
                    _gpioController.OpenPin(_reset, PinMode.Output);
                    ResetPin = PinValue.Low;
                }

                if (_tx0rts != -1)
                {
                    _gpioController.OpenPin(_tx0rts, PinMode.Output);
                }

                if (_tx1rts != -1)
                {
                    _gpioController.OpenPin(_tx1rts, PinMode.Output);
                }

                if (_tx2rts != -1)
                {
                    _gpioController.OpenPin(_tx2rts, PinMode.Output);
                }

                if (_interrupt != -1)
                {
                    _gpioController.OpenPin(_interrupt, PinMode.Input);
                }

                if (_rx0bf != -1)
                {
                    _gpioController.OpenPin(_rx0bf, PinMode.Input);
                }

                if (_rx1bf != -1)
                {
                    _gpioController.OpenPin(_rx1bf, PinMode.Input);
                }

                if (_clkout != -1)
                {
                    _gpioController.OpenPin(_clkout, PinMode.Input);
                }
            }
        }

        /// <summary>
        /// Writes a value to Tx0RTS pin.
        /// </summary>
        public PinValue Tx0RtsPin
        {
            set
            {
                if (_gpioController is null)
                {
                    throw new Exception("GPIO controller is not configured");
                }

                _gpioController.Write(_tx0rts, value);
            }
        }

        /// <summary>
        /// Writes a value to Tx1RTS pin.
        /// </summary>
        public PinValue Tx1RtsPin
        {
            set
            {
                if (_gpioController is null)
                {
                    throw new Exception("GPIO controller is not configured");
                }

                _gpioController.Write(_tx1rts, value);
            }
        }

        /// <summary>
        /// Writes a value to Tx2RTS pin.
        /// </summary>
        public PinValue Tx2RtsPin
        {
            set
            {
                if (_gpioController is null)
                {
                    throw new Exception("GPIO controller is not configured");
                }

                _gpioController.Write(_tx2rts, value);
            }
        }

        /// <summary>
        /// Writes a value to Reset pin.
        /// </summary>
        public PinValue ResetPin
        {
            set
            {
                if (_gpioController is null)
                {
                    throw new Exception("GPIO controller is not configured");
                }

                _gpioController.Write(_reset, value);
            }
        }

        /// <summary>
        /// Reads the current value of Interrupt pin.
        /// </summary>
        public PinValue InterruptPin
        {
            get
            {
                if (_gpioController is null)
                {
                    throw new Exception("GPIO controller is not configured");
                }

                return _gpioController.Read(_interrupt);
            }
        }

        /// <summary>
        /// Reads the current value of Rx0BF pin.
        /// </summary>
        public PinValue Rx0BfPin
        {
            get
            {
                if (_gpioController is null)
                {
                    throw new Exception("GPIO controller is not configured");
                }

                return _gpioController.Read(_rx0bf);
            }
        }

        /// <summary>
        /// Reads the current value of Rx1BF pin.
        /// </summary>
        public PinValue Rx1BfPin
        {
            get
            {
                if (_gpioController is null)
                {
                    throw new Exception("GPIO controller is not configured");
                }

                return _gpioController.Read(_rx1bf);
            }
        }

        /// <summary>
        /// Resets the internal registers to the default state and sets Configuration mode.
        /// </summary>
        public void Reset()
        {
            _spiDevice.WriteByte((byte)InstructionFormat.Reset);
        }

        /// <summary>
        /// If RXB0 contains a valid message and another valid message is received,
        /// an overflow error will not occur and the new message will be moved into RXB1
        /// </summary>
        public void EnableRollover()
        {
            WriteByte(
                new RxB0Ctrl(
                    false,
                    true,
                    false,
                    OperatingMode.TurnsMaskFiltersOff));
        }

        /// <summary>
        /// The configuration registers (CNF1, CNF2, CNF3) control the bit timing for the CAN bus interface.
        /// </summary>
        /// <param name="frequencyAndSpeed">CAN bus frequency and speed</param>
        public void SetBitrate(FrequencyAndSpeed frequencyAndSpeed)
        {
            var (cnf1Config, cnf2Config, cnf3Config) = McpBitrate.GetBitTimingConfiguration(frequencyAndSpeed);
            WriteByte(Address.Cnf1, cnf1Config);
            WriteByte(Address.Cnf2, cnf2Config);
            WriteByte(Address.Cnf3, cnf3Config);
        }

        /// <summary>
        /// Set mode of operation
        /// </summary>
        /// <param name="operationMode">type of operation Mode</param>
        public void SetMode(OperationMode operationMode)
        {
            WriteByte(
                new CanCtrl(
                    CanCtrl.PinPrescaler.ClockDivideBy8,
                    true,
                    false,
                    false,
                    operationMode));
        }

        /// <summary>
        /// Read arrived messages
        /// </summary>
        /// <returns>List of messages received</returns>
        public ReceivedCanMessage[] ReadMessages()
        {
            var rxStatusResponse = RxStatus();

            switch (rxStatusResponse.ReceivedMessage)
            {
                case RxStatusResponse.ReceivedMessageType.MessageInRxB0:
                    byte[] messageRxB0 = ReadRxBuffer(RxBufferAddressPointer.RxB0Sidh, TransmitBufferMaxSize);
                    return new[] { new ReceivedCanMessage(ReceiveBuffer.RxB0, messageRxB0) };
                case RxStatusResponse.ReceivedMessageType.MessageInRxB1:
                    byte[] messageRxB1 = ReadRxBuffer(RxBufferAddressPointer.RxB1Sidh, TransmitBufferMaxSize);
                    return new[] { new ReceivedCanMessage(ReceiveBuffer.RxB1, messageRxB1) };
                case RxStatusResponse.ReceivedMessageType.MessagesInBothBuffers:
                    var firstMessage = ReadRxBuffer(RxBufferAddressPointer.RxB0Sidh, TransmitBufferMaxSize);
                    var secondMessage = ReadRxBuffer(RxBufferAddressPointer.RxB1Sidh, TransmitBufferMaxSize);
                    return new[] { new ReceivedCanMessage(ReceiveBuffer.RxB0, firstMessage), new ReceivedCanMessage(ReceiveBuffer.RxB1, secondMessage) };
                case RxStatusResponse.ReceivedMessageType.NoRxMessage:
                    return Array.Empty<ReceivedCanMessage>();
                default:
                    throw new Exception(
                        $"Invalid value for {nameof(rxStatusResponse.ReceivedMessage)}: {rxStatusResponse.ReceivedMessage}.");
            }
        }

        /// <summary>
        /// Reads data from the register beginning at the selected address.
        /// </summary>
        /// <param name="address">The address to read.</param>
        /// <returns>The value of address read.</returns>
        public byte Read(Address address)
        {
            const byte dontCare = 0x00;
            ReadOnlySpan<byte> writeBuffer = stackalloc byte[]
            {
                (byte)InstructionFormat.Read,
                (byte)address,
                dontCare
            };
            Span<byte> readBuffer = stackalloc byte[3];
            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
            return readBuffer[2];
        }

        /// <summary>
        /// When reading a receive buffer, reduces the overhead of a normal READ
        /// command by placing the Address Pointer at one of four locations for the receive buffer.
        /// </summary>
        /// <param name="addressPointer">The Address Pointer to one of four locations for the receive buffer.</param>
        /// <param name="byteCount">Number of bytes to read.  This must be one or more to read.</param>
        /// <returns>The value of address read.</returns>
        public byte[] ReadRxBuffer(RxBufferAddressPointer addressPointer, int byteCount = 1)
        {
            if (byteCount < 1)
            {
                throw new ArgumentException("Invalid number of bytes.", nameof(byteCount));
            }

            const int StackThreshold = 31; // Usually won't read more than this at a time.

            Span<byte> writeBuffer =
                byteCount < StackThreshold ? stackalloc byte[byteCount + 1] : new byte[byteCount + 1];

            Span<byte> readBuffer =
                byteCount < StackThreshold ? stackalloc byte[byteCount + 1] : new byte[byteCount + 1];

            // This instruction has a base value of 0x90.
            // The 2nd and 3rd bits are used for the pointer address for reading.
            writeBuffer[0] = (byte)((byte)InstructionFormat.ReadRxBuffer | ((byte)addressPointer << 1));
            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
            return readBuffer.Slice(1).ToArray();
        }

        /// <summary>
        /// Writes one byte to the register beginning at the selected address.
        /// </summary>
        /// <param name="address">The address to write the data.</param>
        /// <param name="value">The value to be written.</param>
        public void WriteByte(Address address, byte value)
        {
            ReadOnlySpan<byte> buffer = stackalloc byte[1]
            {
                value
            };
            Write(address, buffer);
        }

        /// <summary>
        /// Writes a byte to the selected register address.
        /// </summary>
        /// <param name="register">The register to write the data.</param>
        public void WriteByte(IRegister register)
        {
            Write(register.Address, new byte[]
            {
                register.ToByte()
            });
        }

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="message">CAN message</param>
        public void SendMessage(SendingCanMessage message)
        {
            var txBuffer = GetEmptyTxBuffer();
            SendMessageFromBuffer(txBuffer, message);
            const int tries = 10;
            for (var i = 0; i < tries; i++)
            {
                if (IsMessageSend(txBuffer))
                {
                    return;
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(5));
            }

            AbortAllPendingTransmissions();
            throw new IOException($"Cannot Send: {message.Id}#{string.Join(";", message.Data)}");
        }

        /// <summary>
        /// Get witch buffer empty now
        /// </summary>
        /// <returns>Buffer</returns>
        public TransmitBuffer GetEmptyTxBuffer()
        {
            var readStatusResponse = ReadStatus();
            var tx0Full = readStatusResponse.HasFlag(ReadStatusResponse.Tx0Req);
            var tx1Full = readStatusResponse.HasFlag(ReadStatusResponse.Tx1Req);
            var tx2Full = readStatusResponse.HasFlag(ReadStatusResponse.Tx2Req);

            if (!tx0Full)
            {
                return TransmitBuffer.Tx0;
            }

            if (!tx1Full)
            {
                return TransmitBuffer.Tx1;
            }

            if (!tx2Full)
            {
                return TransmitBuffer.Tx2;
            }

            return TransmitBuffer.None;
        }

        /// <summary>
        /// Send message from specific buffer
        /// </summary>
        /// <param name="transmitBuffer">Buffer</param>
        /// <param name="message">CAN message</param>
        public void SendMessageFromBuffer(TransmitBuffer transmitBuffer, SendingCanMessage message)
        {
            var (instructionsAddress, dataAddress) = GetInstructionsAddress(transmitBuffer);
            var txBufferNumber = TxBxSidh.GetTxBufferNumber(instructionsAddress);
            ReadOnlySpan<byte> buffer = stackalloc byte[5]
            {
                new TxBxSidh(txBufferNumber, message.Id[0]).ToByte(),
                new TxBxSidl(txBufferNumber, message.Id[1]).ToByte(),
                new TxBxEid8(txBufferNumber, message.Id[2]).ToByte(),
                new TxBxEid0(txBufferNumber, message.Id[3]).ToByte(),
                new TxBxDlc(txBufferNumber, message.Data.Length, false).ToByte()
            };

            Write(instructionsAddress, buffer);
            Write(dataAddress, (byte[]?)message.Data);
            SendFromBuffer(transmitBuffer);
        }

        /// <summary>
        /// Get instructions address from buffer
        /// </summary>
        /// <param name="transmitBuffer">Type of transmit buffer</param>
        /// <returns>Instructions for specific buffer</returns>
        public Tuple<Address, Address> GetInstructionsAddress(TransmitBuffer transmitBuffer)
        {
            return transmitBuffer switch
            {
                TransmitBuffer.Tx0 => new Tuple<Address, Address>(Address.TxB0Sidh, Address.TxB0D0),
                TransmitBuffer.Tx1 => new Tuple<Address, Address>(Address.TxB1Sidh, Address.TxB1D0),
                TransmitBuffer.Tx2 => new Tuple<Address, Address>(Address.TxB2Sidh, Address.TxB2D0),
                TransmitBuffer.None => throw new ArgumentException("Can not use this Tx buffer", nameof(transmitBuffer), null),
                _ => throw new ArgumentOutOfRangeException(nameof(transmitBuffer), transmitBuffer, null)
            };
        }

        /// <summary>
        /// Command to mcp25xx to send bytes from specific buffer
        /// </summary>
        /// <param name="transmitBuffer">Type of transmit buffer</param>
        public void SendFromBuffer(TransmitBuffer transmitBuffer)
        {
            switch (transmitBuffer)
            {
                case TransmitBuffer.Tx0:
                    RequestToSend(true, false, false);
                    break;
                case TransmitBuffer.Tx1:
                    RequestToSend(false, true, false);
                    break;
                case TransmitBuffer.Tx2:
                    RequestToSend(false, false, true);
                    break;
                case TransmitBuffer.None:
                    throw new ArgumentException("Can not use this Tx buffer", nameof(transmitBuffer), null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(transmitBuffer), transmitBuffer, null);
            }
        }

        /// <summary>
        /// Check is buffer empty
        /// </summary>
        /// <param name="transmitBuffer">buffer type</param>
        /// <returns></returns>
        public bool IsMessageSend(TransmitBuffer transmitBuffer)
        {
            var readStatusResponse = ReadStatus();

            switch (transmitBuffer)
            {
                case TransmitBuffer.Tx0 when readStatusResponse.HasFlag(ReadStatusResponse.Tx0If) &&
                                       !readStatusResponse.HasFlag(ReadStatusResponse.Tx0Req):
                case TransmitBuffer.Tx1 when readStatusResponse.HasFlag(ReadStatusResponse.Tx1If) &&
                                       !readStatusResponse.HasFlag(ReadStatusResponse.Tx1Req):
                case TransmitBuffer.Tx2 when readStatusResponse.HasFlag(ReadStatusResponse.Tx2If) &&
                                       !readStatusResponse.HasFlag(ReadStatusResponse.Tx2Req):
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Abort send all messages from buffers, buffers will be empty
        /// </summary>
        public void AbortAllPendingTransmissions()
        {
            WriteByte(
                new CanCtrl(
                    CanCtrl.PinPrescaler.ClockDivideBy8,
                    true,
                    false,
                    true,
                    OperationMode.NormalOperation));
        }

        /// <summary>
        /// Writes data to the register beginning at the selected address.
        /// </summary>
        /// <param name="address">The starting address to write data.</param>
        /// <param name="buffer">The buffer that contains the data to be written.</param>
        public void Write(Address address, ReadOnlySpan<byte> buffer)
        {
            Span<byte> writeBuffer = stackalloc byte[buffer.Length + 2];
            writeBuffer[0] = (byte)InstructionFormat.Write;
            writeBuffer[1] = (byte)address;
            buffer.CopyTo(writeBuffer.Slice(2));
            _spiDevice.Write(writeBuffer);
        }

        /// <summary>
        /// When loading a transmit buffer, reduces the overhead of a normal WRITE
        /// command by placing the Address Pointer at one of six locations for transmit buffer.
        /// </summary>
        /// <param name="addressPointer">The Address Pointer to one of six locations for the transmit buffer.</param>
        /// <param name="buffer">The data to load in transmit buffer.</param>
        public void LoadTxBuffer(TxBufferAddressPointer addressPointer, ReadOnlySpan<byte> buffer)
        {
            Span<byte> writeBuffer = stackalloc byte[buffer.Length + 1];

            // This instruction has a base value of 0x90.
            // The 3 lower bits are used for the pointer address for loading.
            writeBuffer[0] = (byte)((byte)InstructionFormat.LoadTxBuffer + (byte)addressPointer);
            buffer.CopyTo(writeBuffer.Slice(1));
            _spiDevice.Write(writeBuffer);
        }

        /// <summary>
        /// Instructs the controller to begin the message transmission sequence for any of the transmit buffers.
        /// </summary>
        /// <param name="txb0">Instructs the controller to begin the message transmission sequence for TxB0.</param>
        /// <param name="txb1">Instructs the controller to begin the message transmission sequence for TxB1.</param>
        /// <param name="txb2">Instructs the controller to begin the message transmission sequence for TxB2.</param>
        public void RequestToSend(bool txb0, bool txb1, bool txb2)
        {
            byte GetInstructionFormat()
            {
                // This instruction has a base value of 0xA0.
                // The 3 lower bits are used for the pointer address for reading.
                byte value = (byte)InstructionFormat.RequestToSend;

                if (txb0)
                {
                    value |= 0x01;
                }

                if (txb1)
                {
                    value |= 0x02;
                }

                if (txb2)
                {
                    value |= 0x04;
                }

                return value;
            }

            byte instructionFormat = GetInstructionFormat();
            _spiDevice.WriteByte(instructionFormat);
        }

        /// <summary>
        /// Quick polling command that reads several Status bits for transmit and receive functions.
        /// </summary>
        /// <returns>The response from READ STATUS instruction.</returns>
        public ReadStatusResponse ReadStatus()
        {
            const byte dontCare = 0x00;
            ReadOnlySpan<byte> writeBuffer = stackalloc byte[]
            {
                (byte)InstructionFormat.ReadStatus,
                dontCare
            };
            Span<byte> readBuffer = stackalloc byte[2];
            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
            ReadStatusResponse readStatusResponse = (ReadStatusResponse)readBuffer[1];
            return readStatusResponse;
        }

        /// <summary>
        /// Quick polling command that indicates a filter match and message type
        /// (standard, extended and/or remote) of the received message.
        /// </summary>
        /// <returns>Response from RX STATUS instruction.</returns>
        public RxStatusResponse RxStatus()
        {
            const byte dontCare = 0x00;
            ReadOnlySpan<byte> writeBuffer = stackalloc byte[]
            {
                (byte)InstructionFormat.RxStatus,
                dontCare
            };
            Span<byte> readBuffer = stackalloc byte[2];
            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
            RxStatusResponse rxStatusResponse = new RxStatusResponse(readBuffer[1]);
            return rxStatusResponse;
        }

        /// <summary>
        /// Allows the user to set or clear individual bits in a particular register.
        /// Not all registers can be bit modified with this command.
        /// </summary>
        /// <param name="address">The address to write data.</param>
        /// <param name="mask">The mask to determine which bits in the register will be allowed to change.
        /// A '1' will allow a bit to change while a '0' will not.</param>
        /// <param name="value">The value to be written.</param>
        public void BitModify(Address address, byte mask, byte value)
        {
            Span<byte> writeBuffer = stackalloc byte[]
            {
                (byte)InstructionFormat.BitModify,
                (byte)address,
                mask,
                value
            };
            _spiDevice.Write(writeBuffer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _gpioController?.Dispose();
                _gpioController = null;
            }

            _spiDevice?.Dispose();
            _spiDevice = null!;
        }
    }
}
