// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1307 // Field should begin with upper-case letter
#pragma warning disable SX1309 // Field should begin with an underscore

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Iot.Device.SocketCan
{
    internal class Interop
    {
        private const int AF_CAN = 29;

        // SFF = Standard Frame Format - 11 bit
        public const uint CAN_SFF_MASK = 0x000007FF;

        // EFF = Extended Frame Format - 29 bit
        public const uint CAN_EFF_MASK = 0x1FFFFFFF;
        public const uint CAN_ERR_MASK = 0x1FFFFFFF;

        public const int SOL_CAN_BASE = 100;
        public const int SOL_CAN_RAW = SOL_CAN_BASE + (int)CanProtocol.CAN_RAW;

        [DllImport("libc", EntryPoint = "ioctl", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Ioctl3(int fd, uint request, ref ifreq ifr);

        [DllImport("libc", EntryPoint = "setsockopt", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int SetSocketOpt(int fd, int level, int optName, byte* optVal, int optlen);

        public static bool SetCanRawSocketOption<T>(Socket socket, CanSocketOption optName, ReadOnlySpan<T> data)
            where T : struct =>
            SetSocketOption(socket.Handle, SOL_CAN_RAW, optName, data);

        private static unsafe bool SetSocketOption<T>(IntPtr fd, int level, CanSocketOption optName, ReadOnlySpan<T> data)
            where T : struct
        {
            ReadOnlySpan<byte> buf = MemoryMarshal.AsBytes(data);
            fixed (byte* pinned = buf)
            {
                return SetSocketOpt((int)fd, level, (int)optName, pinned, buf.Length) == 0;
            }
        }

        public static SocketAddress GetCanSocketAddress(Socket socket, string name)
        {
            var canSockAddr = new CanSocketAddress()
            {
                can_family = AF_CAN,
                can_ifindex = Interop.GetInterfaceIndex(socket.Handle, name),
                tx_id = 0,
                rx_id = 0
            };

            var size = Marshal.SizeOf(canSockAddr);
            byte[] buffer = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(canSockAddr, ptr, true);
            Marshal.Copy(ptr, buffer, 0, size);
            Marshal.FreeHGlobal(ptr);

            var sockAddr = new SocketAddress(AddressFamily.ControllerAreaNetwork, Marshal.SizeOf(typeof(CanSocketAddress)));
            for (int i = 0; i < size; i++)
            {
                sockAddr[i] = buffer[i];
            }

            return sockAddr;
        }

        private static unsafe int GetInterfaceIndex(IntPtr fd, string name)
        {
            const uint SIOCGIFINDEX = 0x8933;
            const int MaxLen = ifreq.IFNAMSIZ - 1;

            if (name.Length >= MaxLen)
            {
                throw new ArgumentException(nameof(name), $"Value exceeds maximum allowed length of {MaxLen} size.");
            }

            ifreq ifr = new ifreq();
            fixed (char* inIntefaceName = name)
            {
                int written = Encoding.ASCII.GetBytes(inIntefaceName, name.Length, ifr.ifr_name, MaxLen);
                ifr.ifr_name[written] = 0;
            }

            int ret = Ioctl3(fd.ToInt32(), SIOCGIFINDEX, ref ifr);
            if (ret == -1)
            {
                throw new IOException($"Could not get interface index for `{name}`");
            }

            return ifr.ifr_ifindex;
        }

        internal unsafe struct ifreq
        {
            internal const int IFNAMSIZ = 16;
            public fixed byte ifr_name[IFNAMSIZ];
            public int ifr_ifindex;
            private fixed byte _padding[IFNAMSIZ - sizeof(int)];

        }

        internal struct CanSocketAddress
        {
            public short can_family;
            public int can_ifindex;
            public uint rx_id;
            public uint tx_id;
        }

        internal struct CanFilter
        {
            public uint can_id;
            public uint can_mask;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct CanFdFrame
        {
            private const int CANFD_MAX_DLEN = 64;
            // can_id includes EFF, RTR and ERR flags
            public uint can_id;
            public byte len;
            private CanFdFlags flags;
            private byte _res0;
            private byte _res1;
            public fixed byte data[CANFD_MAX_DLEN];
        }

        internal enum CanProtocol : int
        {
            CAN_RAW = 1,
            // Broadcast Manager
            CAN_BCM = 2,
            // VAG Transport Protocol v1.6
            CAN_TP16 = 3,
            // VAG Transport Protocol v2.0
            CAN_TP20 = 4,
            // Bosch MCNet
            CAN_MCNET = 5,
            // ISO 15765-2 Transport Protocol
            CAN_ISOTP = 6,
            CAN_NPROTO = 7,
        }

        internal enum CanSocketOption : int
        {
            // set 0 .. n can_filter(s)
            CAN_RAW_FILTER = 1,
            // set filter for error frames
            CAN_RAW_ERR_FILTER,
            // local loopback (default:on)
            CAN_RAW_LOOPBACK,
            // receive my own msgs (default:off)
            CAN_RAW_RECV_OWN_MSGS,
            // allow CAN FD frames (default:off)
            CAN_RAW_FD_FRAMES,
            // all filters must match to trigger
            CAN_RAW_JOIN_FILTERS,
        }

        [Flags]
        internal enum CanFdFlags : byte
        {
            CANFD_BRS = 0x01,
            CANFD_ESI = 0x02,
        }
    }
}
