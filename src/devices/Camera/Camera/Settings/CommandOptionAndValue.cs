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
public record class CommandOptionAndValue(CommandOption Option, string Value = "")
{
}
