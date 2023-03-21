// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Spi;
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
        public void DisplayActivations()
        {
            try
            {
                // This is not really a test but a way to see the configuration of the Raspberry PR used to run all the tests
                Console.WriteLine("This is not really a test but a way to see the configuration of the Raspberry PR used to run all the tests.");
                var isI2c = board.IsI2cActivated();
                Console.WriteLine($"Is I2C overlay actvated? {isI2c}");

                for (int busid = 0; busid < 2; busid++)
                {
                    var pins = board.GetOverlayPinAssignmentForI2c(busid);
                    if (pins != null && pins.Length == 2)
                    {
                        Console.WriteLine($"I2C overlay pins on busID {busid}: {pins[0]} {pins[1]}");
                    }
                    else
                    {
                        Console.WriteLine($"No I2C pins defined in the overlay on busID {busid}");
                    }
                }

                var isSpi = board.IsSpiActivated();
                Console.WriteLine($"Is SPI overlay actvated? {isSpi}");

                for (int busid = 0; busid < 2; busid++)
                {
                    // If you want to check chip select, place the number of the chip select pin instead of -1.
                    var pins = board.GetOverlayPinAssignmentForSpi(new SpiConnectionSettings(busid, -1));
                    if (pins != null)
                    {
                        Console.WriteLine($"SPI overlay pins on busID {busid}: MISO {pins[0]} MOSI {pins[1]} Clock {pins[2]}.");
                    }
                    else
                    {
                        Console.WriteLine($"No SPI pins defined in the overlay on busID {busid}");
                    }
                }

                var isPwm = board.IsPwmActivated();
                Console.WriteLine($"Is PWM overlay actvated? {isPwm}");

                for (int busid = 0; busid < 2; busid++)
                {
                    var pin = board.GetOverlayPinAssignmentForPwm(busid);
                    if (pin != -1)
                    {
                        Console.WriteLine($"PWM overlay pin on channel {busid}: {pin}.");
                    }
                    else
                    {
                        Console.WriteLine($"No PWM pins defined in the overlay for channel {busid}");
                    }
                }
            }
            catch (Exception ex)
            {
                // This is aimed for the test NOT to fail in case there is a problem. This is just a test to display the configuration.
                Console.WriteLine($"Exception trying to get access to the overlay confirguration: {ex.Message}");
            }
        }

        [Fact]
        public void CheckI2cConfig()
        {
            board.ConfigurationFile = Path.Combine("ConfigFiles", "config.I2c.txt");
            Assert.True(board.IsI2cActivated());
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
            Assert.True(board.IsSpiActivated());
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
            Assert.True(board.IsPwmActivated());
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
            Assert.False(board.IsI2cActivated());
            Assert.False(board.IsPwmActivated());
            Assert.False(board.IsSpiActivated());
        }
    }
}
