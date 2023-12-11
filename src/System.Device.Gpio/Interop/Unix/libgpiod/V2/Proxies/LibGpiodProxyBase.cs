// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    public static void TryCallGpiod(Action action)
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
    public static TResult TryCallGpiod<TResult>(Func<TResult> func)
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
    public void TryCallGpiodLocked(Action action)
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
    public TResult TryCallGpiodLocked<TResult>(Func<TResult> func)
    {
        lock (_lock)
        {
            return TryCallGpiod(func);
        }
    }
}
