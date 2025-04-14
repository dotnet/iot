// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Device.Gpio
{
    /// <summary>
    /// Provides information about a GPIO chip
    /// </summary>
    [Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
    public record GpioChipInfo(int Id, string Name, string Label, int NumLines)
    {
        /// <summary>
        /// Converts the whole snapshot to string
        /// </summary>
        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Label)}: {Label}, {nameof(NumLines)}: {NumLines}";
        }
    }
}
