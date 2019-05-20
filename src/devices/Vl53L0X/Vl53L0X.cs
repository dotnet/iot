// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This code has been ported from the Dexter Industries Python code
// https://github.com/DexterInd/DI_Sensors/blob/master/Python/di_sensors/VL53L0X.py
// It is based as well on the offical ST Microelectronics API in C
// https://www.st.com/content/st_com/en/products/embedded-software/proximity-sensors-software/stsw-img005.html

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Iot.Device.Vl53L0X
{
    public class Vl53L0X : IDisposable
    {
        /// <summary>
        /// The default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x29;
        // Default address can be found in documentation
        // page 18 with value 0x52 >> 1 = 0x29
        private readonly I2cDevice _i2cDevice;
        private readonly bool _autoDisposable;
        private byte _stopData;
        private readonly int _operationTimeout;
        private uint _measurementTimingBudgetMicrosecond;
        private bool _continuousInitialized = false;
        private bool _highResolution;
        private Precision _precision;

        /// <summary>
        /// Get the sensor information including internal signal and distance offsets
        /// </summary>
        public Information Information { get; internal set; }

        /// <summary>
        /// Used to find a clean measurement when reading in single shot 
        /// </summary>
        public int MaxTryReadSingle { get; set; }

        /// <summary>
        /// Set/Get high resolution measurement
        /// </summary>
        public bool HighResolution
        {
            get
            {
                return _highResolution;
            }
            set
            {
                _highResolution = value;
                WriteRegister((byte)Registers.SYSTEM_RANGE_CONFIG, _highResolution ? (byte)0x01 : (byte)0x00);
            }
        }

        /// <summary>
        /// Create a VL53L0X Sensor class
        /// </summary>
        /// <param name="i2cDevice">The I2C Device</param>
        /// <param name="operationTimeoutMilliseconds">Timeout for reading data, by default 500 milliseonds</param>
        /// <param name="autoDisposable">True to dispose the I2C Device at dispose</param>
        public Vl53L0X(I2cDevice i2cDevice, int operationTimeoutMilliseconds = 500, bool autoDisposable = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentException($"{nameof(i2cDevice)} can't be null.");
            _autoDisposable = autoDisposable;
            _operationTimeout = operationTimeoutMilliseconds;
            Reset();
            Init();
            GetInfo();
            MaxTryReadSingle = 3;
            // Set longer range
            Precision = Precision.LongRange;
        }

        /// <summary>
        /// The sensor can be changed for other I2C Address, this function allows to change it
        /// </summary>
        /// <param name="i2cDevice">The current I2C Device</param>
        /// <param name="newAddress">The new I2C Address from 0x00 to 0x7F</param>
        public static void ChangeI2cAddress(I2cDevice i2cDevice, byte newAddress)
        {
            if (newAddress > 0x7F)
                throw new ArgumentException($"{nameof(newAddress)} can't exceed 0x7F");

            try
            {
                i2cDevice.Write(new byte[] { (byte)Registers.I2C_SLAVE_DEVICE_ADDRESS, newAddress });
            }
            catch (IOException ex)
            {
                throw new IOException($"Can't change I2C Address to {newAddress}", ex);
            }
        }

        /// <summary>
        /// Start continuous ranging measurements. If periodMilliseconds is 0
        /// continuous back-to-back mode is used (the sensor takes measurements as
        /// often as possible) otherwise, continuous timed mode is used, with the given
        /// inter-measurement period in milliseconds determining how often the sensor
        /// takes a measurement.
        /// </summary>
        /// <param name="periodMilliseconds">The interval period between 2 measurements. Default is 0</param>
        public void StartContinuousMeasurement(int periodMilliseconds = 0)
        {
            // Initialize the measurement
            InitMeasurement();
            _continuousInitialized = true;
            if (periodMilliseconds != 0)
            {
                // If we have a period, then change the register for continuous measurement
                var osc_calibrate_val = ReadUInt16((byte)Registers.OSC_CALIBRATE_VAL);
                if (osc_calibrate_val != 0)
                    periodMilliseconds *= osc_calibrate_val;
                WriteUInt32((byte)Registers.SYSTEM_INTERMEASUREMENT_PERIOD, (uint)periodMilliseconds);
                WriteRegister((byte)Registers.SYSRANGE_START, 0x04);
            }
            else
            {
                // If we don't just start the continuous measurement
                WriteRegister((byte)Registers.SYSRANGE_START, 0x02);
            }
        }

        /// <summary>
        /// Reads the measurement when the mode is set to continuious. 
        /// </summary>
        /// <returns>The range in millimeters, a maximum value is returned depending on the various settings</returns>
        private ushort ReadContinuousMeasrurementMillimeters()
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            var expirationMilliseconds = stopWatch.ElapsedMilliseconds + _operationTimeout;
            while ((ReadByte((byte)Registers.RESULT_INTERRUPT_STATUS) & 0x07) == 0)
            {
                if (stopWatch.ElapsedMilliseconds > expirationMilliseconds)
                    throw new IOException($"{nameof(ReadContinuousMeasrurementMillimeters)} timeout error");
            }
            // assumptions: Linearity Corrective Gain is 1000 (default)
            // fractional ranging is not enabled
            var range = ReadUInt16((byte)Registers.RESULT_RANGE_STATUS + 10);
            WriteRegister((byte)Registers.SYSTEM_INTERRUPT_CLEAR, 0x01);
            return range;
        }

        /// <summary>
        /// Get the distance depending on the measurement mode
        /// </summary>
        public ushort Distance => MeasurementMode == MeasurementMode.Continuous ? DistanceContinous : GetDistanceOnce(true);

        /// <summary>
        /// Get/Set the measurement mode used to return the distance property
        /// </summary>
        public MeasurementMode MeasurementMode { get; set; }

        /// <summary>
        /// Get a distance in millimeters from the continous measurement feature.
        /// It is recommended to used this method to gethigher quality measurements
        /// </summary>
        /// <returns>Returns the distance in millimeters, if any error, returns the maximum range so 8190</returns>
        public ushort DistanceContinous
        {
            get
            {
                if (!_continuousInitialized)
                {
                    StartContinuousMeasurement();
                }
                return ReadContinuousMeasrurementMillimeters();
            }
        }

        /// <summary>
        /// Get a distance in millimeters
        /// </summary>
        /// <param name="multipleReads">True if you want multiple try to get a clean value</param>
        /// <returns>Returns the distance in millimeters, if any error, returns the maximum range so 8190</returns>
        public ushort GetDistanceOnce(bool multipleReads = false)
        {
            try
            {
                var value = DistanceSingleMeasurement;
                // Sensor can read maximum range while there is an object in front
                // Make an average with a few reading withg the good readings
                // Catch any exception and return the maximum range
                Double average = 0.0;
                int goodCount = 0;
                if (multipleReads)
                {
                    for (int i = 0; i < MaxTryReadSingle; i++)
                    {
                        if (value < (ushort)OperationRange.OutOfRange)
                        {
                            goodCount++;
                            average += value;
                        }
                        value = DistanceSingleMeasurement;
                    }

                }
                else
                {
                    return value;
                }
                return goodCount != 0 ? (ushort)(average / goodCount) : (ushort)OperationRange.OutOfRange;
            }
            catch (IOException)
            {
                return (ushort)OperationRange.OutOfRange;
            }

        }

        /// <summary>
        /// Performs a single-shot range measurement and returns the reading in millimeters 
        /// </summary>
        /// <returns>Returns distance in millimeters</returns>
        public ushort DistanceSingleMeasurement
        {
            get
            {
                InitMeasurement();
                WriteRegister((byte)Registers.SYSRANGE_START, 0x01);
                // "Wait until start bit has been cleared"
                Stopwatch stopWatch = Stopwatch.StartNew();
                var expirationMilliseconds = stopWatch.ElapsedMilliseconds + _operationTimeout;

                while ((ReadByte((byte)Registers.SYSRANGE_START) & 0x01) == 0x01)
                {
                    if (stopWatch.ElapsedMilliseconds > expirationMilliseconds)
                        throw new IOException($"{nameof(DistanceSingleMeasurement)} timeout error");
                }
                return ReadContinuousMeasrurementMillimeters();
            }
        }

        /// <summary>
        /// Set the VCSEL (vertical cavity surface emitting laser) pulse period for the
        /// given period type (pre-range or final range) to the given value in PCLKs.
        /// Longer periods seem to increase the potential range of the sensor.
        /// Valid values are (even numbers only):
        /// pre:  12 to 18 (initialized default: 14)
        /// final: 8 to 14 (initialized default: 10)
        /// based on official API
        /// </summary>
        /// <param name="type">The type of VCSEL</param>
        /// <param name="periodPclks">The period part of the supported periods. Be aware periods are a bit different depending on the VCSEL you are targetting.</param>
        /// <returns></returns>
        public bool SetVcselPulsePeriod(VcselType type, PeriodPulse periodPclks)
        {
            var vcselPeriodReg = EncoreVcselPeriod((byte)periodPclks);
            var enables = GetSequenceStepEnables();
            var timeouts = GetSequenceStepTimeouts(enables.PreRange);

            // "When the VCSEL period for the pre or final range is changed,
            // the corresponding timeout must be read from the device using
            // the current VCSEL period, then the new VCSEL period can be
            // applied. The timeout then must be written back to the device
            // using the new VCSEL period.
            // For the MSRC timeout, the same applies - this timeout being
            // dependant on the pre-range vcsel period.
            switch (type)
            {
                case VcselType.VcselPeriodPreRange:
                    // "Set phase check limits"
                    if (periodPclks == PeriodPulse.Period12)
                        WriteRegister((byte)Registers.PRE_RANGE_CONFIG_VALID_PHASE_HIGH, 0x18);
                    else if (periodPclks == PeriodPulse.Period14)
                        WriteRegister((byte)Registers.PRE_RANGE_CONFIG_VALID_PHASE_HIGH, 0x30);
                    else if (periodPclks == PeriodPulse.Period16)
                        WriteRegister((byte)Registers.PRE_RANGE_CONFIG_VALID_PHASE_HIGH, 0x40);
                    else if (periodPclks == PeriodPulse.Period18)
                        WriteRegister((byte)Registers.PRE_RANGE_CONFIG_VALID_PHASE_HIGH, 0x50);
                    else
                        return false;

                    WriteRegister((byte)Registers.PRE_RANGE_CONFIG_VALID_PHASE_LOW, 0x08);
                    // apply new VCSEL period
                    WriteRegister((byte)Registers.PRE_RANGE_CONFIG_VCSEL_PERIOD, vcselPeriodReg);

                    // update timeouts
                    var newPreRangeTimeoutMclks = TimeoutMicrosecondsToMclks(timeouts.PreRangeMicroseconds, (byte)periodPclks);
                    WriteUInt16((byte)Registers.PRE_RANGE_CONFIG_TIMEOUT_MACROP_HI, (ushort)EncodeTimeout(newPreRangeTimeoutMclks));
                    var newMsrcTimeoutMclks = TimeoutMicrosecondsToMclks(timeouts.MsrcDssTccMicroseconds, (byte)periodPclks);
                    if (newMsrcTimeoutMclks > 256)
                        WriteRegister((byte)Registers.MSRC_CONFIG_TIMEOUT_MACROP, 255);
                    else
                        WriteRegister((byte)Registers.MSRC_CONFIG_TIMEOUT_MACROP, (byte)(newMsrcTimeoutMclks - 1));
                    break;
                case VcselType.VcselPeriodFinalRange:
                    if (periodPclks == PeriodPulse.Period08)
                    {
                        WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VALID_PHASE_HIGH, 0x10);
                        WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VALID_PHASE_LOW, 0x08);
                        WriteRegister((byte)Registers.GLOBAL_CONFIG_VCSEL_WIDTH, 0x02);
                        WriteRegister((byte)Registers.ALGO_PHASECAL_CONFIG_TIMEOUT, 0x0C);
                        WriteRegister(0xFF, 0x01);
                        WriteRegister((byte)Registers.ALGO_PHASECAL_LIM, 0x30);
                        WriteRegister(0xFF, 0x00);
                    }
                    else if (periodPclks == PeriodPulse.Period10)
                    {
                        WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VALID_PHASE_HIGH, 0x28);
                        WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VALID_PHASE_LOW, 0x08);
                        WriteRegister((byte)Registers.GLOBAL_CONFIG_VCSEL_WIDTH, 0x03);
                        WriteRegister((byte)Registers.ALGO_PHASECAL_CONFIG_TIMEOUT, 0x09);
                        WriteRegister(0xFF, 0x01);
                        WriteRegister((byte)Registers.ALGO_PHASECAL_LIM, 0x20);
                        WriteRegister(0xFF, 0x00);
                    }
                    else if (periodPclks == PeriodPulse.Period12)
                    {
                        WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VALID_PHASE_HIGH, 0x38);
                        WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VALID_PHASE_LOW, 0x08);
                        WriteRegister((byte)Registers.GLOBAL_CONFIG_VCSEL_WIDTH, 0x03);
                        WriteRegister((byte)Registers.ALGO_PHASECAL_CONFIG_TIMEOUT, 0x08);
                        WriteRegister(0xFF, 0x01);
                        WriteRegister((byte)Registers.ALGO_PHASECAL_LIM, 0x20);
                        WriteRegister(0xFF, 0x00);
                    }
                    else if (periodPclks == PeriodPulse.Period14)
                    {
                        WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VALID_PHASE_HIGH, 0x48);
                        WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VALID_PHASE_LOW, 0x08);
                        WriteRegister((byte)Registers.GLOBAL_CONFIG_VCSEL_WIDTH, 0x03);
                        WriteRegister((byte)Registers.ALGO_PHASECAL_CONFIG_TIMEOUT, 0x07);
                        WriteRegister(0xFF, 0x01);
                        WriteRegister((byte)Registers.ALGO_PHASECAL_LIM, 0x20);
                        WriteRegister(0xFF, 0x00);
                    }

                    // apply new VCSEL period
                    WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VCSEL_PERIOD, vcselPeriodReg);

                    // update timeouts
                    // For the final range timeout, the pre-range timeout
                    // must be added. To do this both final and pre-range
                    // timeouts must be expressed in macro periods MClks
                    // because they have different vcsel periods.
                    var newFinalRangeTimeoutMclks = TimeoutMicrosecondsToMclks(timeouts.FinalRangeMicroseconds, (byte)periodPclks);
                    if (enables.PreRange)
                    {
                        newFinalRangeTimeoutMclks += timeouts.PreRangeMclks;
                    }
                    WriteUInt16((byte)Registers.FINAL_RANGE_CONFIG_TIMEOUT_MACROP_HI, (ushort)EncodeTimeout(newFinalRangeTimeoutMclks));
                    break;
                default:
                    return false;
            }

            // Finally, the timing budget must be re-applied, using the previous calculated value
            SetMeasurementTimingBudget(_measurementTimingBudgetMicrosecond);
            // Perform the phase calibration. This is needed after changing on vcsel period.
            var sequence_config = ReadByte((byte)Registers.SYSTEM_SEQUENCE_CONFIG);
            WriteRegister((byte)Registers.SYSTEM_SEQUENCE_CONFIG, 0x02);
            PerformSingleRefCalibration((byte)Registers.SYSRANGE_START);
            WriteRegister((byte)Registers.SYSTEM_SEQUENCE_CONFIG, sequence_config);
            return true;
        }

        /// <summary>
        /// Set the type of precision needed for measurement
        /// </summary>
        /// <param name="precision">The type of precision needed</param>
        public Precision Precision
        {
            get { return _precision; }

            set
            {
                _precision = value;
                switch (_precision)
                {
                    case Precision.ShortRange:
                        HighResolution = false;
                        SetSignalRateLimit(0.25);
                        SetVcselPulsePeriod(VcselType.VcselPeriodPreRange, PeriodPulse.Period14);
                        SetVcselPulsePeriod(VcselType.VcselPeriodFinalRange, PeriodPulse.Period10);
                        break;
                    case Precision.LongRange:
                        HighResolution = false;
                        SetSignalRateLimit(0.1);
                        SetVcselPulsePeriod(VcselType.VcselPeriodPreRange, PeriodPulse.Period18);
                        SetVcselPulsePeriod(VcselType.VcselPeriodFinalRange, PeriodPulse.Period14);
                        break;
                    case Precision.HighPrecision:
                        HighResolution = true;
                        SetSignalRateLimit(0.1);
                        SetVcselPulsePeriod(VcselType.VcselPeriodPreRange, PeriodPulse.Period18);
                        SetVcselPulsePeriod(VcselType.VcselPeriodFinalRange, PeriodPulse.Period14);
                        break;
                    default:
                        break;
                }
            }

        }

        /// <summary>
        /// Reset the sensor
        /// </summary>
        private void Reset()
        {
            WriteRegister((byte)Registers.SOFT_RESET_GO2_SOFT_RESET_N, 0x00);
            Thread.Sleep(5);
            WriteRegister((byte)Registers.SOFT_RESET_GO2_SOFT_RESET_N, 0x01);
            Thread.Sleep(5);
        }

        /// <summary>
        /// Initialization of the sensor, include a long sequence of writing
        /// which is coming from the offical API with no more information on the
        /// registers and their functions. Few can be reversed engineer based on 
        /// other functions but not all
        /// </summary>
        private void Init()
        {
            // Prepare the initialization
            WriteRegister((byte)Registers.VHV_CONFIG_PAD_SCL_SDA__EXTSUP_HV, ReadByte((byte)(Registers.VHV_CONFIG_PAD_SCL_SDA__EXTSUP_HV) | 0x01));
            WriteRegister(0x88, 0x00);
            WriteRegister((byte)Registers.POWER_MANAGEMENT_GO1_POWER_FORCE, 0x01);
            WriteRegister(0xFF, 0x01);
            WriteRegister((byte)Registers.SYSRANGE_START, 0x00);
            _stopData = ReadByte(0x91);
            WriteRegister((byte)Registers.SYSRANGE_START, 0x01);
            WriteRegister(0xFF, 0x00);
            WriteRegister((byte)Registers.POWER_MANAGEMENT_GO1_POWER_FORCE, 0x00);

            // disable SIGNAL_RATE_MSRC (bit 1) and SIGNAL_RATE_PRE_RANGE (bit 4) limit checks
            WriteRegister((byte)Registers.MSRC_CONFIG_CONTROL, ReadByte((byte)(Registers.MSRC_CONFIG_CONTROL) | 0x12));
            // set final range signal rate limit to 0.25 MCPS (million counts per second)
            SetSignalRateLimit(0.25);

            // Start intialization sequence
            WriteRegister((byte)Registers.SYSTEM_SEQUENCE_CONFIG, 0xFF);
            var spad = GetSpadInfo();

            // Get the SPAD information
            _i2cDevice.WriteByte((byte)Registers.GLOBAL_CONFIG_SPAD_ENABLES_REF_0);
            // Allocate 1 more byte as it will be used to send back the configuration
            Span<byte> ReferenceSpadMap = stackalloc byte[7];
            // Skip the first byte for reading, it will be used for writting
            _i2cDevice.Read(ReferenceSpadMap.Slice(1));

            // Set the squads, prepare the registers firsts
            WriteRegister(0xFF, 0x01);
            WriteRegister((byte)Registers.DYNAMIC_SPAD_REF_EN_START_OFFSET, 0x00);
            WriteRegister((byte)Registers.DYNAMIC_SPAD_NUM_REQUESTED_REF_SPAD, 0x2C);
            WriteRegister(0xFF, 0x00);
            WriteRegister((byte)Registers.GLOBAL_CONFIG_REF_EN_START_SELECT, 0xB4);

            int firstSpadToEnable = spad.TypeIsAperture ? 12 : 0;
            int squadEnable = 0;

            for (int i = 0; i < 48; i++)
            {
                if ((i < firstSpadToEnable) || (squadEnable == spad.Count))
                {
                    // Enable only the SPAD that ware not yet
                    ReferenceSpadMap[i / 8 + 1] &= (byte)~(1 << (i % 8));
                }
                else if ((ReferenceSpadMap[i / 8 + 1] >> ((i % 8)) & 0x1) == 0x01)
                {
                    squadEnable++;
                }
            }
            // Write back the SPAD configuration
            ReferenceSpadMap[0] = (byte)Registers.GLOBAL_CONFIG_SPAD_ENABLES_REF_0;
            _i2cDevice.Write(ReferenceSpadMap);

            // Those commands come from the offical API C code from STM
            // Documentation does not provide register names
            // In the main API, this block of command are hard coded
            // The names of Registers provided are based on know ones.
            WriteRegister(0xFF, 0x01);
            WriteRegister((byte)Registers.SYSRANGE_START, 0x00);

            WriteRegister(0xFF, 0x00);
            WriteRegister((byte)Registers.SYSTEM_RANGE_CONFIG, 0x00);
            WriteRegister(0x10, 0x00);
            WriteRegister(0x11, 0x00);

            WriteRegister(0x24, 0x01);
            WriteRegister(0x25, 0xFF);
            WriteRegister(0x75, 0x00);

            WriteRegister(0xFF, 0x01);
            WriteRegister((byte)Registers.DYNAMIC_SPAD_NUM_REQUESTED_REF_SPAD, 0x2C);
            WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VALID_PHASE_HIGH, 0x00);
            WriteRegister(0x30, 0x20);

            WriteRegister(0xFF, 0x00);
            WriteRegister(0x30, 0x09);
            WriteRegister(0x54, 0x00);
            WriteRegister(0x31, 0x04);
            WriteRegister(0x32, 0x03);
            WriteRegister(0x40, 0x83);
            WriteRegister(0x46, 0x25);
            WriteRegister(0x60, 0x00);
            WriteRegister(0x27, 0x00);
            WriteRegister((byte)Registers.PRE_RANGE_CONFIG_VCSEL_PERIOD, 0x06);
            WriteRegister((byte)Registers.PRE_RANGE_CONFIG_TIMEOUT_MACROP_HI, 0x00);
            WriteRegister((byte)Registers.PRE_RANGE_CONFIG_TIMEOUT_MACROP_LO, 0x96);
            WriteRegister((byte)Registers.PRE_RANGE_CONFIG_VALID_PHASE_LOW, 0x08);
            WriteRegister((byte)Registers.PRE_RANGE_CONFIG_VALID_PHASE_HIGH, 0x30);
            WriteRegister((byte)Registers.PRE_RANGE_CONFIG_SIGMA_THRESH_HI, 0x00);
            WriteRegister((byte)Registers.PRE_RANGE_CONFIG_SIGMA_THRESH_LO, 0x00);
            WriteRegister((byte)Registers.PRE_RANGE_MIN_COUNT_RATE_RTN_LIMIT, 0x00);
            WriteRegister(0x65, 0x00);
            WriteRegister(0x66, 0xA0);

            WriteRegister(0xFF, 0x01);
            WriteRegister(0x22, 0x32);
            WriteRegister(0x47, 0x14);
            WriteRegister(0x49, 0xFF);
            WriteRegister(0x4A, 0x00);

            WriteRegister(0xFF, 0x00);
            WriteRegister(0x7A, 0x0A);
            WriteRegister(0x7B, 0x00);
            WriteRegister(0x78, 0x21);

            WriteRegister(0xFF, 0x01);
            WriteRegister(0x23, 0x34);
            WriteRegister(0x42, 0x00);
            WriteRegister(0x44, 0xFF);
            WriteRegister(0x45, 0x26);
            WriteRegister(0x46, 0x05);
            WriteRegister(0x40, 0x40);
            WriteRegister(0x0E, 0x06);
            WriteRegister(0x20, 0x1A);
            WriteRegister(0x43, 0x40);

            WriteRegister(0xFF, 0x00);
            WriteRegister(0x34, 0x03);
            WriteRegister(0x35, 0x44);

            WriteRegister(0xFF, 0x01);
            WriteRegister(0x31, 0x04);
            WriteRegister(0x4B, 0x09);
            WriteRegister(0x4C, 0x05);
            WriteRegister(0x4D, 0x04);

            WriteRegister(0xFF, 0x00);
            WriteRegister(0x44, 0x00);
            WriteRegister(0x45, 0x20);
            WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VALID_PHASE_LOW, 0x08);
            WriteRegister((byte)Registers.FINAL_RANGE_CONFIG_VALID_PHASE_HIGH, 0x28);
            WriteRegister(0x67, 0x00);
            WriteRegister(0x70, 0x04);
            WriteRegister(0x71, 0x01);
            WriteRegister(0x72, 0xFE);
            WriteRegister(0x76, 0x00);
            WriteRegister(0x77, 0x00);

            WriteRegister(0xFF, 0x01);
            WriteRegister(0x0D, 0x01);

            WriteRegister(0xFF, 0x00);
            WriteRegister((byte)Registers.POWER_MANAGEMENT_GO1_POWER_FORCE, 0x01);
            WriteRegister(0x01, 0xF8);

            WriteRegister(0xFF, 0x01);
            WriteRegister(0x8E, 0x01);
            WriteRegister((byte)Registers.SYSRANGE_START, 0x01);
            WriteRegister(0xFF, 0x00);
            WriteRegister((byte)Registers.POWER_MANAGEMENT_GO1_POWER_FORCE, 0x00);
            // End of initialization sequence took from official API docuementation

            // Set the interruptions
            WriteRegister((byte)Registers.SYSTEM_INTERRUPT_CONFIG_GPIO, 0x04);
            // active low
            WriteRegister((byte)Registers.GPIO_HV_MUX_ACTIVE_HIGH, ReadByte((byte)(Registers.GPIO_HV_MUX_ACTIVE_HIGH) & ~0x10));
            WriteRegister((byte)Registers.SYSTEM_INTERRUPT_CLEAR, 0x01);

            // Calculate the measurement timing budget
            _measurementTimingBudgetMicrosecond = GetMeasurementTimingBudget();
            // Disable MSRC and TCC by default
            // MSRC = Minimum Signal Rate Check
            // TCC = Target CentreCheck
            WriteRegister((byte)Registers.SYSTEM_SEQUENCE_CONFIG, 0xE8);
            // Recalculate timing budget based on previous measurement
            SetMeasurementTimingBudget(_measurementTimingBudgetMicrosecond);

            // Calibration section
            WriteRegister((byte)Registers.SYSTEM_SEQUENCE_CONFIG, 0x01);
            if (!PerformSingleRefCalibration(0x40))
                throw new Exception($"{nameof(Init)} can't make calibration");

            WriteRegister((byte)Registers.SYSTEM_SEQUENCE_CONFIG, 0x02);
            if (!PerformSingleRefCalibration((byte)Registers.SYSRANGE_START))
                throw new Exception($"{nameof(Init)} can't make calibration");

            // Restore the previous Sequence Config
            WriteRegister((byte)Registers.SYSTEM_SEQUENCE_CONFIG, 0xE8);
        }

        /// <summary>
        /// Create the Info class. Initialization and closing sequences
        /// are coming form the official API
        /// </summary>
        private void GetInfo()
        {
            // Initialization sequance
            WriteRegister(0x80, 0x01);
            WriteRegister(0xFF, 0x01);
            WriteRegister(0x00, 0x00);
            WriteRegister(0xFF, 0x06);
            var ids = ReadByte(0x83);
            WriteRegister(0x83, (byte)(ids | 4));
            WriteRegister(0xFF, 0x07);
            WriteRegister(0x81, 0x01);
            Thread.Sleep(30);
            WriteRegister(0x80, 0x01);
            // Reading the data from the sensor
            Information = new Information()
            {
                ModuleId = GetDeviceInfo(InfoDevice.ModuleId),
                Revision = new Version(GetDeviceInfo(InfoDevice.PartUIDUpper), GetDeviceInfo(InfoDevice.PartUIDLower)),
                ProductId = GetProductId(),
                SignalRateMeasFixed1104_400_Micrometers = GetSignalRate(),
                DistMeasFixed1104_400_Micrometers = GetDistanceFixed()
            };
            // Closing sequence
            WriteRegister(0x81, 0x00);
            WriteRegister(0xFF, 0x06);
            ids = ReadByte(0x83);
            WriteRegister(0x83, (byte)(ids & 0xfb));
            WriteRegister(0xFF, 0x01);
            WriteRegister(0x00, 0x01);
            WriteRegister(0xFF, 0x00);
            WriteRegister(0x80, 0x00);
        }

        private byte GetDeviceInfo(InfoDevice infoDevice)
        {
            WriteRegister((byte)Registers.GET_INFO_DEVICE, (byte)infoDevice);
            ReadStrobe();
            return ReadByte((byte)Registers.DEVICE_INFO_READING);
        }

        private uint GetSignalRate()
        {
            WriteRegister((byte)Registers.GET_INFO_DEVICE, (byte)(InfoDevice.SignalRate1));
            ReadStrobe();
            var intermediate = ReadUIn32(0x90);
            var SignalRateMeasFixed1104_400_mm = (intermediate & 0x0000000ff) << 8;
            WriteRegister((byte)Registers.GET_INFO_DEVICE, (byte)(InfoDevice.SignalRate2));
            ReadStrobe();
            intermediate = ReadUIn32(0x90);
            SignalRateMeasFixed1104_400_mm |= ((intermediate & 0xff000000) >> 24);
            return SignalRateMeasFixed1104_400_mm;
        }

        private uint GetDistanceFixed()
        {
            WriteRegister((byte)Registers.GET_INFO_DEVICE, (byte)(InfoDevice.DistanceFixed1));
            ReadStrobe();
            var intermediate = ReadUIn32(0x90);
            var DistMeasFixed1104_400_mm = (intermediate & 0x0000000ff) << 8;
            WriteRegister((byte)Registers.GET_INFO_DEVICE, (byte)(InfoDevice.DistanceFixed2));
            ReadStrobe();
            intermediate = ReadUIn32(0x90);
            DistMeasFixed1104_400_mm |= ((intermediate & 0xff000000) >> 24);
            return DistMeasFixed1104_400_mm;
        }

        /// <summary>
        /// Get the product ID. Coming from the official API
        /// </summary>
        /// <returns></returns>
        private string GetProductId()
        {
            // This is according to the SDK. Encoding and reading the sensor
            // Need to be done by 4 bytes each time and then decode them step by step
            char[] product = new char[18];
            WriteRegister((byte)Registers.GET_INFO_DEVICE, 0x77);
            ReadStrobe();
            var retTemp = ReadUIn32(0x90);
            product[0] = (char)((retTemp >> 25) & 0x07f);
            product[1] = (char)((retTemp >> 18) & 0x07f);
            product[2] = (char)((retTemp >> 11) & 0x07f);
            product[3] = (char)((retTemp >> 4) & 0x07f);
            byte tmp = (byte)((retTemp & 0x0F) << 3);
            WriteRegister((byte)Registers.GET_INFO_DEVICE, 0x78);
            ReadStrobe();
            retTemp = ReadUIn32(0x90);
            product[4] = (char)(tmp + ((retTemp >> 29) & 0x07f));
            product[5] = (char)((retTemp >> 22) & 0x07f);
            product[6] = (char)((retTemp >> 15) & 0x07f);
            product[7] = (char)((retTemp >> 8) & 0x07f);
            product[8] = (char)((retTemp >> 1) & 0x07f);
            tmp = (byte)((retTemp & 0x001) << 6);
            WriteRegister((byte)Registers.GET_INFO_DEVICE, 0x79);
            ReadStrobe();
            retTemp = ReadUIn32(0x90);
            product[9] = (char)(tmp + ((retTemp >> 26) & 0x07f));
            product[10] = (char)((retTemp >> 19) & 0x07f);
            product[11] = (char)((retTemp >> 12) & 0x07f);
            product[12] = (char)((retTemp >> 5) & 0x07f);
            tmp = (byte)((retTemp & 0x01f) << 2);
            WriteRegister((byte)Registers.GET_INFO_DEVICE, 0x7A);
            ReadStrobe();
            retTemp = ReadUIn32(0x90);
            product[13] = (char)(tmp + ((retTemp >> 30) & 0x07f));
            product[14] = (char)((retTemp >> 23) & 0x07f);
            product[15] = (char)((retTemp >> 16) & 0x07f);
            product[16] = (char)((retTemp >> 9) & 0x07f);
            product[17] = (char)((retTemp >> 2) & 0x07f);
            return new string(product, 0, 18);
        }

        /// <summary>
        /// Used to read some data from the sensor, it needs to be ready for the operation
        /// Exception raised if timeout is overdue
        /// </summary>
        private void ReadStrobe()
        {
            WriteRegister(0x83, 0x00);
            Stopwatch stopWatch = Stopwatch.StartNew();
            var expirationMilliseconds = stopWatch.ElapsedMilliseconds + _operationTimeout; ;
            while (ReadByte(0x83) == 0x00)
            {
                if (stopWatch.ElapsedMilliseconds > expirationMilliseconds)
                    throw new IOException($"{nameof(ReadStrobe)} timeout error");
            }
            WriteRegister(0x83, 0x01);
        }

        /// <summary>
        /// Used to intialize a measurement. From the official API
        /// </summary>
        private void InitMeasurement()
        {
            WriteRegister((byte)Registers.POWER_MANAGEMENT_GO1_POWER_FORCE, 0x01);
            WriteRegister(0xFF, 0x01);
            WriteRegister((byte)Registers.SYSRANGE_START, 0x00);
            WriteRegister(0x91, _stopData);
            WriteRegister((byte)Registers.SYSRANGE_START, 0x01);
            WriteRegister(0xFF, 0x00);
            WriteRegister((byte)Registers.POWER_MANAGEMENT_GO1_POWER_FORCE, 0x00);
        }

        /// <summary>
        /// Get the reference SPAD (single photon avalanche diode) count and type
        /// </summary>
        /// <returns>Returns the SPAD information</returns>
        private SpadInfo GetSpadInfo()
        {
            // Initialization sequence before reading, from official API
            WriteRegister((byte)Registers.POWER_MANAGEMENT_GO1_POWER_FORCE, 0x01);
            WriteRegister(0xFF, 0x01);
            WriteRegister((byte)Registers.SYSRANGE_START, 0x00);
            WriteRegister(0xFF, 0x06);
            WriteRegister(0x83, (byte)(ReadByte(0x83) | 0x04));
            WriteRegister(0xFF, 0x07);
            WriteRegister(0x81, 0x01);
            WriteRegister((byte)Registers.POWER_MANAGEMENT_GO1_POWER_FORCE, 0x01);
            // Reading SPAD informartion
            WriteRegister((byte)Registers.GET_INFO_DEVICE, (byte)InfoDevice.SquadInfo);
            ReadStrobe();
            var tmp = ReadByte(0x92);
            var retSquad = new SpadInfo()
            {
                Count = (byte)(tmp & 0x7f),
                TypeIsAperture = (byte)((tmp >> 7) & 0x01) == 0x01
            };
            // Closing sequence
            WriteRegister(0x81, 0x00);
            WriteRegister(0xFF, 0x06);
            WriteRegister(0x83, ReadByte(0x83 & ~0x04));
            WriteRegister(0xFF, 0x01);
            WriteRegister((byte)Registers.SYSRANGE_START, 0x01);
            WriteRegister(0xFF, 0x00);
            WriteRegister((byte)Registers.POWER_MANAGEMENT_GO1_POWER_FORCE, 0x00);

            return retSquad;
        }

        /// <summary>
        /// Get the measurement timing budget in microseconds. Based on official API
        /// </summary>
        /// <returns>The measurement timing budget in microseconds</returns>
        private uint GetMeasurementTimingBudget()
        {
            // Note that this is different than the value in SetMEasurementTimingBudget
            // This is setup like this in the API
            const int StartOverhead = 1910;
            const int EndOverhead = 960;
            const int MsrcOverhead = 660;
            const int TccOverhead = 590;
            const int DssOverhead = 690;
            const int PreRangeOverhead = 660;
            const int FinalRangeOverhead = 550;

            // "Start and end overhead times always present"
            uint budget_us = StartOverhead + EndOverhead;

            var enables = GetSequenceStepEnables();
            var timeouts = GetSequenceStepTimeouts(enables.PreRange);

            if (enables.Tcc)
                budget_us += (timeouts.MsrcDssTccMicroseconds + TccOverhead);

            if (enables.Dss)
                budget_us += 2 * (timeouts.MsrcDssTccMicroseconds + DssOverhead);
            else if (enables.Msrc)
                budget_us += (timeouts.MsrcDssTccMicroseconds + MsrcOverhead);

            if (enables.PreRange)
                budget_us += (timeouts.PreRangeMicroseconds + PreRangeOverhead);

            if (enables.FinalRange)
                budget_us += (timeouts.FinalRangeMicroseconds + FinalRangeOverhead);

            // store for internal reuse
            _measurementTimingBudgetMicrosecond = budget_us;
            return budget_us;
        }

        /// <summary>
        /// Set the measurement timing budget in microseconds, which is the time allowed
        /// for one measurement the ST API and this library take care of splitting the
        /// timing budget among the sub-steps in the ranging sequence. A longer timing
        /// budget allows for more accurate measurements. Increasing the budget by a
        /// factor of N decreases the range measurement standard deviation by a factor of
        /// sqrt(N). Defaults to about 33 milliseconds the minimum is 20 ms.
        /// based on VL53L0X_set_measurement_timing_budget_micro_seconds() from API
        /// </summary>
        /// <param name="budgetMicroseconds">Take the exisitng measurement budget to calculate the new one</param>
        /// <returns>True in case all goes right</returns>
        private bool SetMeasurementTimingBudget(uint budgetMicroseconds)
        {
            // note that this is different than the value in GetMeasurementTimingBudget function
            const int StartOverhead = 1320;
            const int EndOverhead = 960;
            const int MsrcOverhead = 660;
            const int TccOverhead = 590;
            const int DssOverhead = 690;
            const int PreRangeOverhead = 660;
            const int FinalRangeOverhead = 550;
            // The minimum period
            const int MinTimingBudget = 20000;
            // We can't have a shorter period than the minimum one
            if (budgetMicroseconds < MinTimingBudget)
                return false;

            uint used_budget_us = StartOverhead + EndOverhead;

            // Get the enablers and timeouts
            var enables = GetSequenceStepEnables();
            var timeouts = GetSequenceStepTimeouts(enables.PreRange);

            if (enables.Tcc)
                used_budget_us += (timeouts.MsrcDssTccMicroseconds + TccOverhead);

            if (enables.Dss)
            {
                used_budget_us += 2 * (timeouts.MsrcDssTccMicroseconds + DssOverhead);
            }
            else if (enables.Msrc)
            {
                used_budget_us += (timeouts.MsrcDssTccMicroseconds + MsrcOverhead);
            }

            if (enables.PreRange)
                used_budget_us += (timeouts.PreRangeMicroseconds + PreRangeOverhead);

            if (enables.FinalRange)
            {
                used_budget_us += FinalRangeOverhead;

                // Note that the final range timeout is determined by the timing
                // budget and the sum of all other timeouts within the sequence.
                // If there is no room for the final range timeout, then an error
                // will be set. Otherwise the remaining time will be applied to
                // the final range.

                // Requested timeout too big.
                if (used_budget_us > budgetMicroseconds)
                    return false;

                uint final_range_timeout_us = (uint)(budgetMicroseconds - used_budget_us);

                // For the final range timeout, the pre-range timeout
                // must be added. To do this both final and pre-range
                // timeouts must be expressed in macro periods MClks
                // because they have different vcsel periods.
                uint final_range_timeout_mclks = TimeoutMicrosecondsToMclks(final_range_timeout_us, timeouts.FinalRangeVcselPeriodPclks);

                if (enables.PreRange)
                    final_range_timeout_mclks += timeouts.PreRangeMclks;

                // Finally write the new timing budget
                WriteUInt16((byte)Registers.FINAL_RANGE_CONFIG_TIMEOUT_MACROP_HI, (ushort)EncodeTimeout(final_range_timeout_mclks));

                // Store for internal reuse if any change require to recalculate it again
                _measurementTimingBudgetMicrosecond = budgetMicroseconds;
            }
            return true;
        }

        /// <summary>
        /// Get all the step sequence states (enabled or not)
        /// </summary>
        /// <returns>State of step enabled</returns>
        private StepEnables GetSequenceStepEnables()
        {
            StepEnables reEnables = new StepEnables();
            var sequence_config = ReadByte((byte)Registers.SYSTEM_SEQUENCE_CONFIG);
            reEnables.Tcc = ((sequence_config >> 4) & 0x01) == 0x01;
            reEnables.Dss = ((sequence_config >> 3) & 0x01) == 0x01;
            reEnables.Msrc = ((sequence_config >> 2) & 0x01) == 0x01;
            reEnables.PreRange = ((sequence_config >> 6) & 0x01) == 0x01;
            reEnables.FinalRange = ((sequence_config >> 7) & 0x01) == 0x01;
            return reEnables;
        }

        /// <summary>
        /// Get the setp timeouts
        /// </summary>
        /// <param name="preRange">True for to include the pre range</param>
        /// <returns>The step timeouts</returns>
        private StepTimeouts GetSequenceStepTimeouts(bool preRange)
        {
            StepTimeouts sequence = new StepTimeouts();
            sequence.PreRangeVcselPeriodPclks = GetVcselPulsePeriod(VcselType.VcselPeriodPreRange);
            sequence.MsrcDssTccMclks = (uint)(ReadByte((byte)Registers.MSRC_CONFIG_TIMEOUT_MACROP) + 1);
            sequence.MsrcDssTccMicroseconds = TimeoutMclksToMicroseconds(sequence.MsrcDssTccMclks, sequence.PreRangeVcselPeriodPclks);
            sequence.PreRangeMclks = DecodeTimeout(ReadUInt16((byte)Registers.PRE_RANGE_CONFIG_TIMEOUT_MACROP_HI));
            sequence.PreRangeMicroseconds = TimeoutMclksToMicroseconds(sequence.PreRangeMclks, sequence.PreRangeVcselPeriodPclks);
            sequence.FinalRangeVcselPeriodPclks = GetVcselPulsePeriod(VcselType.VcselPeriodFinalRange);
            sequence.FinalRangeMclks = DecodeTimeout(ReadUInt16((byte)Registers.FINAL_RANGE_CONFIG_TIMEOUT_MACROP_HI));

            if (preRange)
                sequence.FinalRangeMclks -= sequence.PreRangeMclks;

            sequence.FinalRangeMicroseconds = TimeoutMclksToMicroseconds(sequence.FinalRangeMclks, sequence.FinalRangeVcselPeriodPclks);
            return sequence;
        }

        /// <summary>
        /// Convert sequence step timeout from MCLKs to microseconds with given VCSEL period in PCLKs 
        /// </summary>
        /// <param name="timeoutPeriodMclks"></param>
        /// <param name="vcselPeriodPclks"></param>
        /// <returns></returns>
        private uint TimeoutMclksToMicroseconds(uint timeoutPeriodMclks, byte vcselPeriodPclks)
        {
            var macroPeriodNanoseconds = CalcMacroPeriod(vcselPeriodPclks);
            return (uint)(((timeoutPeriodMclks * macroPeriodNanoseconds) + (macroPeriodNanoseconds / 2)) / 1000);
        }

        /// <summary>
        /// Convert sequence step timeout from microseconds to MCLKs with given VCSEL period in PCLKs 
        /// </summary>
        /// <param name="timeoutPeriodMicroseconds">The timeout period in microseconds</param>
        /// <param name="vcselPeriodPclks">The VCSEL period in PCLKs</param>
        /// <returns>Returns the converted timeout</returns>
        private uint TimeoutMicrosecondsToMclks(uint timeoutPeriodMicroseconds, byte vcselPeriodPclks)
        {
            var macroPeriodNanoseconds = CalcMacroPeriod(vcselPeriodPclks);
            return (uint)(((timeoutPeriodMicroseconds * 1000) + (macroPeriodNanoseconds / 2)) / macroPeriodNanoseconds);
        }

        /// <summary>
        /// Get the VCSEL pulse period in PCLKs for the given period type. 
        /// </summary>
        /// <param name="type">The VCSEL period to decode</param>
        /// <returns>The decoded period</returns>
        private byte GetVcselPulsePeriod(VcselType type)
        {
            switch (type)
            {
                case VcselType.VcselPeriodPreRange:
                    return DecodeVcselPeriod(ReadByte((byte)Registers.PRE_RANGE_CONFIG_VCSEL_PERIOD));
                case VcselType.VcselPeriodFinalRange:
                    return DecodeVcselPeriod(ReadByte((byte)Registers.FINAL_RANGE_CONFIG_VCSEL_PERIOD));
                default:
                    // Should not arrive
                    return byte.MaxValue;
            }
        }

        /// <summary>
        /// Encode VCSEL pulse period register value from period in PCLKs 
        /// </summary>
        /// <param name="periodPclks">The priod in PCLKs</param>
        /// <returns>the period encoded</returns>
        private byte EncoreVcselPeriod(byte periodPclks) => (byte)((periodPclks >> 1) - 1);

        /// <summary>
        /// Decode sequence step timeout in MCLKs from register value 
        /// format: (LSByte * 2^MSByte) + 1
        /// </summary>
        /// <param name="valueToDecode"></param>
        /// <returns>The decoded value</returns>
        private uint DecodeTimeout(int valueToDecode) => (uint)(((valueToDecode & 0x00FF) << ((valueToDecode & 0xFF00) >> 8)) + 1);

        /// <summary>
        /// Encode sequence step timeout register value from timeout in MCLKs 
        /// From official API:
        /// format: (LSByte * 2^MSByte) + 1
        /// </summary>
        /// <param name="timeoutMclks">The timeout in MCLKs to encode</param>
        /// <returns>The encoded value</returns>
        private uint EncodeTimeout(uint timeoutMclks)
        {
            uint ls_byte = 0;
            uint ms_byte = 0;

            if (timeoutMclks > 0)
            {
                ls_byte = timeoutMclks - 1;
                while (((ls_byte) & 0xFFFFFF00) > 0)
                {
                    ls_byte /= 2;
                    ms_byte++;
                }

                return ((ms_byte << 8) | (ls_byte & 0xFF));
            }
            return 0;
        }

        /// <summary>
        /// Decode VCSEL (vertical cavity surface emitting laser) pulse period in PCLKs 
        /// </summary>
        /// <param name="valueToDecode">The VCSEL period to decode</param>
        /// <returns>The decoded period</returns>
        private byte DecodeVcselPeriod(byte valueToDecode) => (byte)((valueToDecode + 1) << 1);

        /// <summary>
        /// Calculate macro period in *nanoseconds* from VCSEL period in PCLKs 
        /// From the official API:
        /// PLL_period_ps = 1655; macro_period_vclks = 2304
        /// </summary>
        /// <param name="vcselPeriodPclks">the VCSEL perios in PCKLs</param>
        /// <returns>The macro period in nanoseconds</returns>
        private uint CalcMacroPeriod(uint vcselPeriodPclks)
        {
            return (uint)((((2304 * vcselPeriodPclks * 1655) + 500) / 1000));
        }

        /// <summary>
        /// Set the signal rate limit in MCPS
        /// </summary>
        /// <param name="limitMcps">The limit in MCPS, minimum value 0, maximum value 511.99</param>
        public void SetSignalRateLimit(double limitMcps)
        {

            if ((limitMcps < 0) || (limitMcps > 511.99))
                throw new ArgumentException($"{nameof(limitMcps)} can't be negative and more than 511.99");

            // Q9.7 fixed point format (9 integer bits, 7 fractional bits)
            WriteUInt16((byte)Registers.FINAL_RANGE_CONFIG_MIN_COUNT_RATE_RTN_LIMIT, (ushort)(limitMcps * (1 << 7)));
        }

        /// <summary>
        /// Perform a single reference calibration
        /// Based on the official API.
        /// </summary>
        /// <param name="vhvInitByte">The initialisation byte</param>
        /// <returns>True if all goes right</returns>
        private bool PerformSingleRefCalibration(byte vhvInitByte)
        {
            WriteRegister((byte)Registers.SYSRANGE_START, (byte)(0x01 | vhvInitByte));

            Stopwatch stopWatch = Stopwatch.StartNew();
            var expirationMilliseconds = stopWatch.ElapsedMilliseconds + _operationTimeout;
            // Check the status and make sure we have the intialization done
            while ((ReadByte((byte)Registers.RESULT_INTERRUPT_STATUS) & 0x07) == 0)
            {
                if (stopWatch.ElapsedMilliseconds > expirationMilliseconds)
                    return false;
            }

            // Set interrupt and get ready for measurement
            WriteRegister((byte)Registers.SYSTEM_INTERRUPT_CLEAR, 0x01);
            WriteRegister((byte)Registers.SYSRANGE_START, 0x00);
            return true;
        }

        public void Dispose()
        {
            if (_autoDisposable)
            {
                _i2cDevice.Dispose();
            }
        }

        private void WriteRegister(byte reg, byte param)
        {
            _i2cDevice.Write(new byte[] { reg, param });
        }

        private byte ReadByte(byte reg)
        {
            _i2cDevice.WriteByte(reg);
            return _i2cDevice.ReadByte();
        }

        private Int16 ReadInt16(byte reg)
        {
            Span<byte> outArray = stackalloc byte[2];
            _i2cDevice.WriteByte(reg);
            _i2cDevice.Read(outArray);
            return BinaryPrimitives.ReadInt16BigEndian(outArray);
        }

        private ushort ReadUInt16(byte reg)
        {
            Span<byte> outArray = stackalloc byte[2];
            _i2cDevice.WriteByte(reg);
            _i2cDevice.Read(outArray);
            return BinaryPrimitives.ReadUInt16BigEndian(outArray);
        }

        private uint ReadUIn32(byte reg)
        {
            Span<byte> outArray = stackalloc byte[4];
            _i2cDevice.WriteByte(reg);
            _i2cDevice.Read(outArray);
            return BinaryPrimitives.ReadUInt32BigEndian(outArray);
        }

        private void WriteInt16(byte reg, short data)
        {
            Span<byte> outArray = stackalloc byte[3];
            outArray[0] = reg;
            BinaryPrimitives.WriteInt16BigEndian(outArray.Slice(1), data);
            _i2cDevice.Write(outArray);
        }

        private void WriteUInt16(byte reg, ushort data)
        {
            Span<byte> outArray = stackalloc byte[3];
            outArray[0] = reg;
            BinaryPrimitives.WriteUInt16BigEndian(outArray.Slice(1), data);
            _i2cDevice.Write(outArray);
        }

        private void WriteInt32(byte reg, int data)
        {
            Span<byte> outArray = stackalloc byte[5];
            outArray[0] = reg;
            BinaryPrimitives.WriteInt32BigEndian(outArray.Slice(1), data);
            _i2cDevice.Write(outArray);
        }

        private void WriteUInt32(byte reg, uint data)
        {
            Span<byte> outArray = stackalloc byte[5];
            outArray[0] = reg;
            BinaryPrimitives.WriteUInt32BigEndian(outArray.Slice(1), data);
            _i2cDevice.Write(outArray);
        }
    }
}
