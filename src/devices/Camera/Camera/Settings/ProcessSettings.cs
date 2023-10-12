// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// The settings used to run the external process driving
/// the image or video acquisition
/// </summary>
public class ProcessSettings
{
    /// <summary>
    /// The relative or absolute executable file to run
    /// </summary>
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// The size of the buffer used to copy the stream of incoming data
    /// </summary>
    public int BufferSize { get; set; } = 81920;

    /// <summary>
    /// The working directory when running the process.
    /// If null, the Directory.GetCurrentDirectory() will be used.
    /// </summary>
    public string? WorkingDirectory { get; set; } = null;

}
