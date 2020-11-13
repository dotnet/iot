// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
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
        private ArduinoBoard _board;

        public BasicFirmataTests()
        {
            var b = ArduinoBoard.FindBoard(ArduinoBoard.GetSerialPortNames(), new List<int>() { 115200 });
            if (b == null)
            {
                throw new NotSupportedException("No board found");
            }

            _board = b;
        }

        public void Dispose()
        {
            _board.Dispose();
        }

        [Fact]
        public void CheckFirmwareVersion()
        {
            Assert.NotNull(_board.FirmwareName);
            Assert.NotEqual(new Version(), _board.FirmwareVersion);
            Assert.True(_board.FirmwareVersion >= Version.Parse("2.11"));
        }

        [Fact]
        public void CanBlink()
        {
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
    }
}
