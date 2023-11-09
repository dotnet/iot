// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Iot.Device.Camera;

namespace TestCamera;

/// <summary>
/// Tests for the CameraInfo class.
/// </summary>
public class CameraInfoTests
{
    private const string Text1 = "test.txt";

    /// <summary>
    /// Test parsing --list-cameras
    /// </summary>
    /// <returns>The test task operation.</returns>
    [Fact]
    public async Task TestCameraList()
    {
        var source = await File.ReadAllTextAsync(Text1, Encoding.UTF8);
        var cameras = await CameraInfo.From(source);

        Assert.Equal(2, cameras.Count());
        var cam0 = cameras.First();
        Assert.Equal(0, cam0.Index);
        Assert.Equal("imx219", cam0.Name);
        Assert.Equal("3280x2464", cam0.MaxResolution);
        Assert.Equal("/base/soc/i2c0mux/i2c@1/imx219@10", cam0.DevicePath);

        var cam1 = cameras.Skip(1).First();
        Assert.Equal(1, cam1.Index);
        Assert.Equal("imx477", cam1.Name);
        Assert.Equal("4056x3040", cam1.MaxResolution);
        Assert.Equal("/base/soc/i2c0mux/i2c@1/imx477@1a", cam1.DevicePath);
    }
}
