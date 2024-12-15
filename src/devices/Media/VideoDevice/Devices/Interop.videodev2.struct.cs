// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1307 // Field should begin with upper-case letter

using System;
using System.Runtime.InteropServices;
using Iot.Device.Media;

internal partial class InteropVideodev2
{
    internal struct V4l2FrameBuffer
    {
        public IntPtr Start;
        public uint Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_capability
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string driver;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string card;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string bus_info;

        public uint version;
        public uint capabilities;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] reserved;
    }

    internal enum v4l2_ctrl_type : uint
    {
        V4L2_CTRL_TYPE_INTEGER = 1,
        V4L2_CTRL_TYPE_BOOLEAN = 2,
        V4L2_CTRL_TYPE_MENU = 3,
        V4L2_CTRL_TYPE_BUTTON = 4,
        V4L2_CTRL_TYPE_INTEGER64 = 5,
        V4L2_CTRL_TYPE_CTRL_CLASS = 6,
        V4L2_CTRL_TYPE_STRING = 7,
        V4L2_CTRL_TYPE_BITMASK = 8,
        V4L2_CTRL_TYPE_INTEGER_MENU = 9,
        V4L2_CTRL_COMPOUND_TYPES = 0x0100,
        V4L2_CTRL_TYPE_U8 = 0x0100,
        V4L2_CTRL_TYPE_U16 = 0x0101,
        V4L2_CTRL_TYPE_U32 = 0x0102,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_queryctrl
    {
        public VideoDeviceValueType id;
        public v4l2_ctrl_type type;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string name;

        public int minimum;
        public int maximum;
        public int step;
        public int default_value;
        public uint flags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_control
    {
        public VideoDeviceValueType id;
        public int value;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_fmtdesc
    {
        public uint index;
        public v4l2_buf_type type;
        public uint flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string description;

        public VideoPixelFormat pixelformat;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] reserved;
    }

    internal enum v4l2_buf_type : uint
    {
        V4L2_BUF_TYPE_VIDEO_CAPTURE = 1,
        V4L2_BUF_TYPE_VIDEO_OUTPUT = 2,
        V4L2_BUF_TYPE_VIDEO_OVERLAY = 3,
        V4L2_BUF_TYPE_VBI_CAPTURE = 4,
        V4L2_BUF_TYPE_VBI_OUTPUT = 5,
        V4L2_BUF_TYPE_SLICED_VBI_CAPTURE = 6,
        V4L2_BUF_TYPE_SLICED_VBI_OUTPUT = 7,
        V4L2_BUF_TYPE_VIDEO_OUTPUT_OVERLAY = 8,
        V4L2_BUF_TYPE_VIDEO_CAPTURE_MPLANE = 9,
        V4L2_BUF_TYPE_VIDEO_OUTPUT_MPLANE = 10,
        V4L2_BUF_TYPE_SDR_CAPTURE = 11,
        V4L2_BUF_TYPE_SDR_OUTPUT = 12,
        V4L2_BUF_TYPE_META_CAPTURE = 13,
        V4L2_BUF_TYPE_META_OUTPUT = 14,
        V4L2_BUF_TYPE_PRIVATE = 0x80,
    }

    internal enum v4l2_field : uint
    {
        V4L2_FIELD_ANY = 0,
        V4L2_FIELD_NONE = 1,
        V4L2_FIELD_TOP = 2,
        V4L2_FIELD_BOTTOM = 3,
        V4L2_FIELD_INTERLACED = 4,
        V4L2_FIELD_SEQ_TB = 5,
        V4L2_FIELD_SEQ_BT = 6,
        V4L2_FIELD_ALTERNATE = 7,
        V4L2_FIELD_INTERLACED_TB = 8,
        V4L2_FIELD_INTERLACED_BT = 9,
    }

    internal enum v4l2_colorspace : uint
    {
        V4L2_COLORSPACE_DEFAULT = 0,
        V4L2_COLORSPACE_SMPTE170M = 1,
        V4L2_COLORSPACE_SMPTE240M = 2,
        V4L2_COLORSPACE_REC709 = 3,
        V4L2_COLORSPACE_BT878 = 4,
        V4L2_COLORSPACE_470_SYSTEM_M = 5,
        V4L2_COLORSPACE_470_SYSTEM_BG = 6,
        V4L2_COLORSPACE_JPEG = 7,
        V4L2_COLORSPACE_SRGB = 8,
        V4L2_COLORSPACE_ADOBERGB = 9,
        V4L2_COLORSPACE_BT2020 = 10,
        V4L2_COLORSPACE_RAW = 11,
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct v4l2_pix_format
    {
        [FieldOffset(0)]
        public uint width;
        [FieldOffset(4)]
        public uint height;
        [FieldOffset(8)]
        public VideoPixelFormat pixelformat;
        [FieldOffset(12)]
        public v4l2_field field;
        [FieldOffset(16)]
        public uint bytesperline;
        [FieldOffset(20)]
        public uint sizeimage;
        [FieldOffset(24)]
        public v4l2_colorspace colorspace;
        [FieldOffset(28)]
        public uint priv;
        [FieldOffset(32)]
        public uint flags;
        [FieldOffset(36)]
        public uint ycbcr_enc;
        [FieldOffset(36)]
        public uint hsv_enc;
        [FieldOffset(40)]
        public uint quantization;
        [FieldOffset(44)]
        public uint xfer_func;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_rect
    {
        public int left;
        public int top;
        public uint width;
        public uint height;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct v4l2_clip
    {
        public v4l2_rect c;
        public v4l2_clip* next;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct v4l2_window
    {
        public v4l2_rect w;
        public v4l2_field field;
        public uint chromakey;
        public v4l2_clip* clips;
        public uint clipcount;
        public void* bitmap;
        public byte global_alpha;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct v4l2_vbi_format
    {
        public uint sampling_rate;
        public uint offset;
        public uint samples_per_line;
        public uint sample_format;
        public fixed int start[2];
        public fixed uint count[2];
        public uint flags;
        public fixed uint reserved[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct v4l2_sliced_vbi_format
    {
        public ushort service_set;
        public fixed ushort service_lines[48];
        public uint io_size;
        public fixed uint reserved[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct v4l2_sdr_format
    {
        public VideoPixelFormat pixelformat;
        public uint buffersize;
        public fixed byte reserved[24];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_meta_format
    {
        public uint dataformat;
        public uint buffersize;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct V412_Fmt
    {
        [FieldOffset(0)]
        public v4l2_pix_format pix;
        [FieldOffset(0)]
        public v4l2_window win;
        [FieldOffset(0)]
        public v4l2_vbi_format vbi;
        [FieldOffset(0)]
        public v4l2_sliced_vbi_format sliced;
        [FieldOffset(0)]
        public v4l2_sdr_format sdr;
        [FieldOffset(0)]
        public v4l2_meta_format meta;
        [FieldOffset(0)]
        public fixed byte raw[200];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_format
    {
        public v4l2_buf_type type;
        public V412_Fmt fmt;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_fract
    {
        public uint numerator;
        public uint denominator;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_cropcap
    {
        public v4l2_buf_type type;
        public v4l2_rect bounds;
        public v4l2_rect defrect;
        public v4l2_fract pixelaspect;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_crop
    {
        public v4l2_buf_type type;
        public v4l2_rect c;
    }

    internal enum v4l2_memory : uint
    {
        V4L2_MEMORY_MMAP = 1,
        V4L2_MEMORY_USERPTR = 2,
        V4L2_MEMORY_OVERLAY = 3,
        V4L2_MEMORY_DMABUF = 4,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_requestbuffers
    {
        public uint count;
        public v4l2_buf_type type;
        public v4l2_memory memory;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_timeval
    {
        public uint tv_sec;
        public uint tv_usec;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_timecode
    {
        public uint type;
        public uint flags;
        public byte frames;
        public byte seconds;
        public byte minutes;
        public byte hours;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] userbits;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_buffer
    {
        public uint index;
        public v4l2_buf_type type;
        public uint bytesused;
        public uint flags;
        public v4l2_field field;

        [StructLayout(LayoutKind.Sequential)]
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
        public struct timeval
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
        {
            public nint tv_sec;
            public nint tv_usec;
        }

        public timeval timestamp;

        public v4l2_timecode timecode;
        public uint sequence;
        public v4l2_memory memory;

        [StructLayout(LayoutKind.Explicit)]
        public struct m_union
        {
            [FieldOffset(0)]
            public uint offset;
            [FieldOffset(0)]
            public nuint userptr;
        }

        public m_union m;

        public uint length;
        public uint input;
        public uint reserved;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct v4l2_frmsizeenum
    {
        [FieldOffset(0)]
        public uint index;
        [FieldOffset(4)]
        public VideoPixelFormat pixel_format;
        [FieldOffset(8)]
        public v4l2_frmsizetypes type;
        [FieldOffset(12)]
        public v4l2_frmsize_discrete discrete;
        [FieldOffset(12)]
        public v4l2_frmsize_stepwise stepwise;
        [FieldOffset(36)]
        public fixed uint reserved[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_frmsize_discrete
    {
        public uint width;
        public uint height;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct v4l2_frmsize_stepwise
    {
        public uint min_width;
        public uint max_width;
        public uint step_width;
        public uint min_height;
        public uint max_height;
        public uint step_height;
    }

    internal enum v4l2_frmsizetypes : uint
    {
        V4L2_FRMSIZE_TYPE_DISCRETE = 1,
        V4L2_FRMSIZE_TYPE_CONTINUOUS = 2,
        V4L2_FRMSIZE_TYPE_STEPWISE = 3,
    }

}
