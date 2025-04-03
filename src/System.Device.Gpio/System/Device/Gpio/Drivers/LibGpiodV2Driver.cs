// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Device.Gpio.Drivers;
using System.Device.Gpio.Libgpiod;
using System.Device.Gpio.Libgpiod.V2;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace System.Device.Gpio.Drivers;

/// <summary>
/// Driver that uses libgpiod V2 for GPIO control.
/// <remarks>
/// At the time of this writing, this driver is only available when compiling from source. See instructions at
/// https://libgpiod.readthedocs.io/en/latest/building.html.
/// </remarks>
/// </summary>
[Experimental(DiagnosticIds.SDGPIO0001, UrlFormat = DiagnosticIds.UrlFormat)]
public sealed class LibGpiodV2Driver : UnixDriver
{
    private static readonly string ConsumerId = $"C#-{nameof(LibGpiodV2Driver)}-{Process.GetCurrentProcess().Id}";

    private readonly Chip _chip;
    private readonly object _lockObject = new();
    private readonly Dictionary<Offset, RequestedLines> _requestedLineByLineOffset = new();
    private readonly CancellationTokenSource _disposalTokenSource = new();
    private readonly LibGpiodV2EventObserver _eventObserver;

    /// <summary>
    /// Creates a driver instance for the specified GPIO chip.
    /// </summary>
    /// <param name="chipNumber">Chip number to use.</param>
    public LibGpiodV2Driver(int chipNumber)
    {
        _chip = LibGpiodProxyFactory.CreateChip(chipNumber);
        _eventObserver = new LibGpiodV2EventObserver { WaitEdgeEventsTimeout = TimeSpan.FromMilliseconds(100) };
    }

    /// <summary>
    /// Construct an instance of this driver with the provided chip.
    /// </summary>
    /// <param name="chip">The chip to use. Should be one of the elements returned by <see cref="GetAvailableChips"/></param>
    public LibGpiodV2Driver(GpioChipInfo chip)
        : this(chip.Id)
    {
    }

    /// <inheritdoc/>
    protected internal override int PinCount
    {
        get
        {
            lock (_lockObject)
            {
                using var chipInfo = _chip.GetInfo();
                return chipInfo.GetNumLines();
            }
        }
    }

    /// <summary>
    /// Returns the list of available chips.
    /// </summary>
    /// <returns>A list of available chips. Can be used to determine the chipNumber when calling the constructor</returns>
    public static IList<GpioChipInfo> GetAvailableChips()
    {
        var ret = new List<GpioChipInfo>();
        var files = Directory.GetFiles("/dev", "gpiochip*", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < files.Length; i++)
        {
            string number = files[i].Replace("/dev/gpiochip", string.Empty);
            if (Int32.TryParse(number, CultureInfo.InvariantCulture, out int chipNumber))
            {
                var c = LibGpiodProxyFactory.CreateChip(chipNumber, files[i]);
                var info = c.GetInfo();
                ret.Add(new GpioChipInfo(chipNumber, info.GetName(), info.GetLabel(), info.GetNumLines()));
                c.Dispose();
            }
        }

        return ret;
    }

    /// <inheritdoc/>
    protected internal override int ConvertPinNumberToLogicalNumberingScheme(int lineOffset)
    {
        throw new NotSupportedException($"{nameof(LibGpiodV2Driver)} uses GPIO line numbering. For more information please refer to " +
            $"https://docs.kernel.org/driver-api/gpio/using-gpio.html or consider using the gpioinfo cmd line tool provided by libgpiod to " +
            $"find out more about present GPIO lines on the system");
    }

    /// <inheritdoc/>
    /// <remarks>This only requests the line for it to be reserved and the consumer to be set.</remarks>
    protected internal override void OpenPin(int lineOffset)
    {
        Offset offset = lineOffset;
        CreateLineRequestForSingleOffset(offset, LibGpiodProxyFactory.CreateLineSettings);
    }

    /// <inheritdoc/>
    /// <remarks>Un-reserves a line by closing a present line request.</remarks>
    protected internal override void ClosePin(int lineOffset)
    {
        Offset offset = lineOffset;

        lock (_lockObject)
        {
            if (!_requestedLineByLineOffset.TryGetValue(offset, out var requestedLines))
            {
                return;
            }

            requestedLines.LineRequest.Dispose();
            _eventObserver.RemoveSubscriptions(new[] { offset });
            _requestedLineByLineOffset.Remove(offset);
        }
    }

    /// <inheritdoc/>
    /// <remarks>Sets direction of line by reconfiguring a present line request or creating new line request.</remarks>
    protected internal override void SetPinMode(int lineOffset, PinMode mode)
    {
        Offset offset = lineOffset;

        (GpiodLineDirection? direction, GpiodLineBias? bias) = Translator.Translate(mode);

        void ChangeExistingLineSettings(LineSettings lineSettings)
        {
            if (direction != null)
            {
                lineSettings.SetDirection(direction.Value);
                if (direction == GpiodLineDirection.Output)
                {
                    // gpiod rejects request when direction is output and edge detection is not none
                    lineSettings.SetEdgeDetection(GpiodLineEdge.None);
                }
            }
            else if (bias != null)
            {
                lineSettings.SetBias(bias.Value);
            }
        }

        lock (_lockObject)
        {
            if (_requestedLineByLineOffset.TryGetValue(offset, out RequestedLines? requestedLines))
            {
                GpiodLineDirection gpiodLineDirection = requestedLines.LineConfig.GetLineSettings(offset).GetDirection();
                if (gpiodLineDirection == direction)
                {
                    return;
                }

                ReconfigureExistingRequest(requestedLines, offset, ChangeExistingLineSettings);
                return;
            }

            CreateLineRequestForSingleOffset(offset, () =>
            {
                var newLineSettings = LibGpiodProxyFactory.CreateLineSettings();
                ChangeExistingLineSettings(newLineSettings);
                return newLineSettings;
            });
        }
    }

    /// <inheritdoc/>
    protected internal override PinMode GetPinMode(int lineOffset)
    {
        Offset offset = lineOffset;

        LineInfo lineInfo;

        lock (_lockObject)
        {
            lineInfo = _chip.GetLineInfo(offset);
        }

        var direction = lineInfo.GetDirection();
        switch (direction)
        {
            case GpiodLineDirection.Input:
            {
                var bias = lineInfo.GetBias();
                return bias switch
                {
                    GpiodLineBias.AsIs => PinMode.Input,
                    GpiodLineBias.Unknown => PinMode.Input,
                    GpiodLineBias.Disabled => PinMode.Input,
                    GpiodLineBias.PullUp => PinMode.InputPullUp,
                    GpiodLineBias.PullDown => PinMode.InputPullDown,
                    _ => throw new GpiodException($"Bias '{bias}' out of range")
                };
            }

            case GpiodLineDirection.Output:
                return PinMode.Output;
            case GpiodLineDirection.AsIs:
            default:
                throw new GpiodException($"Could not get pin mode of line '{offset}'");
        }
    }

    /// <inheritdoc/>
    protected internal override bool IsPinModeSupported(int lineOffset, PinMode mode)
    {
        return mode is PinMode.Input or PinMode.Output or PinMode.InputPullDown or PinMode.InputPullUp;
    }

    /// <inheritdoc/>
    /// <remarks>Gets value by asking present line request or creating a new line request.</remarks>
    protected internal override PinValue Read(int lineOffset)
    {
        Offset offset = lineOffset;

        lock (_lockObject)
        {
            if (_requestedLineByLineOffset.TryGetValue(offset, out RequestedLines? requestedLines))
            {
                GpiodLineValue gpiodLineValue = requestedLines.LineRequest.GetValue(offset);
                if (gpiodLineValue == GpiodLineValue.Error)
                {
                    throw new GpiodException($"Could not read value of offset '{offset}' because it is in error state");
                }

                return Translator.Translate(gpiodLineValue, offset);
            }

            LineRequest request = CreateLineRequestForSingleOffset(offset, () =>
            {
                var newLineSettings = LibGpiodProxyFactory.CreateLineSettings();
                newLineSettings.SetDirection(GpiodLineDirection.Input);
                return newLineSettings;
            });

            GpiodLineValue lineValue = request.GetValue(offset);

            return Translator.Translate(lineValue, offset);
        }
    }

    /// <inheritdoc/>
    /// <remarks>Writes value by accessing present line request or creating a new line request.</remarks>
    protected internal override void Write(int lineOffset, PinValue value)
    {
        Offset offset = lineOffset;
        GpiodLineValue lineValue = Translator.Translate(value);

        lock (_lockObject)
        {
            if (_requestedLineByLineOffset.TryGetValue(offset, out RequestedLines? requestedLines))
            {
                GpiodLineDirection lineDirection = requestedLines.SettingsByLine[offset].GetDirection();
                if (lineDirection != GpiodLineDirection.Output)
                {
                    void SetValue(LineSettings lineSettings)
                    {
                        lineSettings.SetDirection(GpiodLineDirection.Output);
                        lineSettings.SetOutputValue(lineValue);
                    }

                    ReconfigureExistingRequest(requestedLines, offset, SetValue);
                }
                else
                {
                    requestedLines.LineRequest.SetValue(offset, lineValue);
                }

                return;
            }

            CreateLineRequestForSingleOffset(offset, () =>
            {
                var newLineSettings = LibGpiodProxyFactory.CreateLineSettings();
                newLineSettings.SetDirection(GpiodLineDirection.Output);
                newLineSettings.SetOutputValue(Translator.Translate(value));
                return newLineSettings;
            });
        }
    }

    /// <inheritdoc/>
    /// <remarks>Waits for an edge event to happen on a line request. If line request does not exist it gets created.</remarks>
    protected internal override WaitForEventResult WaitForEvent(int lineOffset, PinEventTypes eventTypes, CancellationToken cancellationToken)
    {
        Offset offset = lineOffset;

        var adjustLineSettings = (LineSettings lineSettings) =>
        {
            GpiodLineEdge presentEdgeDetection = lineSettings.GetEdgeDetection();
            lineSettings.SetEdgeDetection(Translator.Combine(eventTypes, presentEdgeDetection));
            lineSettings.SetEventClock(GpiodLineClock.Monotonic);
        };

        LineRequest lineRequest;

        lock (_lockObject)
        {
            if (_requestedLineByLineOffset.TryGetValue(offset, out RequestedLines? requestedLines))
            {
                GpiodLineDirection configuredDirection = requestedLines.SettingsByLine[offset].GetDirection();
                if (configuredDirection == GpiodLineDirection.Output)
                {
                    throw new GpiodException($"Offset '{offset}' is configured as output, only inputs can be monitored");
                }

                lineRequest = ReconfigureExistingRequest(requestedLines, offset, adjustLineSettings);
            }
            else
            {
                lineRequest = CreateLineRequestForSingleOffset(offset, () =>
                {
                    var newLineSettings = LibGpiodProxyFactory.CreateLineSettings();
                    adjustLineSettings.Invoke(newLineSettings);
                    return newLineSettings;
                });
            }
        }

        var eventSubscription = new LibGpiodV2EventObserver.EventSubscription(offset, Translator.Translate(eventTypes));

        LibGpiodV2EventObserver.EventWaitHandle eventWaitHandle = _eventObserver.ObserveSingleEvent(lineRequest, eventSubscription);

        return eventWaitHandle.WaitForEventResult(cancellationToken);
    }

    /// <inheritdoc/>
    /// <remarks>Adds a callback for edge events. If a line request is present for the specified line offset, it is adjusted.</remarks>
    protected internal override void AddCallbackForPinValueChangedEvent(int lineOffset, PinEventTypes eventTypes, PinChangeEventHandler callback)
    {
        Offset offset = lineOffset;

        void AdjustEdgeDetection(LineSettings lineSettings)
        {
            GpiodLineEdge presentEdgeDetection = lineSettings.GetEdgeDetection();
            lineSettings.SetEdgeDetection(Translator.Combine(eventTypes, presentEdgeDetection));
            lineSettings.SetEventClock(GpiodLineClock.Monotonic);
        }

        lock (_lockObject)
        {
            if (_requestedLineByLineOffset.TryGetValue(offset, out var requestedLines))
            {
                LineSettings lineSettings = requestedLines.SettingsByLine[offset];

                if (lineSettings.GetDirection() == GpiodLineDirection.Output)
                {
                    throw new GpiodException($"Offset '{offset}' is configured as output, only inputs can be monitored");
                }

                bool reconfigurationRequired = !Translator.Includes(lineSettings.GetEdgeDetection(), eventTypes);

                LineRequest request = requestedLines.LineRequest;

                if (reconfigurationRequired)
                {
                    request = ReconfigureExistingRequest(requestedLines, offset, AdjustEdgeDetection);
                }

                _eventObserver.Observe(request, new LibGpiodV2EventObserver.EventSubscription(offset, Translator.Translate(eventTypes)),
                    callback);
            }
        }
    }

    /// <inheritdoc/>
    /// <remarks>Removes a callback for edge events. If a line request is present for the specified line offset, it is adjusted.</remarks>
    protected internal override void RemoveCallbackForPinValueChangedEvent(int lineOffset, PinChangeEventHandler callback)
    {
        Offset offset = lineOffset;

        lock (_lockObject)
        {
            _eventObserver.RemoveCallback(offset, callback);

            if (_eventObserver.HasSubscriptionsFor(offset))
            {
                return;
            }

            void RemoveEdgeDetection(LineSettings lineSettings)
            {
                lineSettings.SetEdgeDetection(GpiodLineEdge.None);
            }

            if (_requestedLineByLineOffset.TryGetValue(offset, out var requestedLines))
            {
                ReconfigureExistingRequest(requestedLines, offset, RemoveEdgeDetection);
            }
        }
    }

    private LineRequest CreateLineRequestForSingleOffset(Offset offset, Func<LineSettings> lineSettingsFactory)
    {
        lock (_lockObject)
        {
            using RequestConfig requestConfig = LibGpiodProxyFactory.CreateRequestConfig();
            requestConfig.SetConsumer(ConsumerId);
            var lineSettings = lineSettingsFactory.Invoke();
            LineConfig lineConfig = LibGpiodProxyFactory.CreateLineConfig();
            lineConfig.AddLineSettings(offset, lineSettings);
            _eventObserver.EnrichLineConfigWithPresentEventSubscriptions(lineConfig);

            LineRequest request = _chip.RequestLines(requestConfig, lineConfig);

            _requestedLineByLineOffset[offset] =
                new RequestedLines(lineConfig, new Dictionary<Offset, LineSettings> { { offset, lineSettings } }, request);

            return request;
        }
    }

    /// <summary>
    /// Changes a field of an existing request object by
    ///     1. making a copy of the settings of the target offset (to not interfere with other offsets that may use the same setting)
    ///     2. changing the field on these settings
    ///     3. getting other line settings for other offsets, grouping them together by setting (to make as least calls to AddLineSettings as possible)
    ///     4. resetting current line config object, and assigning new settings
    ///     5. reconfiguring request object with new line config
    /// </summary>
    private LineRequest ReconfigureExistingRequest(RequestedLines requestedLines, Offset offset, Action<LineSettings> changeExistingLineSettings)
    {
        IReadOnlyDictionary<Offset, LineSettings> settingsByLine = requestedLines.SettingsByLine;

        var targetLineSettings = settingsByLine[offset];

        // 1.
        LineSettings lineSettingsCopy = targetLineSettings.Copy();

        // 2.
        changeExistingLineSettings.Invoke(lineSettingsCopy);

        // 3.
        var anyOtherLineSettings = settingsByLine.Where(kv => kv.Key != offset).ToDictionary(kv => kv.Key, kv => kv.Value);
        Dictionary<LineSettings, List<Offset>> distinctOtherSettings = new();
        foreach (var otherLineSetting in anyOtherLineSettings)
        {
            var os = otherLineSetting.Key;
            var ls = otherLineSetting.Value;
            if (distinctOtherSettings.TryGetValue(ls, out var offsetGroup))
            {
                offsetGroup.Add(os);
            }
            else
            {
                distinctOtherSettings[ls] = new List<Offset> { os };
            }
        }

        // 4.
        LineConfig lineConfig = requestedLines.LineConfig;
        lineConfig.Reset();
        lineConfig.AddLineSettings(offset, lineSettingsCopy);
        foreach (var offsetGroupSettings in distinctOtherSettings)
        {
            lineConfig.AddLineSettings(offsetGroupSettings.Value.ToArray(), offsetGroupSettings.Key);
        }

        // sub step: In case direction is input, set edge detection
        _eventObserver.EnrichLineConfigWithPresentEventSubscriptions(lineConfig);

        // 5.
        LineRequest lineRequest = requestedLines.LineRequest;
        lineRequest.ReconfigureLines(lineConfig);

        // update driver cache
        _requestedLineByLineOffset[offset] = new RequestedLines(lineConfig, lineConfig.GetSettingsByLine(), lineRequest);

        return lineRequest;
    }

    /// <inheritdoc />
    public override GpioChipInfo GetChipInfo()
    {
        var info = _chip.GetInfo();
        return new GpioChipInfo(info.ChipNumber, info.GetName(), info.GetLabel(), info.GetNumLines());
    }

    #region Dispose

    private bool _isDisposed;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            lock (_lockObject)
            {
                if (_isDisposed)
                {
                    return;
                }

                foreach (var requestedLines in _requestedLineByLineOffset.Select(x => x.Value).Distinct())
                {
                    requestedLines.LineRequest.Dispose();
                    requestedLines.LineConfig.Dispose();
                    foreach (var lineSettings in requestedLines.SettingsByLine.Values)
                    {
                        lineSettings.Dispose();
                    }
                }

                _requestedLineByLineOffset.Clear();

                _eventObserver.Dispose();

                _chip.Dispose();

                _disposalTokenSource.Cancel();
                _disposalTokenSource.Dispose();

                _isDisposed = true;
            }
        }
    }

    #endregion

    private sealed record RequestedLines(LineConfig LineConfig, IReadOnlyDictionary<Offset, LineSettings> SettingsByLine, LineRequest LineRequest);
}
