// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Pwm;
using Moq;
using Xunit;

namespace Iot.Device.ServoMotor.Tests
{
    public class ServoMotorTests
    {
        [Theory]
        [InlineData(-1, 1_000, 2_000)]
        [InlineData(361, 1_000, 2_000)]
        [InlineData(180, -1, -1)]
        [InlineData(180, -1, 2_000)]
        [InlineData(180, 1_000, -1)]
        [InlineData(180, 1_000, 999)]
        public void Exception_Should_Throw_When_Providing_Invalid_Constructor_Arguments(
            int maximumAngle,
            int minimumPulseWidthMicroseconds,
            int maximumPulseWidthMicroseconds)
        {
            Mock<PwmChannel> mockPwmChannel = new Mock<PwmChannel>();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new ServoMotor(mockPwmChannel.Object, maximumAngle, minimumPulseWidthMicroseconds, maximumPulseWidthMicroseconds);
            });
        }

        [Fact]
        public void Pwm_Channel_Should_Start_And_Stop_When_Starting_And_Stopping_Servo()
        {
            Mock<PwmChannel> mockPwmChannel = new Mock<PwmChannel>();
            ServoMotor servo = new ServoMotor(mockPwmChannel.Object);

            servo.Start();
            mockPwmChannel.Verify(channel => channel.Start());
            mockPwmChannel.VerifyNoOtherCalls();

            mockPwmChannel.Reset();

            servo.Stop();
            mockPwmChannel.Verify(channel => channel.Stop());
            mockPwmChannel.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(50, 180, 1_000, 2_000, 0, 0.05)]
        [InlineData(50, 180, 1_000, 2_000, 45, 0.0625)]
        [InlineData(50, 180, 1_000, 2_000, 135, 0.0875)]
        [InlineData(50, 180, 1_000, 2_000, 180, 0.1)]
        [InlineData(50, 90, 1_000, 1_500, 0, 0.05)]
        [InlineData(50, 90, 1_000, 1_500, 90, 0.075)]
        public void Verify_Duty_Cycle_When_Writing_Angle(
            int frequency,
            int maxiumAngle,
            int minimumPulseWidthMicroseconds,
            int maximumPulseWidthMicroseconds,
            int angle,
            double expectedDutyCycle)
        {
            Mock<PwmChannel> mockPwmChannel = new Mock<PwmChannel>();
            mockPwmChannel.SetupAllProperties();
            mockPwmChannel.Object.Frequency = frequency;
            ServoMotor servo = new ServoMotor(
                mockPwmChannel.Object,
                maxiumAngle,
                minimumPulseWidthMicroseconds,
                maximumPulseWidthMicroseconds);

            servo.WriteAngle(angle);

            Assert.Equal(expectedDutyCycle, mockPwmChannel.Object.DutyCycle, 5);
        }

        [Theory]
        [InlineData(50, 1_000, 0.05)]
        [InlineData(50, 2_000, 0.1)]
        [InlineData(60, 1_000, 0.06)]
        [InlineData(60, 2_000, 0.12)]
        public void Verify_Duty_Cycle_When_Writing_Pulse_Width(
            int frequency,
            int pulseWithInMicroseconds,
            double expectedDutyCycle)
        {
            Mock<PwmChannel> mockPwmChannel = new Mock<PwmChannel>();
            mockPwmChannel.SetupAllProperties();
            mockPwmChannel.Object.Frequency = frequency;
            ServoMotor servo = new ServoMotor(mockPwmChannel.Object);

            servo.WritePulseWidth(pulseWithInMicroseconds);

            Assert.Equal(expectedDutyCycle, mockPwmChannel.Object.DutyCycle, 5);
        }

        [Theory]
        [InlineData(90, -1)]
        [InlineData(45, 46)]
        [InlineData(90, 180)]
        public void Exception_Should_Throw_When_Writing_Invalid_Angle(int maximumAngle, int angle)
        {
            Mock<PwmChannel> mockPwmChannel = new Mock<PwmChannel>();
            ServoMotor servoMotor = new ServoMotor(mockPwmChannel.Object, maximumAngle);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                servoMotor.WriteAngle(angle);
            });
        }
    }
}
