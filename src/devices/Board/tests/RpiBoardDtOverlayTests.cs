// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Iot.Device.Board.Tests
{
    /// <summary>
    /// Tests for the dtoverlay file for Raspberry Pi.
    /// </summary>
    public class RpiBoardDtOverlayTests
    {
        private readonly RaspberryPiBoard board;

        public RpiBoardDtOverlayTests()
        {
            board = new RaspberryPiBoard();
        }

        [Fact]
        public void CheckI2cConfig()
        {
            board.ConfigurationFile = Path.Combine("ConfigFiles", "config.I2c.txt");
            Assert.True(board.IsI2cOverlayActivate());
        }

        [Fact]
        public void CheckI2cPinConfig()
        {
            // Arrange
            board.ConfigurationFile = Path.Combine("ConfigFiles", "config.I2c.txt");

            // Act
            var pins = board.GetOverlayPinAssignmentForI2c(3);

            // dtoverlay=i2c3,pins_2_3
            Assert.True(pins.Length == 2);
            Assert.Equal(2, pins[0]);
            Assert.Equal(3, pins[1]);
        }

        [Fact]
        public void CheckSpiConfig()
        {
            board.ConfigurationFile = Path.Combine("ConfigFiles", "config.Spi.txt");
            Assert.True(board.IsSpiOverlayActivate());
        }

        [Fact]
        public void CheckSpi0PinConfig()
        {
            // Arrange
            board.ConfigurationFile = Path.Combine("ConfigFiles", "config.Spi.txt");

            // Act
            var pins = board.GetOverlayPinAssignmentForSpi(new System.Device.Spi.SpiConnectionSettings(0, -1));

            // dtoverlay=spi0-0cs,no_miso
            Assert.True(pins.Length == 3);
            Assert.Equal(-1, pins[0]);
            Assert.Equal(10, pins[1]);
            Assert.Equal(11, pins[2]);
        }

        [Fact]
        public void CheckSpi2PinConfig()
        {
            // Arrange
            board.ConfigurationFile = Path.Combine("ConfigFiles", "config.Spi.txt");

            // Act
            var pins = board.GetOverlayPinAssignmentForSpi(new System.Device.Spi.SpiConnectionSettings(2, 0));

            // dtoverlay=spi2-2cs,cs0_pin=27,cs1_pin=22
            Assert.True(pins.Length == 4);
            Assert.Equal(40, pins[0]);
            Assert.Equal(41, pins[1]);
            Assert.Equal(42, pins[2]);
            Assert.Equal(27, pins[3]);
        }

        [Fact]
        public void CheckPwmConfig()
        {
            board.ConfigurationFile = Path.Combine("ConfigFiles", "config.Pwm.txt");
            Assert.True(board.IsPwmOverlayActivate());
        }

        [Fact]
        public void CheckPwm0PinConfig()
        {
            // Arrange
            board.ConfigurationFile = Path.Combine("ConfigFiles", "config.Pwm.txt");

            // Act
            var pin0 = board.GetOverlayPinAssignmentForPwm(0);
            var pin1 = board.GetOverlayPinAssignmentForPwm(1);

            // Assert
            Assert.Equal(12, pin0);
            Assert.Equal(13, pin1);
        }

        [Fact]
        public void CheckPwm1PinConfig()
        {
            // Arrange
            board.ConfigurationFile = Path.Combine("ConfigFiles", "config.Pwm1.txt");

            // Act
            var pin0 = board.GetOverlayPinAssignmentForPwm(0);
            var pin1 = board.GetOverlayPinAssignmentForPwm(1);

            // Assert
            Assert.Equal(19, pin0);
            Assert.Equal(-1, pin1);
        }

        [Fact]
        public void CheckNothingConfigured()
        {
            board.ConfigurationFile = Path.Combine("ConfigFiles", "config.nothing.txt");
            Assert.False(board.IsI2cOverlayActivate());
            Assert.False(board.IsPwmOverlayActivate());
            Assert.False(board.IsSpiOverlayActivate());
        }
    }
}
