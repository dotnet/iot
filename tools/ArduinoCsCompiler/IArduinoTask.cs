// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace ArduinoCsCompiler
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
