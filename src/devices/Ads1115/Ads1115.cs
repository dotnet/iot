// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Threading;
using System.Device;
using System.Device.I2c;
using System.Device.Gpio;

namespace Iot.Device.Ads1115
{
    /// <summary>
    /// Analog-to-Digital Converter ADS1115
    /// </summary>
    public class Ads1115 : IDisposable
    {
        private I2cDevice _i2cDevice = null;

        private InputMultiplexer _inputMultiplexer;

        private MeasuringRange _measuringRange;

        private DataRate _dataRate;
        private DeviceMode _deviceMode;

        private GpioController _gpioController;
        private int _alrtRdyPin;
        private ComparatorMode _comparatorMode;
        private ComparatorPolarity _comparatorPolarity;
        private ComparatorLatching _comparatorLatching;
        private ComparatorQueue _comparatorQueue;

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
        /// When set to <see cref="DeviceMode.Continuous"/> the chip constantly measures the input and the values can be read directly.
        /// If set to <see cref="DeviceMode.PowerDown"/> the chip enters idle mode after each conversion and
        /// a new value will be requested each time a read request is performed. This is the recommended setting when constantly
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
        /// Only relevant if the comparator trigger event is set up and is changed by <see cref="EnableComparator(short, short, ComparatorMode, ComparatorQueue, GpioController, int)"/>.
        /// </summary>
        public ComparatorMode ComparatorMode
        {
            get
            {
                return _comparatorMode;
            }
        }

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
        /// This can only be set with <see cref="EnableComparator(short, short, ComparatorMode, ComparatorQueue, GpioController, int)"/>.
        /// </summary>
        public ComparatorQueue ComparatorQueue
        {
            get
            {
                return _comparatorQueue;
            }
        }

        /// <summary>
        /// This event fires when a new value is available (in conversion ready mode) or the comparator threshold is exceeded.
        /// Requires setup through <see cref="EnableConversionReady"/> or <see cref="EnableComparator(double, double, ComparatorMode, ComparatorQueue, GpioController, int)"/>.
        /// </summary>
        public event Action AlertReadyAsserted;

        /// <summary>
        /// Initialize a new Ads1115 device connected through I2C
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="inputMultiplexer">Input Multiplexer</param>
        /// <param name="measuringRange">Programmable Gain Amplifier</param>
        /// <param name="dataRate">Data Rate</param>
        /// <param name="deviceMode">Initial device mode</param>
        public Ads1115(I2cDevice i2cDevice, InputMultiplexer inputMultiplexer = InputMultiplexer.AIN0, MeasuringRange measuringRange = MeasuringRange.FS4096, DataRate dataRate = DataRate.SPS128,
            DeviceMode deviceMode = DeviceMode.Continuous)
        {
            _i2cDevice = i2cDevice;
            _inputMultiplexer = inputMultiplexer;
            _measuringRange = measuringRange;
            _dataRate = dataRate;
            _gpioController = null;
            _alrtRdyPin = -1;
            _deviceMode = deviceMode;
            _comparatorMode = ComparatorMode.Traditional;
            _comparatorPolarity = ComparatorPolarity.Low;
            _comparatorLatching = ComparatorLatching.NonLatching;
            _comparatorQueue = ComparatorQueue.Disable;

            SetConfig();
            DisableAlrtReadyPin();
        }

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
                            ((byte)_deviceMode));

            byte configLo = (byte)(((byte)_dataRate << 5) |
                            ((byte)_comparatorMode << 4) |
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
        }

        /// <summary>
        /// Resets the comparator registers to default values (effectively disabling the comparator) and disables the
        /// Alert / Ready pin (if configured)
        /// </summary>
        private void DisableAlrtReadyPin()
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
            if (_gpioController != null)
            {
                _gpioController.UnregisterCallbackForPinValueChangedEvent(_alrtRdyPin, ConversionReadyCallback);
                _gpioController.ClosePin(_alrtRdyPin);
                _gpioController = null;
                _alrtRdyPin = -1;
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
        /// <param name="gpioController">Reference to the GPIO controller</param>
        /// <param name="pin">Pin to which the ALRT/RDY Pin is connected</param>
        /// <exception cref="InvalidOperationException">The conversion ready event is already set up</exception>
        public void EnableConversionReady(GpioController gpioController, int pin)
        {
            if (gpioController == null)
            {
                throw new ArgumentNullException(nameof(gpioController));
            }

            if (_gpioController != null)
            {
                throw new InvalidOperationException("Event mode already set.");
            }

            try
            {
                // The ALRT/RDY Pin requires a pull-up resistor
                gpioController.OpenPin(pin, PinMode.InputPullUp);

                // Must be set to something other than disable
                _comparatorQueue = ComparatorQueue.AssertAfterOne;
                _deviceMode = DeviceMode.Continuous;
                SetConfig();
                // Writing a negative value to the max value register and a positive value to the min value register
                // configures the ALRT/RDY pin to trigger after each conversion (with a transition from high to low, when ComparatorPolarity is Low)
                WriteComparatorRegisters(short.MaxValue, short.MinValue);

                gpioController.RegisterCallbackForPinValueChangedEvent(pin, ComparatorPolarity == ComparatorPolarity.Low ? PinEventTypes.Falling : PinEventTypes.Rising, ConversionReadyCallback);

            }
            catch (Exception)
            {
                gpioController.ClosePin(pin);
                throw;
            }

            _gpioController = gpioController;
            _alrtRdyPin = pin;
        }

        private void ConversionReadyCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            if (AlertReadyAsserted != null)
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
        /// <param name="gpioController">The GPIO controller, to configure the pin</param>
        /// <param name="pin">The pin where the ALRT/RDY pin is connected to. </param>
        public void EnableComparator(double lowerValue, double upperValue, ComparatorMode mode,
            ComparatorQueue queueLength, GpioController gpioController, int pin)
        {
            EnableComparator(VoltageToRaw(lowerValue), VoltageToRaw(upperValue), mode, queueLength, gpioController, pin);
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
        /// <param name="gpioController">The GPIO controller, to configure the pin</param>
        /// <param name="pin">The pin where the ALRT/RDY pin is connected to. </param>
        public void EnableComparator(short lowerValue, short upperValue, ComparatorMode mode,
            ComparatorQueue queueLength, GpioController gpioController, int pin)
        {
            if (gpioController == null)
            {
                throw new ArgumentNullException(nameof(gpioController));
            }

            if (queueLength == ComparatorQueue.Disable)
            {
                throw new ArgumentException("Must set the ComparatorQueue to something other than disable.", nameof(queueLength));
            }

            if (upperValue <= lowerValue)
            {
                throw new ArgumentException("Lower comparator limit must be larger than upper comparator limit");
            }

            if (_gpioController != null)
            {
                throw new InvalidOperationException("Event mode already set.");
            }

            try
            {
                // The ALRT/RDY Pin requires a pull-up resistor
                gpioController.OpenPin(pin, PinMode.InputPullUp);

                _comparatorQueue = queueLength;
                _comparatorMode = mode;
                // Event callback mode is only useful in Continuous mode
                _deviceMode = DeviceMode.Continuous;
                SetConfig();
                // Writing a negative value to the max value register and a positive value to the min value register
                // configures the ALRT/RDY pin to trigger after each conversion (with a transition from high to low, when ComparatorPolarity is Low)
                WriteComparatorRegisters(lowerValue, upperValue);

                gpioController.RegisterCallbackForPinValueChangedEvent(pin, ComparatorPolarity == ComparatorPolarity.Low ? PinEventTypes.Falling : PinEventTypes.Rising, ConversionReadyCallback);

            }
            catch (Exception)
            {
                gpioController.ClosePin(pin);
                throw;
            }

            _gpioController = gpioController;
            _alrtRdyPin = pin;
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
        /// </summary>
        /// <exception cref="TimeoutException">A timeout occured waiting for the ADC to finish the conversion (in powerdown-mode only)</exception>
        private void WaitWhileBusy()
        {
            if (DeviceMode == DeviceMode.PowerDown)
            {
                // In powerdown-mode, wait until the busy bit goes high
                Span<byte> readBuff = stackalloc byte[2];
                ushort reg = ReadConfigRegister();
                int timeout = 5000;
                while ((reg & 0x8000) == 0 && (timeout-- > 0))
                {
                    DelayHelper.DelayMicroseconds(2, true);
                    reg = ReadConfigRegister();
                }

                if (timeout <= 0)
                {
                    throw new TimeoutException("Timeout waiting for ADC to complete conversion");
                }
            }
            else
            {
                // In continuous mode, we have to wait two cycles, the current one needs to end and then another one.
                // (Checking the busy bit is pointless, as it is always cleared, because a new conversion starts right after the last ended)
                double waitTime = 2.0 * (1.0 / FrequencyFromDataRate(DataRate));
                Thread.Sleep(TimeSpan.FromSeconds(waitTime));
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
        public short ReadRaw(InputMultiplexer inputMultiplexer)
        {
            return ReadRaw(inputMultiplexer, MeasuringRange, DataRate);
        }

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
        /// Returns the voltage of the currently selected input.
        /// </summary>
        /// <returns>The measured voltage of the currently selected input channel. In volts. </returns>
        public double ReadVoltage()
        {
            short raw = ReadRaw();
            return RawToVoltage(raw);
        }

        /// <summary>
        /// Returns the voltage of the given channel, performs a measurement first
        /// </summary>
        /// <param name="inputMultiplexer">Channel to use</param>
        /// <returns>The voltage at the selected channel</returns>
        public double ReadVoltage(InputMultiplexer inputMultiplexer)
        {
            short raw = ReadRaw(inputMultiplexer, MeasuringRange, DataRate);
            return RawToVoltage(raw);
        }

        /// <summary>
        /// Convert Raw Data to Voltage
        /// </summary>
        /// <param name="val">Raw Data</param>
        /// <returns>Voltage, based on the current measuring range</returns>
        public double RawToVoltage(short val)
        {
            double maxVoltage;
            double resolution;

            maxVoltage = MaxVoltageFromMeasuringRange(MeasuringRange);

            resolution = 32768.0;
            return val * (maxVoltage / resolution);
        }

        /// <summary>
        /// Converts voltage to raw data.
        /// </summary>
        /// <param name="voltage">Input voltage</param>
        /// <returns>Corresponding raw value, based on the current measuring range</returns>
        public short VoltageToRaw(double voltage)
        {
            double resolution;
            double maxVoltage;
            maxVoltage = MaxVoltageFromMeasuringRange(MeasuringRange);
            resolution = 32768.0;
            return (short)Math.Round(voltage / (maxVoltage / resolution));
        }

        /// <summary>
        /// Returns the voltage assigned to the given MeasuringRange enumeration value.
        /// </summary>
        /// <param name="measuringRange">One of the <see cref="MeasuringRange"/> enumeration members</param>
        /// <returns>A voltage, as double.</returns>
        public double MaxVoltageFromMeasuringRange(MeasuringRange measuringRange)
        {
            double voltage;
            switch (measuringRange)
            {
                case MeasuringRange.FS6144:
                    voltage = 6.144;
                    break;
                case MeasuringRange.FS4096:
                    voltage = 4.096;
                    break;
                case MeasuringRange.FS2048:
                    voltage = 2.048;
                    break;
                case MeasuringRange.FS1024:
                    voltage = 1.024;
                    break;
                case MeasuringRange.FS0512:
                    voltage = 0.512;
                    break;
                case MeasuringRange.FS0256:
                    voltage = 0.256;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(measuringRange), "Unknown measuring range used");
            }

            return voltage;
        }

        /// <summary>
        /// Returns the sampling frequency in Hz for the given data rate enumeration member.
        /// </summary>
        /// <param name="dataRate">One of the <see cref="DataRate"/> enumeration members.</param>
        /// <returns>A frequency, in Hertz</returns>
        public double FrequencyFromDataRate(DataRate dataRate)
        {
            switch (dataRate)
            {
                case DataRate.SPS008:
                    return 8.0;
                case DataRate.SPS016:
                    return 16.0;
                case DataRate.SPS032:
                    return 32.0;
                case DataRate.SPS064:
                    return 64.0;
                case DataRate.SPS128:
                    return 128.0;
                case DataRate.SPS250:
                    return 250.0;
                case DataRate.SPS475:
                    return 475.0;
                case DataRate.SPS860:
                    return 860.0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataRate), "Unknown data rate used");
            }
        }

        /// <summary>
        /// Cleanup.
        /// Failing to dispose this class, especially when callbacks are active, may lead to undefined behavior.
        /// </summary>
        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                DisableAlrtReadyPin();
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }
    }
}
