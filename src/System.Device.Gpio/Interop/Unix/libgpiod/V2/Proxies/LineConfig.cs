// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.Gpio.Drivers;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// The line-config object contains the configuration for lines that can be used in two cases:
/// <list type="number">
///     <item><description>when making a line request</description></item>
///     <item><description>when reconfiguring a set of already requested lines</description></item>
/// </list>
/// A new line-config object is empty. Using it in a request will lead to an error. In order to a line-config to become useful, it needs to be
/// assigned at least one offset-to-settings mapping by calling gpiod_line_config_add_line_settings.
/// When calling gpiod_chip_request_lines, the library will request all offsets that were assigned settings in the order that they were assigned.
/// If any of the offsets was duplicated, the last one will take precedence.
/// </summary>
/// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__config.html"/>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
internal class LineConfig : LibGpiodProxyBase
{
    internal LineConfigSafeHandle Handle { get; }

    /// <summary>
    /// Constructor for a line-config-proxy object. This call will create a new gpiod line-config object.
    /// </summary>
    /// <param name="handle">Safe handle to the libgpiod object.</param>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__config.html#ga17ebc375388c6588fc96cf0f069d58b3"/>
    public LineConfig(LineConfigSafeHandle handle)
        : base(handle)
    {
        Handle = handle;
    }

    /// <summary>
    /// Reset the line config object
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__config.html#gaf4a8d7fba169608fe39cacdb8871c5bc"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public void Reset()
    {
        CallLibpiodLocked(() => LibgpiodV2.gpiod_line_config_reset(Handle));
    }

    /// <summary>
    /// Add line settings for a set of offsets
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__config.html#ga4323e3c73aa52bbbd285d581f5dcaeee"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public void AddLineSettings(Offset[] offsets, LineSettings lineSettings)
    {
        CallLibpiodLocked(() => LibgpiodV2.gpiod_line_config_add_line_settings(Handle, offsets.Convert(), offsets.Length, lineSettings.Handle));
    }

    /// <summary>
    /// Add line settings for an offsets
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__config.html#ga4323e3c73aa52bbbd285d581f5dcaeee"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public void AddLineSettings(Offset offset, LineSettings lineSettings)
    {
        AddLineSettings(new[] { offset }, lineSettings);
    }

    /// <summary>
    /// Get line settings for offset
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__config.html#ga692d9e29a469810ea1b1c90f6353c187"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public LineSettings GetLineSettings(Offset offset)
    {
        return CallLibgpiod(() => new LineSettings(LibgpiodV2.gpiod_line_config_get_line_settings(Handle, offset)));
    }

    /// <summary>
    /// Set output values for a number of lines
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__config.html#gaff8e6b7614ab69cbed81a6279cf7d1ba"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public void SetOutputValues(IEnumerable<GpiodLineValue> values)
    {
        CallLibpiodLocked(() =>
        {
            var valArr = values.ToArray();
            int result = LibgpiodV2.gpiod_line_config_set_output_values(Handle, valArr, valArr.Length);
            if (result < 0)
            {
                throw new GpiodException($"Could not set output values: {LastErr.GetMsg()}");
            }
        });
    }

    /// <summary>
    /// Get the number of configured line offsets
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__config.html#ga209df9d3c45b9cdcd683d3f96cf49e6f"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public int GetNumConfiguredOffsets()
    {
        return CallLibgpiod(() => LibgpiodV2.gpiod_line_config_get_num_configured_offsets(Handle));
    }

    /// <summary>
    /// Get configured offsets
    /// </summary>
    /// <seealso href="https://libgpiod.readthedocs.io/en/latest/group__line__config.html#ga36ca8a2ce78a9294d2336b4d9b7c0c17"/>
    /// <exception cref="GpiodException">Unexpected error invoking native function</exception>
    public IEnumerable<Offset> GetConfiguredOffsets()
    {
        return CallLibgpiod(() =>
        {
            int numConfiguredOffsets = GetNumConfiguredOffsets();
            uint[] configuredOffsets = new uint[numConfiguredOffsets];
            int nStored = LibgpiodV2.gpiod_line_config_get_configured_offsets(Handle, configuredOffsets, configuredOffsets.Length);
            Array.Resize(ref configuredOffsets, nStored);
            return configuredOffsets.Convert();
        });
    }

    /// <summary>
    /// Gets which lines has which settings.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public IReadOnlyDictionary<Offset, LineSettings> GetSettingsByLine()
    {
        var configuredOffsets = GetConfiguredOffsets();
        return configuredOffsets.ToDictionary(offset => offset, GetLineSettings);
    }

    /// <summary>
    /// Helper function for capturing information and creating an immutable snapshot instance.
    /// </summary>
    /// <exception cref="GpiodException">Unexpected error when invoking native function</exception>
    public Snapshot MakeSnapshot()
    {
        var configuredOffsets = GetConfiguredOffsets();
        var configuredLineSettingsSnapshots = configuredOffsets.ToDictionary(offset => offset, offset => GetLineSettings(offset).MakeSnapshot());
        return new Snapshot(GetNumConfiguredOffsets(), configuredLineSettingsSnapshots);
    }

    /// <summary>
    /// Contains all readable information that was recorded at one and the same time
    /// </summary>
    public sealed record Snapshot(int NumConfiguredOffsets, IReadOnlyDictionary<Offset, LineSettings.Snapshot> ConfiguredLineSettings)
    {
        /// <summary>
        /// Converts the whole snapshot to string
        /// </summary>
        public override string ToString()
        {
            return
                $"{nameof(NumConfiguredOffsets)}: {NumConfiguredOffsets}, {nameof(ConfiguredLineSettings)}: {string.Join("\n", ConfiguredLineSettings)}";
        }
    }
}
