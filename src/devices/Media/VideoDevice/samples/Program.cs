// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.IO;
using Iot.Device.Graphics;
using Iot.Device.Media;

VideoConnectionSettings settings = new(0, (2560, 1920), VideoPixelFormat.JPEG);
using VideoDevice device = VideoDevice.Create(settings);

// Get the supported formats of the device
foreach (VideoPixelFormat item in device.GetSupportedPixelFormats())
{
    Console.Write($"{item} ");
}

Console.WriteLine();

// Get the resolutions of the format
foreach (var resolution in device.GetPixelFormatResolutions(VideoPixelFormat.YUYV))
{
    Console.Write($"[{resolution.MinWidth}x{resolution.MinHeight}]->[{resolution.MaxWidth}x{resolution.MaxHeight}], Step [{resolution.StepWidth},{resolution.StepHeight}] ");
}

Console.WriteLine();

// Query v4l2 controls default and current value
VideoDeviceValue value = device.GetVideoDeviceValue(VideoDeviceValueType.Rotate);
Console.WriteLine($"{value.Name} Min: {value.Minimum} Max: {value.Maximum} Step: {value.Step} Default: {value.DefaultValue} Current: {value.CurrentValue}");

string path = Directory.GetCurrentDirectory();

// Take photos
device.Capture($"{path}/jpg_direct_output.jpg");

// Change capture setting
device.Settings.PixelFormat = VideoPixelFormat.YUV420;

// Convert pixel format
using var stream = new MemoryStream(device.Capture());
Color[] colors = VideoDevice.Yv12ToRgb(stream, settings.CaptureSize);
var bitmap = VideoDevice.RgbToBitmap(settings.CaptureSize, colors);
bitmap.SaveToFile($"{path}/yuyv_to_jpg.jpg", ImageFileType.Jpg);
