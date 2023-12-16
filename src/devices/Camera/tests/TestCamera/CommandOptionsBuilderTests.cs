// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Iot.Device.Camera.Settings;

namespace TestCamera;

/// <summary>
/// Tests for the CommandOptionsBuilder class
/// </summary>
public class CommandOptionsBuilderTests
{
    /// <summary>
    /// Test the command line arguments.
    /// </summary>
    /// <returns>The test task operation.</returns>
    [Fact]
    public Task TestCommandLineArguments1()
    {
        // repeating the options will NOT repeat the arguments
        // "baseline" is the profile name as for H.264 specifications
        // "4" is the minimum level of the protocol as for the H.264 specifications
        // 15 tells the app to encode the H.264 stream with one "I" frame every 15 frames
        // The "I" frames can be decoded without looking at previous or following frames
        // Other type of frames are computed on the base of an "I" frame.
        var builder = new CommandOptionsBuilder()
            .WithContinuousStreaming()
            .WithContinuousStreaming()
            .WithH264VideoOptions("baseline", "4", 15)
            .WithResolution(640, 480);

        var args = builder.GetArguments();
        // Total are:
        // new CommandOptionsBuilder() => 1
        // .WithContinuousStreaming() => 2
        // .WithH264VideoOptions(...) => 5
        // .WithResolution(...) => 2
        Assert.Equal(10, args.Length);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Test the command line arguments.
    /// </summary>
    /// <returns>The test task operation.</returns>
    [Fact]
    public Task TestCommandLineArguments2()
    {
        var timeout = CommandOptionAndValue.Create(Command.Timeout, "5000");

        var builder = new CommandOptionsBuilder(false)
            .With(timeout);

        var args = builder.GetArguments();
        Assert.Single(args);

        return Task.CompletedTask;
    }

}
