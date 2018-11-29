// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport("libc", SetLastError = true)]
    internal static extern int ioctl(int fd, uint request, IntPtr argp);

    [DllImport("libc", SetLastError = true)]
    internal static extern int ioctl(int fd, uint request, ulong argp);
}

[Flags]
internal enum I2cFunctionalityFlags : ulong
{
    I2C_FUNC_I2C = 0x00000001,
    I2C_FUNC_SMBUS_BLOCK_DATA = 0x03000000
}

internal enum I2cSettings : uint
{
    /// <summary>Get the adapter functionality mask</summary>
    I2C_FUNCS = 0x0705,
    /// <summary>Use this slave address, even if it is already in use by a driver</summary>
    I2C_SLAVE_FORCE = 0x0706,
    /// <summary>Combined R/W transfer (one STOP only)</summary>
    I2C_RDWR = 0x0707,
    /// <summary>Smbus transfer</summary>
    I2C_SMBUS = 0x0720
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct i2c_msg
{
    public ushort addr;
    public I2cMessageFlags flags;
    public ushort len;
    public byte* buf;
}

[Flags]
internal enum I2cMessageFlags : ushort
{
    /// <summary>Write data to slave</summary>
    I2C_M_WR = 0x0000,
    /// <summary>Read data from slave</summary>
    I2C_M_RD = 0x0001
}

internal unsafe struct i2c_rdwr_ioctl_data
{
    public i2c_msg* msgs;
    public uint nmsgs;
};
