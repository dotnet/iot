// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// The list of all the commands supported by the LibcameraAppsSettings
/// </summary>
public enum Command
{
    /// <summary>
    /// The command for the --help option
    /// </summary>
    Help,

    /// <summary>
    /// The command for the --version option
    /// </summary>
    Version,

    /// <summary>
    /// The command for the --list-cameras option
    /// </summary>
    ListCameras,

    /// <summary>
    /// The command for the --camera option
    /// </summary>
    Camera,

    /// <summary>
    /// The command for the --config option
    /// </summary>
    Config,

    /// <summary>
    /// The command for the --timeout option
    /// </summary>
    Timeout,

    /// <summary>
    /// The command for the --preview option
    /// </summary>
    Preview,

    /// <summary>
    /// The command for the --fullscreen option
    /// </summary>
    Fullscreen,

    /// <summary>
    /// The command for the --qt-preview option
    /// </summary>
    QtPreview,

    /// <summary>
    /// The command for the --nopreview option
    /// </summary>
    Nopreview,

    /// <summary>
    /// The command for the --info-text option
    /// </summary>
    InfoText,

    /// <summary>
    /// The command for the --width option
    /// </summary>
    Width,

    /// <summary>
    /// The command for the --height option
    /// </summary>
    Height,

    /// <summary>
    /// The command for the --viewfinder-width option
    /// </summary>
    ViewfinderWidth,

    /// <summary>
    /// The command for the --viewfinder-height option
    /// </summary>
    ViewfinderHeight,

    /// <summary>
    /// The command for the --rawfull option
    /// </summary>
    Rawfull,

    /// <summary>
    /// The command for the --mode option
    /// </summary>
    Mode,

    /// <summary>
    /// The command for the --viewfinder-mode option
    /// </summary>
    ViewfinderMode,

    /// <summary>
    /// The command for the --lores-width option
    /// </summary>
    LoresWidth,

    /// <summary>
    /// The command for the --lores-height option
    /// </summary>
    LoresHeight,

    /// <summary>
    /// The command for the --hflip option
    /// </summary>
    Hflip,

    /// <summary>
    /// The command for the --vflip option
    /// </summary>
    Vflip,

    /// <summary>
    /// The command for the --rotation option
    /// </summary>
    Rotation,

    /// <summary>
    /// The command for the --roi option
    /// </summary>
    Roi,

    /// <summary>
    /// The command for the --hdr option
    /// </summary>
    Hdr,

    /// <summary>
    /// The command for the --sharpness option
    /// </summary>
    Sharpness,

    /// <summary>
    /// The command for the --contrast option
    /// </summary>
    Contrast,

    /// <summary>
    /// The command for the --brightness option
    /// </summary>
    Brightness,

    /// <summary>
    /// The command for the --saturation option
    /// </summary>
    Saturation,

    /// <summary>
    /// The command for the --ev option
    /// </summary>
    Ev,

    /// <summary>
    /// The command for the --shutter option
    /// </summary>
    Shutter,

    /// <summary>
    /// The command for the --gain option
    /// </summary>
    Gain,

    /// <summary>
    /// The command for the --metering option
    /// </summary>
    Metering,

    /// <summary>
    /// The command for the --exposure option
    /// </summary>
    Exposure,

    /// <summary>
    /// The command for the --awb option
    /// </summary>
    Awb,

    /// <summary>
    /// The command for the --awbgains option
    /// </summary>
    Awbgains,

    /// <summary>
    /// The command for the --denoise option
    /// </summary>
    Denoise,

    /// <summary>
    /// The command for the --tuning-file option
    /// </summary>
    TuningFile,

    /// <summary>
    /// The command for the --autofocus-mode option
    /// </summary>
    AutofocusMode,

    /// <summary>
    /// The command for the --autofocus-range option
    /// </summary>
    AutofocusRange,

    /// <summary>
    /// The command for the --autofocus-speed option
    /// </summary>
    AutofocusSpeed,

    /// <summary>
    /// The command for the --autofocus-window option
    /// </summary>
    AutofocusWindow,

    /// <summary>
    /// The command for the --lens-position option
    /// </summary>
    LensPosition,

    /// <summary>
    /// The command for the --output option
    /// </summary>
    Output,

    /// <summary>
    /// The command for the --wrap option
    /// </summary>
    Wrap,

    /// <summary>
    /// The command for the --flush option
    /// </summary>
    Flush,

    /// <summary>
    /// The command for the --post-process-file option
    /// </summary>
    PostProcessFile,

    /// <summary>
    /// The command for the --quality option
    /// </summary>
    Quality,

    /// <summary>
    /// The command for the --exif option
    /// </summary>
    Exif,

    /// <summary>
    /// The command for the --timelapse option
    /// </summary>
    Timelapse,

    /// <summary>
    /// The command for the --framestart option
    /// </summary>
    Framestart,

    /// <summary>
    /// The command for the --datetime option
    /// </summary>
    Datetime,

    /// <summary>
    /// The command for the --timestamp option
    /// </summary>
    Timestamp,

    /// <summary>
    /// The command for the --restart option
    /// </summary>
    Restart,

    /// <summary>
    /// The command for the --keypress option
    /// </summary>
    Keypress,

    /// <summary>
    /// The command for the --signal option
    /// </summary>
    Signal,

    /// <summary>
    /// The command for the --thumb option
    /// </summary>
    Thumb,

    /// <summary>
    /// The command for the --encoding option
    /// </summary>
    Encoding,

    /// <summary>
    /// The command for the --raw option
    /// </summary>
    Raw,

    /// <summary>
    /// The command for the --latest option
    /// </summary>
    Latest,

    /// <summary>
    /// The command for the --autofocus-on-capture option
    /// </summary>
    AutofocusOnCapture,

    /*
    /// <summary>
    /// The command for the --quality option
    /// </summary>
    Quality,
    */

    /// <summary>
    /// The command for the --bitrate option
    /// </summary>
    Bitrate,

    /// <summary>
    /// The command for the --intra option
    /// </summary>
    Intra,

    /// <summary>
    /// The command for the --profile option
    /// </summary>
    Profile,

    /// <summary>
    /// The command for the --level option
    /// </summary>
    Level,

    /// <summary>
    /// The command for the --codec option
    /// </summary>
    Codec,

    /*
    /// <summary>
    /// The command for the --keypress option
    /// </summary>
    Keypress,
    */

    /*
    /// <summary>
    /// The command for the --signal option
    /// </summary>
    Signal,
    */

    /// <summary>
    /// The command for the --initial option
    /// </summary>
    Initial,

    /// <summary>
    /// The command for the --split option
    /// </summary>
    Split,

    /// <summary>
    /// The command for the --segment option
    /// </summary>
    Segment,

    /// <summary>
    /// The command for the --circular option
    /// </summary>
    Circular,

    /// <summary>
    /// The command for the --inline option
    /// </summary>
    Inline,

    /// <summary>
    /// The command for the --listen option
    /// </summary>
    Listen,

    /// <summary>
    /// The command for the --frames option
    /// </summary>
    Frames,

    /// <summary>
    /// The command for the --framerate option
    /// </summary>
    Framerate,
}
