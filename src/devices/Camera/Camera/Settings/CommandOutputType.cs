// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// Represents the type of value expected on the command line
/// </summary>
public enum CommandOutputType
{
    /// <summary>
    /// No value is expected
    /// </summary>
    Void,

    /// <summary>
    /// The list of cameras prefixed by the index
    /// </summary>
    IndexOfCamera,
}
