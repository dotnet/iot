// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Possible results of sending a Firmata command
    /// </summary>
    public enum CommandError
    {
        /// <summary>
        /// No error
        /// </summary>
        None = 0,

        /// <summary>
        /// The remote side is busy. The command was ignored.
        /// </summary>
        EngineBusy = 1,

        /// <summary>
        /// The arguments provided were invalid
        /// </summary>
        InvalidArguments = 2,

        /// <summary>
        /// The remote side is out of memory
        /// </summary>
        OutOfMemory = 3,

        /// <summary>
        /// There was an internal error processing the request.
        /// </summary>
        InternalError = 4,

        /// <summary>
        /// The board timed out executing the command.
        /// </summary>
        Timeout = 5,

        /// <summary>
        /// The device has been reset.
        /// </summary>
        DeviceReset = 6,

        /// <summary>
        /// The command was aborted.
        /// </summary>
        Aborted = 7,
    }
}
