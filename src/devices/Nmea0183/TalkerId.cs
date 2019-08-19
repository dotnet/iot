// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Represents 2 character NMEA0183 talker identifier
    /// </summary>
    public struct TalkerId : IEquatable<TalkerId>
    {
        public char Id1 { get; private set; }
        public char Id2 { get; private set; }

        public override string ToString() => $"{Id1}{Id2}";

        /// <summary>
        /// Constructs NMEA0183 talker identifier
        /// </summary>
        /// <param name="id1">first character identifying the talker</param>
        /// <param name="id2">second character identifying the talker</param>
        public TalkerId(char id1, char id2)
        {
            Id1 = id1;
            Id2 = id2;
        }

        public override bool Equals(object obj)
        {
            if (obj is SentenceId other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id1, Id2);
        }

        public bool Equals(TalkerId other)
        {
            return Id1 == other.Id1 && Id2 == other.Id2;
        }

        public static bool operator== (TalkerId obj1, TalkerId obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator!= (TalkerId obj1, TalkerId obj2) => !(obj1 == obj2);

        // Below information is based on http://www.tronico.fi/OH6NT/docs/NMEA0183.pdf page 3
        // Most up to date list can be found at
        // https://www.nmea.org/Assets/20190303%20nmea%200183%20talker%20identifier%20mnemonics.pdf
        // Only 'GN' was added from that list.

        /// <summary>
        /// Autopilot - General
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId AutopilotGeneral => new TalkerId('A', 'G');

        /// <summary>
        /// Autopilot - Magnetic
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId AutopilotMagnetic => new TalkerId('A', 'P');

        /// <summary>
        /// Communications – Digital Selective Calling (DSC)
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId CommunicationsDigitalSelectiveCalling => new TalkerId('C', 'D');

        /// <summary>
        /// Communications – Receiver / Beacon Receiver
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId CommunicationsReceiverOrBeaconReceiver => new TalkerId('C', 'R');

        /// <summary>
        /// Communications – Satellite
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId CommunicationsSatellite => new TalkerId('C', 'S');

        /// <summary>
        /// Communications – Radio-Telephone (MF/HF)
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId CommunicationsRadioTelephoneMfHf => new TalkerId('C', 'T');

        /// <summary>
        /// Communications – Radio-Telephone (VHF)
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId CommunicationsRadioTelephoneVhf => new TalkerId('C', 'V');

        /// <summary>
        /// Communications – Scanning Receiver
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId CommunicationsScanningReceiver => new TalkerId('C', 'X');

        /// <summary>
        /// Direction Finder
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId DirectionFinder => new TalkerId('D', 'F');

        /// <summary>
        /// Electronic Chart Display and Information System (ECDIS)
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId ElectronicChartDisplayAndInformationSystem => new TalkerId('E', 'C');

        /// <summary>
        /// Emergency Position Indicating Beacon (EPIRB)
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId EmergencyPositionIndicatingBeacon => new TalkerId('E', 'P');

        /// <summary>
        /// Engine Room Monitoring Systems
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId EngineRoomMonitoringSystems => new TalkerId('E', 'R');

        /// <summary>
        /// Global Positioning System (GPS)
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId GlobalPositioningSystem => new TalkerId('G', 'P');

        /// <summary>
        /// Global Navigation Satellite System (GNSS)
        /// </summary>
        /// <returns>TalkerId instance</returns>
        /// <remarks>This identifier can be used when sentence is produced from multiple satellite systems.</remarks>
        public static TalkerId GlobalNavigationSatelliteSystem => new TalkerId('G', 'N');

        /// <summary>
        /// Heading – Magnetic Compass
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId HeadingMagneticCompass => new TalkerId('H', 'C');

        /// <summary>
        /// Heading – North Seeking Gyro
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId HeadingNorthSeekingGyro => new TalkerId('H', 'E');

        /// <summary>
        /// Heading – Non North Seeking Gyro
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId HeadingNonNorthSeekingGyro => new TalkerId('H', 'N');

        /// <summary>
        /// Integrated Instrumentation
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId IntegratedInstrumentation => new TalkerId('I', 'I');

        /// <summary>
        /// Integrated Navigation
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId IntegratedNavigation => new TalkerId('I', 'N');

        /// <summary>
        /// Loran C
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId LoranC => new TalkerId('L', 'C');

        /// <summary>
        /// RADAR and/or ARPA
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId RadarAndOrArpa => new TalkerId('R', 'A');

        /// <summary>
        /// Sounder, Depth
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId SounderDepth => new TalkerId('S', 'D');

        /// <summary>
        /// Electronic Positioning System, other/general
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId ElectronicPositioningSystem => new TalkerId('S', 'N');

        /// <summary>
        /// Sounder, Scanning
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId SounderScanning => new TalkerId('S', 'S');

        /// <summary>
        /// Turn Rate Indicator
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId TurnRateIndicator => new TalkerId('T', 'I');

        /// <summary>
        /// Velocity Sensor, Doppler, other/general
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId VelocitySensorDoppler => new TalkerId('V', 'D');

        /// <summary>
        /// Velocity Sensor, Speed Log, Water, Magnetic
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId VelocitySensorSpeedLogWaterMagnetic => new TalkerId('D', 'M');

        /// <summary>
        /// Velocity Sensor, Speed Log, Water, Mechanical
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId VelocitySensorSpeedLogWaterMechanical => new TalkerId('V', 'W');

        /// <summary>
        /// Weather Instruments
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId WeatherInstruments => new TalkerId('W', 'I');

        /// <summary>
        /// Transducer
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId Transducer => new TalkerId('Y', 'X');

        /// <summary>
        /// Timekeeper – Atomic Clock
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId TimekeeperAtomicClock => new TalkerId('Z', 'A');

        /// <summary>
        /// Timekeeper – Chronometer
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId TimekeeperChronometer => new TalkerId('Z', 'C');

        /// <summary>
        /// Timekeeper – Quartz
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId TimekeeperQuartz => new TalkerId('Z', 'Q');

        /// <summary>
        /// Timekeeper – Radio Update, WWV or WWVH
        /// </summary>
        /// <returns>TalkerId instance</returns>
        public static TalkerId TimekeeperRadioUpdateWwvOrWwvh => new TalkerId('Z', 'V');
    }
}