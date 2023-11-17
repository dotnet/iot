// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// A command line option to configure the application.
/// </summary>
/// <param name="Category">The category of the command option.</param>
/// <param name="Command">The command option enumeration field.</param>
/// <param name="Option">The comamand option string that will be passed to the process.</param>
/// <param name="Help">The help string for the command option.</param>
/// <param name="InputType">The type for the value associated to the command option.</param>
/// <param name="OutputType">The type for the value returned by the process when this command option is used.</param>
public record class CommandOption(
    CommandCategory Category,
    Command Command,
    string Option,
    string Help,
    CommandInputType InputType = CommandInputType.Void,
    CommandOutputType OutputType = CommandOutputType.Void);
