// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text;

using Iot.Device.Camera;
using Iot.Device.Common;

namespace TestCamera;

/// <summary>
/// Unit tests for the Camera binding
/// </summary>
public class ProcessRunnerTests
{
    private const string FakeVideocapture =
        @"FakeVideoCapture.exe";

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
    /// which is currently embedded in the test project
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
    /// Test on the binary output.
    /// </summary>
    /// <returns>The test task operation.</returns>
    [Fact]
    public async Task TestBinary1()
    {
        // The following method was used to create the "test.bin" file
        // that is embedded in the test project
        // Should be needed to create a slightly different binary file
        // the following code can be used to do that
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
    /// Test on the binary output.
    /// This is a "continuous test" that provides the ability to stop
    /// the external process prematurely.
    /// </summary>
    /// <returns>The test task operation.</returns>
    [Fact]
    public async Task TestBinary2()
    {
        // The following method was used to create the "test.bin" file
        // that is embedded in the test project
        // Should be needed to create a slightly different binary file
        // the following code can be used to do that
        /* CreateTestFile(Video1); */

        ProcessSettings settings = new()
        {
            Filename = FakeVideocapture,
            WorkingDirectory = null,
        };

        using var proc = new ProcessRunner(settings);
        using var ms = new MemoryStream();
        var runningTask = await proc.ContinuousRunAsync(Video1, ms);

        // We have two different ways to stop capturing
        // 1. wait for its termination or stop the process invoking Dispose
        await runningTask;
        /*
        // 2. programmatically stop the capture after a period of time
        // These two lines will make the capture stop after 10 seconds
        await Task.Delay(10000);
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
    /// Test on the text output.
    /// </summary>
    /// <returns>The test task operation.</returns>
    [Fact]
    public async Task TestText1()
    {
        // The following method was used to create the "test.bin" file
        // that is embedded in the test project
        // Should be needed to create a slightly different binary file
        // the following code can be used to do that
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
    /// Retrieving failure output.
    /// </summary>
    /// <returns>The test task operation.</returns>
    [Fact]
    public async Task TestText2()
    {
        // The following method was used to create the "test.bin" file
        // that is embedded in the test project
        // Should be needed to create a slightly different binary file
        // the following code can be used to do that
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
}
