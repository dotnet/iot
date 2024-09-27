// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Nmea0183.Sentences;
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
        public void SetToStandby()
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
        public void TurnToHasNoEffectWhenStandby()
        {
            SetToStandby();
            Assert.Equal(AutopilotStatus.Standby, _testee.Status);
            // Also: Shouldn't block, despite no Cancellation given
            Assert.False(_testee.TurnTo(Angle.FromDegrees(100), TurnDirection.Starboard));
        }

        [Fact]
        public void ReturnsToOfflineAfterTimeout()
        {
            SetToStandby();
            Assert.Equal(AutopilotStatus.Standby, _testee.Status);
            _testee.UpdateStatus();
            Assert.Equal(AutopilotStatus.Standby, _testee.Status);
            _testee.DefaultTimeout = TimeSpan.FromSeconds(0);
            Thread.Sleep(100);
            _testee.UpdateStatus();
            Assert.Equal(AutopilotStatus.Offline, _testee.Status);
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

        [Fact]
        public void TurnToPortFullSequence()
        {
            // Ap is in auto mode
            var msg = new CompassHeadingAutopilotCourse()
            {
                AutopilotStatus = AutopilotStatus.Auto,
                AutoPilotCourse = Angle.FromDegrees(10),
                CompassHeading = Angle.FromDegrees(9),
            };

            _interface.Object.OnNewMessage(msg);

            int indexOfCommand = 0;
            _interface.Setup(x => x.SendMessage(It.IsAny<SeatalkMessage>())).Callback<SeatalkMessage>(i =>
            {
                var ks = (Keystroke)i;
                if (indexOfCommand == 0)
                {
                    Assert.True(ks.ButtonsPressed == AutopilotButtons.MinusTen);
                    msg = msg with
                    {
                        AutoPilotCourse = Angle.Zero
                    };
                }
                else if (indexOfCommand == 1)
                {
                    Assert.True(ks.ButtonsPressed == AutopilotButtons.MinusOne);
                    msg = msg with
                    {
                        AutoPilotCourse = Angle.FromDegrees(359)
                    };
                }
                else if (indexOfCommand == 2)
                {
                    Assert.True(ks.ButtonsPressed == AutopilotButtons.MinusOne);
                    msg = msg with
                    {
                        AutoPilotCourse = Angle.FromDegrees(358)
                    };
                }
                else
                {
                    Assert.Fail("More commands sent than necessary");
                }

                _interface.Object.OnNewMessage(msg);
                indexOfCommand++;
            }).Returns(true);

            Assert.True(_testee.TurnBy(Angle.FromDegrees(12), TurnDirection.Port, _tokenSource.Token));
        }

        [Fact]
        public void TurnTo()
        {
            // Ap is in auto mode
            var msg = new CompassHeadingAutopilotCourse()
            {
                AutopilotStatus = AutopilotStatus.Auto,
                AutoPilotCourse = Angle.FromDegrees(10),
                CompassHeading = Angle.FromDegrees(9),
            };

            _interface.Object.OnNewMessage(msg);

            Keystroke ks = new Keystroke(AutopilotButtons.PlusTen, 1); // We expect that it's automatically guessing the "nearer" direction right
            _interface.Setup(x => x.SendMessage(It.IsAny<SeatalkMessage>())).Callback<SeatalkMessage>(i =>
            {
                Assert.Equal(ks, i);
            }).Returns(true);
            _tokenSource.Cancel(); // Cancellation is only tested after the first command was sent
            Assert.False(_testee.TurnTo(Angle.FromDegrees(100), null, _tokenSource.Token));

            _interface.Verify(x => x.SendMessage(ks));
        }

        [Fact]
        public void SetAndResetDeadbandMode()
        {
            SetToStandby();
            Assert.Equal(DeadbandMode.Automatic, _testee.DeadbandMode);
            Assert.False(_testee.SetDeadbandMode(DeadbandMode.Minimal));
            SetToAuto();

            int num = 0;
            Keystroke ks = new Keystroke(0x0a); // We expect that it's automatically guessing the "nearer" direction right
            _interface.Setup(x => x.SendMessage(It.IsAny<SeatalkMessage>())).Callback<SeatalkMessage>(i =>
            {
                if (num == 0)
                {
                    Assert.Equal(ks, i);
                    var msg = new DeadbandSetting()
                    {
                        Mode = DeadbandMode.Minimal,
                    };

                    _interface.Object.OnNewMessage(msg);
                }
                else
                {
                    var ks2 = (Keystroke)i;
                    Assert.Equal(0x09, ks2.KeyCode);
                    var msg = new DeadbandSetting()
                    {
                        Mode = DeadbandMode.Automatic,
                    };

                    _interface.Object.OnNewMessage(msg);
                }

                num++;
            }).Returns(true);

            Assert.True(_testee.SetDeadbandMode(DeadbandMode.Minimal));
            Assert.True(_testee.SetDeadbandMode(DeadbandMode.Automatic));
        }

        [Fact]
        public void AnglesAreClose()
        {
            Assert.True(_testee.AnglesAreClose(Angle.FromDegrees(359), Angle.FromDegrees(0.1)));
            Assert.True(_testee.AnglesAreClose(Angle.FromDegrees(360), Angle.FromDegrees(359)));
            Assert.True(_testee.AnglesAreClose(Angle.FromDegrees(0), Angle.FromDegrees(359)));
            Assert.True(_testee.AnglesAreClose(Angle.FromDegrees(181), Angle.FromDegrees(180)));
        }

        [Fact]
        public void ToStringTest()
        {
            Assert.NotEmpty(_testee.ToString());
        }

        [Fact]
        public void SetToAutoRemotely()
        {
            SetToStandby();
            Keystroke ks = new Keystroke(AutopilotButtons.Auto, 1); // We expect that it's automatically guessing the "nearer" direction right
            _interface.Setup(x => x.SendMessage(It.IsAny<SeatalkMessage>())).Callback<SeatalkMessage>(i =>
            {
                Assert.Equal(ks, i);
                var msg = new CompassHeadingAutopilotCourse()
                {
                    AutopilotStatus = AutopilotStatus.Auto,
                    AutoPilotCourse = Angle.FromDegrees(10),
                    CompassHeading = Angle.FromDegrees(9),
                };

                _interface.Object.OnNewMessage(msg);

            }).Returns(true);

            TurnDirection? ignore = TurnDirection.Starboard;
            Assert.True(_testee.SetStatus(AutopilotStatus.Auto, ref ignore));
        }
    }
}
