// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// This code has been mostly ported from the Arduino library code
// https://github.com/stm32duino/VL53L1X
// It is based as well on the offical ST Microelectronics API in C
// https://www.st.com/en/embedded-software/stsw-img007.html
using System;
using System.Buffers.Binary;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Vl53L1X
{
    /// <summary>
    /// Represents Vl53L1X
    /// </summary>
    public class Vl53L1X : IDisposable
    {
        /// <summary>
        /// The default I2C address
        /// </summary>
        public const ushort DefaultI2cAddress = (ushort)Registers.VL53L1X_DEFAULT_DEVICE_ADDRESS;

        private readonly int _operationTimeoutMilliseconds;
        private readonly bool _shouldDispose;
        private readonly I2cDevice _i2CDevice;

        private readonly byte[] _vl53L1XDefaultConfiguration =
        {
            0x00, /* 0x2d : set bit 2 and 5 to 1 for fast plus mode (1MHz I2C), else don't touch */
            0x01, /* 0x2e : bit 0 if I2C pulled up at 1.8V, else set bit 0 to 1 (pull up at AVDD) */
            0x01, /* 0x2f : bit 0 if GPIO pulled up at 1.8V, else set bit 0 to 1 (pull up at AVDD) */
            0x01, /* 0x30 : set bit 4 to 0 for active high interrupt and 1 for active low (bits 3:0 must be 0x1), use SetInterruptPolarity() */
            0x02, /* 0x31 : bit 1 = interrupt depending on the polarity, use CheckForDataReady() */
            0x00, /* 0x32 : not user-modifiable */ 0x02, /* 0x33 : not user-modifiable */
            0x08, /* 0x34 : not user-modifiable */ 0x00, /* 0x35 : not user-modifiable */
            0x08, /* 0x36 : not user-modifiable */ 0x10, /* 0x37 : not user-modifiable */
            0x01, /* 0x38 : not user-modifiable */ 0x01, /* 0x39 : not user-modifiable */
            0x00, /* 0x3a : not user-modifiable */ 0x00, /* 0x3b : not user-modifiable */
            0x00, /* 0x3c : not user-modifiable */ 0x00, /* 0x3d : not user-modifiable */
            0xff, /* 0x3e : not user-modifiable */ 0x00, /* 0x3f : not user-modifiable */
            0x0F, /* 0x40 : not user-modifiable */ 0x00, /* 0x41 : not user-modifiable */
            0x00, /* 0x42 : not user-modifiable */ 0x00, /* 0x43 : not user-modifiable */
            0x00, /* 0x44 : not user-modifiable */ 0x00, /* 0x45 : not user-modifiable */
            0x20, /* 0x46 : interrupt configuration 0->level low detection, 1-> level high, 2-> Out of window, 3->In window, 0x20-> New sample ready , TBC */
            0x0b, /* 0x47 : not user-modifiable */ 0x00, /* 0x48 : not user-modifiable */
            0x00, /* 0x49 : not user-modifiable */ 0x02, /* 0x4a : not user-modifiable */
            0x0a, /* 0x4b : not user-modifiable */ 0x21, /* 0x4c : not user-modifiable */
            0x00, /* 0x4d : not user-modifiable */ 0x00, /* 0x4e : not user-modifiable */
            0x05, /* 0x4f : not user-modifiable */ 0x00, /* 0x50 : not user-modifiable */
            0x00, /* 0x51 : not user-modifiable */ 0x00, /* 0x52 : not user-modifiable */
            0x00, /* 0x53 : not user-modifiable */ 0xc8, /* 0x54 : not user-modifiable */
            0x00, /* 0x55 : not user-modifiable */ 0x00, /* 0x56 : not user-modifiable */
            0x38, /* 0x57 : not user-modifiable */ 0xff, /* 0x58 : not user-modifiable */
            0x01, /* 0x59 : not user-modifiable */ 0x00, /* 0x5a : not user-modifiable */
            0x08, /* 0x5b : not user-modifiable */ 0x00, /* 0x5c : not user-modifiable */
            0x00, /* 0x5d : not user-modifiable */ 0x01, /* 0x5e : not user-modifiable */
            0xcc, /* 0x5f : not user-modifiable */ 0x0f, /* 0x60 : not user-modifiable */
            0x01, /* 0x61 : not user-modifiable */ 0xf1, /* 0x62 : not user-modifiable */
            0x0d, /* 0x63 : not user-modifiable */
            0x01, /* 0x64 : Sigma threshold MSB (mm in 14.2 format for MSB+LSB), use SetSigmaThreshold(), default value 90 mm  */
            0x68, /* 0x65 : Sigma threshold LSB */
            0x00, /* 0x66 : Min count Rate MSB (MCPS in 9.7 format for MSB+LSB), use SetSignalThreshold() */
            0x80, /* 0x67 : Min count Rate LSB */ 0x08, /* 0x68 : not user-modifiable */
            0xb8, /* 0x69 : not user-modifiable */ 0x00, /* 0x6a : not user-modifiable */
            0x00, /* 0x6b : not user-modifiable */
            0x00, /* 0x6c : Intermeasurement period MSB, 32 bits register, use SetIntermeasurementInMs() */
            0x00, /* 0x6d : Intermeasurement period */ 0x0f, /* 0x6e : Intermeasurement period */
            0x89, /* 0x6f : Intermeasurement period LSB */ 0x00, /* 0x70 : not user-modifiable */
            0x00, /* 0x71 : not user-modifiable */
            0x00, /* 0x72 : distance threshold high MSB (in mm, MSB+LSB), use SetD:tanceThreshold() */
            0x00, /* 0x73 : distance threshold high LSB */
            0x00, /* 0x74 : distance threshold low MSB ( in mm, MSB+LSB), use SetD:tanceThreshold() */
            0x00, /* 0x75 : distance threshold low LSB */ 0x00, /* 0x76 : not user-modifiable */
            0x01, /* 0x77 : not user-modifiable */ 0x0f, /* 0x78 : not user-modifiable */
            0x0d, /* 0x79 : not user-modifiable */ 0x0e, /* 0x7a : not user-modifiable */
            0x0e, /* 0x7b : not user-modifiable */ 0x00, /* 0x7c : not user-modifiable */
            0x00, /* 0x7d : not user-modifiable */ 0x02, /* 0x7e : not user-modifiable */
            0xc7, /* 0x7f : ROI center, use SetROI() */ 0xff, /* 0x80 : XY ROI (X=Width, Y=Height), use SetROI() */
            0x9B, /* 0x81 : not user-modifiable */ 0x00, /* 0x82 : not user-modifiable */
            0x00, /* 0x83 : not user-modifiable */ 0x00, /* 0x84 : not user-modifiable */
            0x01, /* 0x85 : not user-modifiable */ 0x00, /* 0x86 : clear interrupt, use ClearInterrupt() */
            0x00 /* 0x87 : start ranging, use StartRanging() or StopRanging(), If you want an automatic start after VL53L1X_init() call, put 0x40 in location 0x87 */
        };

        private bool _rangingInitialized;

        /// <summary>
        /// Creates a Vl53L1X sensor class
        /// </summary>
        /// <param name="i2CDevice">The I2C Device</param>
        /// <param name="operationTimeoutMilliseconds">Timeout for reading data, by default 500 milliseonds</param>
        /// <param name="shouldDispose">True to dispose the I2C Device at dispose</param>
        public Vl53L1X(I2cDevice i2CDevice, int operationTimeoutMilliseconds = 500, bool shouldDispose = true)
        {
            _operationTimeoutMilliseconds = operationTimeoutMilliseconds;
            _shouldDispose = shouldDispose;
            _i2CDevice = i2CDevice;
            WaitForBooted();
            InitSensor();
        }

        /// <summary>
        /// Gets the measured distance.
        /// If ranging has not been started yet, the function will automatically start the ranging feature of the device.
        /// </summary>
        public Length Distance => GetDistance();

        /// <summary>
        /// Gets the measured distance.
        /// If ranging has not been started yet, the function will automatically start the ranging feature of the device.
        /// </summary>
        /// <returns>Measured distance as <see cref="Length"/></returns>
        public Length GetDistance()
        {
            if (!_rangingInitialized)
            {
                StartRanging();
            }

            WaitForDataReady();
            ushort distance = ReadUInt16((ushort)Registers.VL53L1X_RESULT__FINAL_CROSSTALK_CORRECTED_RANGE_MM_SD0);
            ClearInterrupt();
            return Length.FromMillimeters(distance);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2CDevice.Dispose();
            }
        }

        private void WaitForBooted()
        {
            Stopwatch watch = Stopwatch.StartNew();

            while (BootState != BootState.Booted && watch.ElapsedMilliseconds < _operationTimeoutMilliseconds)
            {
                Thread.Sleep(10);
            }

            if (BootState != BootState.Booted)
            {
                throw new IOException(
                    $"The device did not boot withing the specified timeout of {_operationTimeoutMilliseconds} ms");

            }
        }

        /// <summary>
        /// The sensor can be changed for other I2C Address, this function allows to change it
        /// </summary>
        /// <param name="i2CDevice">The current I2C Device</param>
        /// <param name="newAddress">The new I2C Address from 0x00 to 0x7F</param>
        public static void ChangeI2CAddress(I2cDevice i2CDevice, byte newAddress)
        {
            if (newAddress > 0x7F)
            {
                throw new ArgumentException("Value can't exceed 0x7F", nameof(newAddress));
            }

            try
            {
                Span<byte> writeArray = new byte[3];
                BinaryPrimitives.WriteUInt16BigEndian(writeArray, (byte)Registers.VL53L1X_I2C_SLAVE__DEVICE_ADDRESS);
                writeArray[2] = newAddress;
                i2CDevice.Write(writeArray);
                Thread.Sleep(10);
            }
            catch (IOException ex)
            {
                throw new IOException($"Can't change I2C Address to {newAddress}", ex);
            }
        }

        private void InitSensor()
        {
            for (byte register = 0x2D; register <= 0x87; register++)
            {
                WriteRegister(register, _vl53L1XDefaultConfiguration[register - 0x2D]);
            }

            StartRanging();
            WaitForDataReady();

            ClearInterrupt();
            StopRanging();
            WriteRegister((ushort)Registers.VL53L1X_VHV_CONFIG__TIMEOUT_MACROP_LOOP_BOUND, 0x9); /* two bounds VHV */
            WriteRegister(0xB, 0); /* start VHV from the previous temperature */
        }

        /// <summary>
        /// This function starts the ranging distance operation which is continuous.
        /// The clear interrupt has to be done after each "get data" to allow the interrupt to be raised when the next data are
        /// ready.
        /// 1 = active high (default), 0 =active low.
        /// If required, use <see cref="InterruptPolarity" /> to change the interrupt polarity.
        /// </summary>
        public void StartRanging()
        {
            ClearInterrupt();
            WriteRegister((ushort)Registers.SYSTEM__MODE_START, 0x40);
            _rangingInitialized = true;
        }

        /// <summary>
        /// This function stops the ranging.
        /// </summary>
        public void StopRanging()
        {
            WriteRegister((ushort)Registers.SYSTEM__MODE_START, 0x00);
            _rangingInitialized = false;
        }

        /// <summary>
        /// This function returns whether to ranging is active or not
        /// </summary>
        /// <returns>True if ranging is active. Otherwise false</returns>
        public bool IsRangingActive
        {
            get
            {
                byte systemMode = ReadByte((ushort)Registers.SYSTEM__MODE_START);
                return systemMode == 0x40;
            }
        }

        /// <summary>
        /// Checks if the new ranging data are available by polling the dedicated register.
        /// </summary>
        private bool IsDataReady
        {
            get
            {
                byte temp = ReadByte((byte)Registers.GPIO__TIO_HV_STATUS);
                return temp != 0;
            }
        }

        /// <summary>
        /// This gets or sets the interrupt polarity. PinValue.High is default.
        /// </summary>
        public PinValue InterruptPolarity
        {
            get
            {
                byte polarity = ReadByte((ushort)Registers.GPIO_HV_MUX__CTRL);
                polarity = (byte)(polarity & 0x10);
                return polarity >> 4 == 1 ? PinValue.High : PinValue.Low;
            }

            set
            {
                byte temp = ReadByte((ushort)Registers.GPIO_HV_MUX__CTRL);
                temp = (byte)(temp & 0xEF);
                WriteRegister((ushort)Registers.GPIO_HV_MUX__CTRL, (byte)(temp | (byte)(((byte)value & 1) << 4)));
            }
        }

        /// <summary>
        /// This function clears the interrupt to be called after a ranging data reading, to arm the interrupt for the next
        /// data ready event.
        /// </summary>
        public void ClearInterrupt()
        {
            WriteRegister((ushort)Registers.SYSTEM__INTERRUPT_CLEAR, 0x01);
        }

        /// <summary>
        /// Gets or sets the timing budget in ms.
        /// The predefined values are 15, 33, 20, 50, 100, 200, and 500 <see cref="TimingBudget" />.
        /// This property must be set after changing the <see cref="Precision" />
        /// </summary>
        public TimingBudget TimingBudgetInMs
        {
            get
            {
                ushort temp = ReadUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI);
                switch (temp)
                {
                    case 0x001D:
                        return TimingBudget.Budget15;
                    case 0x0051:
                    case 0x001E:
                        return TimingBudget.Budget20;
                    case 0x00D6:
                    case 0x0060:
                        return TimingBudget.Budget33;
                    case 0x1AE:
                    case 0x00AD:
                        return TimingBudget.Budget50;
                    case 0x02E1:
                    case 0x01CC:
                        return TimingBudget.Budget100;
                    case 0x03E1:
                    case 0x02D9:
                        return TimingBudget.Budget200;
                    case 0x0591:
                    case 0x048F:
                        return TimingBudget.Budget500;
                    default:
                        return TimingBudget.BudgetUnknown;
                }
            }

            set
            {
                switch (Precision)
                {
                    case Precision.Short:
                        switch (value)
                        {
                            case TimingBudget.Budget15:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x01D);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x0027);
                                break;
                            case TimingBudget.Budget20:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x0051);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x006E);
                                break;
                            case TimingBudget.Budget33:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x00D6);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x006E);
                                break;
                            case TimingBudget.Budget50:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x1AE);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x01E8);
                                break;
                            case TimingBudget.Budget100:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x02E1);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x0388);
                                break;
                            case TimingBudget.Budget200:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x03E1);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x0496);
                                break;
                            case TimingBudget.Budget500:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x0591);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x05C1);
                                break;
                        }

                        break;
                    case Precision.Long:
                        switch (value)
                        {
                            case TimingBudget.Budget20:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x001E);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x0022);
                                break;
                            case TimingBudget.Budget33:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x0060);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x006E);
                                break;
                            case TimingBudget.Budget50:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x00AD);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x00C6);
                                break;
                            case TimingBudget.Budget100:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x01CC);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x01EA);
                                break;
                            case TimingBudget.Budget200:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x02D9);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x02F8);
                                break;
                            case TimingBudget.Budget500:
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_A_HI, 0x048F);
                                WriteUInt16((ushort)Registers.RANGE_CONFIG__TIMEOUT_MACROP_B_HI, 0x04A4);
                                break;
                            default:
                                throw new ArgumentException(
                                    $"A timing budget of {TimingBudget.Budget15} can only be set in distance mode {Precision.Short}.",
                                    nameof(value));
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the precision (1 = Short, 2 = Long).
        /// Short mode maximum distance is limited to 1.3m but results in a better ambient immunity.
        /// Long mode can range up to 4 m in the dark with a timing budget of200 ms.
        /// </summary>
        public Precision Precision
        {
            get
            {
                byte temp = ReadByte((ushort)Registers.PHASECAL_CONFIG__TIMEOUT_MACROP);
                if (temp != 0x14 && temp != 0xA)
                {
                    return Precision.Unknown;
                }

                return temp == 0x14 ? Precision.Short : Precision.Long;
            }

            set
            {
                var timingBudget = TimingBudgetInMs;

                switch (value)
                {
                    case Precision.Short:
                        WriteRegister((ushort)Registers.PHASECAL_CONFIG__TIMEOUT_MACROP, 0x14);
                        WriteRegister((ushort)Registers.RANGE_CONFIG__VCSEL_PERIOD_A, 0x07);
                        WriteRegister((ushort)Registers.RANGE_CONFIG__VCSEL_PERIOD_B, 0x05);
                        WriteRegister((ushort)Registers.RANGE_CONFIG__VALID_PHASE_HIGH, 0x38);
                        WriteUInt16((ushort)Registers.SD_CONFIG__WOI_SD0, 0x0705);
                        WriteUInt16((ushort)Registers.SD_CONFIG__INITIAL_PHASE_SD0, 0x0606);
                        break;
                    case Precision.Long:
                        WriteRegister((ushort)Registers.PHASECAL_CONFIG__TIMEOUT_MACROP, 0x0A);
                        WriteRegister((ushort)Registers.RANGE_CONFIG__VCSEL_PERIOD_A, 0x0F);
                        WriteRegister((ushort)Registers.RANGE_CONFIG__VCSEL_PERIOD_B, 0x0D);
                        WriteRegister((ushort)Registers.RANGE_CONFIG__VALID_PHASE_HIGH, 0xB8);
                        WriteUInt16((ushort)Registers.SD_CONFIG__WOI_SD0, 0x0F0D);
                        WriteUInt16((ushort)Registers.SD_CONFIG__INITIAL_PHASE_SD0, 0x0E0E);
                        break;
                }

                TimingBudgetInMs = timingBudget;
            }
        }

        /// <summary>
        /// Gets or sets the intermeasurement period in ms.
        /// Intermeasurement period must be >/= timing budget. This condition is not checked by the API,
        /// the customer has the duty to check the condition. Default = 100 ms.
        /// </summary>
        public ushort InterMeasurementInMs
        {
            get
            {
                uint temp = ReadUInt32((ushort)Registers.VL53L1X_SYSTEM__INTERMEASUREMENT_PERIOD);
                uint clockPll = ReadUInt32((ushort)Registers.VL53L1X_RESULT__OSC_CALIBRATE_VAL);

                clockPll &= 0x3FF;

                return (ushort)(temp / (clockPll * 1.065));
            }

            set
            {
                uint clockPll = ReadUInt32((ushort)Registers.VL53L1X_RESULT__OSC_CALIBRATE_VAL);
                clockPll &= 0x3FF;
                WriteUInt32((ushort)Registers.VL53L1X_SYSTEM__INTERMEASUREMENT_PERIOD,
                    (uint)(clockPll * value * 1.075));
            }
        }

        /// <summary>
        /// Gets the <see cref="BootState" /> of the device (1 = booted, 0 = not booted).
        /// </summary>
        public BootState BootState
        {
            get
            {
                byte bootStateVal = ReadByte((ushort)Registers.VL53L1X_FIRMWARE__SYSTEM_STATUS);
                return
                    bootStateVal > 0
                        ? BootState.Booted
                        : BootState
                            .NotBooted;
            }
        }

        /// <summary>
        /// Gets the sensor ID which must be 0xEEAC.
        /// </summary>
        public ushort SensorId
        {
            get
            {
                return ReadUInt16((ushort)Registers.VL53L1X_IDENTIFICATION__MODEL_ID);
            }
        }

        /// <summary>
        /// Returns the signal per SPAD in kcps/SPAD.
        /// </summary>
        /// <remarks>
        ///  Kcps is kilo count per second. kcps/SPAD is the return ambient rate measured by the VL53L1X.
        /// </remarks>
        public ushort SignalPerSpad
        {
            get
            {
                ushort signal =
                    ReadUInt16((ushort)Registers.VL53L1X_RESULT__PEAK_SIGNAL_COUNT_RATE_CROSSTALK_CORRECTED_MCPS_SD0);
                ushort spNb = ReadUInt16((ushort)Registers.VL53L1X_RESULT__DSS_ACTUAL_EFFECTIVE_SPADS_SD0);
                return (ushort)(2000.0 * signal / spNb);
            }
        }

        /// <summary>
        /// Returns the ambient per SPAD in kcps/SPAD.
        /// </summary>
        /// <remarks>
        ///  Kcps is kilo count per second. kcps/SPAD is the return ambient rate measured by the VL53L1X.
        /// </remarks>
        public ushort AmbientPerSpad
        {
            get
            {
                ushort ambientRate = ReadUInt16((ushort)Registers.RESULT__AMBIENT_COUNT_RATE_MCPS_SD);
                ushort spNb = ReadUInt16((ushort)Registers.VL53L1X_RESULT__DSS_ACTUAL_EFFECTIVE_SPADS_SD0);
                return (ushort)(2000.0 * ambientRate / spNb);
            }
        }

        /// <summary>
        /// Returns the signal in kcps.
        /// </summary>
        /// <remarks>
        ///  Kcps is kilo count per second.
        /// </remarks>
        public ushort SignalRate
        {
            get
            {
                return (ushort)(ReadUInt16((ushort)Registers
                    .VL53L1X_RESULT__PEAK_SIGNAL_COUNT_RATE_CROSSTALK_CORRECTED_MCPS_SD0) * 8);
            }
        }

        /// <summary>
        /// Returns the current number of enabled SPADs.
        /// </summary>
        public ushort SpadNb
        {
            get
            {
                return (ushort)(ReadUInt16((ushort)Registers.VL53L1X_RESULT__DSS_ACTUAL_EFFECTIVE_SPADS_SD0) >> 8);
            }
        }

        /// <summary>
        /// Returns the ambient rate in kcps.
        /// </summary>
        /// <remarks>
        ///  Kcps is kilo count per second.
        /// </remarks>
        public ushort AmbientRate
        {
            get
            {
                return (ushort)(ReadUInt16((ushort)Registers.RESULT__AMBIENT_COUNT_RATE_MCPS_SD) * 8);
            }
        }

        /// <summary>
        /// Returns the <see cref="RangeStatus" /> of the device.
        /// </summary>
        public RangeStatus RangeStatus
        {
            get
            {
                byte rgSt = ReadByte((ushort)Registers.VL53L1X_RESULT__RANGE_STATUS);

                rgSt = (byte)(rgSt & 0x1F);

                switch (rgSt)
                {
                    case 9:
                        return RangeStatus.NoError;
                    case 6:
                        return RangeStatus.SigmaFailure;
                    case 4:
                        return RangeStatus.SignalFailure;
                    case 5:
                        return RangeStatus.OutOfBounds;
                    case 7:
                        return RangeStatus.WrapAround;
                    default:
                        throw new Exception($"The returned range status code {rgSt} of the device is unknown.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the offset correction value.
        /// </summary>
        public Length Offset
        {
            get
            {
                short offset = ReadInt16((ushort)Registers.ALGO__PART_TO_PART_RANGE_OFFSET_MM);
                offset <<= 3;
                offset /= 32;
                return Length.FromMillimeters(offset);
            }

            set
            {
                var length = value.Millimeters;
                WriteInt16((ushort)Registers.ALGO__PART_TO_PART_RANGE_OFFSET_MM, (short)(length * 4));
                WriteInt16((ushort)Registers.MM_CONFIG__INNER_OFFSET_MM, 0x0);
                WriteInt16((ushort)Registers.MM_CONFIG__OUTER_OFFSET_MM, 0x0);
            }
        }

        /// <summary>
        /// Gets or sets the crosstalk correction value in cps.
        /// This is the number of photons reflected back from the cover glass in cps.
        /// </summary>
        public ushort Xtalk
        {
            get
            {
                ushort xTalk = ReadUInt16((ushort)Registers.ALGO__CROSSTALK_COMPENSATION_PLANE_OFFSET_KCPS);

                return (ushort)((xTalk * 1000) >> 9); /* * 1000 to convert kcps to cps and >> 9 (7.9 format) */
            }

            set
            {
                WriteUInt16((ushort)Registers.ALGO__CROSSTALK_COMPENSATION_X_PLANE_GRADIENT_KCPS, 0x0000);
                WriteUInt16((ushort)Registers.ALGO__CROSSTALK_COMPENSATION_Y_PLANE_GRADIENT_KCPS, 0x0000);
                WriteUInt16((ushort)Registers.ALGO__CROSSTALK_COMPENSATION_PLANE_OFFSET_KCPS,
                    (ushort)((value << 9) / 1000)); /* * << 9 (7.9 format) and /1000 to convert cps to kpcs */
            }
        }

        /// <summary>
        /// This function programs the threshold detection mode.
        /// For example:
        /// SetDistanceThreshold(dev,100,300, WindowDetectionMode.Below): below 100
        /// SetDistanceThreshold(dev,100,300, WindowDetectionMode.Above): above 300
        /// SetDistanceThreshold(dev,100,300, WindowDetectionMode.Out): out-of-window
        /// SetDistanceThreshold(dev,100,300, WindowDetectionMode.In): in window
        /// </summary>
        /// <param name="threshLow">
        /// The threshold under which the device raises an interrupt if detectionMode =
        /// WindowDetectionMode.Below
        /// </param>
        /// <param name="threshHigh">
        /// The threshold above which the device raises an interrupt if detectionMode =
        /// WindowDetectionMode.Above
        /// </param>
        /// <param name="detectionMode">The <see cref="WindowDetectionMode" /> where 0 = below, 1 = above, 2 = out, and 3 = in</param>
        public void SetDistanceThreshold(Length threshLow, Length threshHigh, WindowDetectionMode detectionMode)
        {
            byte temp = ReadByte((ushort)Registers.SYSTEM__INTERRUPT_CONFIG_GPIO);
            temp &= 0x47;
            WriteRegister((ushort)Registers.SYSTEM__INTERRUPT_CONFIG_GPIO,
                (byte)(temp | ((byte)detectionMode & 0x07) | 0x40));
            WriteUInt16((ushort)Registers.SYSTEM__THRESH_HIGH, (ushort)threshHigh.Millimeters);
            WriteUInt16((ushort)Registers.SYSTEM__THRESH_LOW, (ushort)threshLow.Millimeters);
        }

        /// <summary>
        /// Returns the <see cref="WindowDetectionMode" />.
        /// </summary>
        public WindowDetectionMode DistanceThresholdWindowDetectionMode
        {
            get
            {
                byte temp = ReadByte((ushort)Registers.SYSTEM__INTERRUPT_CONFIG_GPIO);
                return
                    (WindowDetectionMode)(temp &
                                          0x7);
            }
        }

        /// <summary>
        /// Returns the low threshold.
        /// </summary>
        public Length DistanceThresholdLow
        {
            get
            {
                return Length.FromMillimeters(ReadUInt16((ushort)Registers.SYSTEM__THRESH_LOW));
            }
        }

        /// <summary>
        /// Returns the high threshold.
        /// </summary>
        public Length DistanceThresholdHigh
        {
            get
            {
                return Length.FromMillimeters(ReadUInt16((ushort)Registers.SYSTEM__THRESH_HIGH));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Roi" />, the position of which is centered about the optical center.
        /// The smallest acceptable ROI size is 4.
        /// The receiving SPAD array of the sensor consists of 16x16 SPADs which covers the full FoV.
        /// It is possible to program a smaller ROI, with a smaller number of SPADs, to reduce the FoV for applications which
        /// require a narrow FoV.
        /// </summary>
        public Roi Roi
        {
            get
            {
                byte temp = ReadByte((ushort)Registers.ROI_CONFIG__USER_ROI_REQUESTED_GLOBAL_XY_SIZE);

                return new Roi((ushort)((temp & 0x0F) + 1), (ushort)(((temp & 0xF0) >> 4) + 1));
            }

            set
            {
                byte opticalCenter = ReadByte((ushort)Registers.VL53L1X_ROI_CONFIG__MODE_ROI_CENTRE_SPAD);
                ushort x = value.Width;
                ushort y = value.Height;

                if (x > 16)
                {
                    x = 16;
                }

                if (y > 16)
                {
                    y = 16;
                }

                if (x > 10 || y > 10)
                {
                    opticalCenter = 199;
                }

                WriteRegister((ushort)Registers.ROI_CONFIG__USER_ROI_CENTRE_SPAD, opticalCenter);
                WriteRegister((ushort)Registers.ROI_CONFIG__USER_ROI_REQUESTED_GLOBAL_XY_SIZE,
                    (byte)(((y - 1) << 4) | (x - 1)));
            }
        }

        /// <summary>
        /// This function programs the new user ROI center, please to be aware that there is no check in this function
        /// if the ROI center vs ROI size is out of border.
        /// </summary>
        public byte RoiCenter
        {
            get
            {
                return ReadByte((ushort)Registers.ROI_CONFIG__USER_ROI_CENTRE_SPAD);
            }

            set
            {
                WriteRegister((ushort)Registers.ROI_CONFIG__USER_ROI_CENTRE_SPAD, value);
            }
        }

        /// <summary>
        /// Gets or sets a new signal threshold in kcps where the default is 1024 kcps.
        /// </summary>
        /// <remarks>
        ///  Kcps is kilo counts per second.
        /// </remarks>
        public ushort SignalThreshold
        {
            get
            {
                return (ushort)(ReadUInt16((ushort)Registers.RANGE_CONFIG__MIN_COUNT_RATE_RTN_LIMIT_MCPS) << 3);
            }

            set
            {
                WriteUInt16((ushort)Registers.RANGE_CONFIG__MIN_COUNT_RATE_RTN_LIMIT_MCPS, (ushort)(value >> 3));
            }
        }

        /// <summary>
        /// This function programs a new sigma threshold. The default value is 15 mm.
        /// </summary>
        public Length SigmaThreshold
        {
            get
            {
                return Length.FromMillimeters((ushort)(ReadByte((ushort)Registers.RANGE_CONFIG__SIGMA_THRESH) >> 2));
            }

            set
            {
                var length = (ushort)value.Millimeters;
                if (length > 0xFFFF >> 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The sigma threshold is too high");
                }

                WriteUInt16((ushort)Registers.RANGE_CONFIG__SIGMA_THRESH, (ushort)(length << 2));
            }
        }

        /// <summary>
        /// This function performs the temperature calibration.
        /// If the sensor has been stopped for a long time, it is recommended to perform the temperature update prior to
        /// restarting the ranging.
        /// By default, the sensor can adequately handle any temperature change as long as it is running, but if the sensor is
        /// stopped for an extended period of time,
        /// a temperature compensation is advised.
        /// </summary>
        public void StartTemperatureUpdate()
        {
            WriteRegister((ushort)Registers.VL53L1X_VHV_CONFIG__TIMEOUT_MACROP_LOOP_BOUND, 0x81); /* full VHV */
            WriteRegister(0x0B, 0x92);
            StartRanging();

            WaitForDataReady();

            ClearInterrupt();
            StopRanging();
            WriteRegister((ushort)Registers.VL53L1X_VHV_CONFIG__TIMEOUT_MACROP_LOOP_BOUND, 0x09); /* two bounds VHV */
            WriteRegister(0x0B, 0); /* start VHV from the previous temperature */
        }

        /// <summary>
        /// This function performs the offset calibration and programs the offset compensation into the device.
        /// Target reflectance should be grey17%.
        /// </summary>
        /// <param name="targetDist">The Target distance, ST recommended 100 mm.</param>
        /// <returns>The offset value found.</returns>
        public short CalibrateOffset(Length targetDist)
        {
            WriteUInt16((ushort)Registers.ALGO__PART_TO_PART_RANGE_OFFSET_MM, 0x0);
            WriteUInt16((ushort)Registers.MM_CONFIG__INNER_OFFSET_MM, 0x0);
            WriteUInt16((ushort)Registers.MM_CONFIG__OUTER_OFFSET_MM, 0x0);
            StartRanging(); /* Enable VL53L1X sensor */
            int averageDistance = 0;

            for (int i = 0; i < 50; i++)
            {
                WaitForDataReady();
                ushort distance = (ushort)GetDistance().Millimeters;
                ClearInterrupt();
                averageDistance += distance;
            }

            StopRanging();
            averageDistance /= 50;

            short offset = (short)(targetDist.Millimeters - averageDistance);

            WriteInt16((ushort)Registers.ALGO__PART_TO_PART_RANGE_OFFSET_MM, (short)(offset * 4));

            return offset;
        }

        /// <summary>
        /// This function performs the xtalk calibration and programs the xtalk compensation to the device.
        /// Target reflectance should be grey 17%.
        /// </summary>
        /// <param name="targetDist">
        /// The target distance.
        /// This is the distance where the sensor starts to "under range"
        /// due to the influence of the photons reflected back from the cover glass becoming strong.
        /// It's also called inflection point.
        /// </param>
        /// <returns>The xtalk value found in cps (number of photons in count per second).</returns>
        public ushort CalibrateXtalk(Length targetDist)
        {
            WriteUInt16(0x0016, 0);
            StartRanging();
            int averageSignalRate = 0;
            int averageDistance = 0;
            int averageSpadNb = 0;

            for (int i = 0; i < 50; i++)
            {
                WaitForDataReady();
                ushort sr = SignalRate;
                ushort distance = (ushort)GetDistance().Millimeters;
                ushort spadNum = SpadNb;

                ClearInterrupt();
                averageDistance += distance;
                averageSignalRate += sr;
                averageSpadNb += spadNum;
            }

            StopRanging();

            averageDistance /= 50;
            averageSignalRate /= 50;
            averageSpadNb /= 50;

            ushort xTalk = (ushort)(512 * averageSignalRate * (1 - averageDistance / targetDist.Millimeters) / averageSpadNb);
            WriteUInt16(0x0016, xTalk);

            return xTalk;
        }

        private void WaitForDataReady()
        {
            Stopwatch watch = new();
            watch.Start();

            while (!IsDataReady && watch.ElapsedMilliseconds > _operationTimeoutMilliseconds)
            {
                Thread.Sleep(50);
            }

            if (!IsDataReady)
            {
                throw new TimeoutException(
                    $"The device did not send any data within the specified timeout of {_operationTimeoutMilliseconds}.");
            }
        }

        private void WriteRegister(ushort reg, byte param)
        {
            Span<byte> writeArray = stackalloc byte[3];
            BinaryPrimitives.WriteUInt16BigEndian(writeArray, reg);
            writeArray[2] = param;
            _i2CDevice.Write(writeArray);
        }

        private byte ReadByte(ushort reg)
        {
            Span<byte> writeBytes = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(writeBytes, reg);
            _i2CDevice.Write(writeBytes);
            return _i2CDevice.ReadByte();
        }

        private short ReadInt16(ushort reg)
        {
            Span<byte> outArray = stackalloc byte[2];
            Span<byte> writeArray = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(writeArray, reg);
            _i2CDevice.Write(writeArray);

            _i2CDevice.Read(outArray);
            return BinaryPrimitives.ReadInt16BigEndian(outArray);
        }

        private ushort ReadUInt16(ushort reg)
        {
            Span<byte> outArray = stackalloc byte[2];
            Span<byte> writeArray = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(writeArray, reg);
            _i2CDevice.Write(writeArray);
            _i2CDevice.Read(outArray);
            return BinaryPrimitives.ReadUInt16BigEndian(outArray);
        }

        private uint ReadUInt32(ushort reg)
        {
            Span<byte> outArray = stackalloc byte[4];
            Span<byte> writeArray = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(writeArray, reg);
            _i2CDevice.Write(writeArray);

            _i2CDevice.Read(outArray);
            return BinaryPrimitives.ReadUInt32BigEndian(outArray);
        }

        private void WriteInt16(ushort reg, short data)
        {
            Span<byte> outArray = stackalloc byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(outArray, reg);
            BinaryPrimitives.WriteInt16BigEndian(outArray.Slice(2), data);
            _i2CDevice.Write(outArray);
        }

        private void WriteUInt16(ushort reg, ushort data)
        {
            Span<byte> outArray = stackalloc byte[4];
            BinaryPrimitives.WriteUInt16BigEndian(outArray, reg);
            BinaryPrimitives.WriteUInt16BigEndian(outArray.Slice(2), data);
            _i2CDevice.Write(outArray);
        }

        private void WriteInt32(ushort reg, int data)
        {
            Span<byte> outArray = stackalloc byte[6];
            BinaryPrimitives.WriteUInt16BigEndian(outArray, reg);
            BinaryPrimitives.WriteInt32BigEndian(outArray.Slice(2), data);
            _i2CDevice.Write(outArray);
        }

        private void WriteUInt32(ushort reg, uint data)
        {
            Span<byte> outArray = stackalloc byte[6];
            BinaryPrimitives.WriteUInt16BigEndian(outArray, reg);
            BinaryPrimitives.WriteUInt32BigEndian(outArray.Slice(2), data);
            _i2CDevice.Write(outArray);
        }
    }
}
