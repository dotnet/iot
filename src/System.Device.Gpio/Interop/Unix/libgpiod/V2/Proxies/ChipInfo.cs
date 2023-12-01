// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

using System.Device.Gpio.Interop.Unix.libgpiod.v2.Binding.Handles;
using System.Runtime.InteropServices;
using Libgpiodv2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Interop.Unix.libgpiod.v2.Proxies;

/// <summary>
/// The chip info contains all the publicly available information about a chip.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chip__info.html"/>
internal class ChipInfo : LibGpiodProxyBase
{
    private readonly ChipInfoSafeHandle _handle;

    /// <summary>
    /// Constructor for a chip-info-proxy object that points to an existing gpiod chip-info object using a safe handle.
    /// </summary>
    public ChipInfo(ChipInfoSafeHandle handle)
    {
        if (handle.IsInvalid)
        {
            throw new ArgumentOutOfRangeException(nameof(handle), "Invalid handle");
        }

        _handle = handle;
    }

    /// <summary>
    /// Get the name of the chip as represented in the kernel
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chip__info.html#gafa15c848b7dd866ea55b4b39da74d6ad"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public string GetName()
    {
        return TryCallGpiodLocked(() => Marshal.PtrToStringAuto(Libgpiodv2.gpiod_chip_info_get_name(_handle)) ??
            throw new GpiodException($"Could not get name from chip info: {LastErr.GetMsg()}"));
    }

    /// <summary>
    /// Get the label of the chip as represented in the kernel
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chip__info.html#ga6d9fa1a356c45ea3c2605667201aa769"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public string GetLabel()
    {
        return TryCallGpiodLocked(() => Marshal.PtrToStringAuto(Libgpiodv2.gpiod_chip_info_get_label(_handle)) ??
            throw new GpiodException($"Could not get label from chip info: {LastErr.GetMsg()}"));
    }

    /// <summary>
    /// Get the number of lines exposed by the chip
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__chip__info.html#gad7c1bdfa5d489e7ecd57a705d4fa217a"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public int GetNumLines()
    {
        return TryCallGpiodLocked(() => Libgpiodv2.gpiod_chip_info_get_num_lines(_handle));
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public Snapshot MakeSnapshot()
    {
        return new Snapshot(GetName(), GetLabel(), GetNumLines());
    }

    /// <summary>
    /// Contains all readable information that was recorded at one and the same time
    /// </summary>
    public sealed record Snapshot(string Name, string Label, int NumLines)
    {
        /// <summary>
        /// Converts the whole snapshot to string
        /// </summary>
        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Label)}: {Label}, {nameof(NumLines)}: {NumLines}";
        }
    }
}
