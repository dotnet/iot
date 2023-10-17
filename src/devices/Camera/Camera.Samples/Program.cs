// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Camera.Settings;

namespace Camera.Samples;

/*

raspistill -n -t 1 -w 640 -h 480 -o still-legacy.jpg
raspivid -o video-legacy.h264

*/

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args.Length > 1)
        {
            return Usage();
        }

        var arg = args[0];
        ProcessSettings? processSettings = arg switch
        {
            "list" => ProcessSettingsFactory.CreateForLibcamerastillAndStderr(),
            "still-legacy" => ProcessSettingsFactory.CreateForRaspistill(),
            "video-legacy" => ProcessSettingsFactory.CreateForRaspivid(),
            "lapse-legacy" => ProcessSettingsFactory.CreateForRaspistill(),
            "still-libcamera" => ProcessSettingsFactory.CreateForLibcamerastill(),
            "video-libcamera" => ProcessSettingsFactory.CreateForLibcameravid(),
            "lapse-libcamera" => ProcessSettingsFactory.CreateForLibcamerastill(),
            _ => null,
        };

        if (processSettings == null)
        {
            return Usage();
        }

        var capture = new Capture(processSettings);
        if (arg == "list")
        {
            var cams = await capture.List();
            Console.WriteLine("List of available cameras:");
            foreach (var cam in cams)
            {
                Console.WriteLine(cam);
            }

            return 0;
        }

        if (arg == "still-legacy" || arg == "still-libcamera")
        {
            var filename = await capture.CaptureStill();
            Console.WriteLine($"Captured the picture: {filename}");

            return 0;
        }

        if (arg == "video-legacy" || arg == "video-libcamera")
        {
            var filename = await capture.CaptureVideo();
            Console.WriteLine($"Captured the video: {filename}");

            return 0;
        }

        if (arg == "lapse-legacy" || arg == "lapse-libcamera")
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
        Console.WriteLine($"list             print the cameras available on (libcamera stack only)");
        Console.WriteLine($"still-legacy     capture a still using raspistill");
        Console.WriteLine($"video-legacy     capture a 10s video using raspivid");
        Console.WriteLine($"lapse-legacy     capture a time lapse (5 images for 10s) using raspistill");
        Console.WriteLine($"still-libcamera  capture a still using libcamera-still");
        Console.WriteLine($"video-libcamera  capture a 10s video using libcamera-vid");
        Console.WriteLine($"lapse-libcamera  capture a time lapse (5 images for 10s) using libcamera-still");
        return -1;
    }
}
