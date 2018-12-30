// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using System;
using System.Device.I2c;
using System.Device.Spi;
using static Iot.Device.Ssd1306.Command.ContinuousVerticalAndHorizontalScrollSetup;
using static Iot.Device.Ssd1306.Command.HorizontalScrollSetup;
using static Iot.Device.Ssd1306.Command.SetVcomhDeselectLevel;

namespace Iot.Device.Ssd1306
{
    public class Ssd1306 : IDisposable
    {
        private readonly I2cDevice _i2cDevice;
        private readonly SpiDevice _spiDevice;
        private readonly ConnectionType _connectionType;

        public Ssd1306(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _connectionType = ConnectionType.I2c;
        }

        public Ssd1306(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
            _connectionType = ConnectionType.Spi;
        }

        public void Dispose()
        {
        }

        public void SendCommand(ICommand command, bool continuation = false)
        {
            byte controlByte = 0x00;
            byte[] commandBytes = command.GetBytes();
            byte[] writeBuffer = new byte[commandBytes.Length + 1];
            commandBytes.CopyTo(writeBuffer, 1);

            if (continuation)
            {
                controlByte |= 0x80;
            }

            writeBuffer[0] = controlByte;

            if (_connectionType == ConnectionType.I2c)
            {
                _i2cDevice.Write(writeBuffer);
            }
            else
            {
                _spiDevice.Write(writeBuffer);
            }
        }

        public void SendData(byte[] data, bool continuation = false)
        {
            byte controlByte = 0x40;
            byte[] writeBuffer = new byte[data.Length + 1];
            data.CopyTo(writeBuffer, 1);

            if (continuation)
            {
                controlByte |= 0x80;
            }

            writeBuffer[0] = controlByte;
            _i2cDevice.Write(writeBuffer);
        }

        public void SetLowerColumnStartAddressForPageAddressingMode(byte lowerColumnStartAddress = 0x00)
        {
            SendCommand(new SetLowerColumnStartAddressForPageAddressingMode(lowerColumnStartAddress));
        }

        public void SetHigherColumnStartAddressForPageAddressingMode(byte higherColumnStartAddress = 0x00)
        {
            SendCommand(new SetHigherColumnStartAddressForPageAddressingMode(higherColumnStartAddress));
        }

        public void SetMemoryAddressingMode(SetMemoryAddressingMode.AddressingMode memoryAddressingMode = Command.SetMemoryAddressingMode.AddressingMode.Page)
        {
            SendCommand(new SetMemoryAddressingMode(memoryAddressingMode));
        }

        public void SetColumnAddress(byte startAddress = 0x00, byte endAddress = 0x80)
        {
            SendCommand(new SetColumnAddress(startAddress, endAddress));
        }

        public void SetPageAddress(PageAddress startAddress = PageAddress.Page0, PageAddress endAddress = PageAddress.Page7)
        {
            SendCommand(new SetPageAddress(startAddress, endAddress));
        }

        public void SetDisplayStartLine(byte displayStartLine = 0x00)
        {
            SendCommand(new SetDisplayStartLine(displayStartLine));
        }

        public void SetContrastControlForBank0(byte contrastSetting = 0x7F)
        {
            SendCommand(new SetContrastControlForBank0(contrastSetting));
        }

        public void SetSegmentReMap(bool columnAddress0 = true)
        {
            SendCommand(new SetSegmentReMap(columnAddress0));
        }

        public void EntireDisplayOn(bool entireDisplay = false)
        {
            SendCommand(new EntireDisplayOn(entireDisplay));
        }

        public void SetNormalDisplay()
        {
            SendCommand(new SetNormalDisplay());
        }

        public void SetInverseDisplay()
        {
            SendCommand(new SetInverseDisplay());
        }

        public void SetMultiplexRatio(byte multiplexRatio = 0x63)
        {
            SendCommand(new SetMultiplexRatio(multiplexRatio));
        }

        public void SetDisplayOn()
        {
            SendCommand(new SetDisplayOn());
        }

        public void SetDisplayOff()
        {
            SendCommand(new SetDisplayOff());
        }

        public void SetPageStartAddressForPageAddressingMode(PageAddress startAddress = PageAddress.Page0)
        {
            SendCommand(new SetPageStartAddressForPageAddressingMode(startAddress));
        }

        public void SetComOutputScanDirection(bool normalMode = true)
        {
            SendCommand(new SetComOutputScanDirection(normalMode));
        }

        public void SetDisplayOffset(byte displayOffset = 0x00)
        {
            SendCommand(new SetDisplayOffset(displayOffset));
        }

        public void SetDisplayClockDivideRatioOscillatorFrequency(byte displayClockDivideRatio = 0x00, byte oscillatorFrequency = 0x08)
        {
            SendCommand(new SetDisplayClockDivideRatioOscillatorFrequency(displayClockDivideRatio, oscillatorFrequency));
        }

        public void SetPreChargePeriod(byte phase1Period = 0x02, byte phase2Period = 0x02)
        {
            SendCommand(new SetPreChargePeriod(phase1Period, phase2Period));
        }

        public void SetComPinsHardwareConfiguration(bool alternativeComPinConfiguration = true, bool enableLeftRightRemap = false)
        {
            SendCommand(new SetComPinsHardwareConfiguration(alternativeComPinConfiguration, enableLeftRightRemap));
        }

        public void SetVcomhDeselectLevel(DeselectLevel level = DeselectLevel.Vcc0_77)
        {
            SendCommand(new SetVcomhDeselectLevel(level));
        }

        public void HorizontalScrollSetup(HorizontalScrollType scrollType, PageAddress startPageAddress, FrameFrequencyType frequencyType, PageAddress endPageAddress)
        {
            SendCommand(new HorizontalScrollSetup(scrollType, startPageAddress, frequencyType, endPageAddress));
        }

        public void ContinuousVerticalAndHorizontalScrollSetup(
            VerticalHorizontalScrollType scrollType,
            PageAddress startPageAddress,
            FrameFrequencyType frameFrequencyType,
            PageAddress endPageAddress,
            byte verticalScrollingOffset)
        {
            SendCommand(new ContinuousVerticalAndHorizontalScrollSetup(
                scrollType,
                startPageAddress,
                frameFrequencyType,
                endPageAddress,
                verticalScrollingOffset));
        }

        public void DeactivateScroll()
        {
            SendCommand(new DeactivateScroll());
        }

        public void ActivateScroll()
        {
            SendCommand(new ActivateScroll());
        }

        public void SetVerticalScrollArea(byte topFixedAreaRows = 0x00, byte scrollAreaRows = 0x40)
        {
            SendCommand(new SetVerticalScrollArea(topFixedAreaRows, scrollAreaRows));
        }

        public void SetChargePump(bool enableChargePump = false)
        {
            SendCommand(new SetChargePump(enableChargePump));
        }
    }
}
