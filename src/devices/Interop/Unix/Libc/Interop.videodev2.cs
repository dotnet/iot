// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1307 // Field should begin with upper-case letter

internal enum VideoSettings : int
{
    VIDIOC_QUERYCAP = -2140645888,
    VIDIOC_ENUM_FMT = -1069525502,
    VIDIOC_CROPCAP = -1070836166,
    VIDIOC_G_CROP = -1072409029,
    VIDIOC_S_CROP = 1075074620,
    VIDIOC_G_FMT = -1060350460,
    VIDIOC_S_FMT = -1060350459,
    VIDIOC_REQBUFS = -1072409080,
    VIDIOC_QUERYBUF = -1069263351,
    VIDIOC_STREAMON = 1074026002,
    VIDIOC_STREAMOFF = 1074026003,
    VIDIOC_QBUF = -1069263345,
    VIDIOC_DQBUF = -1069263343,
    VIDIOC_ENUM_FRAMESIZES = -1070836150,
    VIDIOC_G_CTRL = -1073195493,
    VIDIOC_S_CTRL = -1073195492,
    VIDIOC_QUERYCTRL = -1069263324,
}

/// <summary>
/// videodev2.h Request Definition
/// </summary>
internal class RawVideoSettings
{
    public static int VIDIOC_QUERYCAP = Interop._IOR('V', 0, typeof(v4l2_capability));
    public static int VIDIOC_ENUM_FMT = Interop._IOWR('V', 2, typeof(v4l2_fmtdesc));
    public static int VIDIOC_G_FMT = Interop._IOWR('V', 4, typeof(v4l2_format));
    public static int VIDIOC_S_FMT = Interop._IOWR('V', 5, typeof(v4l2_format));
    public static int VIDIOC_REQBUFS = Interop._IOWR('V', 8, typeof(v4l2_requestbuffers));
    public static int VIDIOC_QUERYBUF = Interop._IOWR('V', 9, typeof(v4l2_buffer));
    public static int VIDIOC_OVERLAY = Interop._IOW('V', 14, typeof(int));
    public static int VIDIOC_QBUF = Interop._IOWR('V', 15, typeof(v4l2_buffer));
    public static int VIDIOC_DQBUF = Interop._IOWR('V', 17, typeof(v4l2_buffer));
    public static int VIDIOC_STREAMON = Interop._IOW('V', 18, typeof(int));
    public static int VIDIOC_STREAMOFF = Interop._IOW('V', 19, typeof(int));
    public static int VIDIOC_G_CTRL = Interop._IOWR('V', 27, typeof(v4l2_control));
    public static int VIDIOC_S_CTRL = Interop._IOWR('V', 28, typeof(v4l2_control));
    public static int VIDIOC_QUERYCTRL = Interop._IOWR('V', 36, typeof(v4l2_queryctrl));
    public static int VIDIOC_G_INPUT = Interop._IOR('V', 38, typeof(int));
    public static int VIDIOC_S_INPUT = Interop._IOWR('V', 39, typeof(int));
    public static int VIDIOC_G_OUTPUT = Interop._IOR('V', 46, typeof(int));
    public static int VIDIOC_S_OUTPUT = Interop._IOWR('V', 47, typeof(int));
    public static int VIDIOC_CROPCAP = Interop._IOWR('V', 58, typeof(v4l2_cropcap));
    public static int VIDIOC_G_CROP = Interop._IOWR('V', 59, typeof(v4l2_crop));
    public static int VIDIOC_S_CROP = Interop._IOW('V', 60, typeof(v4l2_crop));
    public static int VIDIOC_TRY_FMT = Interop._IOWR('V', 64, typeof(v4l2_format));
    public static int VIDIOC_G_PRIORITY = Interop._IOR('V', 67, typeof(uint));
    public static int VIDIOC_S_PRIORITY = Interop._IOW('V', 68, typeof(uint));
    public static int VIDIOC_ENUM_FRAMESIZES = Interop._IOWR('V', 74, typeof(v4l2_frmsizeenum));
    public static int VIDIOC_PREPARE_BUF = Interop._IOWR('V', 93, typeof(v4l2_buffer));
}