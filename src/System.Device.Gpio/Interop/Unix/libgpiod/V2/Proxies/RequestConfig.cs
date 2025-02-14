// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// Request config objects are used to pass a set of options to the kernel at the time of the line request.
/// The mutators don't return error values. If the values are invalid, in general they are silently adjusted to acceptable ranges.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__request__config.html"/>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal class RequestConfig : LibGpiodProxyBase
{
    internal RequestConfigSafeHandle Handle { get; }

    /// <summary>
    /// Constructor for a request-config-proxy object. This call will create a new gpiod request-config object.
    /// </summary>
    /// <param name="handle">Safe handle to the libgpiod object.</param>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__request__config.html#gaca72f4c114efce4aa3909a3f71fb6c8e"/>
    public RequestConfig(RequestConfigSafeHandle handle)
        : base(handle)
    {
        Handle = handle;
    }

    /// <summary>
    /// Set the consumer name for the request
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__request__config.html#gaa0db92d603ba5150b15a3e985f2c62f8"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public void SetConsumer(string consumer)
    {
        CallLibgpiod(() => LibgpiodV2.gpiod_request_config_set_consumer(Handle, Marshal.StringToHGlobalAuto(consumer)));
    }

    /// <summary>
    /// Get the consumer name configured in the request config
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__request__config.html#ga70373b475c6505d5d755751ebee507ee"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public string GetConsumer()
    {
        return CallLibgpiod(() =>
        {
            IntPtr consumerPtr = LibgpiodV2.gpiod_request_config_get_consumer(Handle);
            return Marshal.PtrToStringAuto(consumerPtr) ?? string.Empty;
        });
    }

    /// <summary>
    /// Set the size of the kernel event buffer for the request
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__request__config.html#ga75f4f38735d08ebd7f07fa57c19442f6"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    /// <exception cref="ArgumentOutOfRangeException">Event buffer size is negative</exception>
    public void SetEventBufferSize(int eventBufferSize)
    {
        CallLibpiodLocked(() =>
        {
            if (eventBufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(eventBufferSize), "Event buffer size must be 0 or greater");
            }

            LibgpiodV2.gpiod_request_config_set_event_buffer_size(Handle, eventBufferSize);
        });
    }

    /// <summary>
    /// Get the edge event buffer size for the request config
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__request__config.html#ga1424773ef1ca72a0f622d20e701eafb7"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public int GetEventBufferSize()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_request_config_get_event_buffer_size(Handle));
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public Snapshot MakeSnapshot()
    {
        return new Snapshot(GetConsumer(), GetEventBufferSize());
    }

    /// <summary>
    /// Contains all readable information that was recorded at one and the same time
    /// </summary>
    public sealed record Snapshot(string Consumer, int EventBufferSize)
    {
        public override string ToString()
        {
            return $"{nameof(Consumer)}: {Consumer}, {nameof(EventBufferSize)}: {EventBufferSize}";
        }
    }
}
