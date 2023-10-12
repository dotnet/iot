// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// Settings for all the Libcamera-apps
/// https://www.raspberrypi.com/documentation/computers/camera_software.html#common-command-line-options
/// </summary>
public class LibcameraAppsSettings
{
    /// <summary>
    /// The default options for the Libcamera-apps
    /// </summary>
    public static IReadOnlyCollection<CommandOption> DefaultOptions { get; } =
        new List<CommandOption>()
        {
            /* Common Command Line Options */

            new CommandOption(CommandCategory.Common,
                "--help", "Print help information for the application"),

            new CommandOption(CommandCategory.Common,
                "--version", "Print out a software version number"),

            new CommandOption(CommandCategory.Common,
                "--list-cameras", "List the cameras available for use", CommandInputType.Void, CommandOutputType.IndexOfCamera),

            new CommandOption(CommandCategory.Common,
                "--config", "Read options from the given file <filename>", CommandInputType.Filename),

            new CommandOption(CommandCategory.Common,
                "--timeout", "Delay before application stops automatically <milliseconds>", CommandInputType.Milliseconds),

            /* Preview Command Line Options */

            new CommandOption(CommandCategory.Preview,
                "--preview", "Preview window settings <x,y,w,h>", CommandInputType.Xywh),

            new CommandOption(CommandCategory.Preview,
                "--fullscreen", "Fullscreen preview mode",
                CommandInputType.Void),

            new CommandOption(CommandCategory.Preview,
                "--qt-preview", "Use Qt-based preview window",
                CommandInputType.Void),

            new CommandOption(CommandCategory.Preview,
                "--nopreview", "Do not display a preview window",
                CommandInputType.Void),

            new CommandOption(CommandCategory.Preview,
                "--info-text", "Set window title bar text <string>",
                CommandInputType.FormattableString),

            /* Camera Resolution And Readout */

            new CommandOption(CommandCategory.CameraResolution,
                "--width", "Capture image width <width>",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution,
                "--height", "Capture image height <height>",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution,
                "--viewfinder-width", "Capture image width <width> (preview only)",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution,
                "--viewfinder-height", "Capture image height <height> (preview only)",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution,
                "--rawfull", "Force sensor to capture in full resolution mode",
                CommandInputType.Void),

            new CommandOption(CommandCategory.CameraResolution,
                "--mode", "Specify sensor mode, given as <width>:<height>:<bit-depth>:<packing>",
                CommandInputType.SensorMode),

            new CommandOption(CommandCategory.CameraResolution,
                "--viewfinder-mode", "Specify sensor mode, given as <width>:<height>:<bit-depth>:<packing> (preview only)",
                CommandInputType.SensorMode),

            new CommandOption(CommandCategory.CameraResolution,
                "--lores-width", "Low resolution image width <width>",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution,
                "--lores-height", "Low resolution image height <height>",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution,
                "--hflip", "Read out with horizontal mirror",
                CommandInputType.Void),

            new CommandOption(CommandCategory.CameraResolution,
                "--vflip", "Read out with vertical flip",
                CommandInputType.Void),

            new CommandOption(CommandCategory.CameraResolution,
                "--rotation", "Use hflip and vflip to create the given rotation <angle>",
                CommandInputType.Rotate0Or180),

            new CommandOption(CommandCategory.CameraResolution,
                "--roi", "Select a crop (region of interest) from the camera <x,y,w,h>",
                CommandInputType.Xywh),

            new CommandOption(CommandCategory.CameraResolution,
                "--hdr", "Run the camera in HDR mode (supported cameras only)",
                CommandInputType.Xywh),

            /* Camera Control */

            new CommandOption(CommandCategory.CameraControl,
                "--sharpness", "Set image sharpness <number> (>=0.0)",
                CommandInputType.Decimal),

            new CommandOption(CommandCategory.CameraControl,
                "--brightness", "Set image brightness <number> (-1.0 to +1.0)",
                CommandInputType.Decimal),

            new CommandOption(CommandCategory.CameraControl,
                "--saturation", "Set image colour saturation <number> (>=0.0)",
                CommandInputType.Decimal),

            new CommandOption(CommandCategory.CameraControl,
                "--ev", "Set EV compensation <number> (-10.0 to 10.0)",
                CommandInputType.Decimal),

            new CommandOption(CommandCategory.CameraControl,
                "--shutter", "Set the exposure time in microseconds <number>",
                CommandInputType.Microseconds),

            new CommandOption(CommandCategory.CameraControl,
                "--gain", "Sets the combined analogue and digital gains <number>",
                CommandInputType.Decimal),

            new CommandOption(CommandCategory.CameraControl,
                "--metering", "Set the metering mode <string>",
                CommandInputType.Metering),

            new CommandOption(CommandCategory.CameraControl,
                "--exposure", "Set the exposure profile <string>",
                CommandInputType.Exposure),

            new CommandOption(CommandCategory.CameraControl,
                "--awb", "Set the AWB mode <string>",
                CommandInputType.WhiteBalance),

            new CommandOption(CommandCategory.CameraControl,
                "--awbgains", "Set fixed colour gains <number,number>",
                CommandInputType.DecimalPair),

            new CommandOption(CommandCategory.CameraControl,
                "--denoise", "Set the denoising mode <string>",
                CommandInputType.Denoise),

            new CommandOption(CommandCategory.CameraControl,
                "--tuning-file", "Specify the camera tuning to use <string>",
                CommandInputType.Filename),

            new CommandOption(CommandCategory.CameraControl,
                "--autofocus-mode", "Specify the autofocus mode <string>",
                CommandInputType.AutofocusMode),

            new CommandOption(CommandCategory.CameraControl,
                "--autofocus-range", "Specify the autofocus range <string>",
                CommandInputType.AutofocusRange),

            new CommandOption(CommandCategory.CameraControl,
                "--autofocus-speed", "Specify the autofocus speed <string>",
                CommandInputType.AutofocusSpeed),

            new CommandOption(CommandCategory.CameraControl,
                "--autofocus-window", "Specify the autofocus window",
                CommandInputType.Xywh),

            new CommandOption(CommandCategory.CameraControl,
                "--lens-position", "Set the lens to a given position <string>",
                CommandInputType.DecimalOrString),

            /* Output File Options */

            new CommandOption(CommandCategory.Output,
                "--output", "Output file name <string>",
                CommandInputType.Output),

            new CommandOption(CommandCategory.Output,
                "--wrap", "Wrap output file counter at <number>",
                CommandInputType.Int),

            new CommandOption(CommandCategory.Output,
                "--flush", "Flush output files immediately"),

            /* Post Processing Options */

            new CommandOption(CommandCategory.PostProcessing,
                "--post-process-file", "Post-processing json file descriptor",
                CommandInputType.Filename),

            /* Still Command Line Options */

            new CommandOption(CommandCategory.Still,
                "--quality", "JPEG quality <number> (0 - 100) default=93",
                CommandInputType.Int),

            new CommandOption(CommandCategory.Still,
                "--exif", "Add extra EXIF tags <string>",
                CommandInputType.String),

            new CommandOption(CommandCategory.Still,
                "--timelapse", "Time interval between timelapse captures <milliseconds> (use --timeout to stop)", CommandInputType.Milliseconds),

            new CommandOption(CommandCategory.Still,
                "--framestart", "The starting value for the frame counter <number>", CommandInputType.Int),

            new CommandOption(CommandCategory.Still,
                "--datetime", "Use date format for the output file names", CommandInputType.Void),

            new CommandOption(CommandCategory.Still,
                "--timestamp", "Use system timestamps for the output file names", CommandInputType.Void),

            new CommandOption(CommandCategory.Still,
                "--restart", "Set the JPEG restart interval <number> (default == 0)", CommandInputType.Int),

            new CommandOption(CommandCategory.Still,
                "--keypress", "Capture image when Enter is pressed", CommandInputType.Void),

            new CommandOption(CommandCategory.Still,
                "--signal", "Capture image when SIGUSR1 is received", CommandInputType.Void),

            new CommandOption(CommandCategory.Still,
                "--thumb", "Set thumbnail parameters <w:h:q> or none", CommandInputType.Whq),

            new CommandOption(CommandCategory.Still,
                "--encoding", "Set the still image codec <string>", CommandInputType.Encoding),

            new CommandOption(CommandCategory.Still,
                "--raw", "Save raw file", CommandInputType.Void),

            new CommandOption(CommandCategory.Still,
                "--latest", "Make symbolic link to latest file saved <string>", CommandInputType.Filename),

            new CommandOption(CommandCategory.Still,
                "--autofocus-on-capture", "Whether to run an autofocus cycle before capture", CommandInputType.Void),

            /* Video Command Line Options */

            new CommandOption(CommandCategory.Video,
                "--quality", "JPEG quality <number> (MJPG only, default == 50)", CommandInputType.Int),

            new CommandOption(CommandCategory.Video,
                "--bitrate", "H.264 bitrate <number> (bits per second)", CommandInputType.Int),

            new CommandOption(CommandCategory.Video,
                "--intra", "Intra-frame period (H.264 only) <number> (default == 60)", CommandInputType.Int),

            new CommandOption(CommandCategory.Video,
                "--profile", "H.264 profile <string>", CommandInputType.H264Profile),

            new CommandOption(CommandCategory.Video,
                "--level", "H.264 level <string>", CommandInputType.H264Level),

            new CommandOption(CommandCategory.Video,
                "--codec", "Encoder to be used <string>", CommandInputType.Codec),

            new CommandOption(CommandCategory.Video,
                "--keypress", "Toggle recording and pausing when Enter is pressed", CommandInputType.Void),

            new CommandOption(CommandCategory.Video,
                "--signal", "Toggle recording and pausing when SIGUSR1 is received", CommandInputType.Void),

            new CommandOption(CommandCategory.Video,
                "--initial", "Start the application in the recording or paused state <string>", CommandInputType.Initial),

            new CommandOption(CommandCategory.Video,
                "--split", "Split multiple recordings into separate files", CommandInputType.Void),

            new CommandOption(CommandCategory.Video,
                "--segment", "Write the video recording into multiple segments <number>ms", CommandInputType.Milliseconds),

            new CommandOption(CommandCategory.Video,
                "--circular", "Write the video recording into a circular buffer of the given <size>", CommandInputType.Megabytes),

            new CommandOption(CommandCategory.Video,
                "--inline", "Write sequence header in every I frame (H.264 only)", CommandInputType.Void),

            new CommandOption(CommandCategory.Video,
                "--listen", "Wait for an incoming TCP connection", CommandInputType.ListeningTcpUri),

            new CommandOption(CommandCategory.Video,
                "--frames", "Record exactly this many frames <number>", CommandInputType.Int),
        };
}
