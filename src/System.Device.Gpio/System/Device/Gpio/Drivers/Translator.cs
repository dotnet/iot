// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Device.Gpio.Libgpiod.V2;
using System.Diagnostics.CodeAnalysis;

namespace System.Device.Gpio.Drivers;

[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal static class Translator
{
    public static (GpiodLineDirection? _direction, GpiodLineBias? _bias) Translate(PinMode pinMode)
    {
        return pinMode switch
        {
            PinMode.Input => (GpiodLineDirection.Input, null),
            PinMode.Output => (GpiodLineDirection.Output, null),
            PinMode.InputPullDown => (null, GpiodLineBias.PullDown),
            PinMode.InputPullUp => (null, GpiodLineBias.PullUp),
            _ => throw new ArgumentOutOfRangeException(nameof(pinMode), pinMode, null)
        };
    }

    public static PinValue Translate(GpiodLineValue gpiodLineValue, Offset offset)
    {
        return gpiodLineValue switch
        {
            GpiodLineValue.Inactive => PinValue.Low,
            GpiodLineValue.Active => PinValue.High,
            GpiodLineValue.Error => throw new GpiodException($"Could not read value of offset '{offset}' because it is in error state"),
            _ => throw new ArgumentOutOfRangeException(nameof(gpiodLineValue), gpiodLineValue, null)
        };
    }

    public static GpiodLineValue Translate(PinValue pinValue)
    {
        return pinValue == PinValue.High ? GpiodLineValue.Active : GpiodLineValue.Inactive;
    }

    public static GpiodLineEdge Translate(PinEventTypes pinEventTypes)
    {
        if (pinEventTypes.HasFlag(PinEventTypes.Falling) && pinEventTypes.HasFlag(PinEventTypes.Rising))
        {
            return GpiodLineEdge.Both;
        }

        if (pinEventTypes.HasFlag(PinEventTypes.Falling))
        {
            return GpiodLineEdge.Falling;
        }

        if (pinEventTypes.HasFlag(PinEventTypes.Rising))
        {
            return GpiodLineEdge.Rising;
        }

        return GpiodLineEdge.None;
    }

    public static GpiodLineEdge Combine(PinEventTypes pinEventTypes, GpiodLineEdge presentEdgeDetection)
    {
        if (pinEventTypes.HasFlag(PinEventTypes.Falling) && pinEventTypes.HasFlag(PinEventTypes.Rising))
        {
            return GpiodLineEdge.Both;
        }

        if (pinEventTypes.HasFlag(PinEventTypes.Falling))
        {
            return presentEdgeDetection == GpiodLineEdge.Rising ? GpiodLineEdge.Both : GpiodLineEdge.Falling;
        }

        if (pinEventTypes.HasFlag(PinEventTypes.Rising))
        {
            return presentEdgeDetection == GpiodLineEdge.Falling ? GpiodLineEdge.Both : GpiodLineEdge.Rising;
        }

        return GpiodLineEdge.None;
    }

    /// <summary>
    /// Returns whether <paramref name="presentEdgeDetection"/> includes <paramref name="pinEventTypes"/>.
    /// </summary>
    /// <returns></returns>
    public static bool Includes(GpiodLineEdge presentEdgeDetection, PinEventTypes pinEventTypes)
    {
        if (pinEventTypes == PinEventTypes.None || presentEdgeDetection == GpiodLineEdge.Both)
        {
            return true;
        }

        if (presentEdgeDetection == GpiodLineEdge.None)
        {
            return false;
        }

        return (pinEventTypes == PinEventTypes.Rising && presentEdgeDetection == GpiodLineEdge.Rising) ||
            (pinEventTypes == PinEventTypes.Falling && presentEdgeDetection == GpiodLineEdge.Falling);
    }

    public static PinEventTypes Translate(GpiodEdgeEventType gpiodEdgeEventType)
    {
        return gpiodEdgeEventType switch
        {
            GpiodEdgeEventType.RisingEdge => PinEventTypes.Rising,
            GpiodEdgeEventType.FallingEdge => PinEventTypes.Falling,
            _ => throw new ArgumentOutOfRangeException(nameof(gpiodEdgeEventType), gpiodEdgeEventType, null)
        };
    }
}
