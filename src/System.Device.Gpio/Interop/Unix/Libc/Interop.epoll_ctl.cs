﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport(LibcLibrary)]
    internal static extern int epoll_ctl(int epfd, PollOperations op, int fd, ref epoll_event @event);
}

internal enum PollOperations
{
    EPOLL_CTL_ADD = 1,
    EPOLL_CTL_DEL = 2
}

internal struct epoll_event
{
    public PollEvents events;
    public epoll_data data;
}

[Flags]
internal enum PollEvents : uint
{
    EPOLLIN = 0x01,
    EPOLLPRI = 0x02,
    EPOLLERR = 0x08,
    EPOLLET = 0x80000000
}

[StructLayout(LayoutKind.Explicit)]
internal struct epoll_data
{
    [FieldOffset(0)]
    public IntPtr ptr;
    [FieldOffset(0)]
    public int fd;
    [FieldOffset(0)]
    public uint u32;
    [FieldOffset(0)]
    public ulong u64;
    [FieldOffset(0)]
    public int pinNumber;
}
