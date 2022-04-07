// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// Interface that all active elements implement
    /// </summary>
    public class ActiveSensor : Sensor
    {
        // This is used when measuring internal sensors, that's the maximum time
        // any sensor will respond according to tsts and reading various sources
        internal const int TimeoutMeasuresSeconds = 4;
        // Thoss fields is populated by the birck once it is enumerated.
        internal List<ModeDetail> ModeDetailsInternal;
        internal List<CombiModes> CombiModesInternal;
        // This is used for advance mapping between the modes and the properties
        internal int[] CombiReadingModes = new int[0];
        // For the property
        internal string[] _valuesAsString = new string[0];
        internal bool _hasValueAsStringUpdated;

        /// <summary>
        /// Property to return the raw value of the sensort as a string. It will contains as the first elements PxCy.
        /// x = the port number, y = 0 for continuous reading, 1 for single reading.
        /// The rest are the measures.
        /// </summary>
        public IEnumerable<string> ValuesAsString
        {
            get => _valuesAsString;
            internal set
            {
                if (!_valuesAsString.SequenceEqual(value))
                {
                    _valuesAsString = value.ToArray();
                    OnPropertyChanged(nameof(ValuesAsString));
                }

                _hasValueAsStringUpdated = true;
                OnPropertyUpdated(nameof(ValuesAsString));
            }
        }

        /// <summary>
        /// Baud rate the sensor is connected
        /// </summary>
        public int BaudRate { get; internal set; }

        /// <summary>
        /// Hardware version
        /// </summary>
        public uint HardwareVersion { get; internal set; }

        /// <summary>
        /// Software version
        /// </summary>
        public uint SoftwareVersion { get; internal set; }

        /// <summary>
        /// Gets the possible combi modes. Note, is will be empty if none.
        /// </summary>
        public IEnumerable<CombiModes> CombiModes { get => CombiModesInternal.ToArray(); }

        /// <summary>
        /// Gets the mode details of the sensor.
        /// </summary>
        public IEnumerable<ModeDetail> ModeDetails { get => ModeDetailsInternal.ToArray(); }

        /// <summary>
        /// Numbers the of modes.
        /// </summary>
        /// <returns>The number of modes</returns>
        public int NumberOfModes { get => ModeDetailsInternal.Count; }

        /// <summary>
        /// Gets the recommended Speed PID settings
        /// </summary>
        public RecommendedPid SpeedPid { get; internal set; }

        /// <summary>
        /// Gets the recommended Speed PID settings
        /// </summary>
        public RecommendedPid PositionPid { get; internal set; }

        /// <summary>
        /// Creates an active element.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        /// <param name="type">The sensor type.</param>
        protected internal ActiveSensor(Brick brick, SensorPort port, SensorType type)
            : base(brick, port, type)
        {
            ModeDetailsInternal = new List<ModeDetail>();
            CombiModesInternal = new List<CombiModes>();
        }

        /// <summary>
        /// Returned the name of the selectd mode
        /// </summary>
        /// <param name="modes">The mode to select</param>
        /// <param name="once">True to read only once, false to set the continuous reading</param>
        public void SelectCombiModesAndRead(int[] modes, bool once) => Brick.SelectCombiModesAndRead(Port, modes, once);

        /// <summary>
        /// Returned the name of the selectd mode
        /// </summary>
        /// <param name="mode">The mode to select</param>
        /// <param name="once">True to read only once, false to set the continuous reading</param>
        public void SelectModeAndRead(int mode, bool once) => Brick.SelectModeAndRead(Port, mode, once);

        /// <inheritdoc/>
        public override string SensorName => "Active sensor";

        /// <summary>
        /// Stop reading continuous data from a specific sensor.
        /// </summary>
        public void StopReading() => Brick.StopContinuousReadingSensor(Port);

        /// <summary>
        /// Switches a sensor on.
        /// </summary>
        /// <remarks>In case of a motor, this will switch the motor on full speed.</remarks>
        public void SwitchOn() => Brick.SwitchSensorOn(Port);

        /// <summary>
        /// Switches a sensor off.
        /// </summary>
        /// <remarks>In case of a motor, this will switch off the motor.</remarks>
        public void SwitchOff(SensorPort port) => Brick?.SwitchSensorOff(Port);

        /// <summary>
        /// Writes directly to the sensor. The bytes to the current port, the first one or two bytes being header bytes. The message is padded if
        /// necessary, and length and checksum fields are automatically populated.
        /// </summary>
        /// <param name="data">The buffer to send.</param>
        /// <param name="singleHeader">True for single header byte.</param>
        public void WriteBytes(ReadOnlySpan<byte> data, bool singleHeader) => Brick.WriteBytesToSensor(Port, data, singleHeader);

        internal bool SetupModeAndRead(int mode, ref bool trigger, bool once = true)
        {
            trigger = false;
            DateTime dt = DateTime.Now.AddSeconds(TimeoutMeasuresSeconds);
            Brick.SelectModeAndRead(Port, mode, once);

            while (!trigger && (dt > DateTime.Now))
            {
                Thread.Sleep(10);
            }

            return trigger;
        }
    }
}
