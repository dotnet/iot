// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Threading;
using Xunit;

namespace Iot.Device.DCMotor.Tests
{
    public class DCMotorTests
    {
        private const int SpeedPin = 19;
        private const int DirectionPin = 20;
        private const int OtherSpeedPin = 21;
        private const int OtherDirectionPin = 26;

        [Fact]
        public void DisposeDisposesTheControllerByDefault()
        {
            FakeGpioDriver driver = new();
            GpioController controller = new(driver);
            DCMotor motor = DCMotor.Create(SpeedPin, DirectionPin, controller);

            motor.Speed = 0.5;
            motor.Dispose();

            Assert.True(driver.IsDisposed);
        }

        [Fact]
        public void DisposeDoesNotDisposeACallerOwnedController()
        {
            FakeGpioDriver driver = new();
            using GpioController controller = new(driver);
            DCMotor motor = DCMotor.Create(SpeedPin, DirectionPin, controller, shouldDispose: false);

            motor.Speed = 0.5;
            motor.Dispose();

            Assert.False(driver.IsDisposed);
            controller.OpenPin(5, PinMode.Output);
            controller.Write(5, PinValue.High);
            Assert.Equal(PinValue.High, driver.GetValue(5));
        }

        [Fact]
        public void TwoMotorsSharingAControllerCanBeDisposedOneAfterTheOther()
        {
            FakeGpioDriver driver = new();
            using GpioController controller = new(driver);
            DCMotor motor1 = DCMotor.Create(SpeedPin, DirectionPin, controller, shouldDispose: false);
            DCMotor motor2 = DCMotor.Create(OtherSpeedPin, OtherDirectionPin, controller, shouldDispose: false);
            motor1.Speed = 0.5;
            motor2.Speed = 0.5;

            motor1.Dispose();
            // The software PWM thread of the second motor keeps writing to the controller
            Thread.Sleep(100);
            motor2.Speed = -0.5;
            motor2.Dispose();

            Assert.False(driver.IsDisposed);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(0.5)]
        [InlineData(-0.5)]
        [InlineData(-1.0)]
        public void DisposeLeavesBothPinsLow(double speed)
        {
            FakeGpioDriver driver = new();
            GpioController controller = new(driver);
            DCMotor motor = DCMotor.Create(SpeedPin, DirectionPin, controller);

            motor.Speed = speed;
            Thread.Sleep(100);
            motor.Dispose();

            // With the direction pin high and the speed pin low, an H-bridge without enable pin drives the motor at full speed
            Assert.Equal(PinValue.Low, driver.GetValue(SpeedPin));
            Assert.Equal(PinValue.Low, driver.GetValue(DirectionPin));
        }
    }
}
