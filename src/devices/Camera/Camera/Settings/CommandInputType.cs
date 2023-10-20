// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera.Settings;

/// <summary>
/// Represents the type of value expected on the command line
/// </summary>
public enum CommandInputType
{
    /// <summary>
    /// No value is expected
    /// </summary>
    Void,

    /// <summary>
    /// The list of cameras prefixed by the index
    /// </summary>
    IndexOfCamera,

    /// <summary>
    /// A filename
    /// </summary>
    Filename,

    /// <summary>
    /// An integer specifying milliseconds
    /// </summary>
    Milliseconds,

    /// <summary>
    /// An integer specifying microseconds
    /// </summary>
    Microseconds,

    /// <summary>
    /// Four integers specifying x,y,w,h
    /// </summary>
    Xywh,

    /// <summary>
    /// Three integers specifying w,h,q (width, height, quality)
    /// </summary>
    Whq,

    /// <summary>
    /// A single string
    /// </summary>
    String,

    /// <summary>
    /// A single formattable string accepting the following directives.<para/>
    /// Directive   Substitution<para/>
    /// %frame      The sequence number of the frame<para/>
    /// %fps        The instantaneous frame rate<para/>
    /// %exp        The shutter speed used to capture the image, in microseconds<para/>
    /// %ag         The analogue gain applied to the image in the sensor<para/>
    /// %dg         The digital gain applied to the image by the ISP<para/>
    /// %rg         The gain applied to the red component of each pixel<para/>
    /// %bg         The gain applied to the blue component of each pixel<para/>
    /// %focus      The focus metric for the image, where a larger value implies a sharper image<para/>
    /// %lp         The current lens position in dioptres (1 / distance in metres).<para/>
    /// %afstate    The autofocus algorithm state (one of idle, scanning, focused or failed).<para/>
    /// </summary>
    FormattableString,

    /// <summary>
    /// An integer
    /// </summary>
    Int,

    /// <summary>
    /// An decimal number (dot is the separator)
    /// </summary>
    Decimal,

    /// <summary>
    /// A pair of decimal numbers separated by a comma
    /// </summary>
    DecimalPair,

    /// <summary>
    /// An decimal number or the 'default' string
    /// </summary>
    DecimalOrString,

    /// <summary>
    /// Sensor Mode string
    /// </summary>
    SensorMode,

    /// <summary>
    /// Rotation 0 or 180
    /// </summary>
    Rotate0Or180,

    /// <summary>
    /// One of the following strings: centre, spot, average, custom
    /// </summary>
    Metering,

    /// <summary>
    /// One of the following strings: normal, sport or long
    /// </summary>
    Exposure,

    /// <summary>
    /// One of the following strings: auto, incandescent, tungsten, fluorescent, indoor, daylight, cloudy, custom
    /// </summary>
    WhiteBalance,

    /// <summary>
    /// One of the following strings: auto, off, cdn_off, cdn_fast, cdn_hq
    /// </summary>
    Denoise,

    /// <summary>
    /// One of the following strings: default, manual, auto, continuous
    /// </summary>
    AutofocusMode,

    /// <summary>
    /// One of the following strings: normal, macro, full
    /// </summary>
    AutofocusRange,

    /// <summary>
    /// One of the following strings: normal, fast
    /// </summary>
    AutofocusSpeed,

    /// <summary>
    /// One of the following strings: '-' (stdout), 'udp://', 'tcp://' or filename (including %d == progressive number)
    /// </summary>
    Output,

    /// <summary>
    /// One of the following strings: jpg, png, bmp, rgb, yuv420
    /// </summary>
    Encoding,

    /// <summary>
    /// One of the following strings: baseline, main or high
    /// </summary>
    H264Profile,

    /// <summary>
    /// One of the following strings: 4, 4.1 or 4.2
    /// </summary>
    H264Level,

    /// <summary>
    /// One of the following strings: h264, mjpeg, yuv420, libav
    /// </summary>
    Codec,

    /// <summary>
    /// One of the following strings: record or pause
    /// </summary>
    Initial,

    /// <summary>
    /// A number expressing megabytes
    /// </summary>
    Megabytes,

    /// <summary>
    /// A listening Uri in the form tcp://0.0.0.0:8123
    /// </summary>
    ListeningTcpUri,
}
