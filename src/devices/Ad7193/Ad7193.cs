using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ad7193.Metadata;
using Iot.Units;

namespace Iot.Device.Ad7193
{
    /// <summary>
    /// Represents the Analog Devices AD7193, the 4-channel, 4.8 kHz, ultralow noise, 24-bit sigma-delta ADC with PGA
    /// </summary>
    public class Ad7193 : IDisposable
    {
        private readonly object _spiTransferLock = new object();
        private readonly Stopwatch _stopWatch = new Stopwatch();

        private SpiDevice _spiDevice = null;

        /// <summary>
        /// Metadata of AD7193
        /// </summary>
        protected static Ad7193Metadata metadata = new Ad7193Metadata();

        /// <summary>
        /// The list of received ADC values
        /// </summary>
        private BlockingCollection<AdcValue> _adcValues = new BlockingCollection<AdcValue>();

        /// <summary>
        /// The event that is fired every time a new value is received from the ADC
        /// </summary>
        public event EventHandler<AdcValueReceivedEventArgs> OnValueReceived;

        private readonly StringBuilder _sb = new StringBuilder();

        private readonly uint[] _registerCache = { 0x00, 0x080060, 0x000117, 0x000000, 0xa2, 0x00, 0x000000, 0x000000 };
        private readonly byte[] _registerSize = { 1, 3, 3, 3, 1, 1, 3, 3 };

        /// <summary>
        /// The external reference voltage value. The default is 2.5V on REFIN1+ and REFIN1- (on the Digilent Pmod AD5 board)
        /// </summary>
        public double VoltageReference { get; set; } = 2.50f;

        /// <summary>
        /// Polarity select bit: true is unipolar, false is bipolar operation.
        /// </summary>
        public bool Unipolar
        {
            get
            {
                return GetRegisterBits(Register.Configuration, BitMask.ConfigurationU) == 1;
            }

            set
            {
                SetRegisterBits(Register.Configuration, BitMask.ConfigurationU, value ? 1U : 0);
            }
        }

        /// <summary>
        /// Gain level on the ADC
        /// </summary>
        public Gain PGAGain
        {
            get
            {
                return (Gain)(GetRegisterBits(Register.Configuration, BitMask.ConfigurationG));
            }

            set
            {
                SetRegisterBits(Register.Configuration, BitMask.ConfigurationG, (uint)value);
            }
        }

        /// <summary>
        /// The value of the current Status register
        /// </summary>
        public uint Status
        {
            get
            {
                _registerCache[(byte)Register.Status] = ReadRegisterValue(Register.Status);
                return _registerCache[(byte)Register.Status];
            }
        }

        /// <summary>
        /// The value of the current Mode register
        /// </summary>
        public uint Mode
        {
            get
            {
                _registerCache[(byte)Register.Mode] = ReadRegisterValue(Register.Mode);
                return _registerCache[(byte)Register.Mode];
            }
        }

        /// <summary>
        /// The value of the current Configuration register
        /// </summary>
        public uint Config
        {
            get
            {
                _registerCache[(byte)Register.Configuration] = ReadRegisterValue(Register.Configuration);
                return _registerCache[(byte)Register.Configuration];
            }
        }

        /// <summary>
        /// Power-down mode. In power-down mode, all AD7193 circuitry, except the bridge power-down switch, is powered down. The bridge power-down switch remains active because the user may need to power up the sensor prior to powering up the AD7193 for settling reasons. The external crystal, if selected, remains active.
        /// </summary>
        public bool IsPowerDown
        {
            get
            {
                ReadRegisterValue(Register.Mode);
                return GetRegisterBits(Register.Mode, BitMask.ModeMD) == 0b011;
            }
        }

        /// <summary>
        /// Ready bit for the ADC. This bit is set when data is written to the ADC data register.
        /// </summary>
        public bool IsReady
        {
            get
            {
                ReadRegisterValue(Register.Status);
                return GetRegisterBits(Register.Mode, BitMask.StatusRDY) != 0b1;
            }
        }

        /// <summary>
        /// True is the ADC has errors
        /// </summary>
        public bool HasErrors
        {
            get
            {
                uint status = ReadRegisterValue(Register.Status);

                return (status & (uint)BitMask.StatusERR) == (uint)BitMask.StatusERR;
            }
        }

        private bool _continuousRead = false;

        /// <summary>
        /// Is the ADC in continuous conversion mode
        /// </summary>
        public bool ContinuousRead
        {
            get
            {
                return _continuousRead;
            }

            set
            {
                WriteRegisterValue(Register.Communications, (uint)(value ? 0b0101_1100 : 0b0101_1000));
                _continuousRead = value;
            }
        }

        /// <summary>
        /// Enables or disables DAT_STA Bit (appends the 8-bit Status register to the 24-bit Data register when reading)
        /// </summary>
        public bool AppendStatusRegisterToData
        {
            get
            {
                return GetRegisterBits(Register.Mode, BitMask.ModeDAT_STA) == 1;
            }

            set
            {
                SetRegisterBits(Register.Mode, BitMask.ModeDAT_STA, value ? 1U : 0);

                if (value)
                {
                    // change register size to 4, because the 8-bit Status register is now appended to the 24-bit ADC value
                    _registerSize[(byte)Register.Data] = 4;
                }
                else
                {
                    // change register size to 3, because we have a 24-bit ADC value (without the 8-bit Status register)
                    _registerSize[(byte)Register.Data] = 3;
                }
            }
        }

        /// <summary>
        /// True if the jitter correction is enabled in the driver. Jitter correction tries to guarantie the steady samples rate even is the hardware rasterization speed jitters.
        /// </summary>
        public bool JitterCorrection { get; set; }

        /// <summary>
        /// Switches from differential input to pseudo differential inputs.
        /// When the pseudo bit is set to 1, the AD7193 is configured to have eight pseudo differential analog inputs. When pseudo bit is set to 0, the AD7193 is configured to have four differential analog inputs.
        /// </summary>
        public AnalogInputMode InputMode
        {
            get
            {
                return (AnalogInputMode)(GetRegisterBits(Register.Configuration, BitMask.ConfigurationPseudo));
            }

            set
            {
                SetRegisterBits(Register.Configuration, BitMask.ConfigurationPseudo, (uint)value);
            }
        }

        /// <summary>
        /// Changes the currently selected channel
        /// </summary>
        public Channel ActiveChannels
        {
            get
            {
                return (Channel)(GetRegisterBits(Register.Configuration, BitMask.ConfigurationCH));
            }

            set
            {
                SetRegisterBits(Register.Configuration, BitMask.ConfigurationCH, (uint)value);
            }
        }

        /// <summary>
        /// Sets the amount of averaging. The data from the sinc filter is averaged by 2, 8, or 16. The averaging reduces the output data rate for a given FS word; however, the RMS noise improves.
        /// </summary>
        public AveragingMode Averaging
        {
            get
            {
                return (AveragingMode)(GetRegisterBits(Register.Mode, BitMask.ModeAVG));
            }

            set
            {
                SetRegisterBits(Register.Mode, BitMask.ModeAVG, (uint)value);
            }
        }

        /// <summary>
        /// Set the filter output data rate select bits. The 10 bits of data programmed into these bits determine the filter cutoff frequency, the position of the first notch of the filter, and the output data rate for the part.
        /// </summary>
        public ushort Filter
        {
            get
            {
                return (ushort)(GetRegisterBits(Register.Mode, BitMask.ModeFS));
            }

            set
            {
                SetRegisterBits(Register.Mode, BitMask.ModeFS, value);
            }
        }

        /// <summary>
        /// The value of the offset calibration register
        /// </summary>
        public uint Offset
        {
            get
            {
                return ReadRegisterValue(Register.Offset) & (uint)BitMask.Offset;
            }

            set
            {
                SetRegisterBits(Register.Offset, BitMask.Offset, value);
            }
        }

        /// <summary>
        /// The value of the full-scale calibration register
        /// </summary>
        public uint FullScale
        {
            get
            {
                return ReadRegisterValue(Register.FullScale) & (uint)BitMask.FullScale;
            }

            set
            {
                SetRegisterBits(Register.FullScale, BitMask.FullScale, value);
            }
        }

        /// <summary>
        /// Gets the metadata class for the device
        /// </summary>
        /// <returns></returns>
        public static IDeviceMetadata GetDeviceMetadata()
        {
            return Ad7193.metadata;
        }

        /// <summary>
        /// Initializes the ADC
        /// </summary>
        /// <param name="spiDevice">The SPI device to initialize the ADC on</param>
        public Ad7193(SpiDevice spiDevice)
        {
            if ((spiDevice.ConnectionSettings.Mode & metadata.ValidSpiModes) != spiDevice.ConnectionSettings.Mode)
            {
                throw new Exception("SPI device must be in SPI mode 3 in order to work with AD7193.");
            }

            if (spiDevice.ConnectionSettings.ClockFrequency > metadata.MaximumSpiFrequency)
            {
                throw new Exception($"SPI device must have a lower clock frequency, because AD7193's maximum operating SPI frequncy is {metadata.MaximumSpiFrequency} Hz.");
            }

            _spiDevice = spiDevice;

            Reset();
        }

        /// <summary>
        /// Resets the ADC
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < 6; i++)
            {
                _spiDevice.Write(new byte[] { 0xFF });
            }
        }

        /// <summary>
        /// Initiates internal calibration, including zero-scale and full-scale calibrations
        /// </summary>
        public void Calibrate()
        {
            // internal zero scale calibration (MD2 = 1, MD1 = 0, MD0 = 0)
            SetRegisterBits(Register.Mode, BitMask.ModeMD, 0b100);
            WaitForADC();

            // internal full scale calibration (MD2 = 1, MD1 = 0, MD0 = 1)
            SetRegisterBits(Register.Mode, BitMask.ModeMD, 0b101);
            WaitForADC();
        }

        /// <summary>
        /// Waits after a single conversion until the DOUT/!RDY goes low to indicate the completion of a conversion
        /// </summary>
        public void WaitForADC()
        {
            while (!IsReady)
            {
                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// Initiates Single Conversion (device will go into low power mode when conversion complete, and DOUT/!RDY goes low to indicate the completion of a conversion)
        /// </summary>
        public void StartSingleConversion()
        {
            // set the single conversion mode bits
            SetRegisterBits(Register.Mode, BitMask.ModeMD, 0b001);

            _stopWatch.Restart();
        }

        /// <summary>
        /// Initiates Continuous Conversion. After calling this method the AdcValueReceived event will be fired continuously. This mode is much faster, but uses significatily more CPU power.
        /// </summary>
        /// <param name="frequency">The target frequency of the sampling. AD7193 has the maximum of 4800 samples per second.</param>
        public void StartContinuousConversion(uint frequency = 0)
        {
            if (frequency == 0)
            {
                frequency = metadata.ADCSamplerate;
            }

            if (frequency > metadata.ADCSamplerate)
            {
                throw new ArgumentException($"The frequency you provided is higher than the maximum sampling rate of AD7193 ({metadata.ADCSamplerate} SPS).");
            }

            // set the continuous conversion mode bits
            SetRegisterBits(Register.Mode, BitMask.ModeMD, 0b000);

            ContinuousRead = true;

            long samplePerTicks = Stopwatch.Frequency / frequency;
            if (samplePerTicks == 0)
            {
                samplePerTicks = 1;
            }

            _stopWatch.Restart();
            Task samplingTask = Task.Run(() =>
            {
                long samples = 0;
                long nextSampleAt = _stopWatch.ElapsedTicks;
                long elapsedTicks = 0;
                long jitter = 0;
                long maxJitterCorrectionPerSample = Math.Max(Stopwatch.Frequency / 100000, 1);
                while (_stopWatch.IsRunning)
                {
                    elapsedTicks = _stopWatch.ElapsedTicks;
                    if (elapsedTicks >= nextSampleAt)
                    {
                        nextSampleAt = elapsedTicks + samplePerTicks;
                        ReadADCValue();
                        samples++;
                        jitter = (elapsedTicks - (samples * samplePerTicks));
                        if (JitterCorrection)
                        {
                            if (jitter > 0)
                            {
                                nextSampleAt -= Math.Min(jitter, maxJitterCorrectionPerSample);
                            }
                            else if (jitter < 0)
                            {
                                nextSampleAt += Math.Min(-jitter, maxJitterCorrectionPerSample);
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Reads the current ADC result value on the currently selected channel without converting it to Volts
        /// </summary>
        /// <returns>24-bit raw value of the last ADC result (+ status byte if enabled)</returns>
        public uint ReadADCValue()
        {
            uint raw = ReadRegisterValue(Register.Data);

            // update the status register cache if we have it here
            if (AppendStatusRegisterToData)
            {
                _registerCache[(byte)Register.Status] = (byte)(raw & 0xFF);
                raw = (raw & 0xFFFFFF00) >> 8;
            }

            // check if we have an error
            if (HasErrors)
            {
                throw new Exception($"An error occured during the conversion. (Status register: {RegisterToString(Register.Status)})");
            }

            // create the new AdcValue object and calculate the voltage
            var adcValue = new AdcValue() { Raw = raw, Time = _stopWatch.ElapsedTicks, Channel = (byte)(GetRegisterBits(Register.Status, BitMask.StatusCHD)), Voltage = RawValueToVoltage(raw) };

            // add it to the collection
            _adcValues.Add(adcValue);

            // call the event handler
            OnValueReceived?.Invoke(this, new AdcValueReceivedEventArgs(adcValue));

            return raw;
        }

        /// <summary>
        /// Selects the channel, starts a single conversion, waits for its completion and returns the read value without converting it to Volts
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <returns>24-bit raw value of the ADC result</returns>
        public uint? ReadSingleADCValue(Channel channel)
        {
            ActiveChannels = channel;

            StartSingleConversion();

            WaitForADC();

            return ReadADCValue();
        }

        /// <summary>
        /// Converts the raw ADC result to Volts
        /// </summary>
        /// <param name="adcValue">The raw ADC result</param>
        /// <returns>The ADC result value in Volts</returns>
        private double RawValueToVoltage(uint adcValue)
        {
            int pgaGain = 1;
            switch (PGAGain)
            {
                case Gain.X1:
                    pgaGain = 1;
                    break;
                case Gain.X8:
                    pgaGain = 8;
                    break;
                case Gain.X16:
                    pgaGain = 16;
                    break;
                case Gain.X32:
                    pgaGain = 32;
                    break;
                case Gain.X64:
                    pgaGain = 64;
                    break;
                case Gain.X128:
                    pgaGain = 128;
                    break;
            }

            double voltage = 0;

            // 0 - bipolar (ranges from ±19.53 mV to ±2.5 V) ; 1 - unipolar (ranges from 0 mV to 19.53 mV to 0 V to 2.5 V)
            if (Unipolar)
            {
                voltage = (double)adcValue / 16777216.0;
            }
            else if (!Unipolar)
            {
                voltage = ((double)adcValue / 8388608.0) - 1.0;
            }

            voltage *= VoltageReference / pgaGain;

            return voltage;
        }

        /// <summary>
        /// Converts the ADC result to Celsius
        /// </summary>
        /// <param name="adcValue">The raw ADC result</param>
        /// <returns></returns>
        protected Temperature ADCValueToTemperature(ulong adcValue)
        {
            double degreeCelsius = ((float)(adcValue - 0x0080_0000) / 2815.0f) - 273.0f;
            return Temperature.FromCelsius(degreeCelsius);
        }

        /// <summary>
        /// Reads the value of a register and updates the register cache
        /// </summary>
        protected uint ReadRegisterValue(Register register)
        {
            byte registerAddress = (byte)register;
            byte byteNumber = _registerSize[registerAddress];
            byte[] writeBuffer = new byte[byteNumber + 1];

            byte commandByte = (byte)((byte)CommunicationsRegisterBits.ReadOperation | GetCommAddress(registerAddress));
            writeBuffer[0] = commandByte;

            byte[] readBuffer = new byte[writeBuffer.Length];
            lock (_spiTransferLock)
            {
                _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
            }

            byte[] trimmedReadBuffer = new byte[readBuffer.Length - 1];
            Array.Copy(readBuffer, 1, trimmedReadBuffer, 0, trimmedReadBuffer.Length);
            readBuffer = trimmedReadBuffer;

            _registerCache[registerAddress] = ByteArrayToUInt32(readBuffer);

            return _registerCache[registerAddress];
        }

        /// <summary>
        /// Writes data into a register
        /// </summary>
        protected void WriteRegisterValue(Register register, uint registerValue)
        {
            byte registerAddress = (byte)register;
            byte byteNumber = _registerSize[registerAddress];
            byte[] writeBuffer = new byte[byteNumber + 1];

            byte commandByte = (byte)((byte)CommunicationsRegisterBits.WriteOperation | GetCommAddress(registerAddress));
            writeBuffer[0] = commandByte;

            byte[] buffer = UInt32ToByteArray(registerValue, byteNumber);
            Array.Copy(buffer, 0, writeBuffer, 1, byteNumber);

            lock (_spiTransferLock)
            {
                _spiDevice.Write(writeBuffer);
            }

            // update the cache
            _registerCache[(byte)register] = registerValue;
        }

        /// <summary>
        /// Gets all the register values
        /// </summary>
        /// <returns>List of the values of the registers of the ADC</returns>
        protected List<uint> GetAllRegisterValues()
        {
            List<uint> result = new List<uint>
            {
                ReadRegisterValue(Register.Status),
                ReadRegisterValue(Register.Mode),
                ReadRegisterValue(Register.Configuration),
                ReadRegisterValue(Register.Data),
                ReadRegisterValue(Register.ID),
                ReadRegisterValue(Register.GPOCON),
                ReadRegisterValue(Register.Offset),
                ReadRegisterValue(Register.FullScale)
            };

            return result;
        }

        private static int GetCommAddress(int x)
        {
            return ((x) & 0x07) << 3;
        }

        /// <summary>
        /// Returns the human-readable representation of a register
        /// </summary>
        /// <param name="register">The register you need the text-representation of</param>
        /// <returns></returns>
        public string RegisterToString(Register register)
        {
            uint registerValue = _registerCache[(byte)register];
            string result;

            switch (register)
            {
                case Register.Status:
                    _sb.Clear();
                    _sb.Append(((registerValue & 0b1000_0000) == 0b1000_0000) ? "Not ready" : "Ready");
                    _sb.Append(" | ");
                    _sb.Append(((registerValue & 0b0100_0000) == 0b0100_0000) ? "Error" : "No errors");
                    _sb.Append(" | ");
                    _sb.Append(((registerValue & 0b0010_0000) == 0b0010_0000) ? "No external reference" : "External reference");
                    _sb.Append(" | ");
                    _sb.Append(((registerValue & 0b0001_0000) == 0b0001_0000) ? "Parity Odd" : "Parity Even");
                    _sb.Append(" | ");
                    _sb.Append($"Result CH: {(registerValue & 0b00001111)}");

                    result = _sb.ToString();
                    break;
                case Register.Mode:
                    string mode = UInt32ToBinaryString((registerValue & 0b1110_0000_0000_0000_0000_0000) >> 21, 3);

                    _sb.Clear();
                    _sb.Append($"Mode: {mode}");
                    switch (mode)
                    {
                        case "000":
                            _sb.Append(" (continuous)");
                            break;
                        case "001":
                            _sb.Append(" (single)");
                            break;
                        case "010":
                            _sb.Append(" (idle)");
                            break;
                        case "011":
                            _sb.Append(" (power down)");
                            break;
                        case "100":
                            _sb.Append(" (internal zero-scale calibration)");
                            break;
                        case "101":
                            _sb.Append(" (internal full-scale calibration)");
                            break;
                        case "110":
                            _sb.Append(" (system zero-scale calibration)");
                            break;
                        case "111":
                            _sb.Append(" (system full-scale calibration)");
                            break;
                    }

                    _sb.Append(" | ");
                    _sb.Append($"DAT_STA: {(registerValue & 0b0001_0000_0000_0000_0000_0000) >> 20}");
                    _sb.Append(" | ");
                    _sb.Append($"CLK: {UInt32ToBinaryString((registerValue & 0b0000_1100_0000_0000_0000_0000) >> 18, 2)}");
                    _sb.Append(" | ");
                    _sb.Append($"AVG: {UInt32ToBinaryString((registerValue & 0b0000_0011_0000_0000_0000_0000) >> 16, 2)}");
                    _sb.Append(" | ");
                    _sb.Append($"SINC3: {(registerValue & 0b0000_0000_1000_0000_0000_0000) >> 15}");
                    _sb.Append(" | ");
                    _sb.Append($"0: {(registerValue & 0b0000_0000_0100_0000_0000_0000) >> 14}");
                    _sb.Append(" | ");
                    _sb.Append($"ENPAR: {(registerValue & 0b0000_0000_0010_0000_0000_0000) >> 13}");
                    _sb.Append(" | ");
                    _sb.Append($"CLK_DIV: {(registerValue & 0b0000_0000_0001_0000_0000_0000) >> 12}");
                    _sb.Append(" | ");
                    _sb.Append($"Single: {(registerValue & 0b0000_0000_0000_1000_0000_0000) >> 11}");
                    _sb.Append(" | ");
                    _sb.Append($"REJ60: {(registerValue & 0b0000_0000_0000_0100_0000_0000) >> 10}");
                    _sb.Append(" | ");
                    _sb.Append($"FS: {UInt32ToBinaryString((registerValue & 0b0000_0000_0000_0011_1111_1111), 10)}");

                    result = _sb.ToString();
                    break;
                case Register.Configuration:
                    _sb.Clear();
                    _sb.Append($"Chop: {(registerValue & 0b1000_0000_0000_0000_0000_0000) >> 23}");
                    _sb.Append(" | ");
                    _sb.Append($"00: {UInt32ToBinaryString((registerValue & 0b0110_0000_0000_0000_0000_0000) >> 21, 2)}");
                    _sb.Append(" | ");
                    _sb.Append($"REFSEL: {(registerValue & 0b0001_0000_0000_0000_0000_0000) >> 20}");
                    _sb.Append(" | ");
                    _sb.Append($"0: {(registerValue & 0b0000_1000_0000_0000_0000_0000) >> 19}");
                    _sb.Append(" | ");
                    _sb.Append($"Pseudo: {(registerValue & 0b0000_0100_0000_0000_0000_0000) >> 18}");
                    _sb.Append(" | ");
                    _sb.Append($"Channel: {UInt32ToBinaryString((registerValue & 0b0000_0011_1111_1111_0000_0000) >> 8, 10)}");
                    _sb.Append(" | ");
                    _sb.Append($"Burn: {(registerValue & 0b0000_0000_0000_0000_1000_0000) >> 7}");
                    _sb.Append(" | ");
                    _sb.Append($"REFDET: {(registerValue & 0b0000_0000_0000_0000_0100_0000) >> 6}");
                    _sb.Append(" | ");
                    _sb.Append($"0: {(registerValue & 0b0000_0000_0000_0000_0010_0000) >> 5}");
                    _sb.Append(" | ");
                    _sb.Append($"BUF: {(registerValue & 0b0000_0000_0000_0000_0001_0000) >> 4}");
                    _sb.Append(" | ");
                    _sb.Append($"Unipolar: {(registerValue & 0b0000_0000_0000_0000_0000_1000) >> 3}");
                    _sb.Append(" | ");
                    _sb.Append($"Gain: {UInt32ToBinaryString((registerValue & 0b0000_0000_0000_0000_0000_0111), 3)}");

                    result = _sb.ToString();
                    break;
                default:
                    result = registerValue.ToString();
                    break;
            }

            return result;
        }

        /// <summary>
        /// Disposes the AD7193 object and closes the SPI device
        /// </summary>
        public void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null;
        }

        /// <summary>
        /// Extracts a value from a register, where the value is located at the position the bitmask indicates
        /// </summary>
        /// <param name="reg">Register to extract the value from</param>
        /// <param name="bitmask">Bitmask indicating the position of the value</param>
        /// <returns>Value of the part of the register</returns>
        private uint GetRegisterBits(Register reg, BitMask bitmask)
        {
            // clear all bit(s) except the bit(s) we would like to read
            uint result = _registerCache[(byte)reg] & (uint)bitmask;

            // remove the bits from the end of the register that are not part of the bitmask
            if (bitmask > 0)
            {
                uint bits = (uint)bitmask;

                while (bits % 2 == 0)
                {
                    result >>= 1;
                    bits /= 2;
                }
            }

            return result;
        }

        private uint SetRegisterBits(Register reg, BitMask bitmask, uint value)
        {
            // keep all bit values except the bit(s) we would like to modify
            uint result = _registerCache[(byte)reg] & ~(uint)bitmask;

            if (bitmask > 0)
            {
                uint bits = (uint)bitmask;

                while (bits % 2 == 0)
                {
                    value <<= 1;
                    bits /= 2;
                }

                value &= (uint)bitmask;
                result |= value;
            }

            WriteRegisterValue(reg, result);

            return result;
        }

        private uint ByteArrayToUInt32(byte[] buffer)
        {
            byte[] fourByteRawValue = new byte[4];
            for (int i = 0; i < buffer.Length; i++)
            {
                fourByteRawValue[buffer.Length - 1 - i] = buffer[i];
            }

            return BitConverter.ToUInt32(fourByteRawValue, 0);
        }

        private byte[] UInt32ToByteArray(uint number, byte byteNumber)
        {
            byte[] result = new byte[byteNumber];
            for (int i = 0; i < result.Length; i++)
            {
                result[byteNumber - 1 - i] = (byte)(number & 0xFF);
                number >>= 8;
            }

            return result;
        }

        private string UInt32ToBinaryString(uint number, byte padding)
        {
            const int mask = 1;
            string binary = string.Empty;
            while (number > 0)
            {
                // Logical AND the number and prepend it to the result string
                binary = (number & mask) + binary;
                number >>= 1;
            }

            return binary.PadLeft(padding, '0');
        }
    }
}
