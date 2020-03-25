using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Ad7193
{
    public class Ad7193 : IDisposable
    {
        private SpiDevice spiDevice = null;

        // metadata for IDevice
        public const string Manufacturer = "Analog Devices";
        public const string Product = "AD7193";
        public const string ProductCategory = "ADC";
        public const string ProductDescription = "4-Channel, 4.8 kHz, Ultralow Noise, 24-Bit Sigma-Delta ADC with PGA";
        public Uri DataSheetURI = new Uri("https://www.analog.com/media/en/technical-documentation/data-sheets/AD7193.pdf");

        // metadata for ISpiDevice
        public const SpiMode ValidSpiModes = SpiMode.Mode3;
        public const int MaximumSpiFrequency = 10000000;    // min 100 ns SCLK pulse width

        // metadata for IAdcDevice
        public const int ADCCount = 1;
        public const int ADCBitrate = 24;
        public const int ADCSamplerate = 4800;
        public const int ADCInputChannelCount = 8;


        private object spiTransferLock = new object();
        private Stopwatch stopWatch = new Stopwatch();
        //public ConcurrentQueue<AdcValue> AdcValues = new ConcurrentQueue<AdcValue>();
        public BlockingCollection<AdcValue> AdcValues = new BlockingCollection<AdcValue>();
        public event EventHandler<AdcValueReceivedEventArgs> AdcValueReceived;


        // AD7193 Register Map
        public const byte AD7193_REG_COMM = 0;          // Communications Register (WO, 8-bit) 
        public const byte AD7193_REG_STAT = 0;          // Status Register         (RO, 8-bit)
        public const byte AD7193_REG_MODE = 1;          // Mode Register           (RW, 24-bit)
        public const byte AD7193_REG_CONF = 2;          // Configuration Register  (RW, 24-bit)
        public const byte AD7193_REG_DATA = 3;          // Data Register           (RO, 24/32-bit) 
        public const byte AD7193_REG_ID = 4;            // ID Register             (RO, 8-bit) 
        public const byte AD7193_REG_GPOCON = 5;        // GPOCON Register         (RW, 8-bit) 
        public const byte AD7193_REG_OFFSET = 6;        // Offset Register         (RW, 24-bit) 
        public const byte AD7193_REG_FULLSCALE = 7;     // Full-Scale Register     (RW, 24-bit)

        // Communications Register Bit Designations (AD7193_REG_COMM)
        public const int AD7193_COMM_WEN = (1 << 7);    // Write Enable. 
        public const int AD7193_COMM_WRITE = (0 << 6);  // Write Operation.
        public const int AD7193_COMM_READ = (1 << 6);   // Read Operation.         
        public const int AD7193_COMM_CREAD = (1 << 2);  // Continuous Read of Data Register.

        // Gain settings
        public enum Gain
        {
            X1 = 0b000,
            X8 = 0b011,
            X16 = 0b100,
            X32 = 0b101,
            X64 = 0b110,
            X128 = 0b111
        }

        [Flags]
        public enum Channel
        {
            CH00 = 0b00_0000_0001,
            CH01 = 0b00_0000_0010,
            CH02 = 0b00_0000_0100,
            CH03 = 0b00_0000_1000,
            CH04 = 0b00_0001_0000,
            CH05 = 0b00_0010_0000,
            CH06 = 0b00_0100_0000,
            CH07 = 0b00_1000_0000,
            TEMP = 0b01_0000_0000,
            Shrt = 0b10_0000_0000
        }

        public enum AveragingModes
        {
            Off = 0b00,
            Avg2 = 0b01,
            Avg8 = 0b10,
            Avg16 = 0b11
        }

        public enum AnalogInputModes
        {
            FourDifferentialAnalogInputs = 0b0,
            EightPseudoDifferentialAnalogInputs = 0b1
        }

        // Default register settings
        private uint[] registerCache = { 0x00, 0x080060, 0x000117, 0x000000, 0xa2, 0x00, 0x000000, 0x000000 };
        private byte[] registerSize = { 1, 3, 3, 3, 1, 1, 3, 3 };

        private StringBuilder sb = new StringBuilder();


        /// <summary>
        /// The external reference voltage value. The default is 2.5V on REFIN1+ and REFIN1- (on the Digilent Pmod AD5 board)
        /// </summary>
        public float VReference { get; set; } = 2.50f;

        public Gain PGAGain
        {
            set
            {
                registerCache[AD7193_REG_CONF] &= 0xFF_FFF8;          // keep all bit values except Gain bits
                registerCache[AD7193_REG_CONF] |= (uint)value;

                SetRegisterValue(AD7193_REG_CONF, registerCache[AD7193_REG_CONF]);
            }
        }


        public string Status
        {
            get
            {
                uint register = GetRegisterValue(AD7193_REG_STAT);

                sb.Clear();
                sb.Append(((register & 0b1000_0000) == 0b1000_0000) ? "Not ready" : "Ready");
                sb.Append(" | ");
                sb.Append(((register & 0b0100_0000) == 0b0100_0000) ? "Error" : "No errors");
                sb.Append(" | ");
                sb.Append(((register & 0b0010_0000) == 0b0010_0000) ? "No external reference" : "External reference");
                sb.Append(" | ");
                sb.Append(((register & 0b0001_0000) == 0b0001_0000) ? "Parity Odd" : "Parity Even");
                sb.Append(" | ");
                sb.Append($"Result CH: {(register & 0b00001111)}");

                return sb.ToString();
            }
        }

        public string Mode
        {
            get
            {
                uint register = GetRegisterValue(AD7193_REG_MODE);

                string mode = UInt32ToBinaryString((register & 0b1110_0000_0000_0000_0000_0000) >> 21, 3);

                sb.Clear();
                sb.Append($"Mode: {mode}");
                switch (mode)
                {
                    case "000":
                        sb.Append(" (continuous)");
                        break;
                    case "001":
                        sb.Append(" (single)");
                        break;
                    case "010":
                        sb.Append(" (idle)");
                        break;
                    case "011":
                        sb.Append(" (power down)");
                        break;
                    case "100":
                        sb.Append(" (internal zero-scale calibration)");
                        break;
                    case "101":
                        sb.Append(" (internal full-scale calibration)");
                        break;
                    case "110":
                        sb.Append(" (system zero-scale calibration)");
                        break;
                    case "111":
                        sb.Append(" (system full-scale calibration)");
                        break;
                }
                sb.Append(" | ");
                sb.Append($"DAT_STA: {(register & 0b0001_0000_0000_0000_0000_0000) >> 20}");
                sb.Append(" | ");
                sb.Append($"CLK: {UInt32ToBinaryString((register & 0b0000_1100_0000_0000_0000_0000) >> 18, 2)}");
                sb.Append(" | ");
                sb.Append($"AVG: {UInt32ToBinaryString((register & 0b0000_0011_0000_0000_0000_0000) >> 16, 2)}");
                sb.Append(" | ");
                sb.Append($"SINC3: {(register & 0b0000_0000_1000_0000_0000_0000) >> 15}");
                sb.Append(" | ");
                sb.Append($"0: {(register & 0b0000_0000_0100_0000_0000_0000) >> 14}");
                sb.Append(" | ");
                sb.Append($"ENPAR: {(register & 0b0000_0000_0010_0000_0000_0000) >> 13}");
                sb.Append(" | ");
                sb.Append($"CLK_DIV: {(register & 0b0000_0000_0001_0000_0000_0000) >> 12}");
                sb.Append(" | ");
                sb.Append($"Single: {(register & 0b0000_0000_0000_1000_0000_0000) >> 11}");
                sb.Append(" | ");
                sb.Append($"REJ60: {(register & 0b0000_0000_0000_0100_0000_0000) >> 10}");
                sb.Append(" | ");
                sb.Append($"FS: {UInt32ToBinaryString((register & 0b0000_0000_0000_0011_1111_1111), 10)}");

                return sb.ToString();
            }
        }

        public string Config
        {
            get
            {
                uint register = GetRegisterValue(AD7193_REG_CONF);

                sb.Clear();
                sb.Append($"Chop: {(register & 0b1000_0000_0000_0000_0000_0000) >> 23}");
                sb.Append(" | ");
                sb.Append($"00: {UInt32ToBinaryString((register & 0b0110_0000_0000_0000_0000_0000) >> 21, 2)}");
                sb.Append(" | ");
                sb.Append($"REFSEL: {(register & 0b0001_0000_0000_0000_0000_0000) >> 20}");
                sb.Append(" | ");
                sb.Append($"0: {(register & 0b0000_1000_0000_0000_0000_0000) >> 19}");
                sb.Append(" | ");
                sb.Append($"Pseudo: {(register & 0b0000_0100_0000_0000_0000_0000) >> 18}");
                sb.Append(" | ");
                sb.Append($"Channel: {UInt32ToBinaryString((register & 0b0000_0011_1111_1111_0000_0000) >> 8, 10)}");
                sb.Append(" | ");
                sb.Append($"Burn: {(register & 0b0000_0000_0000_0000_1000_0000) >> 7}");
                sb.Append(" | ");
                sb.Append($"REFDET: {(register & 0b0000_0000_0000_0000_0100_0000) >> 6}");
                sb.Append(" | ");
                sb.Append($"0: {(register & 0b0000_0000_0000_0000_0010_0000) >> 5}");
                sb.Append(" | ");
                sb.Append($"BUF: {(register & 0b0000_0000_0000_0000_0001_0000) >> 4}");
                sb.Append(" | ");
                sb.Append($"Unipolar: {(register & 0b0000_0000_0000_0000_0000_1000) >> 3}");
                sb.Append(" | ");
                sb.Append($"Gain: {UInt32ToBinaryString((register & 0b0000_0000_0000_0000_0000_0111), 3)}");

                return sb.ToString();
            }
        }

        public bool IsIdle
        {
            get
            {
                uint mode = GetRegisterValue(AD7193_REG_MODE);
                return ((mode & 0b1110_0000_0000_0000_0000_0000) >> 21) == 0b011;
            }
        }

        public bool IsReady
        {
            get
            {
                uint status = GetRegisterValue(AD7193_REG_STAT);

                return (status & 0b1000_0000) != 0b1000_0000;
            }
        }

        public bool HasErrors
        {
            get
            {
                uint status = GetRegisterValue(AD7193_REG_STAT);

                return (status & 0b0100_0000) == 0b0100_0000;
            }
        }

        private bool continuousRead = false;
        public bool ContinuousRead
        {
            set
            {
                if (value)
                {
                    SetRegisterValue(AD7193_REG_COMM, 0b0101_1100);
                }
                else
                {
                    SetRegisterValue(AD7193_REG_COMM, 0b0101_1000);
                }
                continuousRead = value;
            }

            get
            {
                return continuousRead;
            }
        }

        /// <summary>
        /// Enables or disables DAT_STA Bit (appends status register to data register when reading)
        /// </summary>
        public bool AppendStatusRegisterToData
        {
            set
            {
                registerCache[AD7193_REG_MODE] &= 0xEFFFFF;     // keep all bit values except DAT_STA bit

                if (value)
                {
                    registerCache[AD7193_REG_MODE] |= 0x100000;     // set DAT_STA to 1
                }
                else
                {
                    registerCache[AD7193_REG_MODE] |= 0x000000;     // set DAT_STA to 0
                }

                SetRegisterValue(AD7193_REG_MODE, registerCache[AD7193_REG_MODE]);

                if (value)
                {
                    registerSize[AD7193_REG_DATA] = 4;          // change register size to 4, b/c status register is now appended
                }
                else
                {
                    registerSize[AD7193_REG_DATA] = 3;          // change register size to 3
                }
            }

            get
            {
                return ((registerCache[AD7193_REG_MODE] & 0x100000) == 0x100000);
            }
        }

        public bool JitterCorrection { get; set; }

        /// <summary>
        /// Switches from differential input to pseudo differential inputs.
        /// When the pseudo bit is set to 1, the AD7193 is configured to have eight pseudo differential analog inputs. When pseudo bit is set to 0, the AD7193 is configured to have four differential analog inputs.
        /// </summary>
        public AnalogInputModes AnalogInputMode 
        { 
            get
            {
                return (AnalogInputModes)((registerCache[AD7193_REG_CONF] & 0b0000_0100_0000_0000_0000_0000) >> 18);
            }

            set
            {
                registerCache[AD7193_REG_CONF] &= 0b1111_1011_1111_1111_1111_1111;

                if (value == AnalogInputModes.FourDifferentialAnalogInputs)
                {
                    registerCache[AD7193_REG_CONF] |= 0 << 11;
                }

                if (value == AnalogInputModes.EightPseudoDifferentialAnalogInputs)
                {
                    registerCache[AD7193_REG_CONF] |= 1 << 11;
                }

                SetRegisterValue(AD7193_REG_CONF, registerCache[AD7193_REG_CONF]);

            }
        }

        /// <summary>
        /// Changes the currently selected channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        public Channel ActiveChannels
        {
            set
            {
                // generate Channel settings bits for Configuration write
                uint channelBits = (uint)value << 8;

                // write Channel bits to Config register, keeping other bits as is
                registerCache[AD7193_REG_CONF] &= 0xFC00FF;       // keep all bit values except Channel bits
                registerCache[AD7193_REG_CONF] |= channelBits;

                // write channel selected to Configuration register
                SetRegisterValue(AD7193_REG_CONF, registerCache[AD7193_REG_CONF]);
            }
        }


        /// <summary>
        /// Sets the amount of averaging. The data from the sinc filter is averaged by 2, 8, or 16. The averaging reduces the output data rate for a given FS word; however, the RMS noise improves.
        /// </summary>
        public AveragingModes Averaging
        {
            set
            {
                registerCache[AD7193_REG_MODE] &= 0xFC_FFFF;                //keep all bit values except Averaging setting bits
                registerCache[AD7193_REG_MODE] |= ((uint)value) << 16;

                SetRegisterValue(AD7193_REG_MODE, registerCache[AD7193_REG_MODE]);
            }
        }

        /// <summary>
        /// Set the filter output data rate select bits. The 10 bits of data programmed into these bits determine the filter cutoff frequency, the position of the first notch of the filter, and the output data rate for the part.
        /// </summary>
        /// <param name="filterRate"></param>
        public ushort Filter
        {
            set
            {
                if (value > 0x03FF)
                {
                    throw new ArgumentException("Filter rate is too high, it must be a 10-bit value.");
                }

                registerCache[AD7193_REG_MODE] &= 0xFFFC00;         //keep all bit values except Filter setting bits
                registerCache[AD7193_REG_MODE] |= (uint)value << 0;

                SetRegisterValue(AD7193_REG_MODE, registerCache[AD7193_REG_MODE]);
            }
        }

        public uint Offset 
        { 
            get
            {
                registerCache[AD7193_REG_OFFSET] = GetRegisterValue(AD7193_REG_OFFSET) & 0b0000_0000_1111_1111_1111_1111_1111_1111;
                return registerCache[AD7193_REG_OFFSET];
            }

            set
            {
                registerCache[AD7193_REG_OFFSET] = value & 0b0000_0000_1111_1111_1111_1111_1111_1111;
                SetRegisterValue(AD7193_REG_OFFSET, registerCache[AD7193_REG_OFFSET]);
            }
        }

        public uint FullScale
        {
            get
            {
                registerCache[AD7193_REG_FULLSCALE] = GetRegisterValue(AD7193_REG_FULLSCALE) & 0b0000_0000_1111_1111_1111_1111_1111_1111;
                return registerCache[AD7193_REG_FULLSCALE];
            }

            set
            {
                registerCache[AD7193_REG_FULLSCALE] = value & 0b0000_0000_1111_1111_1111_1111_1111_1111;
                SetRegisterValue(AD7193_REG_FULLSCALE, registerCache[AD7193_REG_FULLSCALE]);
            }
        }

        public Ad7193(SpiDevice spiDevice)
        {
            if (spiDevice.ConnectionSettings.Mode != SpiMode.Mode3)
            {
                throw new Exception("SPI device must be in SPI mode 3 in order to work with AD7193.");
            }

            this.spiDevice = spiDevice;

            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < 6; i++)

            {
                spiDevice.Write(new byte[] { 0xFF });
            }
        }

        /// <summary>
        /// Initiates internal calibration, including zero-scale and full-scale calibrations
        /// </summary>
        public void Calibrate()
        {
            registerCache[AD7193_REG_MODE] &= 0x1FFFFF;         // keep all bit values except Mode bits
            registerCache[AD7193_REG_MODE] |= 0x800000;         // internal zero scale calibration (MD2 = 1, MD1 = 0, MD0 = 0)

            SetRegisterValue(AD7193_REG_MODE, registerCache[AD7193_REG_MODE]);     // overwriting previous MODE reg setting 

            WaitForADC();

            registerCache[AD7193_REG_MODE] &= 0x1FFFFF;         // keep all bit values except Mode bits
            registerCache[AD7193_REG_MODE] |= 0xA00000;         // internal full scale calibration (MD2 = 1, MD1 = 0, MD0 = 1)

            SetRegisterValue(AD7193_REG_MODE, registerCache[AD7193_REG_MODE]);     // overwriting previous MODE reg setting 

            WaitForADC();
        }

        /// <summary>
        /// Waits after a single conversion until the DOUT/!RDY goes low to indicate the completion of a conversion
        /// </summary>
        public void WaitForADC()
        {
            while (!this.IsReady)
            {
                Thread.Sleep(5);
            }
        }


        /// <summary>
        /// Initiate Single Conversion (device will go into low power mode when conversion complete, and DOUT/!RDY goes low to indicate the completion of a conversion)
        /// </summary>
        public void StartSingleConversion()
        {
            registerCache[AD7193_REG_MODE] &= 0x1FFFFF; // keep all bit values except Mode bits
            registerCache[AD7193_REG_MODE] |= 0x200000; // single conversion mode bits (MD2 = 0, MD1 = 0, MD0 = 1)

            SetRegisterValue(AD7193_REG_MODE, registerCache[AD7193_REG_MODE]);

            stopWatch.Restart();
        }

        public void StartContinuousConversion(uint frequency = ADCSamplerate)
        {            
            registerCache[AD7193_REG_MODE] &= 0x1FFFFF; // keep all bit values except Mode bits
            registerCache[AD7193_REG_MODE] |= 0x000000; // continuous conversion mode bits (MD2 = 0, MD1 = 0, MD0 = 0)

            SetRegisterValue(AD7193_REG_MODE, registerCache[AD7193_REG_MODE]);

            ContinuousRead = true;

            
            long samplePerTicks = Stopwatch.Frequency / frequency;
            if (samplePerTicks == 0) samplePerTicks = 1;

            stopWatch.Restart();
            Task samplingTask = Task.Run(() =>
            {
                long samples = 0;
                long nextSampleAt = stopWatch.ElapsedTicks;
                long elapsedTicks = 0;
                long jitter = 0;
                long maxJitterCorrectionPerSample = Math.Max(Stopwatch.Frequency / 100000, 1);
                while (stopWatch.IsRunning)
                {
                    elapsedTicks = stopWatch.ElapsedTicks;
                    if (elapsedTicks >= nextSampleAt)
                    {
                        nextSampleAt = elapsedTicks + samplePerTicks;
                        ReadADCValue();
                        samples++;
                        jitter = (elapsedTicks - (samples * samplePerTicks));
                        if (this.JitterCorrection)
                        {
                            if (jitter > 0)
                            {
                                nextSampleAt -= Math.Min(jitter, maxJitterCorrectionPerSample);
                            } else if (jitter < 0)
                            {
                                nextSampleAt += Math.Min(-jitter, maxJitterCorrectionPerSample);
                            }
                        }
                    }
                }
            });           
        }

        /// <summary>
        /// Reads the current ADC result value on the selected channel
        /// </summary>
        /// <returns>24-bit raw value of the last ADC result (+ status byte if enabled)</returns>
        public uint? ReadADCValue()
        {
            uint raw = GetRegisterValue(AD7193_REG_DATA);

            // update the status register cache if we have it here
            if (this.AppendStatusRegisterToData)
            {
                registerCache[AD7193_REG_STAT] = (byte)(raw & 0xFF);
                raw = (raw & 0xFFFFFF00) >> 8;
            }

            // check if we have an error
            if (this.HasErrors)
            {
                return null;
            }

            // create the new AdcValue object and calculate the voltage
            var adcValue = new AdcValue() { Raw = raw, Time = stopWatch.ElapsedTicks, Channel = (byte)(registerCache[AD7193_REG_STAT] & 0b0000_1111), Voltage = RawValueToVoltage(raw) };

            // add it to the collection
            //AdcValues.Enqueue(adcValue);
            AdcValues.Add(adcValue);

            // call the event handler
            AdcValueReceived?.Invoke(this, new AdcValueReceivedEventArgs(adcValue));

            return raw;
        }

        /// <summary>
        /// Reads a single value on the selected channel
        /// </summary>
        /// <param name="channel">Channel index</param>
        /// <returns></returns>
        public uint? ReadSingleADCValue(Channel channel)
        {
            this.ActiveChannels = channel;

            StartSingleConversion();

            WaitForADC();

            return ReadADCValue();
        }


        /// <summary>
        /// Converts the ADC result to Volts
        /// </summary>
        /// <param name="adcValue">The raw ADC result</param>
        /// <returns></returns>
        public float RawValueToVoltage(uint adcValue)
        {
            // 0 - bipolar (ranges from ±19.53 mV to ±2.5 V) ; 1 - unipolar (ranges from 0 mV to 19.53 mV to 0 V to 2.5 V)
            byte mPolarity = (byte)(registerCache[AD7193_REG_CONF] & 0b0000_0000_0000_0000_0000_1000 >> 3);

            ulong pgaSetting = registerCache[AD7193_REG_CONF] & 0b0000_0000_0000_0000_0000_0111;  // keep only the PGA setting bits
            float pgaGain = 1;

            switch (pgaSetting)
            {
                case 0b000:
                    pgaGain = 1;
                    break;
                case 0b011:
                    pgaGain = 8;
                    break;
                case 0b100:
                    pgaGain = 16;
                    break;
                case 0b101:
                    pgaGain = 32;
                    break;
                case 0b110:
                    pgaGain = 64;
                    break;
                case 0b111:
                    pgaGain = 128;
                    break;
            }

            float voltage = 0;
            if (mPolarity == 1)
            {
                voltage = (float)adcValue / 16777216.0f;
            }
            if (mPolarity == 0)
            {
                voltage = ((float)adcValue / 8388608.0f) - 1.0f;
            }

            voltage *= (this.VReference / pgaGain);


            return (voltage);
        }

        /// <summary>
        /// Converts the ADC result to Celsius
        /// </summary>
        /// <param name="adcValue">The raw ADC result</param>
        /// <returns></returns>
        public float ADCValueToCelsius(ulong adcValue)
        {
            float degreeCelsius = ((float)(adcValue - 0x800000) / 2815.0f) - 273.0f;
            float degreeFahrenheit = (degreeCelsius * 9 / 5) + 32;
            return (degreeCelsius);
        }


        /// <summary>
        /// Reads the value of a register
        /// </summary>
        /// <param name="registerAddress"></param>
        /// <param name="bytesNumber"></param>
        /// <returns></returns>
        public uint GetRegisterValue(byte registerAddress)
        {
            byte byteNumber = registerSize[registerAddress];
            byte commandByte = 0;
            byte[] writeBuffer = new byte[byteNumber + 1];


            commandByte = (byte)(AD7193_COMM_READ | GetCommAddress(registerAddress));
            writeBuffer[0] = commandByte;


            byte[] readBuffer = new byte[writeBuffer.Length];
            lock (spiTransferLock)
            {
                spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
            }
            readBuffer = readBuffer[1..];

            registerCache[registerAddress] = ByteArrayToUInt32(readBuffer);

            //Debug.WriteLine($"Read Register - address: {registerAddress.ToString("X2")}, command: {commandByte.ToString("X2")}, received: {String.Join(' ', readBuffer.Select(x => x.ToString("X2")))}");

            return registerCache[registerAddress];
        }

        /// <summary>
        /// Writes data into a register
        /// </summary>
        /// <param name="registerAddress"></param>
        /// <param name="registerValue"></param>
        /// <param name="byteNumber"></param>
        public void SetRegisterValue(byte registerAddress, uint registerValue)
        {
            byte byteNumber = registerSize[registerAddress];
            byte commandByte = 0;
            byte[] writeBuffer = new byte[byteNumber + 1];


            commandByte = (byte)(AD7193_COMM_WRITE | GetCommAddress(registerAddress));
            writeBuffer[0] = commandByte;

            byte[] buffer = UInt32ToByteArray(registerValue, byteNumber);
            Array.Copy(buffer, 0, writeBuffer, 1, byteNumber);

            lock (spiTransferLock)
            {
                spiDevice.Write(writeBuffer);
            }
            writeBuffer = writeBuffer[1..];

            //Debug.WriteLine($"Write Register - address: {registerAddress.ToString("X2")}, command: {commandByte.ToString("X2")}, sent: {String.Join(' ', writeBuffer.Select(x => x.ToString("X2")))}");
        }

        public List<uint> GetAllRegisterValues()
        {
            List<uint> result = new List<uint>();

            result.Add(GetRegisterValue(AD7193_REG_STAT));
            result.Add(GetRegisterValue(AD7193_REG_MODE));
            result.Add(GetRegisterValue(AD7193_REG_CONF));
            result.Add(GetRegisterValue(AD7193_REG_DATA));
            result.Add(GetRegisterValue(AD7193_REG_ID));
            result.Add(GetRegisterValue(AD7193_REG_GPOCON));
            result.Add(GetRegisterValue(AD7193_REG_OFFSET));
            result.Add(GetRegisterValue(AD7193_REG_FULLSCALE));

            return result;
        }

        private static int GetCommAddress(int x)
        {
            return (((x) & 0x07) << 3);
        }

        public void Dispose()
        {
            if (spiDevice != null)
            {
                spiDevice?.Dispose();
                spiDevice = null;
            }
        }

        private uint ByteArrayToUInt32(byte[] buffer)
        {
            byte[] fourByteRawValue = new byte[4];
            for (int i = 0; i < buffer.Length; i++)
            {
                fourByteRawValue[buffer.Length - 1 - i] = buffer[i];
            }
            return BitConverter.ToUInt32(fourByteRawValue);
        }

        private byte[] UInt32ToByteArray(uint number, byte byteNumber)
        {
            byte[] result = new byte[byteNumber];
            for (int i = 0; i < result.Length; i++)
            {
                result[byteNumber - 1 - i] = (byte)(number & 0xFF);
                number = number >> 8;
            }
            return result;
        }

        private string UInt32ToBinaryString(uint number, byte padding)
        {
            const int mask = 1;
            var binary = string.Empty;
            while (number > 0)
            {
                // Logical AND the number and prepend it to the result string
                binary = (number & mask) + binary;
                number = number >> 1;
            }

            return binary.PadLeft(padding, '0');
        }
    }
}
