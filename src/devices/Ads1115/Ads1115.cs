// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Threading;
using System.Device;
using System.Device.I2c;
using System.Device.Gpio;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Analog-to-Digital Converter ADS1115
    /// </summary>
    public class Ads1115 : IDisposable
    {
        private readonly bool _shouldDispose;

        private I2cDevice _i2cDevice;

        private GpioController? _gpioController;

        private InputMultiplexer _inputMultiplexer;

        private MeasuringRange _measuringRange;

        private DataRate _dataRate;
        private DeviceMode _deviceMode;

        /// <summary>
        /// The pin of the GPIO controller that is connected to the interrupt line of the ADS1115
        /// </summary>
        private int _gpioInterruptPin;

        private ComparatorPolarity _comparatorPolarity;
        private ComparatorLatching _comparatorLatching;
        private ComparatorQueue _comparatorQueue;

        /// <summary>
        /// Initialize a new Ads1115 device connected through I2C
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="inputMultiplexer">Input Multiplexer</param>
        /// <param name="measuringRange">Programmable Gain Amplifier</param>
        /// <param name="dataRate">Data Rate</param>
        /// <param name="deviceMode">Initial device mode</param>
        public Ads1115(I2cDevice i2cDevice, InputMultiplexer inputMultiplexer = InputMultiplexer.AIN0, MeasuringRange measuringRange = MeasuringRange.FS4096,
            DataRate dataRate = DataRate.SPS128, DeviceMode deviceMode = DeviceMode.Continuous)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _inputMultiplexer = inputMultiplexer;
            _measuringRange = measuringRange;
            _dataRate = dataRate;
            _gpioController = null;
            _gpioInterruptPin = -1;
            _deviceMode = deviceMode;
            ComparatorMode = ComparatorMode.Traditional;
            _comparatorPolarity = ComparatorPolarity.Low;
            _comparatorLatching = ComparatorLatching.NonLatching;
            _comparatorQueue = ComparatorQueue.Disable;

            SetConfig();
            DisableAlertReadyPin();
        }

        /// <summary>
        /// Initialize a new Ads1115 device connected through I2C with an additional GPIO controller for interrupt handling.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="gpioController">The GPIO Controller used for interrupt handling</param>
        /// <param name="gpioInterruptPin">The pin number where the interrupt line is attached on the GPIO controller</param>
        /// <param name="shouldDispose">True (the default) if the GPIO controller shall be disposed when disposing this instance</param>
        /// <param name="inputMultiplexer">Input Multiplexer</param>
        /// <param name="measuringRange">Programmable Gain Amplifier</param>
        /// <param name="dataRate">Data Rate</param>
        /// <param name="deviceMode">Initial device mode</param>
        public Ads1115(I2cDevice i2cDevice,
            GpioController? gpioController, int gpioInterruptPin, bool shouldDispose = true, InputMultiplexer inputMultiplexer = InputMultiplexer.AIN0, MeasuringRange measuringRange = MeasuringRange.FS4096,
            DataRate dataRate = DataRate.SPS128, DeviceMode deviceMode = DeviceMode.Continuous)
            : this(i2cDevice, inputMultiplexer, measuringRange, dataRate, deviceMode)
        {
            _gpioController = gpioController ?? new GpioController();
            if (gpioInterruptPin < 0 || gpioInterruptPin >= _gpioController.PinCount)
            {
                throw new ArgumentOutOfRangeException(nameof(gpioInterruptPin), $"The given GPIO Controller has no pin number {gpioInterruptPin}");
            }

            _gpioInterruptPin = gpioInterruptPin;
            _shouldDispose = shouldDispose || gpioController is null;
        }

        /// <summary>
        /// ADS1115 Input Multiplexer.
        /// This selects the channel(s) for the next read operation, <see cref="Iot.Device.Ads1115.InputMultiplexer"/>.
        /// Setting this property will wait until a value is available from the newly selected input channel.
        /// </summary>
        public InputMultiplexer InputMultiplexer
        {
            get => _inputMultiplexer;
            set
            {
                _inputMultiplexer = value;
                SetConfig();
            }
        }

        /// <summary>
        /// ADS1115 Programmable Gain Amplifier
        /// This sets the maximum value that can be measured. Regardless of this setting, the input value on any pin must not exceed VDD + 0.3V,
        /// so high ranges are only usable with a VDD of more than 5V.
        /// Setting this property will wait until a new value is available.
        /// </summary>
        public MeasuringRange MeasuringRange
        {
            get => _measuringRange;
            set
            {
                _measuringRange = value;
                SetConfig();
            }
        }

        /// <summary>
        /// ADS1115 Data Rate.
        /// The number of conversions per second that will take place. One conversion will take "1/rate" seconds to become ready. If in
        /// power-down mode, only one conversion will happen automatically, then another request is required.
        /// Setting this property will wait until a new value is available.
        /// </summary>
        public DataRate DataRate
        {
            get => _dataRate;
            set
            {
                _dataRate = value;
                SetConfig();
            }
        }

        /// <summary>
        /// ADS1115 operation mode.
        /// When set to <see cref="DeviceMode.Continuous"/> the chip continously measures the input and the values can be read directly.
        /// If set to <see cref="DeviceMode.PowerDown"/> the chip enters idle mode after each conversion and
        /// a new value will be requested each time a read request is performed. This is the recommended setting when frequently
        /// swapping between input channels, because a change of the channel requires a new conversion anyway.
        /// </summary>
        public DeviceMode DeviceMode
        {
            get
            {
                return _deviceMode;
            }
            set
            {
                _deviceMode = value;
                SetConfig();
            }
        }

        /// <summary>
        /// Comparator mode.
        /// Only relevant if the comparator trigger event is set up and is changed by <see cref="EnableComparator(short, short, ComparatorMode, ComparatorQueue)"/>.
        /// </summary>
        public ComparatorMode ComparatorMode { get; private set; }

        /// <summary>
        /// Comparator polarity. Indicates whether the rising or the falling edge of the ALRT/RDY Pin is relevant.
        /// Default: Low (falling edge)
        /// </summary>
        public ComparatorPolarity ComparatorPolarity
        {
            get
            {
                return _comparatorPolarity;
            }
            set
            {
                _comparatorPolarity = value;
                SetConfig();
            }
        }

        /// <summary>
        /// Comparator latching mode. If enabled, the ALRT/RDY Pin will be kept signaled until the conversion value is read.
        /// Only relevant when the comparator is enabled.
        /// </summary>
        public ComparatorLatching ComparatorLatching
        {
            get
            {
                return _comparatorLatching;
            }
            set
            {
                _comparatorLatching = value;
                SetConfig();
            }
        }

        /// <summary>
        /// Minimum number of samples exceeding the lower/upper threshold before the ALRT pin is asserted.
        /// This can only be set with <see cref="EnableComparator(short, short, ComparatorMode, ComparatorQueue)"/>.
        /// </summary>
        public ComparatorQueue ComparatorQueue => _comparatorQueue;

        /// <summary>
        /// This event fires when a new value is available (in conversion ready mode) or the comparator threshold is exceeded.
        /// Requires setup through <see cref="EnableConversionReady"/> or <see cref="EnableComparator(ElectricPotential, ElectricPotential, ComparatorMode, ComparatorQueue)"/>.
        /// </summary>
        public event Action? AlertReadyAsserted;

        /// <summary>
        /// Set ADS1115 Config Register.
        /// Register Layout:
        /// 15    14    13    12    11    10    9      8     7      6      5       4          3          2        1     0
        /// OS  |      MUX        |       PGA      | MODE |    DATA RATE      | COMP_MODE | COMP_POL | COMP_LAT |   COMP_QUE
        /// </summary>
        private void SetConfig()
        {
            // Details in Datasheet P18
            byte configHi = (byte)(0x80 | // Set conversion enable bit, so we always do (at least) one conversion using the new settings
                            ((byte)_inputMultiplexer << 4) |
                            ((byte)_measuringRange << 1) |
                            ((byte)DeviceMode.PowerDown)); // Always in powerdown mode, otherwise we can't wait properly

            byte configLo = (byte)(((byte)_dataRate << 5) |
                            ((byte)ComparatorMode << 4) |
                            ((byte)_comparatorPolarity << 3) |
                            ((byte)_comparatorLatching << 2) |
                            (byte)_comparatorQueue);

            Span<byte> writeBuff = stackalloc byte[3]
            {
                (byte)Register.ADC_CONFIG_REG_ADDR,
                configHi,
                configLo
            };

            _i2cDevice.Write(writeBuff);

            // waiting for the sensor stability
            WaitWhileBusy();

            if (_deviceMode == DeviceMode.Continuous)
            {
                // We need to wait two cycles when changing the configuration in continuous mode,
                // otherwise we may be getting a value from the wrong input
                _i2cDevice.Write(writeBuff);
                WaitWhileBusy();
                configHi &= 0xFE; // Clear last bit
                writeBuff[1] = configHi;
                _i2cDevice.Write(writeBuff); // And enable continuous mode
            }
        }

        /// <summary>
        /// Resets the comparator registers to default values (effectively disabling the comparator) and disables the
        /// Alert / Ready pin (if configured)
        /// </summary>
        private void DisableAlertReadyPin()
        {
            _comparatorQueue = ComparatorQueue.Disable;
            SetConfig();
            // Reset to defaults
            Span<byte> writeBuff = stackalloc byte[3]
            {
                (byte)Register.ADC_CONFIG_REG_LO_THRESH, 0x80, 0
            };
            _i2cDevice.Write(writeBuff);
            writeBuff = stackalloc byte[3]
            {
                (byte)Register.ADC_CONFIG_REG_HI_THRESH, 0x7F, 0xFF
            };
            _i2cDevice.Write(writeBuff);
            if (_gpioController is object)
            {
                _gpioController.UnregisterCallbackForPinValueChangedEvent(_gpioInterruptPin, ConversionReadyCallback);
                _gpioController.ClosePin(_gpioInterruptPin);
            }
        }

        /// <summary>
        /// Write the two comparator registers
        /// </summary>
        /// <param name="loThreshold">High threshold value (unsigned short)</param>
        /// <param name="hiThreshold">Low threshold value (unsigned short)</param>
        private void WriteComparatorRegisters(short loThreshold, short hiThreshold)
        {
            Span<byte> writeBuff = stackalloc byte[3]
            {
                (byte)Register.ADC_CONFIG_REG_LO_THRESH, (byte)(loThreshold >> 8), (byte)(loThreshold & 0xFF)
            };
            _i2cDevice.Write(writeBuff);
            writeBuff = stackalloc byte[3]
            {
                (byte)Register.ADC_CONFIG_REG_HI_THRESH, (byte)(hiThreshold >> 8), (byte)(hiThreshold & 0xFF)
            };
            _i2cDevice.Write(writeBuff);
        }

        /// <summary>
        /// Enable conversion ready event.
        /// The <see cref="AlertReadyAsserted"/> event fires each time a new value is available after this method is called.
        /// </summary>
        /// <exception cref="InvalidOperationException">The conversion ready event is already set up or no GPIO Controller configured
        /// for interrupt handling.</exception>
        public void EnableConversionReady()
        {
            if (_gpioController is null)
            {
                throw new InvalidOperationException("Must have provided a GPIO Controller for interrupt handling.");
            }

            try
            {
                // The ALRT/RDY Pin requires a pull-up resistor
                _gpioController.OpenPin(_gpioInterruptPin, PinMode.InputPullUp);

                // Must be set to something other than disable
                _comparatorQueue = ComparatorQueue.AssertAfterOne;
                _deviceMode = DeviceMode.Continuous;
                SetConfig();
                // Writing a negative value to the max value register and a positive value to the min value register
                // configures the ALRT/RDY pin to trigger after each conversion (with a transition from high to low, when ComparatorPolarity is Low)
                WriteComparatorRegisters(short.MaxValue, short.MinValue);

                _gpioController.RegisterCallbackForPinValueChangedEvent(_gpioInterruptPin, ComparatorPolarity == ComparatorPolarity.Low ? PinEventTypes.Falling : PinEventTypes.Rising, ConversionReadyCallback);

            }
            catch (Exception)
            {
                _gpioController.ClosePin(_gpioInterruptPin);
                throw;
            }
        }

        private void ConversionReadyCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            if (AlertReadyAsserted is object)
            {
                AlertReadyAsserted();
            }
        }

        /// <summary>
        /// Enable comparator callback mode.
        /// In traditional comparator mode, the callback is triggered each time the measured value exceeds the given upper value (for
        /// the given queueLength number of samples). It deasserts when the lower value is reached.
        /// In window comparator mode, the callback is triggered each time the measured value exceeds the given upper value or gets
        /// less than the given lower value.
        /// </summary>
        /// <param name="lowerValue">Lower value for the comparator</param>
        /// <param name="upperValue">Upper value for the comparator</param>
        /// <param name="mode">Traditional or Window comparator mode</param>
        /// <param name="queueLength">Minimum number of samples that must exceed the threshold to trigger the event</param>
        /// <exception cref="InvalidOperationException">The GPIO Controller for the interrupt handler has not been set up</exception>
        public void EnableComparator(ElectricPotential lowerValue, ElectricPotential upperValue, ComparatorMode mode,
            ComparatorQueue queueLength) => EnableComparator(VoltageToRaw(lowerValue), VoltageToRaw(upperValue), mode, queueLength);

        /// <summary>
        /// Enable comparator callback mode.
        /// In traditional comparator mode, the callback is triggered each time the measured value exceeds the given upper value (for
        /// the given queueLength number of samples). It deasserts when the lower value is reached.
        /// In window comparator mode, the callback is triggered each time the measured value exceeds the given upper value or gets
        /// less than the given lower value.
        /// </summary>
        /// <param name="lowerValue">Lower value for the comparator</param>
        /// <param name="upperValue">Upper value for the comparator</param>
        /// <param name="mode">Traditional or Window comparator mode</param>
        /// <param name="queueLength">Minimum number of samples that must exceed the threshold to trigger the event</param>
        /// <exception cref="InvalidOperationException">The GPIO Controller for the interrupt handler has not been set up</exception>
        public void EnableComparator(short lowerValue, short upperValue, ComparatorMode mode,
            ComparatorQueue queueLength)
        {
            if (_gpioController is null)
            {
                throw new InvalidOperationException("GPIO Controller must have been provided in constructor for this operation to work");
            }

            if (queueLength == ComparatorQueue.Disable)
            {
                throw new ArgumentException("Must set the ComparatorQueue to something other than disable.", nameof(queueLength));
            }

            if (upperValue <= lowerValue)
            {
                throw new ArgumentException("Lower comparator limit must be larger than upper comparator limit");
            }

            try
            {
                // The ALRT/RDY Pin requires a pull-up resistor
                _gpioController.OpenPin(_gpioInterruptPin, PinMode.InputPullUp);

                _comparatorQueue = queueLength;
                ComparatorMode = mode;
                // Event callback mode is only useful in Continuous mode
                _deviceMode = DeviceMode.Continuous;
                SetConfig();
                // Writing a negative value to the max value register and a positive value to the min value register
                // configures the ALRT/RDY pin to trigger after each conversion (with a transition from high to low, when ComparatorPolarity is Low)
                WriteComparatorRegisters(lowerValue, upperValue);

                _gpioController.RegisterCallbackForPinValueChangedEvent(_gpioInterruptPin, ComparatorPolarity == ComparatorPolarity.Low ? PinEventTypes.Falling : PinEventTypes.Rising, ConversionReadyCallback);

            }
            catch (Exception)
            {
                _gpioController.ClosePin(_gpioInterruptPin);
                throw;
            }
        }

        private ushort ReadConfigRegister()
        {
            Span<byte> retBuf = stackalloc byte[2];
            Span<byte> request = stackalloc byte[1]
            {
                (byte)Register.ADC_CONFIG_REG_ADDR
            };

            _i2cDevice.WriteRead(request, retBuf);
            return BinaryPrimitives.ReadUInt16BigEndian(retBuf);
        }

        /// <summary>
        /// Wait until the current conversion finishes.
        /// This method must only be called in powerdown mode, otherwise it would timeout, since the busy bit never changes.
        /// Due to that, we always write the configuration first in power down mode and then enable the continuous bit.
        /// </summary>
        /// <exception cref="TimeoutException">A timeout occurred waiting for the ADC to finish the conversion</exception>
        private void WaitWhileBusy()
        {
            // In powerdown-mode, wait until the busy bit goes high
            ushort reg = ReadConfigRegister();
            int timeout = 10000; // microseconds
            while ((reg & 0x8000) == 0 && (timeout > 0))
            {
                DelayHelper.DelayMicroseconds(2, true);
                timeout -= 2;
                reg = ReadConfigRegister();
            }

            if (timeout <= 0)
            {
                throw new TimeoutException("Timeout waiting for ADC to complete conversion");
            }
        }

        /// <summary>
        /// Read Raw Data.
        /// If in PowerDown (single-shot) mode, one new sample is requested first.
        /// </summary>
        /// <returns>Raw Value</returns>
        public short ReadRaw()
        {
            if (DeviceMode == DeviceMode.PowerDown)
            {
                // If we are in powerdown (single-conversion) mode, we have to set the configuration before each read, otherwise
                // we keep getting the same stored value.
                SetConfig();
            }

            return ReadRawInternal();
        }

        private short ReadRawInternal()
        {
            short val;
            Span<byte> readBuff = stackalloc byte[2];

            _i2cDevice.WriteByte((byte)Register.ADC_CONVERSION_REG_ADDR);
            _i2cDevice.Read(readBuff);

            val = BinaryPrimitives.ReadInt16BigEndian(readBuff);

            return val;
        }

        /// <summary>
        /// Reads the next raw value, first switching to the given input and ranges.
        /// </summary>
        /// <param name="inputMultiplexer">New input multiplexer setting</param>
        /// <returns>Measured value as short</returns>
        /// <remarks>
        /// For performance reasons, it is advised to use this method if quick readings with different input channels are required,
        /// instead of setting all the properties first and then calling <see cref="ReadRaw()"/>.
        /// </remarks>
        public short ReadRaw(InputMultiplexer inputMultiplexer) => ReadRaw(inputMultiplexer, MeasuringRange, DataRate);

        /// <summary>
        /// Reads the next raw value, first switching to the given input and ranges.
        /// </summary>
        /// <param name="inputMultiplexer">New input multiplexer setting</param>
        /// <param name="measuringRange">New measuring range</param>
        /// <param name="dataRate">New data rate</param>
        /// <returns>Measured value as short</returns>
        /// <remarks>
        /// For performance reasons, it is advised to use this method if quick readings with different settings
        /// (i.e. different input channels) are required, instead of setting all the properties first and then
        /// calling <see cref="ReadRaw()"/>.
        /// </remarks>
        public short ReadRaw(InputMultiplexer inputMultiplexer, MeasuringRange measuringRange, DataRate dataRate)
        {
            if (inputMultiplexer != InputMultiplexer || measuringRange != MeasuringRange || dataRate != DataRate)
            {
                _inputMultiplexer = inputMultiplexer;
                _measuringRange = measuringRange;
                _dataRate = dataRate;
                SetConfig();
                // We just set the config, so no need to query another sample
                return ReadRawInternal();
            }

            return ReadRaw();
        }

        /// <summary>
        /// Returns the electric potential (voltage) of the currently selected input.
        /// </summary>
        /// <returns>The measured voltage of the currently selected input channel. In volts. </returns>
        public ElectricPotential ReadVoltage()
        {
            short raw = ReadRaw();
            return RawToVoltage(raw);
        }

        /// <summary>
        /// Returns the electric potential (voltage) of the given channel, performs a measurement first
        /// </summary>
        /// <param name="inputMultiplexer">Channel to use</param>
        /// <returns>The voltage at the selected channel</returns>
        public ElectricPotential ReadVoltage(InputMultiplexer inputMultiplexer)
        {
            short raw = ReadRaw(inputMultiplexer, MeasuringRange, DataRate);
            return RawToVoltage(raw);
        }

        /// <summary>
        /// Convert Raw Data to Voltage
        /// </summary>
        /// <param name="val">Raw Data</param>
        /// <returns>Voltage, based on the current measuring range</returns>
        public ElectricPotential RawToVoltage(short val)
        {
            double resolution;

            ElectricPotential maxVoltage = MaxVoltageFromMeasuringRange(MeasuringRange);

            resolution = 32768.0;
            return ElectricPotential.FromVolts(val * (maxVoltage.Volts / resolution));
        }

        /// <summary>
        /// Converts voltage to raw data.
        /// </summary>
        /// <param name="voltage">Input voltage</param>
        /// <returns>Corresponding raw value, based on the current measuring range</returns>
        public short VoltageToRaw(ElectricPotential voltage)
        {
            double resolution;
            ElectricPotential maxVoltage = MaxVoltageFromMeasuringRange(MeasuringRange);
            resolution = 32768.0;
            return (short)Math.Round(voltage.Volts / (maxVoltage.Volts / resolution));
        }

        /// <summary>
        /// Returns the voltage assigned to the given MeasuringRange enumeration value.
        /// </summary>
        /// <param name="measuringRange">One of the <see cref="MeasuringRange"/> enumeration members</param>
        /// <returns>An electric potential (voltage).</returns>
        public ElectricPotential MaxVoltageFromMeasuringRange(MeasuringRange measuringRange)
        {
            double voltage = measuringRange switch
            {
                MeasuringRange.FS6144 => 6.144,
                MeasuringRange.FS4096 => 4.096,
                MeasuringRange.FS2048 => 2.048,
                MeasuringRange.FS1024 => 1.024,
                MeasuringRange.FS0512 => 0.512,
                MeasuringRange.FS0256 => 0.256,
                _ => throw new ArgumentOutOfRangeException(nameof(measuringRange), "Unknown measuring range used")
            };

            return ElectricPotential.FromVolts(voltage);
        }

        /// <summary>
        /// Returns the sampling frequency in Hz for the given data rate enumeration member.
        /// </summary>
        /// <param name="dataRate">One of the <see cref="DataRate"/> enumeration members.</param>
        /// <returns>A frequency, in Hertz</returns>
        public double FrequencyFromDataRate(DataRate dataRate) => dataRate switch
        {
                DataRate.SPS008 => 8.0,
                DataRate.SPS016 => 16.0,
                DataRate.SPS032 => 32.0,
                DataRate.SPS064 => 64.0,
                DataRate.SPS128 => 128.0,
                DataRate.SPS250 => 250.0,
                DataRate.SPS475 => 475.0,
                DataRate.SPS860 => 860.0,
                _ => throw new ArgumentOutOfRangeException(nameof(dataRate), "Unknown data rate used")
        };

        /// <summary>
        /// Cleanup.
        /// Failing to dispose this class, especially when callbacks are active, may lead to undefined behavior.
        /// </summary>
        public void Dispose()
        {
            if (_i2cDevice is object)
            {
                DisableAlertReadyPin();
                _i2cDevice.Dispose();
                _i2cDevice = null!;
            }

            if (_shouldDispose)
            {
                _gpioController?.Dispose();
                _gpioController = null;
            }
        }
    }
}
