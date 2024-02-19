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
/// Build the command line options using a fluent API.
/// The provided values are NOT validated by this builder.
/// </summary>
public class CommandOptionsBuilder
{
    private HashSet<CommandOptionAndValue> _commands = new();

    /// <summary>
    /// Gets the CommmandOption, given the matching Command field.
    /// </summary>
    /// <param name="command">The command to retrieve.</param>
    /// <returns>The command option.</returns>
    public static CommandOption Get(Command command)
        => LibcameraAppsSettings.DefaultOptions.Single(d => d.Command == command);

    /// <summary>
    /// Gets the CommmandOption, given the matching CommandCategory and Command fields.
    /// </summary>
    /// <param name="category">The category of the command to retrieve.</param>
    /// <param name="command">The command to retrieve.</param>
    /// <returns>An instance of the CommandOption.</returns>
    public static CommandOption GetByCategory(CommandCategory category, Command command)
        => LibcameraAppsSettings.DefaultOptions.Single(d => d.Category == category && d.Command == command);

    /// <summary>
    /// Allow to easily build the command line options needed to capture pictures or videos.
    /// </summary>
    /// <param name="includeOutputToStdio">True to redirect the stdio output to the given stream.</param>
    public CommandOptionsBuilder(bool includeOutputToStdio = true)
    {
        if (includeOutputToStdio)
        {
            // This is needed to output the binary to stdout
            AddOutput("-");
        }
    }

    /// <summary>
    /// Retrieves all the command line options and values in a string array.
    /// </summary>
    /// <returns>An array of strings with all the command options accumulated in this instance.</returns>
    public string[] GetArguments()
    {
        var args = _commands
            .Select(c => $"{c.Option.Option}{(string.IsNullOrEmpty(c.Value) ? string.Empty : " ")}{c.Value}")
            .ToArray();

        return args;
    }

    /// <summary>
    /// Adds the option specified in the argument.
    /// </summary>
    /// <param name="optionAndValue">The command option and its value to be added to the current instance.</param>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder With(CommandOptionAndValue optionAndValue)
    {
        _commands.Add(optionAndValue);
        return this;
    }

    /// <summary>
    /// Remove the specified option.
    /// </summary>
    /// <param name="command">The command to remove.</param>
    /// <returns>A reference to this instance.</returns>
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
    /// Tells the app to output the text with all the installed cameras and their characteristics.
    /// </summary>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithListCameras()
    {
        AddListCameras();
        return this;
    }

    /// <summary>
    /// Tells the app to use the camera with the specified index.
    /// The indexes are obtained from the output when WithListCameras is used.
    /// </summary>
    /// <param name="index">The index of the camera to be used.</param>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithCamera(int index)
    {
        AddCamera(index);
        return this;
    }

    /// <summary>
    /// Tells the app to output the binary content towards the specified output.
    /// </summary>
    /// <param name="output">A valid option, URL or filename.</param>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithOutput(string output)
    {
        AddOutput(output);
        return this;
    }

    /// <summary>
    /// Adds the options to capture the stream for the given amount of milliseconds.
    /// The value 0 (default) will capture indefinitely until the process gets stopped.
    /// This option makes only sense for videos.
    /// </summary>
    /// <param name="ms">The length of the capture operation in milliseconds.</param>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithContinuousStreaming(int ms = 0)
    {
        AddTimeout(ms);
        AddFlush();
        return this;
    }

    /// <summary>
    /// Sets the timeout option to 1ms which is the minimum delay to take a still picture.
    /// When capturing pictures, the value '0' will continue to capture indefinitely which is rarely desired.
    /// Instead, to capture a single still picture immediately, use the value '1' (1ms delay).
    /// </summary>
    /// <param name="ms">The time after which a still picture is captured, in milliseconds.</param>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithTimeout(int ms = 1)
    {
        AddTimeout(ms);
        return this;
    }

    /// <summary>
    /// This option is only valid on still pictures.
    /// It captures a new image every interval (specified in milliseconds).
    /// The format of the filename should use the counter. For example: "image%d.jpg".
    /// </summary>
    /// <param name="ms">The interval in milliseconds.</param>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithTimelapse(int ms)
    {
        AddTimelapse(ms);
        return this;
    }

    /// <summary>
    /// Adds the option to mirror horizontally.
    /// </summary>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithHflip()
    {
        AddHflip();
        return this;
    }

    /// <summary>
    /// Adds the option to mirror vertically.
    /// </summary>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithVflip()
    {
        AddVflip();
        return this;
    }

    /// <summary>
    /// Adds the resolution options.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The heighy of the image.</param>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithResolution(int width, int height)
    {
        AddWidthAndHeight(width, height);
        return this;
    }

    /// <summary>
    /// Adds the image quality options.
    /// </summary>
    /// <param name="sharpness">The sharpness value (must be positive, typically no more than 2.0).</param>
    /// <param name="contrast">The contrast value (must be zero or positive, typically no more than 2.0).</param>
    /// <param name="brightness">The brightness value (must be between -1.0=black and 1.0=white).</param>
    /// <param name="saturation">The saturation value (0 is gray, 1.0 is the default, larger values are saturated).</param>
    /// <returns>A reference to this instance.</returns>
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
    /// Adds the option to take a still picture.
    /// </summary>
    /// <param name="quality">0 to 100, default is 93.</param>
    /// <param name="encoding">One of the following strings: jpg, png, bmp, rgb, yuv420.</param>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithPictureOptions(int quality = 93, string encoding = "jpg")
    {
        AddPictureQuality(quality);
        AddEncoding(encoding);
        return this;
    }

    /// <summary>
    /// Adds the option to capture a video stream in H.264 format.
    /// This method will automatically set H264 and the 'inline' option that writes the H264 header to
    /// every Intra frame. The frequency of Intra frames can be changed.
    /// </summary>
    /// <param name="profile">The H264 profile used by the hardware encoder: baseline, main or high.</param>
    /// <param name="level">The level of the H264 protocol: 4, 4.1 or 4.2.</param>
    /// <param name="intra">The frequency of I (Intra) frames (number of frames).</param>
    /// <returns>A reference to this instance.</returns>
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
    /// Adds the option to capture a video stream in MJPEG format.
    /// </summary>
    /// <param name="quality">The quality of each MJPEG picture: 100 is maximum quality and 50 is the default.</param>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithMJPEGVideoOptions(int quality)
    {
        AddCodec("mjpeg");
        AddMJPEGQuality(quality);
        return this;
    }

    /// <summary>
    /// Adds the option to capture a video stream at a certain Framerate.
    /// </summary>
    /// <param name="rate">The number of frames per second, if not specified uses the camera default</param>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithVideoFramerate(int rate)
    {
        AddFramerate(rate);
        return this;
    }

    /// <summary>
    /// Adds the option to load a tuning file for the camera
    /// </summary>
    /// <param name="path"> The path to the tuning file.</param>
    /// <returns>A reference to this instance.</returns>
    public CommandOptionsBuilder WithTuningFile(string path)
    {
        AddTuningFile(path);
        return this;
    }

    private void AddFramerate(int rate)
    {
        var cmd = GetByCategory(CommandCategory.Video, Command.Framerate);
        _commands.Add(new CommandOptionAndValue(cmd, rate.ToString()));
    }

    private void AddTuningFile(string path)
    {
        var cmd = Get(Command.TuningFile);
        _commands.Add(new CommandOptionAndValue(cmd, path));
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
    /// This option has a different meaning for still pictures and videos.
    /// When capturing videos, the value '0' means to capture forever.
    /// When capturing pictures, the value '0' will continue to capture indefinitely which is rarely desired.
    /// Instead, to capture a single still picture immediately, use the value '1' (1ms delay).
    /// </summary>
    /// <param name="ms">When a still picture is captured, it represents the time, in milliseconds, after which the still picture is saved.
    /// When a video is captured, it represents the time after which the capture will start.</param>
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
