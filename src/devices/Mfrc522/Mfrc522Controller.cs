using System;
using System.Diagnostics;
using System.Device.Gpio;
using System.Device.Spi;
using System.Collections.Generic;

using static Iot.Device.Mfrc522.Status;
using static Iot.Device.Mfrc522.Register;
using static Iot.Device.Mfrc522.RequestMode;
using static Iot.Device.Mfrc522.Command;

namespace Iot.Device.Mfrc522
{
    public class Mfrc522Controller : IDisposable
    {
        const byte NRSTPD = 22;

        const byte MAX_LEN = 16;

        private readonly SpiDevice _spiDevice;
        private GpioController _controller;

        public Mfrc522Controller(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;

            _controller = new GpioController();

            _controller.OpenPin(NRSTPD, PinMode.Output);
            _controller.Write(NRSTPD, PinValue.High);

            Init();
        }

        public static readonly byte[] DefaultAuthKey = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        private void Init()
        {
            _controller.Write(NRSTPD, PinValue.High);

            SoftReset();

            WriteSpi(TModeReg, 0x8D);
            WriteSpi(TPrescalerReg, 0x3E);
            WriteSpi(TReloadRegL, 30);
            WriteSpi(TReloadRegH, (byte)0);

            WriteSpi(TxAutoReg, 0x40);
            WriteSpi(ModeReg, 0x3D);

            AntennaOn();
        }

        private void SoftReset()
        {
            WriteSpi(CommandReg, (byte)ResetPhase);
        }

        private void WriteSpi(byte address, byte value)
        {
            Span<byte> buffer = stackalloc byte[2] {
                (byte) ((address << 1) & 0x7E),
                value
            };
            _spiDevice.Write(buffer);
        }
        private void WriteSpi(Register register, byte value)
        {
            WriteSpi((byte)register, value);
        }

        private void WriteSpi(Register register, Command command)
        {
            WriteSpi((byte)register, (byte)command);
        }

        private byte ReadSpi(byte address)
        {
            Span<byte> buffer = stackalloc byte[2] {
                (byte) (((address << 1) & 0x7E) | 0x80),
                0
            };
            _spiDevice.TransferFullDuplex(buffer, buffer);
            return buffer[1];
        }

        private byte ReadSpi(Register register)
        {
            return ReadSpi((byte)register);
        }

        public (Status status, byte[] data) ReadCardData(byte blockAddress)
        {
            List<byte> buff = new List<byte>
            {
              (byte)RequestMode.Read,
              blockAddress
            };
            var crc = CalulateCRC(buff.ToArray());
            buff.Add(crc[0]);
            buff.Add(crc[1]);

            var (status, data, _) = SendCommand(Command.Transceive, buff.ToArray());
            return (status, data);
        }

        private void SetBitMask(Register register, byte mask)
        {
            var tmp = ReadSpi(register);
            WriteSpi(register, (byte)(tmp | mask));
        }

        private void ClearBitMask(Register register, byte mask)
        {
            var tmp = ReadSpi(register);
            WriteSpi(register, (byte)(tmp & (~mask)));
        }

        private void AntennaOn()
        {
            var temp = ReadSpi(TxControlReg);
            SetBitMask(TxControlReg, 0x03);
        }

        private void AntennaOff()
        {
            ClearBitMask(TxControlReg, 0x03);
        }

        private (Status status, byte[] backData, byte backLen) SendCommand(Command command, byte[] sendData)
        {
            var backData = new List<byte>();
            byte backLen = 0;
            var status = Error;
            byte irqEn = 0x00;
            byte waitIRq = 0x00;
            byte n = 0;
            var i = 0;

            switch(command)
            {
                case Command.Authenticate:
                    irqEn = 0x12;
                    waitIRq = 0x10;
                    break;

                case Command.Transceive:
                    irqEn = 0x77;
                    waitIRq = 0x30;
                    break;
            }

            WriteSpi(CommIEnReg, (byte)(irqEn | 0x80));
            ClearBitMask(CommIrqReg, 0x80);
            SetBitMask(FIFOLevelReg, 0x80);

            WriteSpi(CommandReg, (byte)Idle);

            while (i < sendData.Length)
            {
                WriteSpi(FIFODataReg, sendData[i]);
                i++;
            }

            WriteSpi(CommandReg, (byte)command);

            if (command == Transceive)
            {
                SetBitMask(BitFramingReg, 0x80);
            }

            i = 2000;
            do
            {
                n = ReadSpi((byte)CommIrqReg);
            }
            while (--i != 0 && (n & 0x01) == 0 && (n & waitIRq) == 0);

            ClearBitMask(BitFramingReg, 0x80);

            if (i == 0) return (status, backData.ToArray(), backLen);

            if ((ReadSpi(ErrorReg) & 0x1B) == 0x00)
            {
                status = OK;

                if (Convert.ToBoolean(n & irqEn & 0x01))
                {
                    status = NoTag;
                }

                if (command == Transceive)
                {
                    n = ReadSpi(FIFOLevelReg);
                    byte? lastBits = (byte)(ReadSpi(ControlReg) & 0x07);
                    if (lastBits != 0)
                    {
                        backLen = (byte)(((n - 1) * 8) + (byte)lastBits);
                    }
                    else
                    {
                        backLen = (byte)(n * 8);
                    }

                    if (n == 0)
                    {
                        n = 1;
                    }

                    if (n > MAX_LEN)
                    {
                        n = MAX_LEN;
                    }

                    i = 0;
                    while (i < n)
                    {
                        backData.Add(ReadSpi(FIFODataReg));
                        i++;
                    }
                }
            }
            else
            {
                status = Error;
            }

            return (status, backData.ToArray(), backLen);
        }

        public (Status status, int backBits) Request(RequestMode requestMode)
        {
            var tagType = new List<byte> { (byte)requestMode };

            WriteSpi(BitFramingReg, 0x07);

            var (status, backData, backBits) = SendCommand(Transceive, tagType.ToArray());

            if ((status != OK) | (backBits != 0x10))
            {
                status = Error;
            }

            return (status, backBits);
        }

        public (Status status, byte[] data) AntiCollision()
        {
            byte serNumCheck = 0;

            var serNum = new List<byte>();

            WriteSpi(BitFramingReg, (byte)0x00);

            serNum.Add((byte)RequestMode.AntiCollision);
            serNum.Add(0x20);

            var (status, backData, _) = SendCommand(Transceive, serNum.ToArray());

            var i = 0;
            if (status == OK)
            {
                i = 0;
            }

            if (backData.Length == 5)
            {
                while (i < 4)
                {
                    serNumCheck = (byte)(serNumCheck ^ backData[i]);
                    i = i + 1;
                }

                if (serNumCheck != backData[i])
                {
                    status = Error;
                }
            }
            else
            {
                status = Error;
            }

            return (status, backData);
        }

        private byte[] CalulateCRC(byte[] pIndata)
        {
            ClearBitMask(DivIrqReg, 0x04);
            SetBitMask(FIFOLevelReg, 0x80);
            byte i = 0;
            while (i < pIndata.Length)
            {
                WriteSpi(FIFODataReg, pIndata[i]);
                i++;
            }

            WriteSpi(CommandReg, (byte)CalculateCRC);
            i = 0xFF;
            while (true)
            {
                var n = ReadSpi(DivIrqReg);
                i--;
                if (!((i != 0) && !Convert.ToBoolean(n & 0x04)))
                {
                    break;
                }
            }

            var pOutData = new List<byte> { ReadSpi(CRCResultRegL), ReadSpi(CRCResultRegM) };
            return pOutData.ToArray();
        }

        public byte SelectTag(byte[] serialNumber)
        {
            var buf = new List<byte> { (byte)RequestMode.SelectTag, 0x70 };
            var i = 0;
            while (i < 5)
            {
                buf.Add(serialNumber[i]);
                i++;
            }

            var pOut = CalulateCRC(buf.ToArray());
            buf.Add(pOut[0]);
            buf.Add(pOut[1]);
            var (status, backData, backBits) = SendCommand(Transceive, buf.ToArray());

            if (status != OK || backBits != 0x18)
                return 0;

            Debug.WriteLine($"Size: {backData[0]}");
            return backData[0];

        }

        public Status Authenticate(RequestMode authenticationMode, byte blockAddress, byte[] sectorKey, byte[] serialNumber)
        {
            // First byte should be the authMode (A or B) Second byte is the trailerBlock (usually 7)
            var buff = new List<byte> { (byte)authenticationMode, blockAddress };

            // Now we need to append the authKey which usually is 6 bytes of 0xFF
            var i = 0;
            while (i < sectorKey.Length)
            {
                buff.Add(sectorKey[i]);
                i++;
            }

            i = 0;

            // Next we append the first 4 bytes of the UID
            while (i < 4)
            {
                buff.Add(serialNumber[i]);
                i++;
            }

            // Now we start the authentication itself
            var (status, _, _) = SendCommand(Command.Authenticate, buff.ToArray());

            // Check if an error occurred
            if (status != OK)
            {
                throw new Mfrc522Exception("AUTH ERROR!!");
            }

            if ((ReadSpi(Status2Reg) & 0x08) == 0)
            {
                throw new Mfrc522Exception("AUTH ERROR(status2reg & 0x08) != 0");
            }

            // Return the status
            return status;
        }

        public void ClearSelection()
        {
            ClearBitMask(Status2Reg, 0x08);
        }

        public void WriteCardData(byte blockAddress, byte[] writeData)
        {
            var buff = new List<byte> { (byte)RequestMode.Write, blockAddress };
            var crc = CalulateCRC(buff.ToArray());
            buff.Add(crc[0]);
            buff.Add(crc[1]);
            var (status, backData, backLen) = SendCommand(Transceive, buff.ToArray());

            if (status != OK || backLen != 4 || (backData[0] & 0x0F) != 0x0A)
            {
                status = Error;
            }

            Debug.WriteLine($"{backLen} backdata &0x0F == 0x0A {backData[0] & 0x0F}");

            if (status != OK) return;

            var i = 0;
            var buf = new List<byte>();
            while (i < 16)
            {
                buf.Add(writeData[i]);
                i++;
            }

            crc = CalulateCRC(buf.ToArray());
            buf.Add(crc[0]);
            buf.Add(crc[1]);

            (status, backData, backLen) = SendCommand(Transceive, buf.ToArray());

            if (status != OK || backLen != 4 || (backData[0] & 0x0F) != 0x0A)
            {
                throw new Mfrc522Exception("Failed to write to");
            }
        }

        public void DumpClassic1K(byte[] key, byte[] uid)
        {
            byte i = 0;

            while (i < 64)
            {
                var status = Authenticate(Authenticate1A, i, key, uid);

                // Check if authenticated
                if (status == OK)
                {
                    ReadSpi(i);
                }
                else
                {
                    throw new Mfrc522Exception("Authentication error");
                }

                i++;
            }
        }

        public void Dispose()
        {
            _spiDevice?.Dispose();
            _controller?.Dispose();
        }
    }
}
