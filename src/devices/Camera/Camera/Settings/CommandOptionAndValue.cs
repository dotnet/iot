// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// The association between the command option and the desired stringified value,
/// </summary>
/// <param name="Option">The command option describing the command line to be passed to the process.</param>
/// <param name="Value">The value for the command option, formatted as a string</param>
public record class CommandOptionAndValue(CommandOption Option, string Value = "")
{
    /// <summary>
    /// Create an instance of CommadnOptionAndValue
    /// </summary>
    /// <param name="command">The command to use in the command line.</param>
    /// <param name="value">The value associated to the command option.</param>
    /// <returns>An instance of this class representing the command option and the provided value.</returns>
    public static CommandOptionAndValue Create(Command command, string value = "")
        => new CommandOptionAndValue(CommandOptionsBuilder.Get(command), value);
}
