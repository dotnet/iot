// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Tests;
using System.Device.I2c;
using System.Device.Spi;
using System.Reflection;
using Board.Tests;
using Moq;
using Xunit;

namespace Iot.Device.Board.Tests
{
    public class BoardTests : IDisposable
    {
        private Mock<MockableGpioDriver> _mockedGpioDriver;

        public BoardTests()
        {
            _mockedGpioDriver = new Mock<MockableGpioDriver>(MockBehavior.Default);
            _mockedGpioDriver.CallBase = true;
        }

        public void Dispose()
        {
            _mockedGpioDriver.VerifyAll();
        }

        [Fact]
        public void ThereIsAlwaysAMatchingBoard()
        {
            // This should always return something valid, and be it only something with an empty controller
            var board = Board.Create();
            Assert.NotNull(board);
            var property = board.GetType().GetProperty("Initialized", BindingFlags.Instance | BindingFlags.NonPublic)!;
            Assert.True((bool)property.GetValue(board)!);
            board.Dispose();
        }

        [Fact]
        public void GpioControllerCreateOpenClosePin()
        {
            var board = CreateBoard();
            _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Output)).Returns(true);
            _mockedGpioDriver.Setup(x => x.GetPinModeEx(1)).Returns(PinMode.Output);
            _mockedGpioDriver.Setup(x => x.WriteEx(1, PinValue.High));
            _mockedGpioDriver.Setup(x => x.ClosePinEx(1));
            var ctrl = board.CreateGpioController();
            Assert.NotNull(ctrl);
            ctrl.OpenPin(1, PinMode.Output);
            ctrl.Write(1, PinValue.High);
            ctrl.ClosePin(1);
        }

        [Fact]
        public void GetSameGpioPinWhenOpen()
        {
            var board = CreateBoard();
            var ctrl = board.CreateGpioController();
            var firstGpioPin = ctrl.OpenPin(1);
            var secondGpioPin = ctrl.OpenPin(1);
            Assert.Equal(firstGpioPin, secondGpioPin);
        }

        [Fact]
        public void OpenPinAlreadyAssignedToOtherControllerThrows()
        {
            var board = CreateBoard();
            var ctrl = board.CreateGpioController();
            ctrl.OpenPin(1);
            var ctrl2 = board.CreateGpioController(); // This so far is valid
            Assert.Throws<InvalidOperationException>(() => ctrl2.OpenPin(1));
        }

        [Fact]
        public void UsingMultiplePinsWorks()
        {
            _mockedGpioDriver.Setup(x => x.OpenPinEx(2));
            _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(2, PinMode.Output)).Returns(true);
            Board b = new CustomGenericBoard(_mockedGpioDriver.Object);
            var ctrl = b.CreateGpioController();
            ctrl.OpenPin(2, PinMode.Output); // Our test board maps physical pin 2 to logical pin 1
        }

        [Fact]
        public void ReservePinI2c()
        {
            var board = CreateBoard();
            board.ReservePin(1, PinUsage.I2c, this);
            // Already in use for I2c
            Assert.Throws<InvalidOperationException>(() => board.ReservePin(1, PinUsage.Gpio, this));
            // Also fails, use the shared bus instance if creating multiple devices
            Assert.Throws<InvalidOperationException>(() => board.ReservePin(1, PinUsage.Gpio, this));
        }

        [Fact]
        public void ReservePinGpio()
        {
            var board = CreateBoard();
            board.ReservePin(1, PinUsage.Gpio, this);
            // Already in use for Gpio
            Assert.Throws<InvalidOperationException>(() => board.ReservePin(1, PinUsage.I2c, this));
            // Fails, Gpio cannot share pins
            Assert.Throws<InvalidOperationException>(() => board.ReservePin(1, PinUsage.Gpio, this));
        }

        [Fact]
        public void ReserveReleasePin()
        {
            var board = CreateBoard();
            board.ReservePin(1, PinUsage.I2c, this);
            // Not reserved for Gpio
            Assert.Throws<InvalidOperationException>(() => board.ReleasePin(1, PinUsage.Gpio, this));
            // Reserved by somebody else
            Assert.Throws<InvalidOperationException>(() => board.ReleasePin(1, PinUsage.I2c, new object()));
            // Not reserved
            Assert.Throws<InvalidOperationException>(() => board.ReleasePin(2, PinUsage.Pwm, this));

            board.ReleasePin(1, PinUsage.I2c, this);
        }

        [Fact]
        public void CreateI2cDeviceDefault()
        {
            var board = CreateBoard();
            Assert.Equal(PinUsage.Unknown, board.DetermineCurrentPinUsage(0));
            Assert.Equal(PinUsage.Unknown, board.DetermineCurrentPinUsage(1));
            var device = board.CreateI2cDevice(new I2cConnectionSettings(0, 3));
            Assert.NotNull(device);
            // The mocked board has pins 0 and 1 for the i2c bus
            Assert.Equal(PinUsage.I2c, board.DetermineCurrentPinUsage(0));
            Assert.Equal(PinUsage.I2c, board.DetermineCurrentPinUsage(1));
        }

        [Fact]
        public void CreateI2cBusDefaultAndRelease()
        {
            var board = CreateBoard();
            Assert.Equal(PinUsage.Unknown, board.DetermineCurrentPinUsage(0));
            Assert.Equal(PinUsage.Unknown, board.DetermineCurrentPinUsage(1));
            var bus = board.CreateOrGetI2cBus(0, new int[] { 0, 1 });
            Assert.NotNull(bus);
            // The mocked board has pins 0 and 1 for the i2c bus
            Assert.Equal(PinUsage.I2c, board.DetermineCurrentPinUsage(0));
            Assert.Equal(PinUsage.I2c, board.DetermineCurrentPinUsage(1));
            bus.Dispose();
            // This stays like this, because DetermineCurrentPinUsage returns the last known usage,
            // even if the pin is closed
            Assert.Equal(PinUsage.I2c, board.DetermineCurrentPinUsage(0));
            Assert.Equal(PinUsage.I2c, board.DetermineCurrentPinUsage(1));
            // This usage should be fine now
            board.ReservePin(0, PinUsage.Pwm, this);
        }

        [Fact]
        public void TwoI2cDevicesCanSharePins()
        {
            var board = CreateBoard();
            var device1 = board.CreateI2cDevice(new I2cConnectionSettings(0, 0x55));
            var device2 = board.CreateI2cDevice(new I2cConnectionSettings(0, 0x52));
            // Now all fine
            Assert.Equal(0xff, device1.ReadByte());
            Assert.Equal(0xff, device2.ReadByte());
            Assert.Equal(PinUsage.I2c, board.DetermineCurrentPinUsage(0));
            Assert.Equal(PinUsage.I2c, board.DetermineCurrentPinUsage(1));
            device1.Dispose();

            // Still fine
            Assert.Equal(0xff, device2.ReadByte());
            // Not so fine
            Assert.Throws<ObjectDisposedException>(() => device1.ReadByte());
            // Also not fine (since pins still open)
            var ctrl = board.CreateGpioController();
            Assert.Throws<InvalidOperationException>(() => ctrl.OpenPin(0));
            device2.Dispose();
            // Still bad (device.Dispose does not close the bus)
            Assert.Throws<InvalidOperationException>(() => ctrl.OpenPin(0));
            var bus = board.CreateOrGetI2cBus(0);
            bus.Dispose();
            // Now fine
            ctrl.OpenPin(0);
        }

        [Fact]
        public void CreateAndRemoveI2cDevice()
        {
            using Board board = CreateBoard();
            I2cBus bus = board.CreateOrGetI2cBus(0);

            I2cDevice device1 = bus.CreateDevice(0x55);
            device1.ReadByte();
            bus.RemoveDevice(0x55);

            I2cDevice device2 = bus.CreateDevice(0x55);
            device2.ReadByte();
        }

        [Fact]
        public void CreateAndDisposeI2cDevice()
        {
            using Board board = CreateBoard();
            I2cBus bus = board.CreateOrGetI2cBus(0);

            I2cDevice device1 = bus.CreateDevice(0x55);
            device1.ReadByte();
            device1.Dispose();

            I2cDevice device2 = bus.CreateDevice(0x55);
            device2.ReadByte();
        }

        [Fact]
        public void CreateSpiDeviceDefault()
        {
            var board = CreateBoard();
            var device = board.CreateSpiDevice(new SpiConnectionSettings(0, 0)) as SpiDeviceManager;
            Assert.NotNull(device);
            var simDevice = device!.RawDevice as SpiDummyDevice;
            Assert.NotNull(simDevice);
            Assert.Equal(0xF8, simDevice!.ReadByte());
            // See simulation board implementation why this should be the case
            Assert.Equal(new int[] { 2, 3, 4, 10 }, simDevice.Pins);
        }

        [Fact]
        public void TwoSpiDevicesCanSharePins()
        {
            var board = CreateBoard();
            var device1 = board.CreateSpiDevice(new SpiConnectionSettings(0, 1));
            var device2 = board.CreateSpiDevice(new SpiConnectionSettings(0, 2));
            // Now all fine
            Assert.Equal(0xf8, device1.ReadByte());
            Assert.Equal(0xf8, device2.ReadByte());
            Assert.Equal(PinUsage.Spi, board.DetermineCurrentPinUsage(2));
            Assert.Equal(PinUsage.Spi, board.DetermineCurrentPinUsage(3));
            device1.Dispose();

            // Still fine
            Assert.Equal(0xf8, device2.ReadByte());
            // Not so fine
            Assert.Throws<ObjectDisposedException>(() => device1.ReadByte());
            // Also not fine (since pins still open)
            var ctrl = board.CreateGpioController();
            Assert.Throws<InvalidOperationException>(() => ctrl.OpenPin(2));
            device2.Dispose();
            // Now fine
            ctrl.OpenPin(0);
        }

        private Board CreateBoard()
        {
            return new CustomGenericBoard(_mockedGpioDriver.Object);
        }

        private sealed class CustomGenericBoard : GenericBoard
        {
            public CustomGenericBoard(GpioDriver mockedDriver)
            {
                MockedDriver = mockedDriver;
            }

            public GpioDriver MockedDriver
            {
                get;
            }

            protected override GpioDriver? TryCreateBestGpioDriver()
            {
                return MockedDriver;
            }

            public override int[] GetDefaultPinAssignmentForI2c(int busId)
            {
                if (busId == 0)
                {
                    return new int[] { 0, 1 };
                }

                throw new NotSupportedException($"No simulated bus id {busId}");
            }

            public override int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings)
            {
                if (connectionSettings.BusId == 0)
                {
                    if (connectionSettings.ChipSelectLine == 0 || connectionSettings.ChipSelectLine == -1)
                    {
                        return new int[]
                        {
                            2, 3, 4, 10 // simulate: CE0 is logical pin 10
                        };
                    }
                    else
                    {
                        return new int[] { 2, 3, 4 };
                    }
                }

                throw new NotSupportedException($"No simulated bus id {connectionSettings.BusId}");
            }

            protected override I2cBusManager CreateI2cBusCore(int busNumber, int[]? pins)
            {
                return new I2cBusManager(this, busNumber, pins, new I2cDummyBus(busNumber));
            }

            protected override SpiDevice CreateSimpleSpiDevice(SpiConnectionSettings connectionSettings, int[] pins)
            {
                return new SpiDummyDevice(connectionSettings, pins);
            }
        }
    }
}
