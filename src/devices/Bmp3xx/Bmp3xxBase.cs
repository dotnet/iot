// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Model;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Bmp3xx
{
    /// <summary>
    /// Base class for the Bosch BMP3xx family of barometric pressure and temperature sensors (BMP390, BMP388).
    /// </summary>
    /// <remarks>
    /// Section and table numbers in this binding refer to the BMP390 datasheet (BST-BMP390-DS002);
    /// the BMP388 datasheet (BST-BMP388-DS001) describes the same registers.
    /// </remarks>
    [Interface("Represents a Bosch BMP3xx barometric pressure and temperature sensor.")]
    public abstract class Bmp3xxBase : IDisposable
    {
        /// <summary>
        /// The default I2C address, used when the SDO pin is connected to VDDIO (section 5.2).
        /// </summary>
        public const byte DefaultI2cAddress = 0x77;

        /// <summary>
        /// The secondary I2C address, used when the SDO pin is connected to GND (section 5.2).
        /// </summary>
        public const byte SecondaryI2cAddress = 0x76;

        // Register bit fields.

        // PWR_CTRL (0x1B), section 4.3.17, table 42: bit 0 press_en, bit 1 temp_en, bits 5..4 mode.
        private const byte PressureEnableBit = 0x01;
        private const byte TemperatureEnableBit = 0x02;
        private const int PowerModeShift = 4;
        private const byte PowerModeMask = 0b0011_0000;

        // OSR (0x1C), section 4.3.18, table 43: bits 2..0 osr_p, bits 5..3 osr_t.
        private const int PressureOversamplingShift = 0;
        private const byte PressureOversamplingMask = 0b0000_0111;
        private const int TemperatureOversamplingShift = 3;
        private const byte TemperatureOversamplingMask = 0b0011_1000;

        // ODR (0x1D), section 4.3.19, table 44: bits 4..0 odr_sel.
        private const byte OutputDataRateMask = 0b0001_1111;

        // CONFIG (0x1F), section 4.3.21, table 46: bits 3..1 iir_filter.
        private const int FilterShift = 1;
        private const byte FilterMask = 0b0000_1110;

        // EVENT (0x10), section 4.3.8, table 33: bit 0 por_detected, set after a power-on reset or soft reset.
        private const byte PowerOnResetDetectedBit = 0x01;

        // CMD (0x7E), section 4.3.23, table 48: the soft reset command.
        private const byte SoftResetCommand = 0xB6;

        // Timing.

        // Start-up time, the only duration the datasheet gives for a reset (section 1, table 2: t_startup = 2 ms).
        private const int StartupTimeMilliseconds = 2;

        // Upper bound for waiting on a status or event flag that the datasheet gives no duration for.
        private const int FlagTimeoutMilliseconds = 10;

        // Measurement time (section 3.9.2): 234 µs + press_en * (392 µs + 2^osr_p * T) + temp_en * (S + 2^osr_t * T).
        // The BMP390 datasheet gives T = 2020 µs and S = 163 µs; the BMP388 datasheet gives T = 2000 µs and S = 313 µs.
        private const int MeasurementBaseMicroseconds = 234;
        private const int PressureSettleMicroseconds = 392;
        private const int Bmp390ConversionMicroseconds = 2020;
        private const int Bmp390TemperatureSettleMicroseconds = 163;
        private const int Bmp388ConversionMicroseconds = 2000;
        private const int Bmp388TemperatureSettleMicroseconds = 313;
        private const byte Bmp388ChipId = 0x50;

        // Waiting for a forced measurement: poll every millisecond, give up after twice the measurement time plus a margin.
        private const int PollIntervalMilliseconds = 1;
        private const int TimeoutMarginMilliseconds = 10;

        // Data registers (section 4.3.5 and 4.3.6): 3 bytes of pressure, then 3 bytes of temperature, least significant byte first.
        private const int DataLength = 6;

        // Operating range (section 1, table 2). Values outside it are reported as invalid.
        private const double MinimumTemperatureCelsius = -40;
        private const double MaximumTemperatureCelsius = 85;
        private const double MinimumPressurePascals = 30_000;
        private const double MaximumPressurePascals = 125_000;

        private readonly int _conversionMicroseconds;
        private readonly int _temperatureSettleMicroseconds;
        private I2cDevice? _i2cDevice;

        // The settings as last written, so they can be re-applied after a reset.
        private Bmp3xxOversampling _pressureSampling = Bmp3xxOversampling.X1;
        private Bmp3xxOversampling _temperatureSampling = Bmp3xxOversampling.X1;
        private Bmp3xxFilterCoefficient _filterCoefficient = Bmp3xxFilterCoefficient.Off;
        private Bmp3xxOutputDataRate _outputDataRate = Bmp3xxOutputDataRate.Period5Milliseconds;

        // The longest measurement time of the settings since normal or forced mode was last entered from sleep: a
        // measurement started before an oversampling change may still be running when the mode is left.
        private int _longestPendingMeasurementMilliseconds;

        // Whether the data registers hold a measurement made since the last reset. The data itself can't tell: after a
        // reset it reads 0x800000 (table 25), which can also be a genuine measurement. Only measurements this instance
        // observes count, so one made and read out through another connection to the sensor isn't seen.
        private bool _measurementAvailable;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bmp3xxBase"/> class.
        /// Checks the chip ID, resets the sensor, reads its calibration data, and configures it with
        /// x1 oversampling for pressure and temperature, the filter off, a 5 ms output data rate,
        /// both measurements enabled and <see cref="Bmp3xxPowerMode.Sleep"/> mode.
        /// </summary>
        /// <param name="chipId">The chip ID expected in the CHIP_ID register.</param>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> used to communicate with the sensor. It is disposed with this instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="i2cDevice"/> is null.</exception>
        /// <exception cref="IOException">The chip ID doesn't match, or the reset failed.</exception>
        protected Bmp3xxBase(byte chipId, I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            byte foundChipId = ReadRegister(Bmp3xxRegister.ChipId);
            if (foundChipId != chipId)
            {
                throw new IOException($"Unable to find a chip with id 0x{chipId:X2}. Found one with id 0x{foundChipId:X2}.");
            }

            bool isBmp388 = chipId == Bmp388ChipId;
            _conversionMicroseconds = isBmp388 ? Bmp388ConversionMicroseconds : Bmp390ConversionMicroseconds;
            _temperatureSettleMicroseconds = isBmp388 ? Bmp388TemperatureSettleMicroseconds : Bmp390TemperatureSettleMicroseconds;

            Reset();

            Span<byte> calibration = stackalloc byte[Bmp3xxCalibrationData.Length];
            ReadRegisters(Bmp3xxRegister.CalibrationData, calibration);
            CalibrationData = Bmp3xxCalibrationData.Parse(calibration);
        }

        /// <summary>
        /// Gets or sets the pressure oversampling. More oversampling lowers noise but makes a measurement take longer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is not a defined <see cref="Bmp3xxOversampling"/>.</exception>
        [Property]
        public Bmp3xxOversampling PressureSampling
        {
            get
            {
                ThrowIfDisposed();
                return _pressureSampling;
            }
            set
            {
                ThrowIfUndefined(value);
                UpdateRegister(Bmp3xxRegister.Oversampling, PressureOversamplingMask, (byte)((byte)value << PressureOversamplingShift));
                _pressureSampling = value;
                _longestPendingMeasurementMilliseconds = Math.Max(_longestPendingMeasurementMilliseconds, GetMeasurementDuration());
            }
        }

        /// <summary>
        /// Gets or sets the temperature oversampling. For pressure measurements, more than x2 brings little benefit (section 3.4.2).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is not a defined <see cref="Bmp3xxOversampling"/>.</exception>
        [Property]
        public Bmp3xxOversampling TemperatureSampling
        {
            get
            {
                ThrowIfDisposed();
                return _temperatureSampling;
            }
            set
            {
                ThrowIfUndefined(value);
                UpdateRegister(Bmp3xxRegister.Oversampling, TemperatureOversamplingMask, (byte)((byte)value << TemperatureOversamplingShift));
                _temperatureSampling = value;
                _longestPendingMeasurementMilliseconds = Math.Max(_longestPendingMeasurementMilliseconds, GetMeasurementDuration());
            }
        }

        /// <summary>
        /// Gets or sets the IIR filter coefficient. Writing it resets the filter (section 3.4.3).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is not a defined <see cref="Bmp3xxFilterCoefficient"/>.</exception>
        [Property]
        public Bmp3xxFilterCoefficient FilterCoefficient
        {
            get
            {
                ThrowIfDisposed();
                return _filterCoefficient;
            }
            set
            {
                ThrowIfUndefined(value);
                UpdateRegister(Bmp3xxRegister.Configuration, FilterMask, (byte)((byte)value << FilterShift));
                _filterCoefficient = value;
            }
        }

        /// <summary>
        /// Gets or sets the output data rate used in <see cref="Bmp3xxPowerMode.Normal"/> mode.
        /// The sampling period must be longer than <see cref="GetMeasurementDuration"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is not a defined <see cref="Bmp3xxOutputDataRate"/>.</exception>
        [Property]
        public Bmp3xxOutputDataRate OutputDataRate
        {
            get
            {
                ThrowIfDisposed();
                return _outputDataRate;
            }
            set
            {
                ThrowIfUndefined(value);
                UpdateRegister(Bmp3xxRegister.OutputDataRate, OutputDataRateMask, (byte)value);
                _outputDataRate = value;
            }
        }

        /// <summary>
        /// Gets the calibration data read from the sensor.
        /// </summary>
        internal Bmp3xxCalibrationData CalibrationData { get; }

        /// <summary>
        /// Reads the current power mode. After a forced measurement has finished, the sensor reports <see cref="Bmp3xxPowerMode.Sleep"/>.
        /// </summary>
        /// <returns>The current power mode.</returns>
        [Property("PowerMode")]
        public Bmp3xxPowerMode ReadPowerMode() => DecodePowerMode(ReadRegister(Bmp3xxRegister.PowerControl));

        /// <summary>
        /// Sets the power mode. <see cref="Bmp3xxPowerMode.Forced"/> starts a single measurement.
        /// Leaving <see cref="Bmp3xxPowerMode.Normal"/> or <see cref="Bmp3xxPowerMode.Forced"/> mode waits for a running
        /// measurement to finish, which takes up to <see cref="GetMeasurementDuration"/> plus a margin.
        /// </summary>
        /// <param name="powerMode">The power mode to set.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="powerMode"/> is not a defined <see cref="Bmp3xxPowerMode"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="Bmp3xxPowerMode.Normal"/> was requested but the sensor reports a configuration error, usually because the
        /// <see cref="OutputDataRate"/> period is shorter than <see cref="GetMeasurementDuration"/>. The sensor is put back to sleep.
        /// </exception>
        [Property("PowerMode")]
        public void SetPowerMode(Bmp3xxPowerMode powerMode)
        {
            ThrowIfUndefined(powerMode);

            byte powerControl = ReadRegister(Bmp3xxRegister.PowerControl);
            Bmp3xxPowerMode currentMode = DecodePowerMode(powerControl);

            // The sensor ignores mode changes it considers illegal (section 3.3.4). Going through sleep when switching
            // between forced and normal mode is always allowed.
            bool leavingMode = currentMode != Bmp3xxPowerMode.Sleep && currentMode != powerMode;
            if (leavingMode)
            {
                WritePowerMode(powerControl, Bmp3xxPowerMode.Sleep);

                // A measurement that is running finishes first, and the sensor ignores a new mode until then, although
                // the power control register already reads sleep (observed on a BMP390). Waiting here also protects the
                // next call. The datasheet times are typical, hence the margin.
                int duration = Math.Max(GetMeasurementDuration(), _longestPendingMeasurementMilliseconds);
                Thread.Sleep(duration + (duration / 10) + 1);
            }

            if (powerMode != Bmp3xxPowerMode.Sleep)
            {
                if (powerMode == Bmp3xxPowerMode.Normal)
                {
                    // A configuration error stays set until the error register is read (table 28). Clear one left by an
                    // earlier request or settings change, so the check below only sees errors caused by this request.
                    ReadErrors();
                }

                // Requesting the mode that is already active doesn't end a running measurement, so the longest is kept.
                _longestPendingMeasurementMilliseconds = currentMode == powerMode
                    ? Math.Max(_longestPendingMeasurementMilliseconds, GetMeasurementDuration())
                    : GetMeasurementDuration();
                WritePowerMode(powerControl, powerMode);
            }
            else if (!leavingMode)
            {
                WritePowerMode(powerControl, powerMode);
            }

            // A configuration error is only detected in normal mode (section 4.3.3, table 28).
            if (powerMode == Bmp3xxPowerMode.Normal && (ReadErrors() & Bmp3xxErrors.Configuration) != 0)
            {
                WritePowerMode(powerControl, Bmp3xxPowerMode.Sleep);
                throw new InvalidOperationException(
                    $"The sensor rejected normal mode: the output data rate is too fast for the oversampling settings. " +
                    $"Choose an {nameof(OutputDataRate)} period longer than {GetMeasurementDuration()} ms.");
            }
        }

        /// <summary>
        /// Gets the time a measurement takes with the current oversampling settings, rounded up to whole milliseconds (section 3.9.2).
        /// </summary>
        /// <returns>The measurement duration in milliseconds.</returns>
        [Property("MeasurementDuration")]
        public int GetMeasurementDuration()
        {
            ThrowIfDisposed();

            // Both measurements are always enabled by this binding.
            int microseconds = MeasurementBaseMicroseconds
                + PressureSettleMicroseconds + ((1 << (int)_pressureSampling) * _conversionMicroseconds)
                + _temperatureSettleMicroseconds + ((1 << (int)_temperatureSampling) * _conversionMicroseconds);

            return (int)Math.Ceiling(microseconds / 1000.0);
        }

        /// <summary>
        /// Performs a measurement and returns its result. Unless the sensor is in <see cref="Bmp3xxPowerMode.Normal"/> mode,
        /// this starts a forced measurement and waits for it to finish.
        /// </summary>
        /// <returns>
        /// The measured temperature and pressure. A value is null if it is outside the sensor's operating range
        /// (-40 to 85 °C, 300 to 1250 hPa). The pressure is also null if the temperature is out of range, because it is
        /// computed from the temperature. Both are null if the measurement didn't finish in time.
        /// </returns>
        public Bmp3xxReadResult Read()
        {
            if (!StartForcedMeasurementIfNeeded(out int durationMilliseconds))
            {
                return ReadLatestResult();
            }

            Thread.Sleep(durationMilliseconds);
            var stopwatch = Stopwatch.StartNew();
            while (!IsDataReady())
            {
                if (stopwatch.ElapsedMilliseconds > durationMilliseconds + TimeoutMarginMilliseconds)
                {
                    return new Bmp3xxReadResult(null, null);
                }

                Thread.Sleep(PollIntervalMilliseconds);
            }

            _measurementAvailable = true;
            return ReadResult();
        }

        /// <summary>
        /// Performs a measurement and returns its result, waiting asynchronously. See <see cref="Read"/>.
        /// </summary>
        /// <returns>The measured temperature and pressure; see <see cref="Read"/>.</returns>
        public async Task<Bmp3xxReadResult> ReadAsync()
        {
            if (!StartForcedMeasurementIfNeeded(out int durationMilliseconds))
            {
                return ReadLatestResult();
            }

            await Task.Delay(durationMilliseconds).ConfigureAwait(false);
            var stopwatch = Stopwatch.StartNew();
            while (!IsDataReady())
            {
                if (stopwatch.ElapsedMilliseconds > durationMilliseconds + TimeoutMarginMilliseconds)
                {
                    return new Bmp3xxReadResult(null, null);
                }

                await Task.Delay(PollIntervalMilliseconds).ConfigureAwait(false);
            }

            _measurementAvailable = true;
            return ReadResult();
        }

        /// <summary>
        /// Reads the temperature of the latest measurement, without starting a new one.
        /// In <see cref="Bmp3xxPowerMode.Sleep"/> mode, call <see cref="Read"/> or <see cref="SetPowerMode"/> with
        /// <see cref="Bmp3xxPowerMode.Forced"/> first.
        /// </summary>
        /// <param name="temperature">The temperature, or the default value if the method returns false.</param>
        /// <returns>
        /// True if a valid value was read; false if no measurement has been made since the last reset, or the value is
        /// outside the sensor's operating range of -40 to 85 °C.
        /// </returns>
        [Telemetry("Temperature")]
        public bool TryReadTemperature(out Temperature temperature)
        {
            Temperature? value = ReadLatestResult().Temperature;
            temperature = value.GetValueOrDefault();
            return value.HasValue;
        }

        /// <summary>
        /// Reads the pressure of the latest measurement, without starting a new one. See <see cref="TryReadTemperature"/>.
        /// </summary>
        /// <param name="pressure">The pressure, or the default value if the method returns false.</param>
        /// <returns>
        /// True if a valid value was read; false if no measurement has been made since the last reset, or the value is
        /// outside the sensor's operating range of 300 to 1250 hPa, or the temperature is outside its range.
        /// </returns>
        [Telemetry("Pressure")]
        public bool TryReadPressure(out Pressure pressure)
        {
            Pressure? value = ReadLatestResult().Pressure;
            pressure = value.GetValueOrDefault();
            return value.HasValue;
        }

        /// <summary>
        /// Calculates the altitude from the latest measurement and the given sea-level pressure, without starting a new measurement.
        /// </summary>
        /// <param name="seaLevelPressure">The pressure at sea level, for example from a nearby weather station.</param>
        /// <param name="altitude">The altitude, or the default value if the method returns false.</param>
        /// <returns>True if valid pressure and temperature values were available; see <see cref="TryReadPressure"/>.</returns>
        public bool TryReadAltitude(Pressure seaLevelPressure, out Length altitude)
        {
            // Pressure and temperature come from the same burst read, so they belong to the same measurement.
            Bmp3xxReadResult result = ReadLatestResult();
            if (result.Pressure is not Pressure pressure || result.Temperature is not Temperature temperature)
            {
                altitude = default;
                return false;
            }

            altitude = WeatherHelper.CalculateAltitude(pressure, seaLevelPressure, temperature);
            return true;
        }

        /// <summary>
        /// Calculates the altitude from the latest measurement, assuming the mean sea-level pressure of 1013.25 hPa.
        /// </summary>
        /// <param name="altitude">The altitude, or the default value if the method returns false.</param>
        /// <returns>True if valid pressure and temperature values were available; see <see cref="TryReadPressure"/>.</returns>
        public bool TryReadAltitude(out Length altitude) => TryReadAltitude(WeatherHelper.MeanSeaLevelPressure, out altitude);

        /// <summary>
        /// Reads the status flags.
        /// </summary>
        /// <returns>The status flags.</returns>
        [Telemetry("Status")]
        public Bmp3xxStatus ReadStatus()
        {
            const byte statusMask = (byte)(Bmp3xxStatus.CommandReady | Bmp3xxStatus.PressureDataReady | Bmp3xxStatus.TemperatureDataReady);
            return (Bmp3xxStatus)(ReadRegister(Bmp3xxRegister.Status) & statusMask);
        }

        /// <summary>
        /// Reads the error flags. Reading clears <see cref="Bmp3xxErrors.Command"/> and <see cref="Bmp3xxErrors.Configuration"/>.
        /// </summary>
        /// <returns>The error flags.</returns>
        public Bmp3xxErrors ReadErrors()
        {
            const byte errorMask = (byte)(Bmp3xxErrors.Fatal | Bmp3xxErrors.Command | Bmp3xxErrors.Configuration);
            return (Bmp3xxErrors)(ReadRegister(Bmp3xxRegister.Error) & errorMask);
        }

        /// <summary>
        /// Performs a soft reset, then writes the current settings back, so the properties keep describing the sensor.
        /// The sensor is in <see cref="Bmp3xxPowerMode.Sleep"/> mode afterwards.
        /// </summary>
        /// <exception cref="IOException">The sensor is not ready for a command, reports that the reset failed, or doesn't signal its completion.</exception>
        [Command]
        public void Reset()
        {
            if (!WaitFor(() => (ReadStatus() & Bmp3xxStatus.CommandReady) != 0))
            {
                throw new IOException("The sensor is not ready to accept a command.");
            }

            // Clear a stale "reset detected" flag (it is cleared on read) so the next one belongs to this reset.
            ReadRegister(Bmp3xxRegister.Event);
            WriteRegister(Bmp3xxRegister.Command, SoftResetCommand);
            _measurementAvailable = false;
            Thread.Sleep(StartupTimeMilliseconds);

            if ((ReadErrors() & Bmp3xxErrors.Command) != 0)
            {
                throw new IOException("The sensor reported that the soft reset failed.");
            }

            // Completion of a soft reset is signalled by por_detected (section 3.2).
            if (!WaitFor(() => (ReadRegister(Bmp3xxRegister.Event) & PowerOnResetDetectedBit) != 0))
            {
                throw new IOException("The sensor did not signal the end of the soft reset.");
            }

            ApplySettings();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources used by the sensor, including the <see cref="I2cDevice"/>.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }

        private static void ThrowIfUndefined<T>(T value)
            where T : struct, Enum
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"Not a valid {typeof(T).Name}.");
            }
        }

        private static Bmp3xxPowerMode DecodePowerMode(byte powerControl)
        {
            // 01 and 10 both mean forced mode (section 4.3.17, table 42).
            return ((powerControl & PowerModeMask) >> PowerModeShift) switch
            {
                0b00 => Bmp3xxPowerMode.Sleep,
                0b11 => Bmp3xxPowerMode.Normal,
                _ => Bmp3xxPowerMode.Forced,
            };
        }

        private static bool WaitFor(Func<bool> condition)
        {
            var stopwatch = Stopwatch.StartNew();
            while (!condition())
            {
                if (stopwatch.ElapsedMilliseconds > FlagTimeoutMilliseconds)
                {
                    return false;
                }

                Thread.Sleep(1);
            }

            return true;
        }

        private bool StartForcedMeasurementIfNeeded(out int durationMilliseconds)
        {
            if (ReadPowerMode() == Bmp3xxPowerMode.Normal)
            {
                durationMilliseconds = 0;
                return false;
            }

            // The data-ready flags are only cleared by reading the data (table 29). Data from an earlier measurement
            // that was never read would otherwise look like the result of this one. Sleep first, so a measurement
            // that is still running can't set them again.
            SetPowerMode(Bmp3xxPowerMode.Sleep);

            // The flags still show an unread measurement; note it before clearing them.
            IsMeasurementAvailable();
            Span<byte> staleData = stackalloc byte[DataLength];
            ReadRegisters(Bmp3xxRegister.Data, staleData);

            SetPowerMode(Bmp3xxPowerMode.Forced);
            durationMilliseconds = GetMeasurementDuration();
            return true;
        }

        private bool IsDataReady()
        {
            const Bmp3xxStatus dataReady = Bmp3xxStatus.PressureDataReady | Bmp3xxStatus.TemperatureDataReady;
            return (ReadStatus() & dataReady) == dataReady;
        }

        private bool IsMeasurementAvailable()
        {
            // A measurement whose data hasn't been read yet, for example one started with SetPowerMode, shows in the
            // data-ready flags (table 29). Once its data is read, only the field remembers it.
            if (!_measurementAvailable)
            {
                const Bmp3xxStatus dataReady = Bmp3xxStatus.PressureDataReady | Bmp3xxStatus.TemperatureDataReady;
                _measurementAvailable = (ReadStatus() & dataReady) != 0;
            }

            return _measurementAvailable;
        }

        private Bmp3xxReadResult ReadLatestResult() => IsMeasurementAvailable() ? ReadResult() : new Bmp3xxReadResult(null, null);

        private Bmp3xxReadResult ReadResult()
        {
            // One burst read, so pressure and temperature belong to the same measurement (section 3.10.1).
            Span<byte> data = stackalloc byte[DataLength];
            ReadRegisters(Bmp3xxRegister.Data, data);
            uint rawPressure = (uint)(data[0] | (data[1] << 8) | (data[2] << 16));
            uint rawTemperature = (uint)(data[3] | (data[4] << 8) | (data[5] << 16));

            double celsius = CalibrationData.CompensateTemperature(rawTemperature);
            double pascals = CalibrationData.CompensatePressure(rawPressure, celsius);

            // The pressure is computed from the temperature, so an out-of-range temperature invalidates both.
            bool temperatureInRange = celsius >= MinimumTemperatureCelsius && celsius <= MaximumTemperatureCelsius;
            Temperature? temperature = temperatureInRange
                ? Temperature.FromDegreesCelsius(celsius)
                : null;
            Pressure? pressure = temperatureInRange && pascals >= MinimumPressurePascals && pascals <= MaximumPressurePascals
                ? Pressure.FromPascals(pascals)
                : null;

            return new Bmp3xxReadResult(temperature, pressure);
        }

        private void ApplySettings()
        {
            WriteRegister(Bmp3xxRegister.Oversampling, (byte)(((byte)_pressureSampling << PressureOversamplingShift) | ((byte)_temperatureSampling << TemperatureOversamplingShift)));
            WriteRegister(Bmp3xxRegister.Configuration, (byte)((byte)_filterCoefficient << FilterShift));
            WriteRegister(Bmp3xxRegister.OutputDataRate, (byte)_outputDataRate);
            WriteRegister(Bmp3xxRegister.PowerControl, PressureEnableBit | TemperatureEnableBit);
        }

        private void WritePowerMode(byte powerControl, Bmp3xxPowerMode powerMode)
        {
            WriteRegister(Bmp3xxRegister.PowerControl, (byte)((powerControl & ~PowerModeMask) | ((byte)powerMode << PowerModeShift)));
        }

        private void UpdateRegister(Bmp3xxRegister register, byte mask, byte bits)
        {
            byte value = ReadRegister(register);
            WriteRegister(register, (byte)((value & ~mask) | (bits & mask)));
        }

        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_i2cDevice is null, this);

        // All bus access goes through the three methods below, so another transport (SPI) only needs to change them.
        private I2cDevice Device
        {
            get
            {
                ThrowIfDisposed();
                return _i2cDevice!;
            }
        }

        private byte ReadRegister(Bmp3xxRegister register)
        {
            Span<byte> value = stackalloc byte[1];
            ReadRegisters(register, value);
            return value[0];
        }

        private void ReadRegisters(Bmp3xxRegister start, Span<byte> values)
        {
            // One transaction: the register address, then a burst read with auto-increment (section 5.2.2).
            Device.WriteRead(stackalloc byte[] { (byte)start }, values);
        }

        private void WriteRegister(Bmp3xxRegister register, byte value)
        {
            // A write is a (register address, value) pair (section 5.2.1).
            Device.Write(stackalloc byte[] { (byte)register, value });
        }
    }
}
