// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// Interface that all active elements implement
    /// </summary>
    public class ActiveSensor : Sensor
    {
        // This field is populated by the birck
        internal List<ModeDetail> ModeDetailsInternal;
        internal List<CombiModes> CombiModesInternal;

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
        public ActiveSensor(Brick brick, SensorPort port, SensorType type)
            : base(brick, port, type)
        {
            ModeDetailsInternal = new List<ModeDetail>();
            CombiModesInternal = new List<CombiModes>();
        }

        /// <summary>
        /// Returned the name of the selectd mode
        /// </summary>
        /// <param name="modes">The mode to select</param>
        public void SelectedModes(int[] modes)
        {
            Brick.SelectModes(Port, modes);
        }
    }
}
