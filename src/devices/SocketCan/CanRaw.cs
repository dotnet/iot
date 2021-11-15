// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.SocketCan
{
    /// <summary>
    /// Allows reading and writing raw frames to CAN Bus
    /// </summary>
    public class CanRaw : IDisposable
    {
        private readonly Socket _socket;

        /// <summary>
        /// Constructs CanRaw instance
        /// </summary>
        /// <param name="networkInterface">Name of the network interface</param>
        public CanRaw(string networkInterface = "can0")
        {
            _socket = new Socket(AddressFamily.ControllerAreaNetwork, SocketType.Raw, ProtocolType.Raw);

            var endpoint = new CanEndPoint(_socket, networkInterface);
            _socket.Bind(endpoint);
        }

        /// <summary>
        ///   Sets the Id, Length and Data properties of the given <see cref="CanFrame"/> and
        ///   returns a <see cref="ReadOnlySpan{T}"/> that represents the filled frame as byte values.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        private static ReadOnlySpan<byte> PrepareFrame(ref CanFrame frame, ReadOnlySpan<byte> data, CanId id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentException(
                    "Id is not valid. Ensure Error flag is not set and that id is in the valid range (11-bit for standard frame and 29-bit for extended frame).",
                    nameof(id));
            }

            if (data.Length > CanFrame.MaxLength)
            {
                throw new ArgumentException(nameof(data), $"Data length cannot exceed {CanFrame.MaxLength} bytes.");
            }

            frame.Id = id;
            frame.Length = (byte)data.Length;

            Debug.Assert(frame.IsValid, "Frame is not valid");

            unsafe
            {
                fixed (CanFrame* fixedFrame = &frame)
                {
                    Span<byte> frameData = new(fixedFrame->Data, data.Length);
                    data.CopyTo(frameData);
                }
            }

            ReadOnlySpan<CanFrame> frameSpan = MemoryMarshal.CreateReadOnlySpan(ref frame, 1);
            return MemoryMarshal.AsBytes(frameSpan);
        }

        /// <summary>
        /// Writes frame to the CAN Bus
        /// </summary>
        /// <param name="data">Data to write (at most 8 bytes)</param>
        /// <param name="id">Recipient identifier</param>
        /// <remarks><paramref name="id"/> can be ignored by recipient - anyone connected to the bus can read or write any frames</remarks>
        public void WriteFrame(ReadOnlySpan<byte> data, CanId id)
        {
            var frame = new CanFrame();
            var buff = PrepareFrame(ref frame, data, id);

            try
            {
                _socket.Send(buff);
            }
            catch (SocketException ex)
            {
                throw new IOException("Socket write operation failed.", ex);
            }
        }

        /// <summary>
        /// Writes frame to the CAN Bus
        /// </summary>
        /// <param name="data">Data to write (at most 8 bytes)</param>
        /// <param name="id">Recipient identifier</param>
        /// <param name="token">Optional cancellation token to cancel the socket operation</param>
        /// <remarks><paramref name="id"/> can be ignored by recipient - anyone connected to the bus can read or write any frames</remarks>
        /// <returns>The number of bytes send (including the given <paramref name="data"/> and the can-frame metadata)</returns>
        public ValueTask<int> WriteFrameAsync(ReadOnlyMemory<byte> data, CanId id, CancellationToken token = default)
        {
            var frame = new CanFrame();
            var buff = PrepareFrame(ref frame, data.Span, id);

            try
            {
                return _socket.SendAsync(
                    buff.ToArray(), // we need to copy the array in order to get the data from Span (stack) to Memory (heap)
                    SocketFlags.None,
                    token);
            }
            catch (SocketException ex)
            {
                throw new IOException("Socket write operation failed.", ex);
            }
        }

        private static void AssertBuffer(int size)
        {
            if (size < CanFrame.MaxLength)
            {
                throw new ArgumentException("data", $"Value must be a minimum of {CanFrame.MaxLength} bytes.");
            }
        }

        /// <summary>
        /// Reads a frame from the bus
        /// </summary>
        /// <param name="data">Data where output data should be written to</param>
        /// <param name="frameLength">Length of the data read</param>
        /// <param name="id">Recipient identifier</param>
        /// <returns>True, when a valid frame was available; otherwise false</returns>
        public bool TryReadFrame(Span<byte> data, out int frameLength, out CanId id)
        {
            AssertBuffer(data.Length);

            CanFrame frame = new();
            Span<CanFrame> frameSpan = MemoryMarshal.CreateSpan(ref frame, 1);
            Span<byte> buff = MemoryMarshal.AsBytes(frameSpan);
            try
            {
                while (buff.Length > 0)
                {
                    int read = _socket.Receive(buff);
                    buff = buff.Slice(read);
                }
            }
            catch (SocketException ex)
            {
                throw new IOException("Socket read operation failed.", ex);
            }

            id = frame.Id;

            if (!CopyData(frame, data))
            {
                // invalid frame
                // we will leave id filled in case it is useful for anyone
                frameLength = 0;
                return false;
            }

            frameLength = frame.Length;
            return true;
        }

        /// <summary>
        /// Reads a frame from the bus
        /// </summary>
        /// <param name="data">Data where output data should be written to</param>
        /// <param name="token">Optional cancellation token to cancel the socket operation</param>
        /// <returns>
        ///   FrameLength - Length of the data read; id - Recipient identifier
        /// </returns>
        public async ValueTask<(int FrameLength, CanId Id)> ReadFrameAsync(Memory<byte> data, CancellationToken token = default)
        {
            AssertBuffer(data.Length);

            GCHandle handle = default;
            CanFrame frame;
            var array = new byte[Marshal.SizeOf(typeof(CanFrame))];
            var buff = new Memory<byte>(array);
            try
            {
                while (buff.Length > 0)
                {
                    var bytes = await _socket.ReceiveAsync(buff, SocketFlags.None, token);
                    buff = buff.Slice(bytes);
                }

                handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                frame = (CanFrame)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(CanFrame))!;
            }
            catch (SocketException ex)
            {
                throw new IOException("Socket read operation failed.", ex);
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }

            if (!CopyData(frame, data.Span))
            {
                // invalid frame
                // we keep id in case it is useful for anyone
                return (0, frame.Id);
            }

            return (frame.Length, frame.Id);
        }

        private static bool CopyData(CanFrame frame, Span<byte> data)
        {
            if (!frame.IsValid)
            {
                return false;
            }

            // This is guaranteed by minimum buffer length and the fact that frame is valid
            Debug.Assert(frame.Length <= data.Length, "Invalid frame length");

            // We should not use input buffer directly for reading:
            // - we do not know how many bytes will be read up front without reading length first
            // - we should not write anything more than pointed by frameLength
            // - we still need to read the remaining bytes to read the full frame
            // Considering there are at most 8 bytes to read it is cheaper
            // to copy rather than doing multiple sys-calls.
            unsafe
            {
                Span<byte> frameData = new Span<byte>(frame.Data, frame.Length);
                frameData.CopyTo(data);
            }

            return true;
        }

        /// <summary>
        /// Set filter on the bus to read only from specified recipient.
        /// </summary>
        /// <param name="id">Recipient identifier</param>
        public void Filter(CanId id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentException("Value must be a valid CanId", nameof(id));
            }

            Span<Interop.CanFilter> filters = stackalloc Interop.CanFilter[1];
            filters[0].can_id = id.Raw;
            filters[0].can_mask =
                id.Value | (uint)CanFlags.ExtendedFrameFormat | (uint)CanFlags.RemoteTransmissionRequest;

            Interop.SetCanRawSocketOption<Interop.CanFilter>(_socket, Interop.CanSocketOption.CAN_RAW_FILTER, filters);
        }

        private static bool IsEff(uint address)
        {
            // has explicit flag or address does not fit in SFF addressing mode
            return (address & (uint)CanFlags.ExtendedFrameFormat) != 0
                   || (address & Interop.CAN_EFF_MASK) != (address & Interop.CAN_SFF_MASK);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
