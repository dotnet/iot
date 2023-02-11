// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Imports for the ftd2xx.dll as well as libft4222
    /// </summary>
    internal class FtFunction
    {
        /// <summary>
        /// Create Device Information List.
        /// </summary>
        /// <param name="numdevs">number of devices</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx", EntryPoint = "FT_CreateDeviceInfoList")]
        public static extern FtStatus FT_CreateDeviceInfoList(out uint numdevs);

        /// <summary>
        /// Get Device Information Detail.
        /// </summary>
        /// <param name="index">Index of the device</param>
        /// <param name="flags">Flags</param>
        /// <param name="chiptype">Device type</param>
        /// <param name="id">ID</param>
        /// <param name="locid">Location ID</param>
        /// <param name="serialnumber">Serial Number</param>
        /// <param name="description">Description</param>
        /// <param name="ftHandle">Handle</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_GetDeviceInfoDetail(uint index, out uint flags, out FtDeviceType chiptype, out uint id, out uint locid, in byte serialnumber, in byte description, out IntPtr ftHandle);

        /// <summary>
        /// Open a device.
        /// </summary>
        /// <param name="pvArg1">The device element identifying the device, depends on the flag</param>
        /// <param name="dwFlags">The flag how to open the device</param>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_OpenEx(uint pvArg1, FtOpenType dwFlags, out SafeFtHandle ftHandle);

        /// <summary>
        /// Close the device.
        /// </summary>
        /// <param name="ftHandle">The device handle</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_Close(IntPtr ftHandle);

        /// <summary>
        /// Reset the device.
        /// </summary>
        /// <param name="ftHandle">The device handle</param>
        /// <returns>The status</returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_ResetDevice(SafeFtHandle ftHandle);

        /// <summary>
        /// Sets timeouts.
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="dwReadTimeout">Read timeout in milliseconds</param>
        /// <param name="dwWriteTimeout">Write timeout in milliseconds</param>
        /// <returns></returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetTimeouts(SafeFtHandle ftHandle, uint dwReadTimeout, uint dwWriteTimeout);

        /// <summary>
        /// Sets latency timer.
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="ucLatency">The latency in milliseconds</param>
        /// <returns></returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetLatencyTimer(SafeFtHandle ftHandle, byte ucLatency);

        /// <summary>
        /// Sets flow control.
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="usFlowControl">The type of flow control</param>
        /// <param name="uXon">Byte used for Xon</param>
        /// <param name="uXoff">Byte used for Xoff</param>
        /// <returns></returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetFlowControl(SafeFtHandle ftHandle, ushort usFlowControl, byte uXon, byte uXoff);

        /// <summary>
        /// Set bit mode.
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="ucMask">bit mode mask</param>
        /// <param name="ucMode">bit mode</param>
        /// <returns></returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetBitMode(SafeFtHandle ftHandle, byte ucMask, FtBitMode ucMode);

        /// <summary>
        /// Get bit mode.
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="ucMode">The actual value of each pin regarless if they are input or output.</param>
        /// <returns></returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_GetBitMode(SafeFtHandle ftHandle, ref byte ucMode);

        /// <summary>
        /// Get queued status, this is used only to read the status of number of bytes to write.
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="lpdwAmountInRxQueue">number of available bytes in queue</param>
        /// <returns></returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_GetQueueStatus(SafeFtHandle ftHandle, ref uint lpdwAmountInRxQueue);

        /// <summary>
        /// Read.
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="lpBuffer">The buffer to place the bytes in</param>
        /// <param name="dwBytesToRead">The number of bytes to read</param>
        /// <param name="lpdwBytesReturned">The number of byte read</param>
        /// <returns></returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_Read(SafeFtHandle ftHandle, in byte lpBuffer, uint dwBytesToRead, ref uint lpdwBytesReturned);

        /// <summary>
        /// Write.
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="lpBuffer">The buffer containing the bytes to send</param>
        /// <param name="dwBytesToWrite">The number of bytes to write</param>
        /// <param name="lpdwBytesWritten">The number of bytes written</param>
        /// <returns></returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_Write(SafeFtHandle ftHandle, in byte lpBuffer, uint dwBytesToWrite, ref uint lpdwBytesWritten);

        /// <summary>
        /// Sets chars for interruption.
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="uEventCh">0 to ignore the event, 1 use it</param>
        /// <param name="uEventChEn">The char to use</param>
        /// <param name="uErrorCh">0 to ignore the error event, 1 use it</param>
        /// <param name="uErrorChEn">The char to use for error</param>
        /// <returns></returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetChars(SafeFtHandle ftHandle, byte uEventCh, byte uEventChEn, byte uErrorCh, byte uErrorChEn);

        /// <summary>
        /// Sets the USB transfer parameters.
        /// </summary>
        /// <param name="ftHandle">The handle of the open device</param>
        /// <param name="dwInTransferSize">Transfer size of the buffer to read</param>
        /// <param name="dwOutTransferSize">Transfer size of the buffer to write</param>
        /// <returns></returns>
        [DllImport("ftd2xx")]
        public static extern FtStatus FT_SetUSBParameters(SafeFtHandle ftHandle, uint dwInTransferSize, uint dwOutTransferSize);
    }
}
