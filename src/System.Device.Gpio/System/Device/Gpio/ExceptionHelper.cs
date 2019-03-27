// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

        private static string GetResourceString(ExceptionResource resource, int errorCode, int pin)
        {
            string message = "";
            switch (resource)
            {
                case ExceptionResource.NoChipIteratorFound:
                    message = $"Unable to find a chip iterator, error code: {errorCode}";
                    break;
                case ExceptionResource.NoChipFound:
                    message = $"Unable to find a chip, error code: {errorCode}";
                    break;
                case ExceptionResource.PinModeReadError:
                    message = $"Error while reading pin mode for pin: {pin}";
                    break;
                case ExceptionResource.OpenPinError:
                    message = $"Pin {pin} not available, error: {errorCode}";
                    break;
                case ExceptionResource.ReadPinError:
                    message = $"Error while reading value from pin: {pin}, error: {errorCode}";
                    break;
                case ExceptionResource.PinNotOpenedError:
                    message = $"Can not access pin {pin} that is not open.";
                    break;
                case ExceptionResource.SetPinModeError:
                    message = $"Error setting pin mode, for pin: {pin}, error: {errorCode}";
                    break;
                case ExceptionResource.ConvertPinNumberingSchemaError:
                    message = $"This driver is generic so it cannot perform conversions between pin numbering schemes.";
                    break;
                case ExceptionResource.RequestEventError:
                    message = $"Error while requesting event listener for pin {pin}, error code: {errorCode}";
                    break;
                case ExceptionResource.InvalidEventType:
                    message = $"Invalid or not supported event type requested";
                    break;
                case ExceptionResource.EventWaitError:
                    message = $"Error while waiting for event, error code {errorCode}, pin: {pin}";
                    break;
                case ExceptionResource.EventReadError:
                    message = $"Error while reading pin event result, error code {errorCode}";
                    break;
                case ExceptionResource.NotListeningForEventError:
                    message = $"Attempted to remove a callback for a pin that is not listening for events.";
                    break;
                case ExceptionResource.LibGpiodNotInstalled:
                    message = $"Libgpiod driver not installed. More information on: https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/about/";
                    break;
                default:
                    Debug.Fail($"The ExceptionResource enum value: {resource} is not part of the switch. Add the appropriate case and exception message.");
                    break;
            }

            return message;
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
}
