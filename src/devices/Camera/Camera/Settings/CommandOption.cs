// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// A command line option to configure the application
/// </summary>
public record class CommandOption(
    CommandCategory Category,
    string Option,
    string Help,
    CommandInputType InputType = CommandInputType.Void,
    CommandOutputType OutputType = CommandOutputType.Void)
{
}
