// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

using System.Device.Gpio.Interop.Unix.libgpiod.v2.Binding.Enums;
using System.Device.Gpio.Interop.Unix.libgpiod.v2.Binding.Handles;
using Libgpiodv2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Interop.Unix.libgpiod.v2.Proxies;

/// <summary>
/// Callers are notified about changes in a line's status due to GPIO uAPI calls.
/// Each info event contains information about the event itself (timestamp, type)
/// as well as a snapshot of line's status in the form of a line-info object.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__watch.html"/>
internal class LineSettings : LibGpiodProxyBase
{
    internal LineSettingsSafeHandle Handle { get; }

    /// <summary>
    /// Constructor for a line-settings-proxy object. This call will create a new gpiod line-settings object.
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#gab02d1cceffbb24dc95edc851e8519f0b"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public LineSettings()
    {
        Handle = TryCallGpiod(Libgpiodv2.gpiod_line_settings_new);

        if (Handle.IsInvalid)
        {
            throw new GpiodException($"Could not create new line settings: {LastErr.GetMsg()}");
        }
    }

    /// <summary>
    /// Constructor for a line-settings-proxy object that points to an existing gpiod line-settings object using a safe handle.
    /// </summary>
    /// <exception cref="GpiodException"><paramref name="handle"/> is invalid</exception>
    public LineSettings(LineSettingsSafeHandle handle)
    {
        if (handle.IsInvalid)
        {
            throw new GpiodException($"Could not create new line settings");
        }

        Handle = handle;
    }

    /// <summary>
    /// Copies an existing gpiod line-settings object and returns a new line-settings-proxy object.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public LineSettings Copy()
    {
        var handle = TryCallGpiodLocked(() => Libgpiodv2.gpiod_line_settings_copy(Handle));

        if (handle.IsInvalid)
        {
            throw new GpiodException($"Could not copy line settings: {LastErr.GetMsg()}");
        }

        return new LineSettings(handle);
    }

    /// <summary>
    /// Reset the line settings object to its default values
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga324f336505f50fd85fb5f7019b36d766"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void Reset()
    {
        TryCallGpiodLocked(() => Libgpiodv2.gpiod_line_settings_reset(Handle));
    }

    /// <summary>
    /// Set direction
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#gac8aefce211865b025ebd77eebd8d987b"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void SetDirection(GpiodLineDirection lineDirection)
    {
        TryCallGpiodLocked(() =>
        {
            int result = Libgpiodv2.gpiod_line_settings_set_direction(Handle, lineDirection);
            if (result < 0)
            {
                throw new GpiodException($"Could not set line direction: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Get direction
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga25d849e5a328968e8a30ba7fccd3f54d"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public GpiodLineDirection GetDirection()
    {
        return TryCallGpiodLocked(() => Libgpiodv2.gpiod_line_settings_get_direction(Handle));
    }

    /// <summary>
    /// Set edge detection
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga61abb95b4015bd5563c00cf8d1b996ba"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void SetEdgeDetection(GpiodLineEdge edge)
    {
        TryCallGpiodLocked(() =>
        {
            int result = Libgpiodv2.gpiod_line_settings_set_edge_detection(Handle, edge);
            if (result < 0)
            {
                throw new GpiodException($"Could not set line edge detection: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Get edge detection
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#gaae44f5ce10e1cca4cfe147b7b88985e8"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public GpiodLineEdge GetEdgeDetection()
    {
        return TryCallGpiodLocked(() => Libgpiodv2.gpiod_line_settings_get_edge_detection(Handle));
    }

    /// <summary>
    /// Set bias
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga30a68064399a5485bf0aa874b54186cf"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void SetBias(GpiodLineBias bias)
    {
        TryCallGpiodLocked(() =>
        {
            int result = Libgpiodv2.gpiod_line_settings_set_bias(Handle, bias);
            if (result < 0)
            {
                throw new GpiodException($"Could not set line bias: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Get bias
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#gac4e8a58166121e9100747141f2033e9a"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public GpiodLineBias GetBias()
    {
        return TryCallGpiodLocked(() => Libgpiodv2.gpiod_line_settings_get_bias(Handle));
    }

    /// <summary>
    /// Set drive
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#gab4960d461440690519e1fd36f90bc3a9"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void SetDrive(GpiodLineDrive drive)
    {
        TryCallGpiodLocked(() =>
        {
            int result = Libgpiodv2.gpiod_line_settings_set_drive(Handle, drive);
            if (result < 0)
            {
                throw new GpiodException($"Could not set line drive: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Get drive
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga33e9665afd40c03c223f57ad28176d49"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public GpiodLineDrive GetDrive()
    {
        return TryCallGpiodLocked(() => Libgpiodv2.gpiod_line_settings_get_drive(Handle));
    }

    /// <summary>
    /// Set active-low setting
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#gaa3361de155317f8f4b722e53f8407b58"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void SetActiveLow(bool isActiveLow)
    {
        TryCallGpiodLocked(() =>
        {
            int result = Libgpiodv2.gpiod_line_settings_set_active_low(Handle, isActiveLow);
            if (result < 0)
            {
                throw new GpiodException($"Could not set line active low: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Get active-low setting
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga9eb217e64b503a4dcdbd7c1a051d9c89"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public bool GetActiveLow()
    {
        return TryCallGpiodLocked(() => Libgpiodv2.gpiod_line_settings_get_active_low(Handle));
    }

    /// <summary>
    /// Set debounce period
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga61f3cb8d87fa6d642f8854cebf5631c4"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void SetDebouncePeriod(TimeSpan debouncePeriod)
    {
        TryCallGpiodLocked(() =>
        {
            int result = Libgpiodv2.gpiod_line_settings_set_debounce_period_us(Handle, (ulong)(debouncePeriod.TotalMilliseconds * 1000));
            if (result < 0)
            {
                throw new GpiodException($"Could not set line debounce period: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Get debounce period
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga3dacb33e3823674cd34f5fa6de40f90e"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public TimeSpan GetDebouncePeriod()
    {
        return TryCallGpiodLocked(() =>
        {
            ulong debouncePeriodUs = Libgpiodv2.gpiod_line_settings_get_debounce_period_us(Handle);
            return TimeSpan.FromMilliseconds(debouncePeriodUs / 1000f);
        });
    }

    /// <summary>
    /// Set event clock
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga0e1dd0f4cbb9bd6aa0c08c1cf2236036"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void SetEventClock(GpiodLineClock eventClock)
    {
        TryCallGpiodLocked(() =>
        {
            int result = Libgpiodv2.gpiod_line_settings_set_event_clock(Handle, eventClock);
            if (result < 0)
            {
                throw new GpiodException($"Could not set line event clock: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Get event clock setting
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga42b419e0f53e6fd28464edd8f0a5a413"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public GpiodLineClock GetEventClock()
    {
        return TryCallGpiodLocked(() => Libgpiodv2.gpiod_line_settings_get_event_clock(Handle));
    }

    /// <summary>
    /// Set the output value
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga8c2292a52a6239d52c1f61d07cda2077"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public void SetOutputValue(GpiodLineValue lineValue)
    {
        TryCallGpiodLocked(() =>
        {
            int result = Libgpiodv2.gpiod_line_settings_set_output_value(Handle, lineValue);
            if (result < 0)
            {
                throw new GpiodException($"Could not set line output value: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Get the output value
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__settings.html#ga7cad90f652fcc1c8c0000d69db9e8097"/>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public GpiodLineValue GetOutputValue()
    {
        return TryCallGpiodLocked(() => Libgpiodv2.gpiod_line_settings_get_output_value(Handle));
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public Snapshot MakeSnapshot()
    {
        return TryCallGpiodLocked(() => new Snapshot(GetDirection(), GetEdgeDetection(), GetBias(), GetDrive(), GetActiveLow(), GetDebouncePeriod(),
            GetEventClock(), GetOutputValue()));
    }

    /// <summary>
    /// Contains all readable information that was recorded at one and the same time
    /// </summary>
    public sealed record Snapshot(GpiodLineDirection Direction, GpiodLineEdge EdgeDetection, GpiodLineBias Bias, GpiodLineDrive Drive,
        bool IsActiveLow, TimeSpan DebouncePeriod, GpiodLineClock EventClock, GpiodLineValue OutputValue)
    {
        /// <summary>
        /// Converts the whole snapshot to string
        /// </summary>
        public override string ToString()
        {
            return
                $"{nameof(Direction)}: {Direction}, {nameof(EdgeDetection)}: {EdgeDetection}, {nameof(Bias)}: {Bias}, {nameof(Drive)}: {Drive}, {nameof(IsActiveLow)}: {IsActiveLow}, {nameof(DebouncePeriod)}: {DebouncePeriod}, {nameof(EventClock)}: {EventClock}, {nameof(OutputValue)}: {OutputValue}";
        }
    }
}
