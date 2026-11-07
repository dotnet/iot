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
using Microsoft.Extensions.Time.Testing;
using Moq;
using UnitsNet;
using Xunit;

namespace Iot.Device.Nmea0183.Tests
{
    public sealed class AutoPilotRemoteControllerTests : IDisposable
    {
        private readonly AutoPilotRemoteController _testee;
        private readonly TestSource _testSource;
        private readonly FakeTimeProvider _timeProvider;

        public AutoPilotRemoteControllerTests()
        {
            _testSource = new TestSource();
            _timeProvider = new FakeTimeProvider();
            _testee = new AutoPilotRemoteController("Test", _testSource.InterfaceName, _timeProvider);
            _testSource.OnNewSequence += (x, y) => _testee.SendSentence(x, y);
        }

        public void Dispose()
        {
            _testee.Dispose();
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

            _testee.StartDecode();
            _testee.SendSentence(_testSource, msg);

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
            _timeProvider.Advance(TimeSpan.FromSeconds(10));
            // We still need to wait, because the timeout is checked in a background thread with
            // a slow update rate, so we need to wait for the background thread to run and update the status.
            Thread.Sleep(3000);
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
            _testee.OnNewSequence += (s, i) =>
            {
                Assert.True(ks.Equals(i));
                called = true;
            };

            _testee.StartDecode();

            _testee.TurnBy(Angle.FromDegrees(20), TurnDirection.TurnToStarboard);
            int i = 100;
            while (i-- > 0)
            {
                if (_testee.IsTurning == false)
                {
                    break;
                }

                Thread.Sleep(100);
            }

            Assert.NotEqual(0, i);
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
            bool called = false;
            _testee.OnNewSequence += (s, i) =>
            {
                Assert.Equal(ks, i);
                called = true;
            };

            _testee.StartDecode();

            _testee.TurnBy(Angle.FromDegrees(90), TurnDirection.TurnToPort);
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

            _testee.StartDecode();

            int indexOfCommand = 0;
            _testee.OnNewSequence += (s, i) =>
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

            _testee.TurnBy(Angle.FromDegrees(12), TurnDirection.TurnToPort);
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
        public void SetAndResetDeadbandMode()
        {
            SetToStandby();
            Assert.Equal(DeadbandMode.Automatic, _testee.DeadbandMode);
            Assert.False(_testee.SetDeadbandMode(DeadbandMode.Minimal));
            SetToAuto();

            int num = 0;
            Keystroke ks = new Keystroke(0x0a); // We expect that it's automatically guessing the "nearer" direction right
            _testee.OnNewSequence += (s, i) =>
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
            _testee.OnNewSequence += (s, i) =>
            {
                Assert.Equal(ks, i);
                var msg = new CompassHeadingAutopilotCourse()
                {
                    AutopilotStatus = AutopilotStatus.Auto,
                    AutoPilotCourse = Angle.FromDegrees(10),
                    CompassHeading = Angle.FromDegrees(9),
                };

                _testee.SendSentence(_testSource, msg);
            };

            TurnDirection? ignore = TurnDirection.TurnToStarboard;
            Assert.True(_testee.SetStatus(AutopilotStatus.Auto, ref ignore));
        }
    }
}
