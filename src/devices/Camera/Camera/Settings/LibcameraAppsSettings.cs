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
    /// Gets the default options for the Libcamera-apps
    /// </summary>
    public static IReadOnlyCollection<CommandOption> DefaultOptions { get; } =
        new List<CommandOption>()
        {
            /* Common Command Line Options */

            new CommandOption(CommandCategory.Common, Command.Help,
                "--help", "Print help information for the application"),

            new CommandOption(CommandCategory.Common, Command.Version,
                "--version", "Print out a software version number"),

            new CommandOption(CommandCategory.Common, Command.ListCameras,
                "--list-cameras", "List the cameras available for use", CommandInputType.Void, CommandOutputType.IndexOfCamera),

            new CommandOption(CommandCategory.Common, Command.Config,
                "--config", "Read options from the given file <filename>", CommandInputType.Filename),

            new CommandOption(CommandCategory.Common, Command.Camera,
                "--camera", "Selects which camera to use <index>", CommandInputType.IndexOfCamera),

            new CommandOption(CommandCategory.Common, Command.Timeout,
                "--timeout", "Delay before application stops automatically <milliseconds>", CommandInputType.Milliseconds),

            /* Preview Command Line Options */

            new CommandOption(CommandCategory.Preview, Command.Preview,
                "--preview", "Preview window settings <x,y,w,h>", CommandInputType.Xywh),

            new CommandOption(CommandCategory.Preview, Command.Fullscreen,
                "--fullscreen", "Fullscreen preview mode",
                CommandInputType.Void),

            new CommandOption(CommandCategory.Preview, Command.QtPreview,
                "--qt-preview", "Use Qt-based preview window",
                CommandInputType.Void),

            new CommandOption(CommandCategory.Preview, Command.Nopreview,
                "--nopreview", "Do not display a preview window",
                CommandInputType.Void),

            new CommandOption(CommandCategory.Preview, Command.InfoText,
                "--info-text", "Set window title bar text <string>",
                CommandInputType.FormattableString),

            /* Camera Resolution And Readout */

            new CommandOption(CommandCategory.CameraResolution, Command.Width,
                "--width", "Capture image width <width>",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution, Command.Height,
                "--height", "Capture image height <height>",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution, Command.ViewfinderWidth,
                "--viewfinder-width", "Capture image width <width> (preview only)",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution, Command.ViewfinderHeight,
                "--viewfinder-height", "Capture image height <height> (preview only)",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution, Command.Rawfull,
                "--rawfull", "Force sensor to capture in full resolution mode",
                CommandInputType.Void),

            new CommandOption(CommandCategory.CameraResolution, Command.Mode,
                "--mode", "Specify sensor mode, given as <width>:<height>:<bit-depth>:<packing>",
                CommandInputType.SensorMode),

            new CommandOption(CommandCategory.CameraResolution, Command.ViewfinderMode,
                "--viewfinder-mode", "Specify sensor mode, given as <width>:<height>:<bit-depth>:<packing> (preview only)",
                CommandInputType.SensorMode),

            new CommandOption(CommandCategory.CameraResolution, Command.LoresWidth,
                "--lores-width", "Low resolution image width <width>",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution, Command.LoresHeight,
                "--lores-height", "Low resolution image height <height>",
                CommandInputType.Int),

            new CommandOption(CommandCategory.CameraResolution, Command.Hflip,
                "--hflip", "Read out with horizontal mirror",
                CommandInputType.Void),

            new CommandOption(CommandCategory.CameraResolution, Command.Vflip,
                "--vflip", "Read out with vertical flip",
                CommandInputType.Void),

            new CommandOption(CommandCategory.CameraResolution, Command.Rotation,
                "--rotation", "Use hflip and vflip to create the given rotation <angle>",
                CommandInputType.Rotate0Or180),

            new CommandOption(CommandCategory.CameraResolution, Command.Roi,
                "--roi", "Select a crop (region of interest) from the camera <x,y,w,h>",
                CommandInputType.Xywh),

            new CommandOption(CommandCategory.CameraResolution, Command.Hdr,
                "--hdr", "Run the camera in HDR mode (supported cameras only)",
                CommandInputType.Xywh),

            /* Camera Control */

            new CommandOption(CommandCategory.CameraControl, Command.Sharpness,
                "--sharpness", "Set image sharpness <number> (>=0.0)",
                CommandInputType.Decimal),

            new CommandOption(CommandCategory.CameraControl, Command.Contrast,
                "--contrast", "Set image sharpness <number> (>=0.0, default == 1.0)",
                CommandInputType.Decimal),

            new CommandOption(CommandCategory.CameraControl, Command.Brightness,
                "--brightness", "Set image brightness <number> (-1.0 to +1.0)",
                CommandInputType.Decimal),

            new CommandOption(CommandCategory.CameraControl, Command.Saturation,
                "--saturation", "Set image colour saturation <number> (>=0.0)",
                CommandInputType.Decimal),

            new CommandOption(CommandCategory.CameraControl, Command.Ev,
                "--ev", "Set EV compensation <number> (-10.0 to 10.0)",
                CommandInputType.Decimal),

            new CommandOption(CommandCategory.CameraControl, Command.Shutter,
                "--shutter", "Set the exposure time in microseconds <number>",
                CommandInputType.Microseconds),

            new CommandOption(CommandCategory.CameraControl, Command.Gain,
                "--gain", "Sets the combined analogue and digital gains <number>",
                CommandInputType.Decimal),

            new CommandOption(CommandCategory.CameraControl, Command.Metering,
                "--metering", "Set the metering mode <string>",
                CommandInputType.Metering),

            new CommandOption(CommandCategory.CameraControl, Command.Exposure,
                "--exposure", "Set the exposure profile <string>",
                CommandInputType.Exposure),

            new CommandOption(CommandCategory.CameraControl, Command.Awb,
                "--awb", "Set the AWB mode <string>",
                CommandInputType.WhiteBalance),

            new CommandOption(CommandCategory.CameraControl, Command.Awbgains,
                "--awbgains", "Set fixed colour gains <number,number>",
                CommandInputType.DecimalPair),

            new CommandOption(CommandCategory.CameraControl, Command.Denoise,
                "--denoise", "Set the denoising mode <string>",
                CommandInputType.Denoise),

            new CommandOption(CommandCategory.CameraControl, Command.TuningFile,
                "--tuning-file", "Specify the camera tuning to use <string>",
                CommandInputType.Filename),

            new CommandOption(CommandCategory.CameraControl, Command.AutofocusMode,
                "--autofocus-mode", "Specify the autofocus mode <string>",
                CommandInputType.AutofocusMode),

            new CommandOption(CommandCategory.CameraControl, Command.AutofocusRange,
                "--autofocus-range", "Specify the autofocus range <string>",
                CommandInputType.AutofocusRange),

            new CommandOption(CommandCategory.CameraControl, Command.AutofocusSpeed,
                "--autofocus-speed", "Specify the autofocus speed <string>",
                CommandInputType.AutofocusSpeed),

            new CommandOption(CommandCategory.CameraControl, Command.AutofocusWindow,
                "--autofocus-window", "Specify the autofocus window",
                CommandInputType.Xywh),

            new CommandOption(CommandCategory.CameraControl, Command.LensPosition,
                "--lens-position", "Set the lens to a given position <string>",
                CommandInputType.DecimalOrString),

            /* Output File Options */

            new CommandOption(CommandCategory.Output, Command.Output,
                "--output", "Output file name <string>",
                CommandInputType.Output),

            new CommandOption(CommandCategory.Output, Command.Wrap,
                "--wrap", "Wrap output file counter at <number>",
                CommandInputType.Int),

            new CommandOption(CommandCategory.Output, Command.Flush,
                "--flush", "Flush output files immediately"),

            /* Post Processing Options */

            new CommandOption(CommandCategory.PostProcessing, Command.PostProcessFile,
                "--post-process-file", "Post-processing json file descriptor",
                CommandInputType.Filename),

            /* Still Command Line Options */

            new CommandOption(CommandCategory.Still, Command.Quality,
                "--quality", "JPEG quality <number> (0 - 100) default=93",
                CommandInputType.Int),

            new CommandOption(CommandCategory.Still, Command.Exif,
                "--exif", "Add extra EXIF tags <string>",
                CommandInputType.String),

            new CommandOption(CommandCategory.Still, Command.Timelapse,
                "--timelapse", "Time interval between timelapse captures <milliseconds> (use --timeout to stop)", CommandInputType.Milliseconds),

            new CommandOption(CommandCategory.Still, Command.Framestart,
                "--framestart", "The starting value for the frame counter <number>", CommandInputType.Int),

            new CommandOption(CommandCategory.Still, Command.Datetime,
                "--datetime", "Use date format for the output file names", CommandInputType.Void),

            new CommandOption(CommandCategory.Still, Command.Timestamp,
                "--timestamp", "Use system timestamps for the output file names", CommandInputType.Void),

            new CommandOption(CommandCategory.Still, Command.Restart,
                "--restart", "Set the JPEG restart interval <number> (default == 0)", CommandInputType.Int),

            new CommandOption(CommandCategory.Still, Command.Keypress,
                "--keypress", "Capture image when Enter is pressed", CommandInputType.Void),

            new CommandOption(CommandCategory.Still, Command.Signal,
                "--signal", "Capture image when SIGUSR1 is received", CommandInputType.Void),

            new CommandOption(CommandCategory.Still, Command.Thumb,
                "--thumb", "Set thumbnail parameters <w:h:q> or none", CommandInputType.Whq),

            new CommandOption(CommandCategory.Still, Command.Encoding,
                "--encoding", "Set the still image codec <string>", CommandInputType.Encoding),

            new CommandOption(CommandCategory.Still, Command.Raw,
                "--raw", "Save raw file", CommandInputType.Void),

            new CommandOption(CommandCategory.Still, Command.Latest,
                "--latest", "Make symbolic link to latest file saved <string>", CommandInputType.Filename),

            new CommandOption(CommandCategory.Still, Command.AutofocusOnCapture,
                "--autofocus-on-capture", "Whether to run an autofocus cycle before capture", CommandInputType.Void),

            /* Video Command Line Options */

            new CommandOption(CommandCategory.Video, Command.Quality,
                "--quality", "JPEG quality <number> (MJPG only, default == 50)", CommandInputType.Int),

            new CommandOption(CommandCategory.Video, Command.Bitrate,
                "--bitrate", "H.264 bitrate <number> (bits per second)", CommandInputType.Int),

            new CommandOption(CommandCategory.Video, Command.Intra,
                "--intra", "Intra-frame period (H.264 only) <number> (default == 60)", CommandInputType.Int),

            new CommandOption(CommandCategory.Video, Command.Profile,
                "--profile", "H.264 profile <string>", CommandInputType.H264Profile),

            new CommandOption(CommandCategory.Video, Command.Level,
                "--level", "H.264 level <string>", CommandInputType.H264Level),

            new CommandOption(CommandCategory.Video, Command.Codec,
                "--codec", "Encoder to be used <string>", CommandInputType.Codec),

            new CommandOption(CommandCategory.Video, Command.Keypress,
                "--keypress", "Toggle recording and pausing when Enter is pressed", CommandInputType.Void),

            new CommandOption(CommandCategory.Video, Command.Signal,
                "--signal", "Toggle recording and pausing when SIGUSR1 is received", CommandInputType.Void),

            new CommandOption(CommandCategory.Video, Command.Initial,
                "--initial", "Start the application in the recording or paused state <string>", CommandInputType.Initial),

            new CommandOption(CommandCategory.Video, Command.Split,
                "--split", "Split multiple recordings into separate files", CommandInputType.Void),

            new CommandOption(CommandCategory.Video, Command.Segment,
                "--segment", "Write the video recording into multiple segments <number>ms", CommandInputType.Milliseconds),

            new CommandOption(CommandCategory.Video, Command.Circular,
                "--circular", "Write the video recording into a circular buffer of the given <size>", CommandInputType.Megabytes),

            new CommandOption(CommandCategory.Video, Command.Inline,
                "--inline", "Write sequence header in every I frame (H.264 only)", CommandInputType.Void),

            new CommandOption(CommandCategory.Video, Command.Listen,
                "--listen", "Wait for an incoming TCP connection", CommandInputType.ListeningTcpUri),

            new CommandOption(CommandCategory.Video, Command.Frames,
                "--frames", "Record exactly this many frames <number>", CommandInputType.Int),

            new CommandOption(CommandCategory.Video, Command.Framerate,
                "--framerate", "Set the frames per second captured <number>", CommandInputType.Int),

        };
}
