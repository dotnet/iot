// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// Build the command line options using a fluent API
/// </summary>
public class CommandOptionsBuilder
{
    private HashSet<CommandOptionAndValue> _commands = new();

    /// <summary>
    /// Adds the option specified in the argument
    /// </summary>
    public CommandOptionsBuilder With(CommandOptionAndValue optionAndValue)
    {
        _commands.Add(optionAndValue);
        return this;
    }

    /// <summary>
    /// Adds the options to stream indefinitely until the process gets stopped
    /// </summary>
    /// <returns></returns>
    public CommandOptionsBuilder WithContinuousStreaming()
    {
        AddContinuous();
        AddOutput("-");
        AddFlush();
        return this;
    }

    /// <summary>
    /// Adds the option to mirror horizontally
    /// </summary>
    public CommandOptionsBuilder WithHflip()
    {
        AddHflip();
        return this;
    }

    /// <summary>
    /// Adds the resolution options
    /// </summary>
    public CommandOptionsBuilder WithResolution(int width, int height)
    {
        AddWidthAndHeight(width, height);
        return this;
    }

    /// <summary>
    /// Adds the image quality options
    /// </summary>
    /// <param name="sharpness">The sharpness value (must be positive, typically no more than 2.0)</param>
    /// <param name="contrast">The contrast value (must be zero or positive, typically no more than 2.0)</param>
    /// <param name="brightness">The brightness value (must be between -1.0=black and 1.0=white)</param>
    /// <param name="saturation">The saturation value (0 is gray, 1.0 is the default, larger values are saturated)</param>
    /// <returns></returns>
    public CommandOptionsBuilder WithImageQuality(decimal sharpness = 1.0m, decimal contrast = 1.0m,
        decimal brightness = 0.0m, decimal saturation = 1.0m)
    {
        AddSharpness(sharpness);
        AddContrast(contrast);
        AddBrightness(brightness);
        AddSaturation(saturation);
        return this;
    }

    /// <summary>
    /// Adds the option to take a still picture
    /// </summary>
    /// <param name="quality">0 to 100, default is 93</param>
    /// <param name="encoding">One of the following strings: jpg, png, bmp, rgb, yuv420</param>
    /// <returns></returns>
    public CommandOptionsBuilder WithPictureOptions(int quality = 93, string encoding = "jpg")
    {
        AddQuality(quality);
        AddEncoding(encoding);
        return this;
    }

    /// <summary>
    /// Adds the option to capture a video stream
    /// This method will automatically set H264 and the 'inline' option that writes the H264 header to
    /// every Intra frame. The frequency of Intra frames can be changed.
    /// </summary>
    /// <param name="profile">The H264 profile used by the hardware encoder: baseline, main or high</param>
    /// <param name="level">The level of the H264 protocol: 4, 4.1 or 4.2</param>
    /// <param name="intra">The frequency of I (Intra) frames (number of frames)</param>
    /// <returns></returns>
    public CommandOptionsBuilder WithH264VideoOptions(string profile, string level, int intra = 60)
    {
        AddCodec("h264");
        AddInline();
        AddProfile(profile);
        AddLevel(level);
        AddIntra(intra);
        return this;
    }

    private void AddContinuous()
    {
        var cmd = Get(Command.Timeout);
        _commands.Add(new CommandOptionAndValue(cmd, "0"));
    }

    private void AddOutput(string value)
    {
        var cmd = Get(Command.Output);
        _commands.Add(new CommandOptionAndValue(cmd, value));
    }

    private void AddFlush()
    {
        var cmd = Get(Command.Flush);
        _commands.Add(new CommandOptionAndValue(cmd));
    }

    private void AddHflip()
    {
        var cmd = Get(Command.Hflip);
        _commands.Add(new CommandOptionAndValue(cmd));
    }

    private void AddWidthAndHeight(int width, int height)
    {
        var cmdW = Get(Command.Width);
        _commands.Add(new CommandOptionAndValue(cmdW, width.ToString()));
        var cmdH = Get(Command.Height);
        _commands.Add(new CommandOptionAndValue(cmdH, height.ToString()));
    }

    private void AddSharpness(decimal value)
    {
        var cmd = Get(Command.Sharpness);
        _commands.Add(new CommandOptionAndValue(cmd, value.ToString(CultureInfo.InvariantCulture)));
    }

    private void AddContrast(decimal value)
    {
        var cmd = Get(Command.Contrast);
        _commands.Add(new CommandOptionAndValue(cmd, value.ToString(CultureInfo.InvariantCulture)));
    }

    private void AddBrightness(decimal value)
    {
        var cmd = Get(Command.Brightness);
        _commands.Add(new CommandOptionAndValue(cmd, value.ToString(CultureInfo.InvariantCulture)));
    }

    private void AddSaturation(decimal value)
    {
        var cmd = Get(Command.Saturation);
        _commands.Add(new CommandOptionAndValue(cmd, value.ToString(CultureInfo.InvariantCulture)));
    }

    private void AddQuality(int quality)
    {
        var cmd = Get(Command.Quality);
        _commands.Add(new CommandOptionAndValue(cmd, quality.ToString()));
    }

    private void AddEncoding(string encoding)
    {
        var cmd = Get(Command.Encoding);
        _commands.Add(new CommandOptionAndValue(cmd, encoding));
    }

    private void AddCodec(string codec)
    {
        var cmd = Get(Command.Codec);
        _commands.Add(new CommandOptionAndValue(cmd, codec));
    }

    private void AddInline()
    {
        var cmd = Get(Command.Inline);
        _commands.Add(new CommandOptionAndValue(cmd));
    }

    private void AddProfile(string profile)
    {
        var cmd = Get(Command.Profile);
        _commands.Add(new CommandOptionAndValue(cmd, profile));
    }

    private void AddIntra(int intra)
    {
        var cmd = Get(Command.Profile);
        _commands.Add(new CommandOptionAndValue(cmd, intra.ToString()));
    }

    private void AddLevel(string level)
    {
        var cmd = Get(Command.Profile);
        _commands.Add(new CommandOptionAndValue(cmd, level));
    }

    private CommandOption Get(Command command)
        => LibcameraAppsSettings.DefaultOptions.Single(d => d.Command == command);
}
