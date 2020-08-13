// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;
using System.IO;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Ccs811
{
    /// <summary>
    /// Ultra-Low Power Digital Gas Sensor for
    /// Monitoring Indoor Air Quality
    /// Documentation can be found here: https://www.sciosense.com/products/environmental-sensors/ccs811-gas-sensor-solution/
    /// </summary>
    public class Ccs811Sensor : IDisposable
    {
        /// <summary>
        /// The first default I2C address when the Address pin is put to low
        /// </summary>
        public const int I2cFirstAddress = 0x5A;

        /// <summary>
        /// The second default I2C address when the Address pin is put to high
        /// </summary>
        public const int I2cSecondAddress = 0x5B;

        /// <summary>
        /// The typical operating speed for the bus
        /// Note that minimum is 10 KHz and the maximum is 400 KHz
        /// The device can operate in Stretching mode is the transfer is too fast.
        /// This stretching may not be well supported in all the hardware, in case of
        /// issue, it is recommended to lower the operating frequency
        /// </summary>
        public const int I2cTypicalFrequency = 100_000;

        private I2cDevice _i2cDevice = null;
        private GpioController _controller = null;
        private int _pinWake = -1;
        private int _pinInterruption = -1;
        private int _pinReset = -1;
        private bool _shouldDispose;
        private bool _running = false;
        private bool _isRunning = false;

        /// <summary>
        /// Event raised when interruption pin is selected
        /// </summary>
        /// <param name="sender">This sensor</param>
        /// <param name="args">The measurement</param>
        public delegate void MeasurementReadyHandler(object sender, MeasurementArgs args);

        /// <summary>
        /// The event handler for the measurement
        /// </summary>
        public event MeasurementReadyHandler MeasurementReady;

        /// <summary>
        /// The CCS811 sensor constructor
        /// </summary>
        /// <param name="i2cDevice">A valid I2C device</param>
        /// <param name="gpioController">An optional controller, either the default one will be used, either none will be created if any pin is used</param>
        /// <param name="pinWake">An awake pin, it is optional, this pin can be set to the ground if the sensor is always on</param>
        /// <param name="pinInterruption">An interruption pin when a measurement is ready, best use when you specify a threshold</param>
        /// <param name="pinReset">An optional hard reset pin</param>
        /// <param name="shouldDispose">Should the GPIO controller be disposed at the end</param>
        public Ccs811Sensor(I2cDevice i2cDevice, GpioController gpioController = null, int pinWake = -1, int pinInterruption = -1, int pinReset = -1, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice;
            _pinWake = pinWake;
            _pinInterruption = pinInterruption;
            _pinReset = pinReset;
            _shouldDispose = shouldDispose;
            // We need a GPIO controller only if we are using any of the pin
            if ((_pinInterruption >= 0) || (_pinReset >= 0) || (_pinWake >= 0))
            {
                _shouldDispose = _shouldDispose || gpioController == null;
                _controller = gpioController ?? new GpioController();
            }

            if (_pinWake >= 0)
            {
                _controller.OpenPin(_pinWake, PinMode.Output);
                _controller.Write(_pinWake, PinValue.High);
            }

            if (_pinReset >= 0)
            {
                _controller.OpenPin(_pinReset, PinMode.Output);
                _controller.Write(_pinReset, PinValue.Low);
                // Delays from documentation CCS811-Datasheet.pdf page 8
                // 15 micro second
                DelayHelper.DelayMicroseconds(15, true);
                _controller.Write(_pinReset, PinValue.High);
                // Need to wait at least 2 milliseconds before executing anything I2C
                Thread.Sleep(2);
            }

            // Initialization flow page 29
            // https://www.sciosense.com/wp-content/uploads/2020/01/CCS811-Application-Note-Programming-and-interfacing-guide.pdf
            // do a soft reset
            Span<byte> toReset = stackalloc byte[4]
            {
                0x11,
                0xE5,
                0x72,
                0x8A
            };

            WriteRegister(Register.SW_RESET, toReset);
            // Wait 2 milliseconds as per documentation
            Thread.Sleep(2);
            if (HardwareIdentification != 0x81)
            {
                throw new IOException($"CCS811 does not have a valid ID: {HardwareIdentification}. ID must be 0x81.");
            }

            if ((HardwareVersion & 0xF0) != 0x10)
            {
                throw new IOException($"CCS811 does not have a valid version: {HardwareVersion}, should be 0x1X where any X is valid.");
            }

            // Read status
            if (!Status.HasFlag(Status.APP_VALID))
            {
                throw new IOException($"CCS811 has no application firmware loaded.");
            }

            // Switch to app mode and wait 1 millisecond according to doc
            WriteRegister(Register.APP_START);
            Thread.Sleep(1);

            if (!Status.HasFlag(Status.FW_MODE))
            {
                throw new IOException($"CCS811 is not in application mode.");
            }

            // Set interrupt if the interruption pin is valid
            if (_pinInterruption >= 0)
            {
                _controller.OpenPin(_pinInterruption, PinMode.Input);
                byte mode = 0b0000_1000;
                WriteRegister(Register.MEAS_MODE, mode);
                _controller.RegisterCallbackForPinValueChangedEvent(_pinInterruption, PinEventTypes.Falling, InterruptReady);
                _running = true;
                // Start a new thread to monitor the events
                new Thread(() =>
                {
                    _isRunning = true;
                    while (_running)
                    {
                        var res = _controller.WaitForEvent(_pinInterruption, PinEventTypes.Falling, new TimeSpan(0, 0, 0, 0, 50));
                        if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
                        {
                            InterruptReady(_controller, new PinValueChangedEventArgs(res.EventTypes, _pinInterruption));
                            // We know we won't get any new measurement in next 250 milliseconds at least
                            // Waiting to make sure the sensor will have time to remove the interrupt pin
                            Thread.Sleep(50);
                        }
                    }

                    _isRunning = false;
                }).Start();
            }
        }

        private void InterruptReady(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            MeasurementArgs measurement = new MeasurementArgs();
            var success = TryReadGasData(out VolumeConcentration eCo2, out VolumeConcentration eTvoc, out ElectricCurrent current, out int adc);
            measurement.MeasurementSuccess = success;
            measurement.EquivalentCO2 = eCo2;
            measurement.EquivalentTotalVolatileOrganicCompound = eTvoc;
            measurement.RawCurrentSelected = current;
            measurement.RawAdcReading = adc;
            MeasurementReady?.Invoke(this, measurement);
        }

        private Status Status => (Status)ReadRegister(Register.STATUS);

        /// <summary>
        /// Set or get the operation mode
        /// </summary>
        public OperationMode OperationMode
        {
            get
            {
                var mode = ReadRegister(Register.MEAS_MODE);
                mode = (byte)((mode >> 4) & 0b0000_0111);
                return (OperationMode)mode;
            }

            set
            {
                var mode = ReadRegister(Register.MEAS_MODE);
                // Clear previous mode
                mode = (byte)(mode & 0b1000_1111);
                mode = (byte)(mode | (((byte)(value)) << 4));
                WriteRegister(Register.MEAS_MODE, mode);

            }
        }

        /// <summary>
        /// Get the error
        /// </summary>
        /// <returns></returns>
        public Error Error => (Error)ReadRegister(Register.ERROR_ID);

        /// <summary>
        /// Get the hardware identification, it has to be 0x81
        /// </summary>
        public byte HardwareIdentification => ReadRegister(Register.HW_ID);

        /// <summary>
        /// Is the hardware interrupt enabled
        /// </summary>
        public bool InterruptEnable => _pinInterruption >= 0;

        /// <summary>
        /// Hardware version should be 0x1X, any X seems valid
        /// </summary>
        public byte HardwareVersion => ReadRegister(Register.HW_Version);

        /// <summary>
        /// Get the application version
        /// </summary>
        public Version ApplicationVersion
        {
            get
            {
                Span<byte> version = stackalloc byte[2];
                ReadRegister(Register.FW_App_Version, version);
                return new Version(version[0] >> 4, version[0] & 0b0000_1111, version[1]);
            }
        }

        /// <summary>
        /// Get the boot loader version
        /// </summary>
        public Version BootloaderVersion
        {
            get
            {
                Span<byte> version = stackalloc byte[2];
                ReadRegister(Register.FW_Boot_Version, version);
                return new Version(version[0] >> 4, version[0] & 0b0000_1111, version[1]);
            }
        }

        /// <summary>
        /// Is the wake feature enabled
        /// </summary>
        public bool WakeEnable => _pinWake >= 0;

        /// <summary>
        /// Do we have data ready to read?
        /// </summary>
        public bool IsDataReady
        {
            get
            {
                var status = (Status)ReadRegister(Register.STATUS);
                return status.HasFlag(Status.DATA_READY);
            }
        }

        /// <summary>
        /// Read the equivalent CO2 in ppm and equivalent Total Volatile Compound in ppb
        /// </summary>
        /// <param name="equivalentCO2">The equivalent CO2 (eCO2) output range for CCS811 is from
        /// 400ppm up to 29206ppm.</param>
        /// <param name="equivalentTotalVolatileOrganicCompound">The equivalent Total Volatile Organic Compound (eTVOC)
        /// output range for CCS811 is from 0ppb up to 32768ppb</param>
        /// <param name="rawCurrentSelected">Raw data containing the value of the
        /// current through the sensor(0μA to 63μA)</param>
        /// <param name="rawAdcReading">Raw data containing  the
        /// readings of the voltage across the sensor with the selected
        /// current(1023 = 1.65V) where 1023 is the maximum value</param>
        /// <returns>True if success</returns>
        public bool TryReadGasData(out VolumeConcentration equivalentCO2, out VolumeConcentration equivalentTotalVolatileOrganicCompound, out ElectricCurrent rawCurrentSelected, out int rawAdcReading)
        {
            int equivalentCO2InPpm = -1;
            int equivalentTotalVolatileOrganicCompoundInPpb = -1;
            int rawCurrent = -1;
            rawAdcReading = -1;
            Span<byte> toRead = stackalloc byte[8];
            ReadRegister(Register.ALG_RESULT_DATA, toRead);
            if (toRead[5] != (byte)Error.NoError)
            {
                equivalentCO2 = VolumeConcentration.Zero;
                equivalentTotalVolatileOrganicCompound = VolumeConcentration.Zero;
                rawCurrentSelected = ElectricCurrent.Zero;
                return false;
            }

            equivalentCO2InPpm = BinaryPrimitives.ReadInt16BigEndian(toRead.Slice(0, 2));
            equivalentTotalVolatileOrganicCompoundInPpb = BinaryPrimitives.ReadInt16BigEndian(toRead.Slice(2, 2));
            rawCurrent = toRead[6] >> 2;
            rawAdcReading = ((toRead[6] & 0b0000_0011) << 2) + toRead[7];
            equivalentCO2 = VolumeConcentration.FromPartsPerMillion(equivalentCO2InPpm);
            equivalentTotalVolatileOrganicCompound = VolumeConcentration.FromPartsPerBillion(equivalentTotalVolatileOrganicCompoundInPpb);
            rawCurrentSelected = ElectricCurrent.FromMicroamperes(rawCurrent);
            return ((equivalentCO2InPpm >= 400) && (equivalentCO2InPpm <= 29206) && (equivalentTotalVolatileOrganicCompoundInPpb >= 0) && (equivalentTotalVolatileOrganicCompoundInPpb <= 32768));
        }

        /// <summary>
        /// Read the equivalent CO2 in ppm and equivalent Total Volatile Compound in ppb
        /// </summary>
        /// <param name="equivalentCO2">The equivalent CO2 (eCO2) output range for CCS811 is from
        /// 400ppm up to 29206ppm.</param>
        /// <param name="equivalentTotalVolatileOrganicCompound">The equivalent Total Volatile Organic Compound (eTVOC)
        /// output range for CCS811 is from 0ppb up to 32768ppb</param>
        /// <returns>True if success</returns>
        public bool TryReadGasData(out VolumeConcentration equivalentCO2, out VolumeConcentration equivalentTotalVolatileOrganicCompound)
        {
            return TryReadGasData(out equivalentCO2, out equivalentTotalVolatileOrganicCompound, out ElectricCurrent curr, out int adc);
        }

        /// <summary>
        /// Get or set the encoded version of the current baseline used in Algorithm Calculations
        /// </summary>
        /// <remarks>A previously stored value may be written back to this two byte
        /// register and the Algorithms will use the new value in its
        /// calculations(until it adjusts it as part of its internal Automatic
        /// Baseline Correction). Please refer to documentation to understand when to restore a
        /// previous baseline: https://www.sciosense.com/wp-content/uploads/2020/01/Application-Note-Baseline-Save-and-Restore-on-CCS811.pdf</remarks>
        public ushort BaselineAlgorithmCalculation
        {
            get
            {
                Span<byte> baseline = stackalloc byte[2];
                ReadRegister(Register.BASELINE, baseline);
                return BinaryPrimitives.ReadUInt16BigEndian(baseline);
            }

            set
            {
                Span<byte> baseline = stackalloc byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(baseline, value);
                WriteRegister(Register.BASELINE, baseline);
            }
        }

        /// <summary>
        /// Set the environmental data, this is impacting the equivalent calculation
        /// of the gas.
        /// </summary>
        /// <param name="temperature">The temperature</param>
        /// <param name="humidity">The relative humidity, best to use Percent from 0 to 100</param>
        public void SetEnvironmentData(Temperature temperature, Ratio humidity)
        {
            if ((humidity.Percent < 0) || (humidity.Percent > 100))
            {
                throw new ArgumentException($"Humidity can only be between 0 and 100.");
            }

            Span<byte> environment = stackalloc byte[4];
            // convert the humidity first
            ConvertForEnvironement(humidity.Percent, environment.Slice(0, 2));
            // Cap the temperature to the minimum or maximum according to documentation
            var temp = temperature.DegreesCelsius;
            temp += 25;
            temp = Math.Max(temp, 0);
            temp = Math.Min(temp, 127);
            ConvertForEnvironement(temp, environment.Slice(2, 2));
            WriteRegister(Register.ENV_DATA, environment);
        }

        private void ConvertForEnvironement(double toConvert, Span<byte> converted)
        {
            // Format is 7 bits for the integer part and 9 bits for the decimal one
            byte integerPart = (byte)toConvert;
            double decimalPart = toConvert - integerPart;
            converted[0] = (byte)(integerPart << 1);
            // There a 9 bits with fractions so we have to sample in 1/512 = 0.001953125
            uint decimalPartUint = ((uint)(decimalPart / 0.001953125)) & 0x1FF;
            converted[0] = (byte)(converted[0] | (decimalPartUint >> 8));
            converted[1] = (byte)(decimalPartUint & 0xFF);
        }

        /// <summary>
        /// Set the threshold for the equivalent CO2. The pinInterrupt should be existing so
        /// interruptions are activated. If not, then the function will return false
        /// </summary>
        /// <param name="lowEquivalentCO2">The low value for the threshold</param>
        /// <param name="highEquivalentCO2">The high value for the threshold</param>
        /// <returns>True if success</returns>
        /// <remarks>Difference between the low and high value should be more than 50. This is called
        /// the hysteresis value.</remarks>
        public bool SetThreshold(VolumeConcentration lowEquivalentCO2, VolumeConcentration highEquivalentCO2)
        {
            if (_pinInterruption < 0)
            {
                return false;
            }

            if (!IsPpmValidThreshold(lowEquivalentCO2))
            {
                throw new ArgumentException($"{lowEquivalentCO2} can only be between 0 and {ushort.MaxValue}");
            }

            if (!IsPpmValidThreshold(highEquivalentCO2))
            {
                throw new ArgumentException($"{highEquivalentCO2} can only be between 0 and {ushort.MaxValue}");
            }

            if (lowEquivalentCO2 > highEquivalentCO2)
            {
                var temp = highEquivalentCO2;
                highEquivalentCO2 = lowEquivalentCO2;
                lowEquivalentCO2 = temp;
            }

            if (highEquivalentCO2 - lowEquivalentCO2 < VolumeConcentration.FromPartsPerMillion(50))
            {
                throw new ArgumentException($"{highEquivalentCO2}-{lowEquivalentCO2} should be more than 50");
            }

            Span<byte> toSend = stackalloc byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(toSend.Slice(0, 2), (ushort)lowEquivalentCO2.PartsPerMillion);
            BinaryPrimitives.WriteUInt16BigEndian(toSend.Slice(2, 2), (ushort)highEquivalentCO2.PartsPerMillion);
            WriteRegister(Register.THRESHOLDS, toSend);
            // Activate the interrupt threshold as well
            byte mode = ReadRegister(Register.MEAS_MODE);
            mode |= 0b0000_0100;
            WriteRegister(Register.MEAS_MODE, mode);

            return !Status.HasFlag(Status.ERROR);
        }

        private bool IsPpmValidThreshold(VolumeConcentration ppm)
        {
            if ((ppm < VolumeConcentration.Zero) || (ppm > VolumeConcentration.FromPartsPerMillion(ushort.MaxValue)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Dispose the sensor
        /// </summary>
        public void Dispose()
        {
            _running = false;
            while (_isRunning)
            {
                Thread.Sleep(1);
            }

            if (_pinInterruption >= 0)
            {
                _controller.UnregisterCallbackForPinValueChangedEvent(_pinInterruption, InterruptReady);
            }

            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }
            else
            {
                if (_pinInterruption >= 0)
                {
                    _controller.ClosePin(_pinInterruption);
                }

                if (_pinReset >= 0)
                {
                    _controller.ClosePin(_pinReset);
                }

                if (_pinWake >= 0)
                {
                    _controller.ClosePin(_pinWake);
                }
            }
        }

        #region I2C operations

        private void WakeUpDevice()
        {
            if (_pinWake >= 0)
            {
                _controller.Write(_pinWake, PinValue.Low);
                // Doc says wait 50 micro seconds
                DelayHelper.DelayMicroseconds(50, true);
            }
        }

        private void SleepDownDevice()
        {
            if (_pinWake >= 0)
            {
                _controller.Write(_pinWake, PinValue.High);
                // Doc says wait 20 micro seconds
                DelayHelper.DelayMicroseconds(50, true);
            }
        }

        private void WriteRegister(Register register)
        {
            WakeUpDevice();
            _i2cDevice.WriteByte((byte)register);
            SleepDownDevice();
        }

        private void WriteRegister(Register register, byte data)
        {
            Span<byte> toSend = stackalloc byte[2]
            {
                (byte)register,
                data
            };

            WakeUpDevice();
            _i2cDevice.Write(toSend);
            SleepDownDevice();
        }

        private void WriteRegister(Register register, ReadOnlySpan<byte> data)
        {
            Span<byte> toSend = stackalloc byte[data.Length + 1];
            toSend[0] = (byte)register;
            WakeUpDevice();
            data.CopyTo(toSend.Slice(1));
            _i2cDevice.Write(toSend);
            SleepDownDevice();
        }

        private byte ReadRegister(Register register)
        {
            WakeUpDevice();
            _i2cDevice.WriteByte((byte)register);
            var ret = _i2cDevice.ReadByte();
            SleepDownDevice();
            return ret;
        }

        private void ReadRegister(Register register, Span<byte> dataRead)
        {
            WakeUpDevice();
            _i2cDevice.WriteByte((byte)register);
            _i2cDevice.Read(dataRead);
            SleepDownDevice();
        }
        #endregion

    }
}
