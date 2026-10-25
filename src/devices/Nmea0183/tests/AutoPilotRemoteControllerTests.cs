// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Nmea0183.Tests;
using Iot.Device.Seatalk1;
using Iot.Device.Seatalk1.Messages;
using Moq;
using UnitsNet;
using Xunit;

namespace Iot.Device.Tests.Seatalk1
{
    public sealed class AutoPilotRemoteControllerTests
    {
        private readonly AutoPilotRemoteController _testee;
        private readonly CancellationTokenSource _tokenSource;
        private readonly TestSource _testSource;

        public AutoPilotRemoteControllerTests()
        {
            _testSource = new TestSource();
            _testee = new AutoPilotRemoteController("Test", _testSource.InterfaceName);
            _testSource.OnNewSequence += (x, y) => _testee.SendSentence(x, y);
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

            _testSource.SendSentenceToTestClass(msg);
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

            _testSource.SendSentenceToTestClass(msg);
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
            _testee.TurnTo(Angle.FromDegrees(100), TurnDirection.TurnToStarboard);
            Assert.False(_testee.IsTurning);
        }

        [Fact]
        public void ReturnsToOfflineAfterTimeout()
        {
            SetToStandby();
            Assert.Equal(AutopilotStatus.Standby, _testee.Status);
            Assert.Equal(AutopilotStatus.Standby, _testee.Status);
            _testee.DefaultTimeout = TimeSpan.FromSeconds(0);
            Thread.Sleep(100);
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

            _testSource.SendSentenceToTestClass(msg);

            Keystroke ks = new Keystroke(AutopilotButtons.PlusTen, 1);
            bool called = false;
            _testSource.MessageReceived += (s, i) =>
            {
                Assert.True(ks.Equals(i));
                called = true;
            };

            _tokenSource.Cancel(); // Cancellation is only tested after the first command was sent
            _testee.TurnBy(Angle.FromDegrees(20), TurnDirection.TurnToStarboard, _tokenSource.Token);
            int i = 100;
            while (i-- > 0)
            {
                if (_testee.IsTurning == false || called)
                {
                    break;
                }

                Thread.Sleep(100);
            }

            Assert.True(called);
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

            _testSource.SendSentenceToTestClass(msg);

            Keystroke ks = new Keystroke(AutopilotButtons.MinusTen, 1);
            _testSource.MessageReceived += (s, i) =>
            {
                Assert.Equal(ks, i);
            };

            _tokenSource.Cancel(); // Cancellation is only tested after the first command was sent
            _testee.TurnBy(Angle.FromDegrees(90), TurnDirection.TurnToPort, _tokenSource.Token);
            int i = 100;
            while (i-- > 0)
            {
                if (_testee.IsTurning == false)
                {
                    break;
                }

                Thread.Sleep(100);
            }
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

            _testSource.SendSentenceToTestClass(msg);

            int indexOfCommand = 0;
            _testSource.MessageReceived += (s, i) =>
            {
                var ks = (Keystroke)i;
                if (indexOfCommand == 0)
                {
                    Assert.True(ks.ButtonsPressed == AutopilotButtons.MinusTen);
                    msg = new CompassHeadingAutopilotCourse()
                    {
                        AutopilotStatus = AutopilotStatus.Auto,
                        CompassHeading = Angle.FromDegrees(9),
                        AutoPilotCourse = Angle.Zero
                    };
                }
                else if (indexOfCommand == 1)
                {
                    Assert.True(ks.ButtonsPressed == AutopilotButtons.MinusOne);
                    msg = msg = new CompassHeadingAutopilotCourse()
                    {
                        AutopilotStatus = AutopilotStatus.Auto,
                        AutoPilotCourse = Angle.FromDegrees(359),
                        CompassHeading = Angle.FromDegrees(9),
                    };
                }
                else if (indexOfCommand == 2)
                {
                    Assert.True(ks.ButtonsPressed == AutopilotButtons.MinusOne);
                    msg = msg = new CompassHeadingAutopilotCourse()
                    {
                        AutopilotStatus = AutopilotStatus.Auto,
                        AutoPilotCourse = Angle.FromDegrees(358),
                        CompassHeading = Angle.FromDegrees(9),
                    };
                }
                else
                {
                    Assert.Fail("More commands sent than necessary");
                }

                _testSource.SendSentenceToTestClass(msg);
                indexOfCommand++;
            };

            _testee.TurnBy(Angle.FromDegrees(12), TurnDirection.TurnToPort, _tokenSource.Token);
            int i = 100;
            while (i-- > 0)
            {
                if (_testee.IsTurning == false)
                {
                    break;
                }
            }

            Assert.NotEqual(0, i);
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

            _testSource.SendSentenceToTestClass(msg);

            bool called = false;
            Keystroke ks = new Keystroke(AutopilotButtons.PlusTen, 1); // We expect that it's automatically guessing the "nearer" direction right
            _testSource.MessageReceived += (s, i) =>
            {
                Assert.Equal(ks, i);
                called = true;
            };
            _tokenSource.Cancel(); // Cancellation is only tested after the first command was sent
            _testee.TurnTo(Angle.FromDegrees(100), null, _tokenSource.Token);

            int i = 100;
            while (i-- > 0)
            {
                if (_testee.IsTurning == false || called)
                {
                    break;
                }

                Thread.Sleep(100);
            }
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
            _testSource.MessageReceived += (s, i) =>
            {
                if (num == 0)
                {
                    Assert.Equal(ks, i);
                    var msg = new DeadbandSetting()
                    {
                        Mode = DeadbandMode.Minimal,
                    };

                    _testSource.SendSentenceToTestClass(msg);
                }
                else
                {
                    var ks2 = (Keystroke)i;
                    Assert.Equal(0x09, ks2.KeyCode);
                    var msg = new DeadbandSetting()
                    {
                        Mode = DeadbandMode.Automatic,
                    };

                    _testSource.SendSentenceToTestClass(msg);
                }

                num++;
            };

            Assert.True(_testee.SetDeadbandMode(DeadbandMode.Minimal));
            Assert.True(_testee.SetDeadbandMode(DeadbandMode.Automatic));
        }

        [Fact]
        public void AnglesAreClose()
        {
            Assert.True(_testee.AnglesAreClose(Angle.FromDegrees(359), Angle.FromDegrees(0.1), Angle.FromDegrees(2)));
            Assert.True(_testee.AnglesAreClose(Angle.FromDegrees(360), Angle.FromDegrees(359), Angle.FromDegrees(2)));
            Assert.True(_testee.AnglesAreClose(Angle.FromDegrees(0), Angle.FromDegrees(359), Angle.FromDegrees(1.1)));
            Assert.True(_testee.AnglesAreClose(Angle.FromDegrees(181), Angle.FromDegrees(180), Angle.FromDegrees(1.1)));
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
            _testSource.MessageReceived += (s, i) =>
            {
                Assert.Equal(ks, i);
                var msg = new CompassHeadingAutopilotCourse()
                {
                    AutopilotStatus = AutopilotStatus.Auto,
                    AutoPilotCourse = Angle.FromDegrees(10),
                    CompassHeading = Angle.FromDegrees(9),
                };

                _testSource.SendSentenceToTestClass(msg);
            };

            TurnDirection? ignore = TurnDirection.TurnToStarboard;
            Assert.True(_testee.SetStatus(AutopilotStatus.Auto, ref ignore));
        }
    }
}
