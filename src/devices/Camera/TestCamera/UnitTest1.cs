// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text;

using Iot.Device.Camera;
using Iot.Device.Camera.Settings;

namespace TestCamera;

/// <summary>
/// Unit tests for the Camera binding
/// </summary>
public class UnitTest1
{
    private const string FakeVideocapture =
        @"..\..\..\..\FakeVideoCapture\bin\Debug\net6.0\FakeVideoCapture.exe";

    private const string Video1 = "test.bin";
    private const string Text1 = "test.txt";

    private void Compare(byte[] source, byte[] target)
    {
        if (source.Length != target.Length)
        {
            throw new Exception($"Buffer sizes are different");
        }

        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] != target[i])
            {
                throw new Exception($"First difference at offset {i} of {source.Length - 1}");
            }
        }
    }

    /// <summary>
    /// This is used to create the test.bin file
    /// </summary>
    private void CreateTestFile(string filename)
    {
        var blob = new byte[2048 + 512];
        for (int i = 0; i < blob.Length; i++)
        {
            blob[i] = (byte)i;
        }

        File.WriteAllBytes(filename, blob);
    }

    /// <summary>
    /// Test 1
    /// </summary>
    [Fact]
    public async Task TestBinary1()
    {
        /* CreateTestFile(Video1); */

        ProcessSettings settings = new()
        {
            Filename = FakeVideocapture,
            WorkingDirectory = null,
        };

        using var proc = new ProcessRunner(settings);
        using var ms = new MemoryStream();
        await proc.ExecuteAsync(Video1, ms);

        ms.Seek(0, SeekOrigin.Begin);
        var blob = new byte[ms.Length];
        ms.Read(blob.AsSpan());

        var source = await File.ReadAllBytesAsync(Video1);

        Assert.Equal(source.Length, blob.Length);
        Compare(source, blob);
    }

    /// <summary>
    /// Test 2
    /// This is a "continuous test" that provides the ability to stop
    /// the external process prematurely
    /// </summary>
    [Fact]
    public async Task TestBinary2()
    {
        /* CreateTestFile(Video1); */

        ProcessSettings settings = new()
        {
            Filename = FakeVideocapture,
            WorkingDirectory = null,
        };

        using var proc = new ProcessRunner(settings);
        using var ms = new MemoryStream();
        var runningTask = await proc.ContinuousRunAsync(Video1, ms);

        // wait for its termination or stop the process invoking Dispose
        await runningTask;
        /*
        await Task.Delay(20);
        proc.Dispose();
        */

        ms.Seek(0, SeekOrigin.Begin);
        var blob = new byte[ms.Length];
        ms.Read(blob.AsSpan());

        var source = await File.ReadAllBytesAsync(Video1);

        Assert.Equal(source.Length, blob.Length);
        Compare(source, blob);
    }

    /// <summary>
    /// Test 1
    /// </summary>
    [Fact]
    public async Task TestText1()
    {
        /* CreateTestFile(Video1); */

        ProcessSettings settings = new()
        {
            Filename = FakeVideocapture,
            WorkingDirectory = null,
        };

        using var proc = new ProcessRunner(settings);
        var text = await proc.ExecuteReadOutputAsStringAsync(Text1);
        var source = await File.ReadAllTextAsync(Text1, Encoding.UTF8);

        Assert.Equal(source, text);
    }

    /// <summary>
    /// Retrieving failure output
    /// </summary>
    [Fact]
    public async Task TestText2()
    {
        /* CreateTestFile(Video1); */

        ProcessSettings settings = new()
        {
            Filename = FakeVideocapture,
            WorkingDirectory = null,
        };

        using var proc = new ProcessRunner(settings);
        var text = await proc.ExecuteReadOutputAsStringAsync(string.Empty);
        var source = "Expected a filename as agument" + Environment.NewLine;

        Assert.Equal(source, text);
    }

    /// <summary>
    /// Test parsing --list-cameras
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Test the command line arguments
    /// </summary>
    [Fact]
    public Task TestCommandLineArguments1()
    {
        // repeating the options will NOT repeat the arguments
        var builder = new CommandOptionsBuilder()
            .WithContinuousStreaming()
            .WithContinuousStreaming()
            .WithH264VideoOptions("baseline", "4", 15)
            .WithResolution(640, 480);

        var args = builder.GetArguments();
        Assert.Equal(10, args.Length);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Test the command line arguments
    /// </summary>
    [Fact]
    public Task TestCommandLineArguments2()
    {
        var timeout = CommandOptionsBuilder.Create(Command.Timeout, "5000");

        var builder = new CommandOptionsBuilder(false)
            .With(timeout);

        var args = builder.GetArguments();
        Assert.Single(args);

        return Task.CompletedTask;
    }
}
