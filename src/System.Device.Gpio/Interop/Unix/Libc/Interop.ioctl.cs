// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport(LibcLibrary, SetLastError = true)]
    internal static extern int ioctl(int fd, uint request, IntPtr argp);

    [DllImport(LibcLibrary, SetLastError = true)]
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
    /// <summary>Get the adapter functionality mask.</summary>
    I2C_FUNCS = 0x0705,
    /// <summary>Use this slave address, even if it is already in use by a driver.</summary>
    I2C_SLAVE_FORCE = 0x0706,
    /// <summary>Combined R/W transfer (one STOP only).</summary>
    I2C_RDWR = 0x0707,
    /// <summary>Smbus transfer.</summary>
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
    /// <summary>Write data to slave.</summary>
    I2C_M_WR = 0x0000,
    /// <summary>Read data from slave.</summary>
    I2C_M_RD = 0x0001
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct i2c_rdwr_ioctl_data
{
    public i2c_msg* msgs;
    public uint nmsgs;
}

[Flags]
internal enum UnixSpiMode : byte
{
    None = 0x00,
    SPI_CPHA = 0x01,
    SPI_CPOL = 0x02,
    SPI_CS_HIGH = 0x04,
    SPI_LSB_FIRST = 0x08,
    SPI_3WIRE = 0x10,
    SPI_LOOP = 0x20,
    SPI_NO_CS = 0x40,
    SPI_READY = 0x80,
    SPI_MODE_0 = None,
    SPI_MODE_1 = SPI_CPHA,
    SPI_MODE_2 = SPI_CPOL,
    SPI_MODE_3 = SPI_CPOL | SPI_CPHA
}

internal enum SpiSettings : uint
{
    /// <summary>Set SPI mode.</summary>
    SPI_IOC_WR_MODE = 0x40016b01,
    /// <summary>Get SPI mode.</summary>
    SPI_IOC_RD_MODE = 0x80016b01,
    /// <summary>Set bits per word.</summary>
    SPI_IOC_WR_BITS_PER_WORD = 0x40016b03,
    /// <summary>Get bits per word.</summary>
    SPI_IOC_RD_BITS_PER_WORD = 0x80016b03,
    /// <summary>Set max speed (Hz).</summary>
    SPI_IOC_WR_MAX_SPEED_HZ = 0x40046b04,
    /// <summary>Get max speed (Hz).</summary>
    SPI_IOC_RD_MAX_SPEED_HZ = 0x80046b04
}

[StructLayout(LayoutKind.Sequential)]
internal struct spi_ioc_transfer
{
    public ulong tx_buf;
    public ulong rx_buf;
    public uint len;
    public uint speed_hz;
    public ushort delay_usecs;
    public byte bits_per_word;
    public byte cs_change;
    public byte tx_nbits;
    public byte rx_nbits;
    public ushort pad;
}
