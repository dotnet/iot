// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Arduino.Tests;
using Xunit;

namespace Iot.Device.Arduino.Tests
{
    /// <summary>
    /// Basic firmata tests. These tests require functional hardware, that is an Arduino, loaded with a matching firmata firmware. It can also
    /// run against the *ExtendedConfigurableFirmata" simulator
    /// </summary>
    public sealed class BasicFirmataTests : IClassFixture<FirmataTestFixture>
    {
        private readonly FirmataTestFixture _fixture;

        public BasicFirmataTests(FirmataTestFixture fixture)
        {
            _fixture = fixture;
            Board = _fixture.Board;
        }

        public ArduinoBoard Board
        {
            get;
        }

        [Fact]
        public void CheckFirmwareVersion()
        {
            Assert.NotNull(Board.FirmwareName);
            Assert.NotEqual(new Version(), Board.FirmwareVersion);
            Assert.True(Board.FirmwareVersion >= Version.Parse("2.11"));
        }

        [Fact]
        public void CanBlink()
        {
            var ctrl = Board.CreateGpioController(PinNumberingScheme.Logical);
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
