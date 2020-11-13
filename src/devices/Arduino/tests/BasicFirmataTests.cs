// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// Basic firmata tests. These tests require functional hardware, that is an Arduino, loaded with a matching firmata firmware.
    /// </summary>
    public sealed class BasicFirmataTests : IDisposable
    {
        private const String PortName = "COM3";
        private SerialPort _serialPort;
        private ArduinoBoard _board;

        public BasicFirmataTests()
        {
            _serialPort = new SerialPort(PortName, 115200);
            _serialPort.Open();
            _board = new ArduinoBoard(_serialPort.BaseStream);
        }

        public void Dispose()
        {
            _board.Dispose();
            _serialPort.Dispose();
        }

        [Fact]
        public void CheckFirmwareVersion()
        {
            _board.Initialize();
            Assert.NotNull(_board.FirmwareName);
            Assert.NotEqual(new Version(), _board.FirmwareVersion);
            Assert.True(_board.FirmwareVersion >= Version.Parse("2.11"));
        }

        [Fact]
        public void CanBlink()
        {
            _board.Initialize();
            var ctrl = _board.CreateGpioController(PinNumberingScheme.Logical);
            Assert.NotNull(ctrl);
            ctrl.OpenPin(6, PinMode.Output);
            ctrl.SetPinMode(6, PinMode.Output);
            ctrl.Write(6, PinValue.High);
            Thread.Sleep(100);
            ctrl.Write(6, PinValue.Low);
            ctrl.SetPinMode(6, PinMode.Input);
            ctrl.ClosePin(6);
        }

        [Theory]
        [InlineData(2, 2)]
        [InlineData(1, 0)]
        [InlineData(4, 1)]
        [InlineData(10, -1)]
        public void SomeSwitchTest(int input, int output)
        {
            int a = input;
            int requestVal = a switch
            {
                4 | 8 => 1,
                2 => 2,
                1 => 0,
                _ => -1,
            };

            Assert.Equal(output, requestVal);
        }
    }
}
