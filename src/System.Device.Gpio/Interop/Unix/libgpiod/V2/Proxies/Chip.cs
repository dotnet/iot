// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

using System.Collections.Generic;
using System.Device.Gpio.Interop.Unix.libgpiod.V2.Binding.Handles;
using System.Device.Gpio.Interop.Unix.libgpiod.V2.ValueTypes;
using System.Runtime.InteropServices;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Interop.Unix.libgpiod.V2.Proxies;

/// <summary>
/// A GPIO chip object is associated with an open file descriptor to the GPIO character device. It exposes basic information about the chip and
/// allows callers to retrieve information about each line, watch lines for state changes and make line requests.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html"/>
internal class Chip : LibGpiodProxyBase, IDisposable
{
    private readonly ChipSafeHandle _handle;

    /// <summary>
    /// Constructor for a chip-proxy object. This call will try to open the chip.
    /// </summary>
    /// <param name="devicePath">File system path to the chip device, e.g. '/dev/gpiochip4'</param>
    /// <exception cref="GpiodException">The chip does not exist or an unexpected error happened while opening chip</exception>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#ga25097f48949d0ac81e9ab341193da1a4"/>
    public Chip(string devicePath)
    {
        _handle = TryCallGpiod(() => LibgpiodV2.gpiod_chip_open(Marshal.StringToHGlobalAuto(devicePath)));

        if (_handle.IsInvalid)
        {
            throw new GpiodException($"Could not open gpio chip at path '{devicePath}': {LastErr.GetMsg()}");
        }
    }

    /// <summary>
    /// Constructor for a chip-proxy-object. This call will try to open the chip.
    /// </summary>
    /// <param name="chipNumber">Number that will be translated to a device path for example 4 -> '/dev/gpiochip4'</param>
    /// <exception cref="GpiodException">The chip does not exist or an unexpected error happened while opening chip</exception>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#ga25097f48949d0ac81e9ab341193da1a4"/>
    public Chip(int chipNumber)
        : this($"/dev/gpiochip{chipNumber}")
    {
    }

    /// <summary>
    /// Get the path used to open the chip
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#gacdcb43322311212201b9295bfd44e162"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public string GetPath()
    {
        return TryCallGpiodLocked(() => Marshal.PtrToStringAuto(LibgpiodV2.gpiod_chip_get_path(_handle)) ??
            throw new GpiodException($"Could not get chip path: {LastErr.GetMsg()}"));
    }

    /// <summary>
    /// Get information about the chip
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#gad65098ba48da0f22d1b0be149b4cf578"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public ChipInfo GetInfo()
    {
        return TryCallGpiodLocked(() =>
        {
            var chipInfoHandle = LibgpiodV2.gpiod_chip_get_info(_handle);

            if (chipInfoHandle.IsInvalid)
            {
                throw new GpiodException($"Could not get chip info: {LastErr.GetMsg()}");
            }

            return new ChipInfo(chipInfoHandle);
        });
    }

    /// <summary>
    /// Get a snapshot of information about a line.
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#ga391930aadb053f38b59bae20058c7dac"/>
    /// <exception cref="GpiodException"><paramref name="offset"/> not known to chip or other error when invoking native function</exception>
    public LineInfo GetLineInfo(Offset offset)
    {
        return TryCallGpiodLocked(() =>
        {
            var lineInfoHandle = LibgpiodV2.gpiod_chip_get_line_info(_handle, offset);

            if (lineInfoHandle.IsInvalid)
            {
                throw new GpiodException($"Could not get line info for offset '{offset}': {LastErr.GetMsg()}");
            }

            return new LineInfo(lineInfoHandle);
        });
    }

    /// <summary>
    /// Get a snapshot of the status of a line and start watching it for future changes
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#gae616072c7dc98088ed466e6c8030c6f8"/>
    /// <exception cref="GpiodException"><paramref name="offset"/> not known to chip or other error when invoking native function</exception>
    public LineInfo WatchLineInfo(Offset offset)
    {
        return TryCallGpiodLocked(() =>
        {
            var lineInfoHandle = LibgpiodV2.gpiod_chip_watch_line_info(_handle, offset);

            if (lineInfoHandle.IsInvalid)
            {
                throw new GpiodException($"Could not watch line info '{offset}': {LastErr.GetMsg()}");
            }

            return new LineInfo(lineInfoHandle);
        });
    }

    /// <summary>
    /// Stop watching a line for status changes
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#gae94ec19eb6a44e35e8d87bc3d22047ab"/>
    /// <exception cref="GpiodException"><paramref name="offset"/> not known to chip or other error when invoking native function</exception>
    public void UnWatchLineInfo(Offset offset)
    {
        TryCallGpiodLocked(() =>
        {
            int result = LibgpiodV2.gpiod_chip_unwatch_line_info(_handle, offset);
            if (result < 0)
            {
                throw new GpiodException($"Could not unwatch line info '{offset}': {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Get the file descriptor associated with the chip.
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#ga84ceb070e2f8cded775467f9dc780c67"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public int GetFileDescriptor()
    {
        return TryCallGpiodLocked(() => LibgpiodV2.gpiod_chip_get_fd(_handle));
    }

    /// <summary>
    /// Wait for line status change events on any of the watched lines on the chip
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#ga7dc74ad4569a634c51284ffaa48fc4cb"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    /// <returns>0 if wait timed out, 1 if an event is pending</returns>
    public int WaitInfoEvent(long timeoutNs)
    {
        return TryCallGpiodLocked(() =>
        {
            int result = LibgpiodV2.gpiod_chip_wait_info_event(_handle, timeoutNs);
            if (result < 0)
            {
                throw new GpiodException($"Could not wait for line info event: {LastErr.GetMsg()}");
            }

            return result;
        });
    }

    /// <summary>
    /// Read a single line status change event from the chip
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#ga7ad34f33d266705bb85d45f87e8c16cd"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public LineInfoEvent ReadInfoEvent()
    {
        return TryCallGpiodLocked(() =>
        {
            var lineInfoEventHandle = LibgpiodV2.gpiod_chip_read_info_event(_handle);

            if (lineInfoEventHandle.IsInvalid)
            {
                throw new GpiodException($"Could not read line info event: {LastErr.GetMsg()}");
            }

            return new LineInfoEvent(lineInfoEventHandle);
        });
    }

    /// <summary>
    /// Get a line's offset by name
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#gaabdbbb68c2736c97554848ba210c7e76"/>
    /// <exception cref="GpiodException">Line is not exposed by chip or unexpected error invoking native function</exception>
    public int GetLineOffsetFromName(string name)
    {
        return TryCallGpiodLocked(() =>
        {
            int result = LibgpiodV2.gpiod_chip_get_line_offset_from_name(_handle, name);
            if (result < 0)
            {
                throw new GpiodException($"Could not get line offset from name '{name}': {LastErr.GetMsg()}");
            }

            return result;
        });
    }

    /// <summary>
    /// Request a set of lines for exclusive usage
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chips.html#ga83d57e0b534168df5218327541b6c63e"/>
    /// <exception cref="GpiodException">Request denied due to invalid request arguments or an unexpected error happened invoking native function</exception>
    public LineRequest RequestLines(RequestConfig requestConfig, LineConfig lineConfig)
    {
        return TryCallGpiodLocked(() =>
        {
            var lineRequestHandle = LibgpiodV2.gpiod_chip_request_lines(_handle, requestConfig.Handle, lineConfig.Handle);

            if (lineRequestHandle.IsInvalid)
            {
                throw new GpiodException($"Could not request line/s: {LastErr.GetMsg()}, " +
                    $"RequestConfig: {requestConfig.MakeSnapshot()}\nLineConfig: {lineConfig.MakeSnapshot()}");
            }

            return new LineRequest(lineRequestHandle);
        });
    }

    /// <summary>
    /// Frees all associated resources of gpiod chip object
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void Close()
    {
        TryCallGpiodLocked(_handle.Dispose);
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public Snapshot MakeSnapshot()
    {
        var chipInfo = GetInfo();
        var chipInfoSnapshot = chipInfo.MakeSnapshot();
        int numLines = chipInfoSnapshot.NumLines;
        List<LineInfo.Snapshot> lineInfos = new();
        for (uint offset = 0; offset < numLines; offset++)
        {
            var lineInfo = GetLineInfo(offset).MakeSnapshot();
            lineInfos.Add(lineInfo);
        }

        return new Snapshot(chipInfoSnapshot, GetPath(), GetFileDescriptor(), lineInfos);
    }

    /// <summary>
    /// Contains all readable information that was recorded at one and the same time
    /// </summary>
    public sealed record Snapshot(ChipInfo.Snapshot ChipInfo, string Path, int FileDescriptor, IEnumerable<LineInfo.Snapshot> LineInfos)
    {
        /// <summary>
        /// Converts the whole snapshot to string
        /// </summary>
        public override string ToString()
        {
            return
                $"{nameof(ChipInfo)}: {ChipInfo}, {nameof(Path)}: {Path}, {nameof(FileDescriptor)}: {FileDescriptor}, {nameof(LineInfos)}: {string.Join("\n", LineInfos)}";
        }
    }

    #region Dispose

    private readonly object _isDisposedLock = new();
    private bool _isDisposed;

    public void Dispose()
    {
        lock (_isDisposedLock)
        {
            if (_isDisposed)
            {
                return;
            }

            Close();

            _isDisposed = true;
        }
    }

    #endregion
}
