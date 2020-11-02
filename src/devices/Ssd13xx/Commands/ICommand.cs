// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd13xx.Commands
{
    /// <summary>
    /// Interface for all Ssd13xx commands
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The value that represents the command.
        /// </summary>
        byte Id { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        byte[] GetBytes();
    }
}
