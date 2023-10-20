// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Camera;
using Iot.Device.Camera.Settings;
using Iot.Device.Common;

namespace Camera.Samples;

internal class Program
{
    private const string CmdList = "list";
    private const string CmdStillLegacy = "still-legacy";
    private const string CmdVideoLegacy = "video-legacy";
    private const string CmdLapseLegacy = "lapse-legacy";
    private const string CmdStillLibcamera = "still-libcamera";
    private const string CmdVideoLibcamera = "video-libcamera";
    private const string CmdLapseLibcamera = "lapse-libcamera";

    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args.Length > 1)
        {
            return Usage();
        }

        var arg = args[0];
        ProcessSettings? processSettings = arg switch
        {
            CmdList => ProcessSettingsFactory.CreateForLibcamerastillAndStderr(),
            CmdStillLegacy => ProcessSettingsFactory.CreateForRaspistill(),
            CmdVideoLegacy => ProcessSettingsFactory.CreateForRaspivid(),
            CmdLapseLegacy => ProcessSettingsFactory.CreateForRaspistill(),
            CmdStillLibcamera => ProcessSettingsFactory.CreateForLibcamerastill(),
            CmdVideoLibcamera => ProcessSettingsFactory.CreateForLibcameravid(),
            CmdLapseLibcamera => ProcessSettingsFactory.CreateForLibcamerastill(),
            _ => null,
        };

        if (processSettings == null)
        {
            return Usage();
        }

        var capture = new Capture(processSettings);
        if (arg == CmdList)
        {
            var cams = await capture.List();
            Console.WriteLine("List of available cameras:");
            foreach (var cam in cams)
            {
                Console.WriteLine(cam);
            }

            return 0;
        }

        if (arg == CmdStillLegacy || arg == CmdStillLibcamera)
        {
            var filename = await capture.CaptureStill();
            Console.WriteLine($"Captured the picture: {filename}");

            return 0;
        }

        if (arg == CmdVideoLegacy || arg == CmdVideoLibcamera)
        {
            var filename = await capture.CaptureVideo();
            Console.WriteLine($"Captured the video: {filename}");

            return 0;
        }

        if (arg == CmdLapseLegacy || arg == CmdLapseLibcamera)
        {
            await capture.CaptureTimelapse();
            Console.WriteLine($"The time-lapse images have been saved to disk");

            return 0;
        }

        return Usage();
    }

    private static int Usage()
    {
        Console.WriteLine($"Camera.Samples supports one the following arguments:");
        Console.WriteLine($"{CmdList}             print the cameras available on (libcamera stack only)");
        Console.WriteLine($"{CmdStillLegacy}     capture a still using raspistill");
        Console.WriteLine($"{CmdVideoLegacy}     capture a 10s video using raspivid");
        Console.WriteLine($"{CmdLapseLegacy}     capture a time lapse (5 images for 10s) using raspistill");
        Console.WriteLine($"{CmdStillLibcamera}  capture a still using libcamera-still");
        Console.WriteLine($"{CmdVideoLibcamera}  capture a 10s video using libcamera-vid");
        Console.WriteLine($"{CmdLapseLibcamera}  capture a time lapse (5 images for 10s) using libcamera-still");
        return -1;
    }
}
