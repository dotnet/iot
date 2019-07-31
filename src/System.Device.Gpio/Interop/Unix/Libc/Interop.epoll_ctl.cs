// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport(LibcLibrary)]
    internal static extern int epoll_ctl(int epfd, PollOperations op, int fd, ref epoll_event events);
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

internal enum PollEvents : uint
{
    EPOLLIN = 0x01,
    EPOLLPRI = 0x02,
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
