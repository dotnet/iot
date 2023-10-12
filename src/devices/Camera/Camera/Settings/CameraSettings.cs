// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// Settings for running the process that capture images and/or videos
/// </summary>
public class CameraSettings
{
    /// <summary>
    /// Settings for the external process
    /// </summary>
    public ProcessSettings ProcessSettings { get; set; } = new ProcessSettings();

}
