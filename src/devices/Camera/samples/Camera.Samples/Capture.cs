// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Iot.Device.Camera;
using Iot.Device.Camera.Settings;
using Iot.Device.Common;

namespace Camera.Samples;

internal class Capture
{
    private readonly ProcessSettings _processSettings;

    public Capture(ProcessSettings processSettings)
    {
        _processSettings = processSettings;
    }

    private string CreateFilename(string extension)
    {
        var now = DateTime.Now;
        return $"{now.Year:00}{now.Month:00}{now.Day:00}_{now.Hour:00}{now.Minute:00}{now.Second:00}{now.Millisecond:0000}.{extension}";
    }

    public async Task<string> CaptureStill()
    {
        var builder = new CommandOptionsBuilder()
            .WithTimeout(1)
            .WithVflip()
            .WithHflip()
            .WithPictureOptions(90, "jpg")
            .WithResolution(640, 480);
        var args = builder.GetArguments();

        using var proc = new ProcessRunner(_processSettings);
        Console.WriteLine("Using the following command line:");
        Console.WriteLine(proc.GetFullCommandLine(args));
        Console.WriteLine();

        var filename = CreateFilename("jpg");
        using var file = File.OpenWrite(filename);
        await proc.ExecuteAsync(args, file);
        return filename;
    }

    public async Task<string> CaptureVideo()
    {
        var builder = new CommandOptionsBuilder()
            .WithContinuousStreaming(0)
            .WithVflip()
            .WithHflip()
            .WithResolution(640, 480);
        var args = builder.GetArguments();

        using var proc = new ProcessRunner(_processSettings);
        Console.WriteLine("Using the following command line:");
        Console.WriteLine(proc.GetFullCommandLine(args));
        Console.WriteLine();

        var filename = CreateFilename("h264");
        using var file = File.OpenWrite(filename);

        /*
        The following code will stop capturing after 5 seconds
        We could do the same specifying ".WithContinuousStreaming(5000)" instead of 0
        But the following code shows how to stop the capture programmatically
        */

        // The ContinuousRunAsync method offload the capture on a separate thread
        var task = await proc.ContinuousRunAsync(args, file);
        await Task.Delay(5000);
        proc.Dispose();
        // The following try/catch is needed to trash the OperationCanceledException triggered by the Dispose
        try
        {
            await task;
        }
        catch (Exception)
        {
        }

        return filename;
    }

    public async Task CaptureTimelapse()
    {
        // The false argument avoids the app to output to stdio
        // Time lapse images will be directly saved on disk without
        // writing anything on the terminal
        // Alternatively, we can leave the default (true) and
        // use the '.Remove' method
        var builder = new CommandOptionsBuilder(false)
            // .Remove(CommandOptionsBuilder.Get(Command.Output))
            .WithOutput("image_%04d.jpg")
            .WithTimeout(5000)
            .WithTimelapse(1000)
            .WithVflip()
            .WithHflip()
            .WithResolution(640, 480);
        var args = builder.GetArguments();

        using var proc = new ProcessRunner(_processSettings);
        Console.WriteLine("Using the following command line:");
        Console.WriteLine(proc.GetFullCommandLine(args));
        Console.WriteLine();

        // The ContinuousRunAsync method offload the capture on a separate thread
        // the first await is tied the thread being run
        // the second await is tied to the capture
        var task = await proc.ContinuousRunAsync(args, default(Stream));
        await task;
    }

    public async Task<IEnumerable<CameraInfo>> List()
    {
        var builder = new CommandOptionsBuilder()
            .WithListCameras();
        var args = builder.GetArguments();

        using var proc = new ProcessRunner(_processSettings);
        Console.WriteLine("Using the following command line:");
        Console.WriteLine(proc.GetFullCommandLine(args));
        Console.WriteLine();

        var text = await proc.ExecuteReadOutputAsStringAsync(args);
        Console.WriteLine($"Output being parsed:");
        Console.WriteLine(text);
        Console.WriteLine();

        var cameras = await CameraInfo.From(text);
        return cameras;
    }
}
