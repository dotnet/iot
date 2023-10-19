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
/// The provided values are NOT validated by this builder
/// </summary>
public class CommandOptionsBuilder
{
    private HashSet<CommandOptionAndValue> _commands = new();

    /// <summary>
    /// Gets the CommmandOption, given the matching Command field
    /// </summary>
    public static CommandOption Get(Command command)
        => LibcameraAppsSettings.DefaultOptions.Single(d => d.Command == command);

    /// <summary>
    /// Gets the CommmandOption, given the matching CommandCategory and Command fields
    /// </summary>
    public static CommandOption GetByCategory(CommandCategory category, Command command)
        => LibcameraAppsSettings.DefaultOptions.Single(d => d.Category == category && d.Command == command);

    /// <summary>
    /// Allow to easily build the command line options needed to capture pictures or videos
    /// </summary>
    public CommandOptionsBuilder(bool includeOutputToStdio = true)
    {
        if (includeOutputToStdio)
        {
            // This is needed to output the binary to stdout
            AddOutput("-");
        }
    }

    /// <summary>
    /// Retrieves all the command line options and values in a string array
    /// </summary>
    /// <returns></returns>
    public string[] GetArguments()
    {
        var args = _commands
            .Select(c => $"{c.Option.Option}{(string.IsNullOrEmpty(c.Value) ? string.Empty : " ")}{c.Value}")
            .ToArray();

        return args;
    }

    /// <summary>
    /// Adds the option specified in the argument
    /// </summary>
    public CommandOptionsBuilder With(CommandOptionAndValue optionAndValue)
    {
        _commands.Add(optionAndValue);
        return this;
    }

    /// <summary>
    /// Remove the specified option
    /// </summary>
    /// <param name="command">The command to remove</param>
    /// <returns></returns>
    public CommandOptionsBuilder Remove(CommandOption command)
    {
        var optionsAndValues = _commands.Where(cv => cv.Option == command).ToArray();
        foreach (var cv in optionsAndValues)
        {
            _commands.Remove(cv);
        }

        return this;
    }

    /// <summary>
    /// Tells the app to output the text with all the installed cameras and their characteristics
    /// </summary>
    public CommandOptionsBuilder WithListCameras()
    {
        AddListCameras();
        return this;
    }

    /// <summary>
    /// Tells the app to use the camera with the specified index.
    /// The indexes are obtained from the output when WithListCameras is used
    /// </summary>
    public CommandOptionsBuilder WithCamera(int index)
    {
        AddCamera(index);
        return this;
    }

    /// <summary>
    /// Tells the app to output the binary content towards the specified output
    /// </summary>
    /// <param name="output">A valid option, URL or filename</param>
    /// <returns></returns>
    public CommandOptionsBuilder WithOutput(string output)
    {
        AddOutput(output);
        return this;
    }

    /// <summary>
    /// Adds the options to capture the stream for the given amount of milliseconds.
    /// The value 0 (default) will capture indefinitely until the process gets stopped
    /// This option makes only sense for videos.
    /// </summary>
    /// <returns></returns>
    public CommandOptionsBuilder WithContinuousStreaming(int ms = 0)
    {
        AddTimeout(ms);
        AddFlush();
        return this;
    }

    /// <summary>
    /// Set the timeout option to 1ms which is the minimum delay to take a still picture
    /// When capturing pictures, the value '0' will continue to capture indefinitely which is rarely desired.
    /// Instead, to capture a single still picture immediately, use the value '1' (1ms delay)
    /// </summary>
    public CommandOptionsBuilder WithTimeout(int ms = 1)
    {
        AddTimeout(ms);
        return this;
    }

    /// <summary>
    /// This option is only valid on still pictures
    /// It captures a new image every interval (specified in milliseconds)
    /// The format of the filename should use the counter. For example: "image%d.jpg"
    /// </summary>
    /// <param name="ms">The interval in milliseconds</param>
    public CommandOptionsBuilder WithTimelapse(int ms)
    {
        AddTimelapse(ms);
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
    /// Adds the option to mirror vertically
    /// </summary>
    public CommandOptionsBuilder WithVflip()
    {
        AddVflip();
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
        AddPictureQuality(quality);
        AddEncoding(encoding);
        return this;
    }

    /// <summary>
    /// Adds the option to capture a video stream in H.264 format
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

    /// <summary>
    /// Adds the option to capture a video stream in MJPEG format
    /// </summary>
    public CommandOptionsBuilder WithMJPEGVideoOptions(int quality)
    {
        AddCodec("mjpeg");
        AddMJPEGQuality(quality);
        return this;
    }

    private void AddListCameras()
    {
        var cmd = Get(Command.ListCameras);
        _commands.Add(new CommandOptionAndValue(cmd));
    }

    private void AddCamera(int index)
    {
        var cmd = Get(Command.Camera);
        _commands.Add(new CommandOptionAndValue(cmd, index.ToString()));
    }

    /// <summary>
    /// This option has a different meaning for still pictures and videos
    /// When capturing videos, the value '0' means to capture forever
    /// When capturing pictures, the value '0' will continue to capture indefinitely which is rarely desired.
    /// Instead, to capture a single still picture immediately, use the value '1' (1ms delay)
    /// </summary>
    private void AddTimeout(int ms)
    {
        var cmd = Get(Command.Timeout);
        _commands.Add(new CommandOptionAndValue(cmd, ms.ToString()));
    }

    private void AddTimelapse(int ms)
    {
        var cmd = Get(Command.Timelapse);
        _commands.Add(new CommandOptionAndValue(cmd, ms.ToString()));
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

    private void AddVflip()
    {
        var cmd = Get(Command.Vflip);
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

    private void AddPictureQuality(int quality)
    {
        var cmd = GetByCategory(CommandCategory.Still, Command.Quality);
        _commands.Add(new CommandOptionAndValue(cmd, quality.ToString()));
    }

    private void AddMJPEGQuality(int quality)
    {
        var cmd = GetByCategory(CommandCategory.Video, Command.Quality);
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
}
