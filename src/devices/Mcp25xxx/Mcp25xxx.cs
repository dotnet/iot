// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using Iot.Device.Mcp25xxx.Register;

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// A general purpose driver for the Microchip MCP25 CAN controller device family.
    /// </summary>
    public abstract class Mcp25xxx : IDisposable
    {
        private readonly int _reset;
        private readonly int _tx0rts;
        private readonly int _tx1rts;
        private readonly int _tx2rts;
        private readonly int _interrupt;
        private readonly int _rx0bf;
        private readonly int _rx1bf;
        private readonly int _clkout;
        internal GpioController _gpioController;
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
            GpioController gpioController = null,
            bool shouldDispose = true)
        {
            _spiDevice = spiDevice;
            _shouldDispose = gpioController == null ? true : shouldDispose;

            _reset = reset;
            _tx0rts = tx0rts;
            _tx1rts = tx1rts;
            _tx2rts = tx2rts;
            _interrupt = interrupt;
            _rx0bf = rx0bf;
            _rx1bf = rx1bf;
            _clkout = clkout;

            // Only need master controller if there are external pins provided.
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
                throw new ArgumentException($"Invalid number of bytes {byteCount}.", nameof(byteCount));
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
            _spiDevice = null;
        }
    }
}
