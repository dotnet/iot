// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Iot.Device.Card;
using Iot.Device.Rfid;
using Iot.Device.Card.Mifare;

namespace Iot.Device.Mfrc522
{
    /// <summary>
    /// MfRc522 module
    /// </summary>
    public class MfRc522 : CardTransceiver, IDisposable
    {
        /// <summary>
        /// The maximum speed for SPI transfer speed
        /// </summary>
        public const int MaximumSpiClockFrequency = 10_000_000;

        /// <summary>
        /// Only SPI Mode supported is Mode0
        /// </summary>
        public const SpiMode DefaultSpiMode = SpiMode.Mode0;

        private int _pinReset;
        private SpiDevice? _spiDevice;
        private I2cDevice? _i2CDevice;
        private SerialPort? _serialPort;
        private GpioController? _controller;
        private bool _shouldDispose;

        #region constructors

        /// <summary>
        /// Constructor for MFRC5222 with SPI interface.
        /// </summary>
        /// <param name="spiDevice">A SPI device</param>
        /// <param name="pinReset">A reset pin for the hardware reset.</param>
        /// <param name="gpioController">A GpioController for the hardware reset.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        public MfRc522(SpiDevice spiDevice, int pinReset = -1, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _spiDevice = spiDevice;
            _pinReset = pinReset;

            HardReset(gpioController, shouldDispose);
            SetDefaultValues();
        }

        /// <summary>
        /// Constructor for MFRC5222 with I2C interface.
        /// </summary>
        /// <param name="i2cDevice">An I2C device, note that there is no default address for this device, it can be programmed with pins.</param>
        /// <param name="pinReset">A reset pin for the hardware reset.</param>
        /// <param name="gpioController">A GpioController for the hardware reset.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        public MfRc522(I2cDevice i2cDevice, int pinReset = -1, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _i2CDevice = i2cDevice;
            _pinReset = pinReset;

            HardReset(gpioController, shouldDispose);
            SetDefaultValues();
        }

        /// <summary>
        /// Constructor for MFRC5222 with Serial Port interface.
        /// </summary>
        /// <param name="serialPort">A Serial Port name, will construct a SerialPort with default speed of 9600 baud, no parity, 1 bit stop.</param>
        /// <param name="pinReset">A reset pin for the hardware reset.</param>
        /// <param name="gpioController">A GpioController for the hardware reset.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        public MfRc522(string serialPort, int pinReset = -1, GpioController gpioController = null, bool shouldDispose = true)
            : this(new SerialPort(serialPort, 9600, Parity.None, 8, StopBits.One), pinReset, gpioController, shouldDispose)
        {
        }

        /// <summary>
        /// Constructor for MFRC5222 with Serial Port interface.
        /// </summary>
        /// <param name="serialPort">A Serial Port, default speed is 9600 baud, no parity, 1 bit stop.</param>
        /// <param name="pinReset">A reset pin for the hardware reset.</param>
        /// <param name="gpioController">A GpioController for the hardware reset.</param>
        /// <param name="shouldDispose">True to dispose the GpioController.</param>
        public MfRc522(SerialPort serialPort, int pinReset = -1, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _serialPort = serialPort;
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }

            _pinReset = pinReset;

            HardReset(gpioController, shouldDispose);
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            // Switch off the crypto for Mifare card in case it's on
            ClearRegisterBit(Register.Status2, (byte)Status2.MFCrypto1On);
            // Set Timer for Timeout, see documentation on those registers to understand the values
            WriteRegister(Register.TMode, (byte)TMode.TAutoRestart);
            WriteRegister(Register.TPrescaler, 0xA9);
            WriteRegister(Register.TReloadHigh, 0x06);
            WriteRegister(Register.TReloadLow, 0xE8);

            // forces a 100 % ASK modulation independent of the ModGsPReg register setting
            WriteRegister(Register.TxAsk, 0x40);

            // Set CRC to 0x6363 (iso 14443-3 6.1.6)
            WriteRegister(Register.Mode, (byte)(Mode.TxWaitRF | Mode.PolMFinHigh) | (byte)ModeCrc.Preset6363h);
            // Switching on the antenna
            Enabled = true;
        }

        private void HardReset(GpioController? gpioController, bool shouldDispose)
        {
            if (_pinReset >= 0)
            {
                _shouldDispose = shouldDispose || gpioController == null;
                _controller = gpioController ?? new GpioController();
                _controller.OpenPin(_pinReset, PinMode.Output);
                _controller.Write(_pinReset, PinValue.Low);
                // 100 nano seconds at least, take some margin
                Thread.Sleep(1);
                _controller.Write(_pinReset, PinValue.High);
                // 37.7 milliseconds according to documentation
                Thread.Sleep(38);
            }
        }

        #endregion

        #region Properties and functions

        /// <summary>
        /// Get or Set the gain.
        /// </summary>
        public Gain Gain
        {
            get => (Gain)(ReadRegister(Register.RFCfg) & (byte)Gain.G48dB);
            set => WriteRegister(Register.RFCfg, (byte)value);
        }

        /// <summary>
        /// Get the Version.
        /// </summary>
        /// <remarks>Only versions 1.0 and  2.0 are valid for authentic MFRC522.
        /// Some copies may not have a proper version but would just work.</remarks>
        public Version Version
        {
            get
            {
                Version version;
                var rev = ReadRegister(Register.Version);
                // See documentation page 66, chapter 9.3.4.8
                if (rev == 0x91)
                {
                    version = new Version(1, 0);
                }
                else if (rev == 0x92)
                {
                    version = new Version(2, 0);
                }
                else
                {
                    version = new Version(0, 0);
                }

                return version;
            }
        }

        /// <summary>
        /// Switch on or off the antenna.
        /// </summary>
        public bool Enabled
        {
            get => (ReadRegister(Register.TxControl) & (byte)(TxControl.Tx2RFEn | TxControl.Tx1RFEn)) == (byte)(TxControl.Tx2RFEn | TxControl.Tx1RFEn);
            set
            {
                if (value)
                {
                    SetRegisterBit(Register.TxControl, (byte)(TxControl.Tx2RFEn | TxControl.Tx1RFEn));
                }
                else
                {
                    ClearRegisterBit(Register.TxControl, (byte)(TxControl.Tx2RFEn | TxControl.Tx1RFEn));
                }
            }
        }

        /// <summary>
        /// Set or Get the baud rate for the serial port communication.
        /// Default is 9600 baud.
        /// </summary>
        public SerialSpeed SerialSpeed
        {
            get => (SerialSpeed)ReadRegister(Register.SerialSpeed);
            set => WriteRegister(Register.SerialSpeed, (byte)value);
        }

        /// <summary>
        /// Perform a soft reset. The configuration data of the internal buffer
        /// remains unchanged.All registers are set to the reset values.This command automatically
        /// terminates when finished.
        /// </summary>
        /// <remarks>The SerialSpeedReg register is reset and therefore the serial data rate is set to 9600 baud.</remarks>
        public void SoftReset()
        {
            WriteRegister(Register.Command, (byte)MfrcCommand.ResetPhase);
            // 37.7 milliseconds
            Thread.Sleep(38);
        }

        /// <summary>
        /// Listen to any 14443 Type A card.
        /// </summary>
        /// <param name="card">A card once detected.</param>
        /// <param name="timeout">A timeout for pulling the card.</param>
        /// <returns>True if success.</returns>
        public bool ListenToCardIso14443TypeA(out Data106kbpsTypeA card, TimeSpan timeout)
        {
            card = new Data106kbpsTypeA(0, 0, 0, new byte[0], null);
            byte[] atqa = new byte[2];
            DateTime dtTimeout = DateTime.Now.Add(timeout);
            // Switch off the cryptography for Mifare card in case it's on
            ClearRegisterBit(Register.Status2, (byte)Status2.MFCrypto1On);
            do
            {
                Status sc = PiccRequestA(atqa);
                if (sc == Status.Collision || sc == Status.Ok)
                {
                    break;
                }

                if (dtTimeout > DateTime.Now)
                {
                    return false;
                }

                // Give a bit of time for the card and reader
                Thread.Sleep(10);
            }
            while (true);

            card.Atqa = BinaryPrimitives.ReadUInt16BigEndian(atqa);
            var status = Select(out byte[]? nfcId, out byte sak);
            if (status != Status.Ok)
            {
                return false;
            }

            if (nfcId is object)
            {
                card.NfcId = new byte[nfcId.Length];
                nfcId.CopyTo(card.NfcId, 0);
            }

            card.Sak = sak;
            return true;
        }

        /// <summary>
        /// Check if a new card is present.
        /// </summary>
        /// <param name="atqa">ATQA buffer must be 2 bytes length and will contain the ATQA answer if there is a card.</param>
        /// <returns>true if there is a card, else false.</returns>
        public bool IsCardPresent(byte[] atqa)
        {
            if (atqa is not object or { Length: not 2 })
            {
                throw new ArgumentException($"{nameof(atqa)} must be initialized and its size must be 2.");
            }

            // Switch off the cryptography for Mifare card in case it's on
            ClearRegisterBit(Register.Status2, (byte)Status2.MFCrypto1On);
            Status sc = PiccRequestA(atqa);
            if (sc == Status.Collision || sc == Status.Ok)
            {
                return true;
            }

            return false;
        }

        private Status Select(out byte[]? uid, out byte sak)
        {
            sak = 0;
            uid = null;
            bool selectDone = false;
            int sizeUid = 0;
            int bitKnown = 0;
            byte[] uidKnown = new byte[4];
            byte[] tempUid = new byte[10];
            // all received bits will be cleared after a collision
            // only used during bitwise anticollision at 106 kBd,
            // otherwise it is set to logic 1
            ClearRegisterBit(Register.Coll, 0x80);
            int selectCascadeLevel = 1;
            // There are 3 SL maximum. For looping and adjusting the SL
            // SL1 = 0x93, SL2 = 0x95, SL3 = 0x97
            while (!selectDone)
            {
                var bufferLength = bitKnown == 0 ? 2 : 9;
                byte[] dataToCard = new byte[bufferLength];
                byte[] dataFromCard = bitKnown == 0 ? new byte[5] : new byte[3];
                byte numValidBits = (byte)(bitKnown == 0 ? 0x20 : 0x70);
                int destinationIndex;
                switch (selectCascadeLevel)
                {
                    case 1:
                        dataToCard[0] = (byte)CardCommand.SelectCascadeLevel1;
                        sizeUid = 4;
                        destinationIndex = 0;
                        break;
                    case 2:
                        dataToCard[0] = (byte)CardCommand.SelectCascadeLevel2;
                        sizeUid = 7;
                        destinationIndex = 3;
                        break;
                    case 3:
                        dataToCard[0] = (byte)CardCommand.SelectCascadeLevel3;
                        sizeUid = 10;
                        destinationIndex = 6;
                        break;
                    default:
                        return Status.Error;
                }

                dataToCard[1] = numValidBits;
                if (bitKnown != 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        dataToCard[i + 2] = uidKnown[i];
                    }

                    // Standard CRC byte calculation for this specific action
                    dataToCard[6] = (byte)(dataToCard[2] ^ dataToCard[3] ^ dataToCard[4] ^ dataToCard[5]);
                    var crcStatus = CalculateCrc(dataToCard.AsSpan(0, 7), dataToCard.AsSpan(7));
                    if (crcStatus != Status.Ok)
                    {
                        return crcStatus;
                    }
                }

                // Reset Bit Framing
                WriteRegister(Register.BitFraming, 0x00);
                Status sc = SendAndReceiveData(MfrcCommand.Transceive, dataToCard, dataFromCard);
                if (sc != Status.Ok)
                {
                    return sc;
                }

                // Once all the bits will be known
                if (bitKnown >= 32)
                {
                    if (dataToCard[2] == 0x88)
                    {
                        // check that there is no cascade
                        if ((dataFromCard[0] & 0x04) != 0x04)
                        {
                            return Status.Error;
                        }

                        Array.Copy(dataToCard, 3, tempUid, destinationIndex, 3);
                        selectCascadeLevel++;
                        // ready for next CascadeLevel
                        bitKnown = 0;
                    }
                    else
                    {
                        selectDone = true;
                        Array.Copy(dataToCard, 2, tempUid, destinationIndex, 4);
                        sak = dataFromCard[0];
                        // check that there is no cascade
                        if ((dataFromCard[0] & 0x04) == 0x04)
                        {
                            return Status.Error;
                        }
                    }
                }
                else
                {
                    // All bit are known, redo loop to do select the card
                    bitKnown = 32;
                    dataFromCard.AsSpan(0, 4).CopyTo(uidKnown);
                }
            }

            // Finally create the uid buffer
            uid = new byte[sizeUid];
            Array.Copy(tempUid, uid, uid.Length);
            return Status.Ok;
        }

        private Status PiccRequestA(byte[] bufferAtqa)
        {
            // all received bits will be cleared after a collision
            // only used during bitwise anticollision at 106 kBd,
            // otherwise it is set to logic 1
            ClearRegisterBit(Register.Coll, 0x80);
            // Only 7 bits are valid in th ReqA request
            byte validBits = 0x07;
            Status sc = SendAndReceiveData(MfrcCommand.Transceive, new[] { (byte)CardCommand.ReqA }, bufferAtqa, validBits);
            if (sc != Status.Ok)
            {
                return sc;
            }

            // Valid bits coded on 3 bits
            validBits = (byte)(ReadRegister(Register.Control) & 0x07);
            if (validBits != 0)
            {
                return Status.Error;
            }

            return Status.Ok;
        }

        /// <summary>
        /// Sand and Receive Data.
        /// </summary>
        /// <param name="command">The MFRC522 command.</param>
        /// <param name="sendData">The data to send.</param>
        /// <param name="receiveData">The data to receive. Note that you need to have at least the size of data you expect to receive.</param>
        /// <param name="numberValidBitsLastByte">The number of bits valid in the last byte, 8 is the default.</param>
        /// <returns>True if the operation is successful.</returns>
        public Status SendAndReceiveData(MfrcCommand command, ReadOnlySpan<byte> sendData, Span<byte> receiveData, byte numberValidBitsLastByte = 8)
        {
            byte bitFraming = (byte)(numberValidBitsLastByte == 8 ? 0 : numberValidBitsLastByte & (byte)BitFraming.TxLastBitsMask);
            byte waitIrq = command == MfrcCommand.MifareAuthenticate ? (byte)(ComIr.IdleIRq) : (byte)(ComIr.IdleIRq | ComIr.RxIRq);

            // Set to idle, prepare FIFO and bit framing
            WriteRegister(Register.Command, (byte)MfrcCommand.Idle);
            WriteRegister(Register.ComIrq, 0x7f);
            WriteRegister(Register.FifoLevel, 0x80);
            WriteRegister(Register.FifoData, sendData);
            WriteRegister(Register.BitFraming, bitFraming);
            // Set the real command
            WriteRegister(Register.Command, (byte)command);

            if (command == MfrcCommand.Transceive)
            {
                SetRegisterBit(Register.BitFraming, 0x80);
            }

            Status status = WaitForCommandToComplete(waitIrq);
            if (status == Status.Timeout)
            {
                return status;
            }

            // Check all is cleared
            Error error = (Error)ReadRegister(Register.Error);
            if (error.HasFlag(Error.BufferOvfl) || error.HasFlag(Error.ParityErr) || error.HasFlag(Error.ProtocolErr))
            {
                return Status.Error;
            }

            // Read if there is something to read
            if (receiveData != null)
            {
                var bytesRead = ReadRegister(Register.FifoLevel);
                if (bytesRead == 0 && receiveData.Length > 0)
                {
                    return Status.Error;
                }

                // We still read the data even if there are more available
                if (bytesRead > receiveData.Length)
                {
                    return Status.Error;
                }

                ReadRegister(Register.FifoData, receiveData);
            }

            // Check collision
            if (error.HasFlag(Error.CollErr))
            {
                return Status.Collision;
            }

            return Status.Ok;
        }

        private bool MifareRead(byte blockAddress, Span<byte> dataFromCard)
        {
            if (dataFromCard.Length < 16)
            {
                return false;
            }

            // 16 bytes + 2 from CRC
            byte[] receivedBuffer = new byte[18];
            // Command + CRC
            byte[] commandToSend = new byte[4];

            commandToSend[0] = (byte)MifareCardCommand.Read16Bytes;
            commandToSend[1] = blockAddress;
            var status = CalculateCrc(commandToSend.AsSpan(0, 2), commandToSend.AsSpan(2));
            if (status != Status.Ok)
            {
                return false;
            }

            status = SendAndReceiveData(MfrcCommand.Transceive, commandToSend, receivedBuffer);
            if (status != Status.Ok)
            {
                return false;
            }

            // Check CRC
            byte[] crc = new byte[2];
            status = CalculateCrc(receivedBuffer.AsSpan(0, 16), crc.AsSpan());
            if (status != Status.Ok)
            {
                return false;
            }

            if (receivedBuffer[16] == crc[0] && receivedBuffer[17] == crc[1])
            {
                receivedBuffer.AsSpan().Slice(0, 16).CopyTo(dataFromCard);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stop to communicate with a card.
        /// </summary>
        /// <returns>True if success.</returns>
        /// <remarks>It's not because you don't get a positive result the card is not halt.</remarks>
        public bool Halt()
        {
            byte[] buffer = new byte[4];
            buffer[0] = (byte)CardCommand.HaltA;
            buffer[1] = 0;
            var status = CalculateCrc(buffer.AsSpan(0, 2), buffer.AsSpan(2));
            if (status != Status.Ok)
            {
                return false;
            }

            status = SendAndReceiveData(MfrcCommand.Transceive, buffer, null);
            if (status == Status.Timeout)
            {
                return true;
            }

            return status == Status.Error ? false : true;
        }

        /// <summary>
        /// Prepare for sleep, make sure cryptography is off and switch off the antenna.
        /// </summary>
        public void PrepareForSleep()
        {
            Enabled = false;
            ClearRegisterBit(Register.Status2, (byte)Status2.MFCrypto1On);
        }

        /// <summary>
        /// Specific function to authenticate Mifare cards
        /// </summary>
        /// <param name="key">A 6 bytes key</param>
        /// <param name="mifareCommand">MifareCardCommand.AuthenticationA or MifareCardCommand.AuthenticationB</param>
        /// <param name="blockAddress">The block address to authenticate.</param>
        /// <param name="cardUid">The 4 bytes UUID of the card.</param>
        /// <returns>True if success.</returns>
        public Status MifareAuthenticate(Span<byte> key, MifareCardCommand mifareCommand, byte blockAddress, Span<byte> cardUid)
        {
            if (mifareCommand != MifareCardCommand.AuthenticationA && mifareCommand != MifareCardCommand.AuthenticationB)
            {
                throw new ArgumentException("Must be AuthenticateA or AuthenticateB only");
            }

            if (key.Length != 6)
            {
                throw new ArgumentException("Key must have a length of 6.", nameof(key));
            }

            byte[] buffer = new byte[12];
            buffer[0] = (byte)mifareCommand;
            buffer[1] = blockAddress;
            key.CopyTo(buffer.AsSpan(2, 6));
            cardUid.CopyTo(buffer.AsSpan(8, 4));

            return SendAndReceiveData(MfrcCommand.MifareAuthenticate, buffer, null);
        }

        private Status CalculateCrc(ReadOnlySpan<byte> buffer, Span<byte> crc)
        {
            // Timeout for the CRC calculation
            const long Timeout = 89;
            if (crc.Length < 2)
            {
                throw new ArgumentException($"CRC buffer must be at least 2 bytes");
            }

            WriteRegister(Register.Command, (byte)MfrcCommand.Idle);
            WriteRegister(Register.DivIrq, (byte)DivIrq.CRCIRq);
            WriteRegister(Register.FifoLevel, 0x80);
            WriteRegister(Register.FifoData, buffer.ToArray());
            WriteRegister(Register.Command, (byte)MfrcCommand.CalculateCrc);
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                DivIrq irgStatus = (DivIrq)ReadRegister(Register.DivIrq);
                if (irgStatus.HasFlag(DivIrq.CRCIRq))
                {
                    WriteRegister(Register.Command, (byte)MfrcCommand.Idle);
                    crc[0] = ReadRegister(Register.CrcResultLow);
                    crc[1] = ReadRegister(Register.CrcResultHigh);
                    return Status.Ok;
                }
            }
            while (stopwatch.ElapsedMilliseconds < Timeout);

            return Status.Timeout;
        }

        private Status WaitForCommandToComplete(byte waitIrq)
        {
            // Timeout in ms for the CRC calculation, see documentation
            const long Timeout = 36;
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                byte irq = ReadRegister(Register.ComIrq);
                if ((irq & waitIrq) != 0)
                {
                    return Status.Ok;
                }

                if (((ComIr)irq).HasFlag(ComIr.TimerIRq))
                {
                    return Status.Timeout;
                }
            }
            while (stopwatch.ElapsedMilliseconds < Timeout);
            return Status.Timeout;
        }

        #endregion

        #region SPI, I2C and Serial Communication

        private void WriteRegister(Register register, byte toCard)
        {
            if (_spiDevice is object)
            {
                SpiWriteRegister(register, toCard);
            }
            else if (_i2CDevice is object)
            {
                I2cWriteRegister(register, toCard);
            }
            else if (_serialPort is object)
            {
                SerialWriteRegister(register, toCard);
            }
        }

        private void WriteRegister(Register register, ReadOnlySpan<byte> toCard)
        {
            if (_spiDevice is object)
            {
                SpiWriteRegister(register, toCard);
            }
            else if (_i2CDevice is object)
            {
                I2cWriteRegister(register, toCard);
            }
            else if (_serialPort is object)
            {
                SerialWriteRegister(register, toCard);
            }
        }

        private byte ReadRegister(Register register)
        {
            if (_spiDevice is object)
            {
                return SpiReadRegister(register);
            }
            else if (_i2CDevice is object)
            {
                return I2cReadRegister(register);
            }
            else if (_serialPort is object)
            {
                return SerialReadRegister(register);
            }

            throw new IOException("No SPI, I2C or Serial port");
        }

        private void ReadRegister(Register register, Span<byte> fromCard)
        {
            if (_spiDevice is object)
            {
                SpiReadRegister(register, fromCard);
            }
            else if (_i2CDevice is object)
            {
                I2cReadRegister(register, fromCard);
            }
            else if (_serialPort is object)
            {
                SerialReadRegister(register, fromCard);
            }
        }

        private void SetRegisterBit(Register register, byte mask)
        {
            var tmp = ReadRegister(register);
            WriteRegister(register, (byte)(tmp | mask));
        }

        private void ClearRegisterBit(Register register, byte mask)
        {
            var tmp = ReadRegister(register);
            WriteRegister(register, (byte)(tmp & ~mask));
        }

        private void SpiWriteRegister(Register register, byte toCard)
        {
            Span<byte> toWrite = stackalloc byte[2]
            {
                (byte)register,
                toCard
            };
            _spiDevice!.TransferFullDuplex(toWrite, toWrite);
        }

        private void SpiWriteRegister(Register register, ReadOnlySpan<byte> toCard)
        {
            for (int i = 0; i < toCard.Length; i++)
            {
                SpiWriteRegister(register, toCard[i]);
            }
        }

        private byte SpiReadRegister(Register register)
        {
            Span<byte> buffer = stackalloc byte[2]
            {
                (byte)((byte)register | 0x80),
                0x00
            };
            _spiDevice!.TransferFullDuplex(buffer, buffer);
            return buffer[1];
        }

        private void SpiReadRegister(Register register, Span<byte> fromCard)
        {
            if (fromCard is { Length: 0 })
            {
                return;
            }

            byte address = (byte)((byte)register | 0x80);
            Span<byte> buffer = new byte[fromCard.Length + 1];

            for (int i = 0; i < fromCard.Length; i++)
            {
                buffer[i] = address;
            }

            _spiDevice!.TransferFullDuplex(buffer, buffer);
            buffer.Slice(1).CopyTo(fromCard);
        }

        private void I2cWriteRegister(Register register, byte toCard)
        {
            Span<byte> toWrite = stackalloc byte[2]
            {
                (byte)((byte)register >> 1),
                toCard,
            };
            _i2CDevice!.Write(toWrite);
        }

        private void I2cWriteRegister(Register register, ReadOnlySpan<byte> toCard)
        {
            Span<byte> toWrite = stackalloc byte[1 + toCard.Length];
            toWrite[0] = (byte)((byte)register >> 1);
            toCard.CopyTo(toWrite.Slice(1));
            _i2CDevice!.Write(toWrite);
        }

        private byte I2cReadRegister(Register register)
        {
            _i2CDevice!.WriteByte((byte)(((byte)register >> 1) | 0x80));
            return _i2CDevice!.ReadByte();
        }

        private void I2cReadRegister(Register register, Span<byte> fromCard)
        {
            _i2CDevice!.WriteByte((byte)(((byte)register >> 1) | 0x80));
            _i2CDevice!.Read(fromCard);
        }

        private void SerialReadRegister(Register register, Span<byte> fromCard)
        {
            byte[] toSend = new byte[] { (byte)(((byte)register >> 1) | 0x80) };
            for (int i = 0; i < fromCard.Length; i++)
            {
                _serialPort!.Write(toSend, 0, 1);
                fromCard[i] = (byte)_serialPort.ReadByte();
            }
        }

        private byte SerialReadRegister(Register register)
        {
            _serialPort!.Write(new byte[] { (byte)(((byte)register >> 1) | 0x80) }, 0, 1);
            return (byte)_serialPort!.ReadByte();
        }

        private void SerialWriteRegister(Register register, byte toCard)
        {
            _serialPort!.Write(new byte[] { (byte)((byte)(register) >> 1), toCard }, 0, 2);
            // We need to read 1 byte, it's the address as confirmation
            // No need to check it
            _serialPort!.ReadByte();
        }

        private void SerialWriteRegister(Register register, ReadOnlySpan<byte> toCard)
        {
            byte[] toSend = new byte[] { (byte)((byte)(register) >> 1), 0x00 };
            for (int i = 0; i < toCard.Length; i++)
            {
                toSend[1] = toCard[i];
                _serialPort!.Write(toSend, 0, 1);
                // We need to read 1 byte, it's the address
                _serialPort!.ReadByte();
                _serialPort!.Write(toSend, 1, 1);
            }
        }

        #endregion

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_spiDevice is object)
            {
                PrepareForSleep();
                _spiDevice.Dispose();
                _spiDevice = null;
            }

            if (_i2CDevice is object)
            {
                PrepareForSleep();
                _i2CDevice.Dispose();
                _i2CDevice = null;
            }

            if (_serialPort is object)
            {
                _serialPort.Close();
            }

            if (_pinReset >= 0)
            {
                _controller?.ClosePin(_pinReset);
            }

            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }
        }

        /// <inheritdoc/>
        public override int Transceive(byte targetNumber, ReadOnlySpan<byte> dataToSend, Span<byte> dataFromCard)
        {
            // targetNumber is not used here as only 1 card can be selected at a time so will be ignored
            // The dataToSend buffer contains anyway the unique of the card
            Status status = Status.Ok;

            // Use built in functions for authentication
            if ((dataToSend[0] == (byte)MifareCardCommand.AuthenticationA) || (dataToSend[0] == (byte)MifareCardCommand.AuthenticationB))
            {
                status = SendAndReceiveData(MfrcCommand.MifareAuthenticate, dataToSend.ToArray(), null);
                return status == Status.Ok ? 0 : -1;

            }
            else if ((dataToSend[0] == (byte)MifareCardCommand.Incrementation) || (dataToSend[0] == (byte)MifareCardCommand.Decrementation)
                || (dataToSend[0] == (byte)MifareCardCommand.Restore))
            {
                return TwoStepsIncDecRestore(dataToSend, dataFromCard);
            }
            else if (dataToSend[0] == (byte)MifareCardCommand.Read16Bytes)
            {
                var ret = MifareRead(dataToSend[1], dataFromCard);
                return ret ? dataFromCard.Length : -1;
            }

            status = SendAndReceiveData(MfrcCommand.Transceive, dataToSend.ToArray(), dataFromCard);
            return status == Status.Ok ? dataFromCard.Length : -1;
        }

        private int TwoStepsIncDecRestore(ReadOnlySpan<byte> dataToSend, Span<byte> dataFromCard)
        {
            Span<byte> toSendFirst = stackalloc byte[4];
            dataToSend.Slice(0, 2).CopyTo(toSendFirst);
            var status = CalculateCrc(toSendFirst.Slice(0, 2), toSendFirst.Slice(2, 2));

            status = SendAndReceiveData(MfrcCommand.Transceive, toSendFirst.ToArray(), null);
            if (status != Status.Ok)
            {
                LogInfo.Log($"{nameof(TwoStepsIncDecRestore)} - Error {(MfrcCommand)dataToSend[0]}", LogLevel.Debug);
                return -1;
            }

            Span<byte> toSendSecond = stackalloc byte[dataToSend.Length];
            dataToSend.Slice(2).CopyTo(toSendSecond);
            status = CalculateCrc(toSendSecond.Slice(0, 2), toSendSecond.Slice(2, 2));

            status = SendAndReceiveData(MfrcCommand.Transceive, toSendSecond.ToArray(), dataFromCard);

            return status == Status.Ok ? dataFromCard.Length : -1;
        }

        /// <inheritdoc/>
        public override bool ReselectTarget(byte targetNumber)
        {
            // We halt the card, and we don't care if it happens correctly
            Halt();
            // We reselect the card and ignore the target number as reader supports only 1 card
            // And we assume here that the card hasn't been changed in the mean time
            IsCardPresent(new byte[2]);
            return Select(out byte[]? uuid, out byte sak) == Status.Ok;
        }
    }
}
