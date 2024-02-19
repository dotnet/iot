// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pn532.AsTarget
{
    /// <summary>
    /// Status state for the PN532 when setup as a target.
    /// </summary>
    public class TargetStatus
    {
        internal TargetStatus(byte state, byte rbit)
        {
            State = (TargetState)state;
            SpeedInitiator = (TargetBaudRateInitialized)(rbit & 0b0011_0000);
            SpeedTarget = (TargetBaudRateInitialized)((rbit >> 4) & 0b0000_0111);
        }

        /// <summary>
        /// Gets or sets the state of the target.
        /// </summary>
        public TargetState State { get; set; }

        /// <summary>
        /// Gets or sets the initiator baud rate.
        /// </summary>
        public TargetBaudRateInitialized SpeedInitiator { get; set; }

        /// <summary>
        /// Gets or sets the target baud rate.
        /// </summary>
        public TargetBaudRateInitialized SpeedTarget { get; set; }

        /// <summary>
        /// Gets the value indicating whether the target is released.
        /// </summary>
        public bool IsReleased => State == TargetState.NfcipReleased || State == TargetState.PiccReleased;

        /// <summary>
        /// Gets the value indicating whether the target is activated.
        /// </summary>
        public bool IsActivated => State == TargetState.NfcipActivated || State == TargetState.PiccActivated;

        /// <summary>
        /// Gets the value indicating whether the target is deselected.
        /// </summary>
        public bool IsDeselected => State == TargetState.NfcipDeselected || State == TargetState.PiccDeselected;
    }
}
