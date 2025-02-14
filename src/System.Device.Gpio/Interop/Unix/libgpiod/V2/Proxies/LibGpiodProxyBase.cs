// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace System.Device.Gpio.Libgpiod.V2;

[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal abstract class LibGpiodProxyBase : IDisposable
{
    /// <remarks>
    /// Each proxy instance gets its dedicated lock instance.
    /// </remarks>
    private readonly object _safeHandleLock = new();

    protected LibGpiodProxyBase(SafeHandle handle)
    {
        SafeHandle = handle;
    }

    /// <summary>
    /// A safe handle to the libgpiod object
    /// </summary>
    protected SafeHandle SafeHandle { get; }

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
        lock (_safeHandleLock)
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
        lock (_safeHandleLock)
        {
            return CallLibgpiod(func);
        }
    }

    #region Dispose

    private bool _isDisposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposeManagedResources)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposeManagedResources)
        {
            // no managed resources to dispose (yet)
        }

        CallLibpiodLocked(SafeHandle.Dispose);

        _isDisposed = true;
    }

    #endregion
}
