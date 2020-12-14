// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.IO;

namespace System.Device.Gpio
{
    internal static class ExceptionHelper
    {
        public static IOException GetIOException(ExceptionResource resource, int errorCode = -1, int pin = -1)
        {
            return new IOException(GetResourceString(resource, errorCode, pin));
        }

        public static InvalidOperationException GetInvalidOperationException(ExceptionResource resource, int pin = -1)
        {
            return new InvalidOperationException(GetResourceString(resource, -1, pin));
        }

        public static PlatformNotSupportedException GetPlatformNotSupportedException(ExceptionResource resource)
        {
            return new PlatformNotSupportedException(GetResourceString(resource, -1, -1));
        }

        public static ArgumentException GetArgumentException(ExceptionResource resource)
        {
            return new ArgumentException(GetResourceString(resource, -1, -1));
        }

        private static string GetResourceString(ExceptionResource resource, int errorCode, int pin) => resource switch
        {
            ExceptionResource.NoChipIteratorFound => $"Unable to find a chip iterator, error code: {errorCode}",
            ExceptionResource.NoChipFound => $"Unable to find a chip, error code: {errorCode}",
            ExceptionResource.PinModeReadError => $"Error while reading pin mode for pin: {pin}",
            ExceptionResource.OpenPinError => $"Pin {pin} not available, error: {errorCode}",
            ExceptionResource.ReadPinError => $"Error while reading value from pin: {pin}, error: {errorCode}",
            ExceptionResource.PinNotOpenedError => $"Can not access pin {pin} that is not open.",
            ExceptionResource.SetPinModeError => $"Error setting pin mode, for pin: {pin}, error: {errorCode}",
            ExceptionResource.ConvertPinNumberingSchemaError => $"This driver is generic so it cannot perform conversions between pin numbering schemes.",
            ExceptionResource.RequestEventError => $"Error while requesting event listener for pin {pin}, error code: {errorCode}",
            ExceptionResource.InvalidEventType => $"Invalid or not supported event type requested",
            ExceptionResource.EventWaitError => $"Error while waiting for event, error code {errorCode}, pin: {pin}",
            ExceptionResource.EventReadError => $"Error while reading pin event result, error code {errorCode}",
            ExceptionResource.NotListeningForEventError => $"Attempted to remove a callback for a pin that is not listening for events.",
            ExceptionResource.LibGpiodNotInstalled => $"Libgpiod driver not installed. More information on: https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/about/",
            _ => throw new Exception($"The ExceptionResource enum value: {resource} is not part of the switch. Add the appropriate case and exception message."),
        };
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
}
