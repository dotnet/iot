// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Seatalk1;
using Iot.Device.Seatalk1.Messages;
using Moq;
using UnitsNet;
using Xunit;

namespace Iot.Device.Tests.Seatalk1
{
    public sealed class AutoPilotRemoteControllerTests
    {
        private readonly Mock<SeatalkInterface> _interface;
        private readonly AutoPilotRemoteController _testee;
        private readonly CancellationTokenSource _tokenSource;

        public AutoPilotRemoteControllerTests()
        {
            _interface = new Mock<SeatalkInterface>(MockBehavior.Strict);
            _testee = new AutoPilotRemoteController(_interface.Object);
            _tokenSource = new CancellationTokenSource();
        }

        [Fact]
        public void StartupWasSuccessful()
        {
            Assert.Equal(AutopilotStatus.Offline, _testee.Status);
            Assert.Null(_testee.AutopilotDesiredHeading);
            Assert.Null(_testee.AutopilotHeading);
            Assert.Equal(AutopilotAlarms.None, _testee.Alarms);
            Assert.False(_testee.RudderAngleAvailable);
        }

        [Fact]
        public void MakeReady()
        {
            var msg = new CompassHeadingAutopilotCourse()
            {
                AutopilotStatus = AutopilotStatus.Standby, AutoPilotCourse = Angle.FromDegrees(100), CompassHeading = Angle.FromDegrees(95)
            };

            _interface.Object.OnNewMessage(msg);
            Assert.Equal(AutopilotStatus.Standby, _testee.Status);
            Assert.Null(_testee.AutopilotDesiredHeading);
            Assert.Equal(Angle.FromDegrees(95), _testee.AutopilotHeading);
            Assert.Equal(AutopilotAlarms.None, _testee.Alarms);
            Assert.False(_testee.RudderAngleAvailable);
        }

        [Fact]
        public void SetToAuto()
        {
            var msg = new CompassHeadingAutopilotCourse()
            {
                AutopilotStatus = AutopilotStatus.Auto,
                AutoPilotCourse = Angle.FromDegrees(100),
                CompassHeading = Angle.FromDegrees(95),
                Alarms = AutopilotAlarms.OffCourse,
                AutoPilotType = 2,
                RudderPosition = Angle.FromDegrees(-10)
            };

            _interface.Object.OnNewMessage(msg);
            Assert.Equal(AutopilotStatus.Auto, _testee.Status);
            Assert.Equal(Angle.FromDegrees(100), _testee.AutopilotDesiredHeading);
            Assert.Equal(Angle.FromDegrees(95), _testee.AutopilotHeading);
            Assert.Equal(AutopilotAlarms.OffCourse, _testee.Alarms);
            Assert.True(_testee.RudderAngleAvailable);
            Assert.Equal(Angle.FromDegrees(-10), _testee.RudderAngle);
        }

        [Fact]
        public void AnglesOnly()
        {
            var msg = new CompassHeadingAndRudderPosition()
            {
                CompassHeading = Angle.FromDegrees(90),
                RudderPosition = Angle.Zero,
            };

            _interface.Object.OnNewMessage(msg);
            Assert.Null(_testee.AutopilotDesiredHeading);
            Assert.Equal(Angle.FromDegrees(90), _testee.AutopilotHeading);
            Assert.Equal(AutopilotAlarms.None, _testee.Alarms);
            Assert.False(_testee.RudderAngleAvailable);
        }

        [Fact]
        public void TurnToStarboard()
        {
            // Ap is in auto mode
            var msg = new CompassHeadingAutopilotCourse()
            {
                AutopilotStatus = AutopilotStatus.Auto,
                AutoPilotCourse = Angle.FromDegrees(10),
                CompassHeading = Angle.FromDegrees(9),
            };

            _interface.Object.OnNewMessage(msg);

            Keystroke ks = new Keystroke(AutopilotButtons.PlusTen, 1);
            _interface.Setup(x => x.SendMessage(It.IsAny<SeatalkMessage>())).Callback<SeatalkMessage>(i =>
            {
                Assert.True(ks.Equals(i));
            }).Returns(true);
            _tokenSource.Cancel(); // Cancellation is only tested after the first command was sent
            Assert.False(_testee.TurnBy(Angle.FromDegrees(90), TurnDirection.Starboard, _tokenSource.Token));

            _interface.Verify(x => x.SendMessage(ks));
        }

        [Fact]
        public void TurnToPort()
        {
            // Ap is in auto mode
            var msg = new CompassHeadingAutopilotCourse()
            {
                AutopilotStatus = AutopilotStatus.Auto,
                AutoPilotCourse = Angle.FromDegrees(10),
                CompassHeading = Angle.FromDegrees(9),
            };

            _interface.Object.OnNewMessage(msg);

            Keystroke ks = new Keystroke(AutopilotButtons.MinusTen, 1);
            _interface.Setup(x => x.SendMessage(It.IsAny<SeatalkMessage>())).Callback<SeatalkMessage>(i =>
            {
                Assert.Equal(ks, i);
            }).Returns(true);
            _tokenSource.Cancel(); // Cancellation is only tested after the first command was sent
            Assert.False(_testee.TurnBy(Angle.FromDegrees(90), TurnDirection.Port, _tokenSource.Token));

            _interface.Verify(x => x.SendMessage(ks));
        }
    }
}
