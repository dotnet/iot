using System;

namespace Iot.Device.LidarLiteV3
{
    /// <summary>
    /// System Status
    /// </summary>
    [Flags]
    public enum SystemStatus : byte {
        /// <summary>
        /// Process Error Flag
        /// 0 - No error detected
        /// 1 - System error detected during measurement
        /// </summary>
        ProcessError = 0x40,
        /// <summary>
        /// Health Flag
        /// 0 - Error detected
        /// 1 - Reference and receiver bias are operational
        /// </summary>
        Health = 0x20,
        /// <summary>
        /// Secondary Return Flag
        /// 0 - No secondary return detected
        /// 1 - Secondary return detected in correlation record
        /// </summary>
        SecondaryReturn = 0x10,
        /// <summary>
        /// Invalid Signal Flag
        /// 0 - Peak detected
        /// 1 - Peak not detected in correlation record, measurement is invalid.
        /// </summary>
        InvalidSignal = 0x8,
        /// <summary>
        /// Signal Overflow Flag
        /// 0 - Signal data has not overflowed
        /// 1 - Signal data in correlation record has reached the maximum value before
        ///     overflow.  This occurs with a string received signal strength.
        /// </summary>
        SignalOverflow = 0x4,
        /// <summary>
        /// Reference Overflow Flag
        /// 0 - Reference data has not overflowed
        /// 1 - Reference data in correlation record has reached the maximum value before
        ///     overflow.  This occurs periodically.
        /// </summary>
        ReferenceOverflow = 0x2,
        /// <summary>
        /// Busy Flag
        /// 0 - Device is ready for new command
        /// 1 - Device is busy taking a measurement
        /// </summary>
        BusyFlag = 0x1
    }
}
