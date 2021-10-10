using System;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Represents a process currently running on the remote microcontroller
    /// </summary>
    public interface IArduinoTask : IDisposable
    {
        /// <summary>
        /// State of the process.
        /// </summary>
        MethodState State { get; }

        /// <summary>
        /// The task id
        /// </summary>
        short TaskId { get; }
    }
}
