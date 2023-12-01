// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace System.Device.Gpio.Interop.Unix.libgpiod.V2.Proxies;

internal abstract class LibGpiodProxyBase
{
    /// <summary>
    /// Each proxy instance gets its dedicated lock instance.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Helper function that wraps any exception (from a native call) in <see cref="GpiodException"/> for easier exception handling on client side.
    /// </summary>
    protected static void TryCallGpiod(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception e) when (e is not GpiodException)
        {
            throw new GpiodException("Exception while calling libgpiod", e);
        }
    }

    /// <summary>
    /// Helper function that wraps any exception (from a native call) in <see cref="GpiodException"/> for easier exception handling on client side.
    /// </summary>
    protected static TResult TryCallGpiod<TResult>(Func<TResult> func)
    {
        try
        {
            return func.Invoke();
        }
        catch (Exception e) when (e is not GpiodException)
        {
            throw new GpiodException("Exception while calling libgpiod", e);
        }
    }

    /// <summary>
    /// Helper function that calls gpiod synchronized using a lock and wraps any exception (from a native call) in <see cref="GpiodException"/>
    /// for easier exception handling on client side.
    /// </summary>
    protected void TryCallGpiodLocked(Action action)
    {
        lock (_lock)
        {
            TryCallGpiod(action);
        }
    }

    /// <summary>
    /// Helper function that calls gpiod synchronized using a lock and wraps any exception (from a native call) in <see cref="GpiodException"/>
    /// for easier exception handling on client side.
    /// </summary>
    protected TResult TryCallGpiodLocked<TResult>(Func<TResult> func)
    {
        lock (_lock)
        {
            return TryCallGpiod(func);
        }
    }
}
