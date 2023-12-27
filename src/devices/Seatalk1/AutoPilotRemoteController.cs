using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Seatalk1.Messages;
using UnitsNet;

namespace Iot.Device.Seatalk1
{
    public class AutoPilotRemoteController
    {
        private readonly SeatalkInterface _parentInterface;
        private readonly object _lock = new object();

        internal AutoPilotRemoteController(SeatalkInterface parentInterface)
        {
            _parentInterface = parentInterface ?? throw new ArgumentNullException(nameof(parentInterface));
            _parentInterface.MessageReceived += AutopilotMessageInterpretation;
            RudderAngleAvailable = false;
            Status = AutopilotStatus.Offline;
            Alarms = AutopilotAlarms.None;
            DeadbandMode = DeadbandMode.Automatic;
        }

        public Angle? AutopilotHeading { get; private set; }

        public Angle? AutopilotDesiredHeading { get; private set; }
 
        public Angle? RudderAngle { get; private set; }

        public bool RudderAngleAvailable { get; private set; }

        public AutopilotStatus Status { get; private set; }

        public AutopilotAlarms Alarms { get; private set; }

        public DeadbandMode DeadbandMode { get; private set; }

        public event Action<Keystroke>? AutopilotKeysPressed;

        private void AutopilotMessageInterpretation(SeatalkMessage obj)
        {
            lock (_lock)
            {
                if (obj is CompassHeadingAutopilotCourse ch)
                {
                    Status = ch.AutopilotStatus;
                    AutopilotHeading = ch.CompassHeading;
                    AutopilotDesiredHeading = Status != AutopilotStatus.Standby ? ch.AutoPilotCourse : null;
                    if (!RudderAngleAvailable)
                    {
                        RudderAngleAvailable = !ch.RudderPosition.Equals(Angle.Zero, Angle.FromDegrees(0.5));
                    }

                    if (RudderAngleAvailable)
                    {
                        RudderAngle = ch.RudderPosition;
                    }
                    else
                    {
                        RudderAngle = null;
                    }

                    Alarms = ch.Alarms;
                }
                else if (obj is CompassHeadingAndRudderPosition rb)
                {
                    AutopilotHeading = rb.CompassHeading;
                    if (!RudderAngleAvailable)
                    {
                        RudderAngleAvailable = !rb.RudderPosition.Equals(Angle.Zero, Angle.FromDegrees(0.5));
                    }

                    if (RudderAngleAvailable)
                    {
                        RudderAngle = rb.RudderPosition;
                    }
                    else
                    {
                        RudderAngle = null;
                    }
                }
                else if (obj is DeadbandSetting dbs)
                {
                    DeadbandMode = dbs.Mode;
                }
                else if (obj is Keystroke keystroke)
                {
                    AutopilotKeysPressed?.Invoke(keystroke);
                }

                if (Status == AutopilotStatus.Standby)
                {
                    DeadbandMode = DeadbandMode.Automatic; // Resets automatically when going to standby (the message is not sent periodically)
                }
            }
        }
    }
}
