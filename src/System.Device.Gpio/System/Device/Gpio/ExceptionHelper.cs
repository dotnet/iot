// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Runtime.InteropServices;

namespace System.Device.Gpio;

internal static class ExceptionHelper
{
    /// <summary>
    /// Gets the last P/Invoke error code and message.
    /// This should be called immediately after a P/Invoke call that failed.
    /// </summary>
    /// <returns>A formatted string containing the error code and human-readable message</returns>
    internal static string GetLastErrorMessage()
    {
        int errorCode = Marshal.GetLastWin32Error();
        string errorMessage = Marshal.GetLastPInvokeErrorMessage();

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return errorCode.ToString();
        }

        return $"{errorCode} ({errorMessage})";
    }

    /// <summary>
    /// Formats an error code with optional error message into a display string.
    /// </summary>
    private static string FormatError(int errorCode, string? errorMessage = null)
    {
        if (errorCode == -1)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return errorCode.ToString();
        }

        return $"{errorCode} ({errorMessage})";
    }

    public static IOException GetIOException(ExceptionResource resource, int errorCode = -1, int pin = -1)
    {
        return new IOException(GetResourceString(resource, errorCode, pin, null));
    }

    public static IOException GetIOException(ExceptionResource resource, string errorInfo, int pin = -1)
    {
        return new IOException(GetResourceString(resource, -1, pin, errorInfo));
    }

    public static InvalidOperationException GetInvalidOperationException(ExceptionResource resource, int pin = -1)
    {
        return new InvalidOperationException(GetResourceString(resource, -1, pin, null));
    }

    public static PlatformNotSupportedException GetPlatformNotSupportedException(ExceptionResource resource)
    {
        return new PlatformNotSupportedException(GetResourceString(resource, -1, -1, null));
    }

    public static ArgumentException GetArgumentException(ExceptionResource resource)
    {
        return new ArgumentException(GetResourceString(resource, -1, -1, null));
    }

    private static string GetResourceString(ExceptionResource resource, int errorCode, int pin, string? errorInfo)
    {
        // Use errorInfo if provided, otherwise format errorCode
        string formattedError = errorInfo ?? FormatError(errorCode);
        string errorDisplay = string.IsNullOrEmpty(formattedError) ? string.Empty : $", error: {formattedError}";

        return resource switch
        {
            ExceptionResource.NoChipIteratorFound => $"Unable to find a chip iterator{errorDisplay}",
            ExceptionResource.NoChipFound => $"Unable to find a chip{errorDisplay}",
            ExceptionResource.PinModeReadError => $"Error while reading pin mode for pin: {pin}",
            ExceptionResource.OpenPinError => $"Pin {pin} not available{errorDisplay}",
            ExceptionResource.ReadPinError => $"Error while reading value from pin: {pin}{errorDisplay}",
            ExceptionResource.PinNotOpenedError => $"Can not access pin {pin} that is not open.",
            ExceptionResource.SetPinModeError => $"Error setting pin mode, for pin: {pin}{errorDisplay}",
            ExceptionResource.ConvertPinNumberingSchemaError => $"This driver is generic so it cannot perform conversions between pin numbering schemes.",
            ExceptionResource.RequestEventError => $"Error while requesting event listener for pin {pin}{errorDisplay}",
            ExceptionResource.InvalidEventType => $"Invalid or not supported event type requested",
            ExceptionResource.EventWaitError => $"Error while waiting for event{errorDisplay}, pin: {pin}",
            ExceptionResource.EventReadError => $"Error while reading pin event result{errorDisplay}",
            ExceptionResource.NotListeningForEventError => $"Attempted to remove a callback for a pin that is not listening for events.",
            ExceptionResource.LibGpiodNotInstalled => $"Libgpiod driver not installed. More information on: https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/about/",
            _ => throw new Exception($"The ExceptionResource enum value: {resource} is not part of the switch. Add the appropriate case and exception message."),
        };
    }
}

internal enum ExceptionResource
{
    NoChipIteratorFound,
    NoChipFound,
    PinModeReadError,
    OpenPinError,
    ReadPinError,
    PinNotOpenedError,
    SetPinModeError,
    ConvertPinNumberingSchemaError,
    RequestEventError,
    InvalidEventType,
    EventWaitError,
    EventReadError,
    NotListeningForEventError,
    LibGpiodNotInstalled,
}
