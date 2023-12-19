// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;

namespace System.Device.Gpio.Libgpiod.V2;

internal abstract class LibGpiodProxyBase
{
    /// <summary>
    /// Each proxy instance gets its dedicated lock instance.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Helper function that wraps any exception (from a native call) in <see cref="GpiodException"/> for easier exception handling on client side.
    /// </summary>
    public static void CallLibgpiod(Action action)
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
    public static TResult CallLibgpiod<TResult>(Func<TResult> func)
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
    public void CallLibpiodLocked(Action action)
    {
        lock (_lock)
        {
            CallLibgpiod(action);
        }
    }

    /// <summary>
    /// Helper function that calls gpiod synchronized using a lock and wraps any exception (from a native call) in <see cref="GpiodException"/>
    /// for easier exception handling on client side.
    /// </summary>
    public TResult CallLibpiodLocked<TResult>(Func<TResult> func)
    {
        lock (_lock)
        {
            return CallLibgpiod(func);
        }
    }
}
