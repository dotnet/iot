// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio
{
    /// <summary>
    /// Provides information about a GPIO chip
    /// </summary>
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
