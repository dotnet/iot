// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Iot.Device.Card;
using Iot.Device.Pn532.AsTarget;
using Iot.Device.Pn532.ListPassive;
using Iot.Device.Pn532.RfConfiguration;
using Iot.Device.Rfid;
using IoT.Device.Pn532;

#pragma warning disable SA1011

namespace Iot.Device.Pn532
{
    /// <summary>
    /// PN532 RFID/NFC reader
    /// </summary>
    public class Pn532 : CardTransceiver, IDisposable
    {
        private const int I2cMaxBuffer = 1024;
        // Communication way
        private const byte ToHostCheckSumD5 = 0xD5;
        private const byte FromHostCheckSumD4 = 0xD4;
        // Preamble, codes and postamble
        private const byte Preamble = 0x00;
        private const byte Postamble = 0x00;
        private const byte StartCode1 = 0x00;
        private const byte StartCode2 = 0xFF;
        // Operation type for SPI
        private const byte WriteData = 0b0000_0001;
        private const byte ReadStatus = 0b0000_0010;
        private const byte ReadData = 0b0000_0011;

        // Acknowledge
        private byte[] _ackBuffer = { 0x00, 0x00, 0xFF, 0x00, 0xFF, 0x00 };

        // Specific buffer to wake up the sensor in serial HSU mode
        private byte[] _serialWakeUp = new byte[]
        {
            0x55, 0x55, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        private byte[] _i2CWakeUp = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private ParametersFlags _parametersFlags;
        private SpiDevice? _spiDevice = null;
        private I2cDevice? _i2cDevice = null;
        private GpioController? _controller = null;
        private SerialPort? _serialPort = null;
        private int _pin = 18;
        private SecurityAccessModuleMode _securityAccessModuleMode = SecurityAccessModuleMode.Normal;
        private uint _virtualCardTimeout = 0x17;

        /// <summary>
        /// Set or get the read timeout for I2C and SPI
        /// Please refer to the documentation to set the right
        /// timeout value depending on the communication
        /// mode you are using
        /// </summary>
        public int ReadTimeOut { get; set; } = 500;

        /// <summary>
        /// The Log level
        /// </summary>
        public LogLevel LogLevel
        {
            get { return LogInfo.LogLevel; }
            set { LogInfo.LogLevel = value; }
        }

        /// <summary>
        /// The location to log the info
        /// </summary>
        public LogTo LogTo
        {
            get { return LogInfo.LogTo; }
            set { LogInfo.LogTo = value; }
        }

        /// <summary>
        /// Firmware version information
        /// </summary>
        public FirmwareVersion? FirmwareVersion { get; internal set; }

        #region Spi and I2c Settings

        /// <summary>
        /// PN532 SPI Clock Frequency
        /// </summary>
        public const int SpiClockFrequency = 2_000_000;

        /// <summary>
        /// Only SPI Mode supported is Mode0
        /// </summary>
        public const SpiMode SpiMode = System.Device.Spi.SpiMode.Mode0;

        /// <summary>
        /// The default I2C address
        /// </summary>
        public const byte I2cDefaultAddress = 0x24;

        #endregion

        /// <summary>
        /// Create a PN532 using Serial Port
        /// </summary>
        /// <param name="portName">The port name</param>
        /// /// <param name="logLevel">The log level</param>
        public Pn532(string portName, LogLevel logLevel = LogLevel.None)
        {
            LogLevel = logLevel;

            // Data bit : 8 bits,
            // Parity bit : none,
            // Stop bit : 1 bit,
            // Baud rate : 115 200 bauds,
            LogInfo.Log("Opening serial port 115200, Parity.None, 8 bits, StopBits.One", LogLevel.Debug);
            _serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            // Documentation page 39, serial timeout is 89 ms for 115 200
            _serialPort.ReadTimeout = 89;
            _serialPort.WriteTimeout = 89;
            _serialPort.Open();
            // Setting up internals as default version
            _parametersFlags = ParametersFlags.AutomaticATR_RES | ParametersFlags.AutomaticRATS |
                               ParametersFlags.ISO14443_4_PICC;
            _securityAccessModuleMode = SecurityAccessModuleMode.Normal;
            _virtualCardTimeout = 0x17;

            WakeUp();
            // Set the SAM
            bool ret = SetSecurityAccessModule();
            LogInfo.Log($"Setting SAM changed: {ret}", LogLevel.Info);
            // Check the version
            if (!IsPn532())
            {
                throw new Exception("Can't find a PN532");
            }

            // Apply default parameters
            ret = SetParameters(ParametersFlags.AutomaticATR_RES | ParametersFlags.AutomaticRATS);
            LogInfo.Log($"Setting Parameters Flags changed: {ret}", LogLevel.Info);
        }

        /// <summary>
        /// Create a PN532 using SPI
        /// </summary>
        /// <param name="spiDevice">The SPI Device</param>
        /// <param name="logLevel">The log level</param>
        public Pn532(SpiDevice spiDevice, LogLevel logLevel = LogLevel.None)
        {
            LogLevel = logLevel;
            _spiDevice = spiDevice;
            _controller = new GpioController();
            _controller.OpenPin(_pin, PinMode.Output);
            _controller.Write(_pin, PinValue.High);
            Thread.Sleep(2);
            WakeUp();
            // The first time we apply SAM after waking up the device, it always
            // returns false, some timeout appear. So we will need to apply a second time
            bool ret = SetSecurityAccessModule();
            LogInfo.Log($"Setting SAM changed: {ret}", LogLevel.Info);
            // Check the version
            if (!IsPn532())
            {
                throw new Exception("Can't find a PN532");
            }

            // Apply default parameters
            ret = SetParameters(ParametersFlags.AutomaticATR_RES | ParametersFlags.AutomaticRATS);
            LogInfo.Log($"Setting Parameters Flags changed: {ret}", LogLevel.Info);
            ret = SetSecurityAccessModule();
            LogInfo.Log($"Setting SAM changed: {ret}", LogLevel.Info);
        }

        /// <summary>
        /// Create a PN532 using I2C
        /// </summary>
        /// <param name="i2CDevice">The I2C device</param>
        /// /// <param name="logLevel">The log level</param>
        public Pn532(I2cDevice i2CDevice, LogLevel logLevel = LogLevel.None)
        {
            LogLevel = logLevel;
            _i2cDevice = i2CDevice;
            WakeUp();
            bool ret = SetSecurityAccessModule();
            LogInfo.Log($"Setting SAM changed: {ret}", LogLevel.Info);
            // Check the version
            if (!IsPn532())
            {
                throw new Exception("Can't find a PN532");
            }

            // Apply default parameters
            ret = SetParameters(ParametersFlags.AutomaticATR_RES | ParametersFlags.AutomaticRATS);
            LogInfo.Log($"Setting Parameters Flags changed: {ret}", LogLevel.Info);
            ret = SetSecurityAccessModule();
            LogInfo.Log($"Setting SAM changed: {ret}", LogLevel.Info);
        }

        /// <summary>
        /// Run self tests
        /// Note: some self tests are not implemented yet
        /// </summary>
        /// <param name="diagnoseMode">The self test to run</param>
        /// <returns>True when success</returns>
        public bool RunSelfTest(DiagnoseMode diagnoseMode)
        {
            int ret = 0;
            Span<byte> singleParam = stackalloc byte[1]
            {
                (byte)diagnoseMode
            };
            switch (diagnoseMode)
            {
                case DiagnoseMode.CommunicationLineTest:
                    // NumTst = 0x00: Communication Line Test
                    // This test is for communication test between host controller and the PN532. “Parameter
                    // Length” and “Parameters” in response packet are same as “Parameter Length” and
                    // “Parameter” in command packet.
                    // − Parameter Length : m (0 <= m <= 262),
                    // − Parameter : Data,
                    // − Result Length : Same value of m + 1.
                    // OutParam consists of NumTst concatenate with InParam.
                    Span<byte> toTest = stackalloc byte[9]
                    {
                        (byte)DiagnoseMode.CommunicationLineTest,
                        0x11,
                        0x22,
                        0x33,
                        0x44,
                        0x55,
                        0x66,
                        0x77,
                        0x88
                    };
                    ret = WriteCommand(CommandSet.Diagnose, toTest);
                    if (ret < 0)
                    {
                        return false;
                    }

                    Span<byte> resultTest = stackalloc byte[9];
                    ret = ReadResponse(CommandSet.Diagnose, resultTest);
                    LogInfo.Log($"{diagnoseMode} received: {BitConverter.ToString(resultTest.ToArray())}, ret: {ret}",
                        LogLevel.Debug);
                    return resultTest.SequenceEqual(toTest) && (ret >= 0);
                case DiagnoseMode.ROMTest:
                    // NumTst = 0x01: ROM Test
                    // This test is for checking ROM data by 8 bits checksum.
                    // − Parameter Length : 0,
                    // − Result Length : 1,
                    // − Result : 0x00 is OK,
                    // 0xFF is Not Good
                    ret = WriteCommand(CommandSet.Diagnose, singleParam);
                    if (ret < 0)
                    {
                        return false;
                    }

                    // Wait for the test to run
                    Thread.Sleep(1500);
                    Span<byte> romTest = stackalloc byte[1];
                    ret = ReadResponse(CommandSet.Diagnose, romTest);
                    LogInfo.Log($"{diagnoseMode} received: {BitConverter.ToString(romTest.ToArray())}, ret: {ret}",
                        LogLevel.Debug);
                    // Wait for the test to run
                    // TODO: find the right timing, this is empirical
                    Thread.Sleep(100);
                    return (romTest[0] == 0) && (ret >= 0);
                case DiagnoseMode.RAMTest:
                    // NumTst = 0x02: RAM Test
                    // This test is for checking RAM; 768 bytes of XRAM and 128 bytes of IDATA.
                    // The test method used consists of saving original content, writing test data, checking test
                    // data and finally restore original data. So, this test is non destructive.
                    // − Parameter Length : 0,
                    // − Result Length : 1,
                    // − Result : 0x00 is OK,
                    // 0xFF is Not Good.
                    ret = WriteCommand(CommandSet.Diagnose, singleParam);
                    if (ret < 0)
                    {
                        return false;
                    }

                    // Wait for the test to run
                    // TODO: find the right timing, this is empirical
                    Thread.Sleep(1500);
                    Span<byte> ramTest = stackalloc byte[1];
                    ret = ReadResponse(CommandSet.Diagnose, ramTest);
                    LogInfo.Log($"{diagnoseMode} received: {BitConverter.ToString(ramTest.ToArray())}, ret: {ret}",
                        LogLevel.Debug);
                    Thread.Sleep(100);
                    return (ramTest[0] == 0) && (ret >= 0);
                case DiagnoseMode.PollingTestToTarget:
                    // NumTst = 0x04 : Polling Test to Target
                    // This test is for checking the percentage of failure regarding response packet receiving
                    // after polling command transmission. In this test, the PN532 sends a FeliCa polling
                    // command packet 128 times to target. The PN532 counts the number of fails and returns
                    // the failed number to host controller. This test doesn’t require specific system code for
                    // target.
                    // Polling is done with system code (0xFF, 0xFF). The baud rate used is either 212 kbps or
                    // 424 kbps.
                    // One polling is considered as defective after no correct polling response within 4 ms.
                    // During this test, the analog settings used are those defined in command
                    // RFConfiguration within the item n°7 (§7.3.1, p: 101).
                    // − Parameter Length : 1,
                    // − Parameter : 0x01 is 212 kbps,
                    // 0x02 is 424 kbps.
                    // − Result Length : 1,
                    // − Result : Number of fails (Maximum 128).
                    throw new NotImplementedException($"Test {diagnoseMode} not implemented");
                case DiagnoseMode.EchoBackTest:
                    // NumTst = 0x05 : Echo Back Test
                    // In this test, the PN532 is configured in target mode. The analog settings used are those
                    // defined by using the command RFConfiguration with the item n°6 (§7.3.1, p: 101).
                    // This test is running as long as a new command is not received from the host controller.
                    // The principle of this test is that the PN532 waits for a command frame coming from the
                    // initiator and after the Reply Delay, sends it back to it whatever its content and its length
                    // are.
                    // − Parameter Length : 3,
                    // − Parameter 1 : Reply Delay (step of 0.5 ms),
                    // − Parameter 2 : Content of the CIU_TxMode (@0x6302) register
                    // defining the baud rate and the modulation type in
                    // transmission,
                    // − Parameter 3 : Content of the CIU_RxMode (@0x6303) register
                    // defining the baud rate and the modulation type in
                    // reception,
                    // − Result Length : no result, the test runs infinitely, so no output frame is
                    // sent to the host controller.
                    // For example:
                    // − The PN532 is configured to receive frame with passive 106 kbps modulation
                    // type. The frames are sent back immediately.
                    // The MSB bit (CRC enable) of CIU_TxMode and CIU_RxMode must be set to 0.
                    // D4 00 05 00 00 00
                    // − The PN532 is configured to receive frame with passive 212 kbps modulation
                    // type. The frames are sent back with a delay of 64 ms.
                    // The MSB bit (CRC enable) of CIU_TxMode and CIU_RxMode must be set to 1.
                    // D4 00 05 80 92 92
                    // − The PN532 is configured to receive frame with passive 424 kbps modulation
                    // type. The frames are sent back immediately.
                    // The MSB bit (CRC enable) of CIU_TxMode and CIU_RxMode must be set to 1.
                    // D4 00 05 00 A2 A2
                    throw new NotImplementedException($"Test {diagnoseMode} not implemented");
                case DiagnoseMode.AttentionRequestTest:
                    // NumTst = 0x06 : Attention Request Test or ISO/IEC14443-4 card presence detection
                    // This test can be used by an initiator to ensure that a target/card is still in the field:
                    // o In case of DEP target, an Attention Request command is sent to the target, and it
                    // is expected to receive the same answer from the target. In that case, the test is
                    // declared as successful;
                    // o In case of ISO/IEC14443-4 card, a R(NACK) block is sent to the card and it is
                    // expected to receive either a R(ACK) block or the last I-Block. In that case, the test
                    // is declared as successful (ISO/IEC14443-4 card is still in the RF field).
                    // In case of no or incorrect response, the Result informs about the status of the transaction
                    // (refer. to §7.1, p:67)
                    // − Parameter Length : 0,
                    // − Result Length : 1,
                    // − Result : 0x00 is OK,
                    // different from 0x00 is Not OK, Status byte.
                    throw new NotImplementedException($"Test {diagnoseMode} not implemented");
                case DiagnoseMode.SelfAntenaTest:
                    // NumTst = 0x07 : Self Antenna Test
                    // This test is used to check the continuity of the transmission paths of the antenna.
                    // − Parameter Length : 1,
                    // − Parameter : Threshold used for antenna detection
                    // (applied in register Andet_Control (@610C), see Error!
                    // Reference source not found.),
                    // 7 6 5 4 3 2 1 0
                    // andet_bot andet_up andet_ithl[1:0] andet_ithh[1:0] andet_en
                    // − Result Length : 1,
                    // − Result : 0x00 is OK (antenna is detected),
                    // different from 0x00 is not OK (no antenna is detected).
                    throw new NotImplementedException($"Test {diagnoseMode} not implemented");
                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Get or set the timeout when PN532 is in virtual card mode
        /// </summary>
        public uint VirtualCardTimeout
        {
            get
            {
                return _virtualCardTimeout * 50;
            }

            set
            {
                // Timeout defines the time-out only in Virtual card configuration (Mode = 0x02).
                // In Virtual Card mode, this field is mandatory; whereas in the other mode, it is optional.
                // This parameter indicates the timeout value with a LSB of 50ms.
                // There is no timeout control if the value is null (Timeout = 0).
                // The maximum value for the timeout is 12.75 sec (Timeout = 0xFF).
                if (value / 50 > 0xFF)
                {
                    throw new ArgumentException($"{nameof(VirtualCardTimeout)} can't be more than 12750 milliseconds.");
                }

                _virtualCardTimeout = value / 50;
                bool ret = SetSecurityAccessModule();
                LogInfo.Log($"{nameof(VirtualCardTimeout)} changed: {ret}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// Get or set the Security Access Module Mode
        /// </summary>
        public SecurityAccessModuleMode SecurityAccessModuleMode
        {
            get
            {
                return _securityAccessModuleMode;
            }

            set
            {
                bool ret = SetSecurityAccessModule();
                LogInfo.Log($"{nameof(SecurityAccessModuleMode)} changed: {ret}", LogLevel.Debug);
            }
        }

        private bool SetSecurityAccessModule()
        {
            // Pass the SAM, the virtual card timeout and remove IRQ
            Span<byte> toSend = stackalloc byte[3]
            {
                (byte)_securityAccessModuleMode, (byte)(_virtualCardTimeout), 0x00
            };
            var ret = WriteCommand(CommandSet.SAMConfiguration, toSend);
            LogInfo.Log($"{nameof(SetSecurityAccessModule)} Write: {ret}", LogLevel.Debug);
            if (ret < 0)
            {
                return false;
            }

            // We don't expect any result, just that the command went well
            ret = ReadResponse(CommandSet.SAMConfiguration, Span<byte>.Empty);
            LogInfo.Log($"{nameof(SetSecurityAccessModule)} read: {ret}", LogLevel.Debug);
            return ret >= 0;
        }

        private bool IsPn532()
        {
            var ret = WriteCommand(CommandSet.GetFirmwareVersion);
            LogInfo.Log($"GetFirmwareVersion write command returned: {ret}", LogLevel.Info);
            if (ret < 0)
            {
                return false;
            }

            Span<byte> firmware = stackalloc byte[4];
            ret = ReadResponse(CommandSet.GetFirmwareVersion, firmware);
            var ver = firmware.ToArray();
            if (ret >= 0)
            {
                FirmwareVersion = new FirmwareVersion(
                    firmware[0],                                    // IdentificationCode
                    new Version(firmware[1], firmware[2]),          // Version
                    (VersionSupported)(firmware[3] & 0b0000_0111)); // VersionSupported
            }

            LogInfo.Log($"GetFirmwareVersion read command returned: {ret} - Bytes {BitConverter.ToString(ver)}",
                LogLevel.Info);
            return ret >= 0;
        }

        /// <summary>
        /// Get or set the Security Access Module parameters
        /// </summary>
        public ParametersFlags ParametersFlags
        {
            get
            {
                return _parametersFlags;
            }

            set
            {
                if (SetParameters(value))
                {
                    _parametersFlags = value;
                }
            }
        }

        private bool SetParameters(ParametersFlags parametersFlags)
        {
            Span<byte> toSend = stackalloc byte[1]
            {
                (byte)parametersFlags
            };
            var ret = WriteCommand(CommandSet.SetParameters, toSend);
            if (ret < 0)
            {
                return false;
            }

            ret = ReadResponse(CommandSet.SetParameters, Span<byte>.Empty);
            LogInfo.Log($"{nameof(SetParameters)}: {ret}", LogLevel.Debug);
            return ret >= 0;
        }

        /// <summary>
        /// List all targets cards in range
        /// When using this function, you can't determine which target you've read
        /// So you'll need to use the Decode functions to try to get a card type
        /// So use this function only with a specific card type. Prefer the AutoPoll function
        /// As the type identified is returned
        /// </summary>
        /// <param name="maxTarget">The maximum number of targets</param>
        /// <param name="targetBaudRate">The baud rate to use</param>
        /// <returns>A raw byte array with the data of the targets if any has been identified</returns>
        public byte[]? ListPassiveTarget(MaxTarget maxTarget, TargetBaudRate targetBaudRate)
        {
            return ListPassiveTarget(maxTarget, targetBaudRate, Span<byte>.Empty);
        }

        /// <summary>
        /// List all targets cards in range
        /// When using this function, you can't determine which target you've read
        /// So you'll need to use the Decode functions to try to get a card type
        /// So use this function only with a specific card type. Prefer the AutoPoll function
        /// As the type identified is returned
        /// </summary>
        /// <param name="maxTarget">The maximum number of targets</param>
        /// <param name="targetBaudRate">The baud rate to use to find cards</param>
        /// <param name="initiatorData">Specific initialization data</param>
        /// <returns>A raw byte array with the data of the targets if any has been identified</returns>
        public byte[]? ListPassiveTarget(MaxTarget maxTarget, TargetBaudRate targetBaudRate,
            ReadOnlySpan<byte> initiatorData)
        {
            Span<byte> toSend = stackalloc byte[2 + initiatorData.Length];
            toSend[0] = (byte)maxTarget;
            toSend[1] = (byte)targetBaudRate;
            if (initiatorData.Length > 0)
            {
                initiatorData.CopyTo(toSend.Slice(2));
            }

            var ret = WriteCommand(CommandSet.InListPassiveTarget, toSend);
            if (ret < 0)
            {
                return null;
            }

            // TODO: check what is the real maximum size
            Span<byte> listData = stackalloc byte[1024];
            ret = ReadResponse(CommandSet.InListPassiveTarget, listData);
            LogInfo.Log($"{nameof(ListPassiveTarget)}: {ret}, number tags: {listData[0]}", LogLevel.Debug);
            if ((ret >= 0) && (listData[0] > 0))
            {
                return listData.Slice(0, ret).ToArray();
            }

            return null;
        }

        /// <summary>
        /// Try to decode a raw byte array containing target information
        /// to a 106 kbps Type A card
        /// </summary>
        /// <param name="toDecode">The raw byte array</param>
        /// <returns>A decoded card of null if it can't</returns>
        public Data106kbpsTypeA? TryDecode106kbpsTypeA(Span<byte> toDecode)
        {
            try
            {
                byte[] nfcId = new byte[toDecode[4]];
                byte[] ats = new byte[toDecode[5 + nfcId.Length]];

                for (int i = 0; i < nfcId.Length; i++)
                {
                    nfcId[i] = toDecode[5 + i];
                }

                if ((5 + nfcId.Length) > toDecode.Length)
                {
                    for (int i = 0; i < ats.Length; i++)
                    {
                        ats[i] = toDecode[6 + nfcId.Length + i];
                    }
                }

                Data106kbpsTypeA data = new Data106kbpsTypeA(
                    toDecode[0],    // TargetNumber
                    BinaryPrimitives.ReadUInt16BigEndian(toDecode.Slice(1)), // Atqa
                    toDecode[3],    // Sak
                    nfcId,
                    ats);

                return data;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Try to decode a raw byte array containing target information
        /// to a 106 kbps Type B card
        /// </summary>
        /// <param name="toDecode">The raw byte array</param>
        /// <returns>A decoded card of null if it can't</returns>
        public Data106kbpsTypeB? TryDecodeData106kbpsTypeB(Span<byte> toDecode)
        {
            try
            {
                Data106kbpsTypeB data = new Data106kbpsTypeB(toDecode.Slice(1).ToArray());
                data.TargetNumber = toDecode[0];
                return data;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Try to decode a raw byte array containing target information
        /// to a 212 424 kbps card
        /// </summary>
        /// <param name="toDecode">The raw byte array</param>
        /// <returns>A decoded card of null if it can't</returns>
        public Data212_424kbps? TryDecodeData212_424Kbps(Span<byte> toDecode)
        {
            try
            {
                if ((toDecode[1] != 18) || (toDecode[1] != 20))
                {
                    return null;
                }

                byte[] systemCode = new byte[2];
                byte[] nfcId = new byte[8];
                byte[] pad = new byte[8];
                toDecode.Slice(3, 8).CopyTo(nfcId);
                toDecode.Slice(11, 8).CopyTo(pad);

                if (toDecode[1] == 20)
                {
                    toDecode.Slice(19, 2).CopyTo(systemCode);
                }

                Data212_424kbps data = new Data212_424kbps(
                    toDecode[0],    // TargetNumber
                    toDecode[2],    // ResponseCode
                    nfcId,
                    pad,
                    systemCode);

                return data;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Try to decode a raw byte array containing target information
        /// to a 106 kbps Innovision Jewel card
        /// </summary>
        /// <param name="toDecode">The raw byte array</param>
        /// <returns>A decoded card of null if it can't</returns>
        public Data106kbpsInnovisionJewel? TryDecodeData106kbpsInnovisionJewel(Span<byte> toDecode)
        {
            try
            {
                Data106kbpsInnovisionJewel data = new Data106kbpsInnovisionJewel(
                    toDecode[0],                                // TargetNumber
                    new byte[2] { toDecode[1], toDecode[2] },   // Atqa
                    new byte[4] { toDecode[3], toDecode[4], toDecode[5], toDecode[6] }); // JewelId
                return data;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Deselect a specific target number card
        /// </summary>
        /// <param name="targetNumber">Target number card</param>
        /// <returns>True if success</returns>
        public bool DeselectTarget(byte targetNumber)
        {
            Span<byte> toSend = stackalloc byte[1]
            {
                targetNumber
            };
            var ret = WriteCommand(CommandSet.InDeselect, toSend);
            if (ret < 0)
            {
                return false;
            }

            ret = ReadResponse(CommandSet.InDeselect, toSend);
            return (toSend[0] == (byte)ErrorCode.None) && (ret >= 0);
        }

        /// <summary>
        /// Select a specific target number card
        /// </summary>
        /// <param name="targetNumber">Target number card</param>
        /// <returns>True if success</returns>
        public bool SelectTarget(byte targetNumber)
        {
            Span<byte> toSend = stackalloc byte[1]
            {
                targetNumber
            };
            var ret = WriteCommand(CommandSet.InSelect, toSend);
            if (ret < 0)
            {
                return false;
            }

            ret = ReadResponse(CommandSet.InSelect, toSend);
            return (toSend[0] == (byte)ErrorCode.None) && (ret >= 0);
        }

        /// <summary>
        /// Release a specific target number card
        /// </summary>
        /// <param name="targetNumber">Target number card</param>
        /// <returns>True if success</returns>
        public bool ReleaseTarget(byte targetNumber)
        {
            Span<byte> toSend = stackalloc byte[1]
            {
                targetNumber
            };
            var ret = WriteCommand(CommandSet.InRelease, toSend);
            if (ret < 0)
            {
                return false;
            }

            ret = ReadResponse(CommandSet.InRelease, toSend);
            return (toSend[0] == (byte)ErrorCode.None) && (ret >= 0);
        }

        /// <summary>
        /// Write an array of data directly to the card without adding anything
        /// from the PN532 and read the raw data
        /// </summary>
        /// <param name="dataToSend">The data to write to the card</param>
        /// <param name="dataFromCard">The potential data to receive</param>
        /// <returns>The number of bytes read</returns>
        public int WriteReadDirect(Span<byte> dataToSend, Span<byte> dataFromCard)
        {
            var ret = WriteCommand(CommandSet.InCommunicateThru, dataToSend);
            if (ret < 0)
            {
                return -1;
            }

            Span<byte> toReceive = stackalloc byte[1 + dataFromCard.Length];
            ret = ReadResponse(CommandSet.InCommunicateThru, toReceive);
            toReceive.Slice(1).CopyTo(dataFromCard);
            if ((toReceive[0] == (byte)ErrorCode.None) & (ret >= 0))
            {
                return ret;
            }

            return -1;
        }

        /// <summary>
        /// Write data to a card and read what the card responses
        /// </summary>
        /// <param name="targetNumber">The card target number</param>
        /// <param name="dataToSend">The data to write to the card</param>
        /// <param name="dataFromCard">The potential data to receive</param>
        /// <returns>The number of bytes read</returns>
        public override int Transceive(byte targetNumber, ReadOnlySpan<byte> dataToSend, Span<byte> dataFromCard)
        {
            Span<byte> toSend = stackalloc byte[1 + dataToSend.Length];
            toSend[0] = targetNumber;
            if (dataToSend.Length > 0)
            {
                dataToSend.CopyTo(toSend.Slice(1));
            }

            var ret = WriteCommand(CommandSet.InDataExchange, toSend);
            if (ret < 0)
            {
                return -1;
            }

            Span<byte> toReceive = stackalloc byte[1 + dataFromCard.Length];
            ret = ReadResponse(CommandSet.InDataExchange, toReceive);
            toReceive.Slice(1).CopyTo(dataFromCard);
            if ((toReceive[0] == (byte)ErrorCode.None) & (ret >= 0))
            {
                return ret;
            }

            return -1;
        }

        /// <summary>
        /// Automatically poll specific types of devices
        /// </summary>
        /// <param name="numberPolling">The number of polling before accepting a card</param>
        /// <param name="periodMilliSecond">The period of polling before accepting a card</param>
        /// <param name="pollingType">The type of cards to poll</param>
        /// <returns>A raw byte array containing the number of cards, the card type and the raw data. Null if nothing has been polled</returns>
        public byte[]? AutoPoll(byte numberPolling, ushort periodMilliSecond, PollingType[] pollingType)
        {
            if (pollingType == null)
            {
                return null;
            }

            if (pollingType.Length > 15)
            {
                return null;
            }

            Span<byte> toSend = stackalloc byte[2 + pollingType.Length];
            toSend[0] = numberPolling;
            if ((periodMilliSecond / 150) > 0xFF)
            {
                toSend[1] = 0xFF;
            }
            else
            {
                toSend[1] = (byte)(periodMilliSecond / 150);
            }

            for (int i = 0; i < pollingType.Length; i++)
            {
                toSend[2 + i] = (byte)pollingType[i];
            }

            var ret = WriteCommand(CommandSet.InAutoPoll, toSend);
            if (ret < 0)
            {
                return null;
            }

            Span<byte> receivedData = stackalloc byte[1024];
            ret = ReadResponse(CommandSet.InAutoPoll, receivedData);
            LogInfo.Log($"{nameof(AutoPoll)}, success: {ret}", LogLevel.Debug);
            if (ret >= 0)
            {
                return receivedData.Slice(0, ret).ToArray();
            }

            return null;
        }

        #region PN532 as Target

        /// <summary>
        /// Set the PN532 as a target, so as a card
        /// </summary>
        public (TargetModeInitialized? modeInialized, byte[]? initiator) InitAsTarget(TargetModeInitialization mode,
            TargetMifareParameters mifare, TargetFeliCaParameters feliCa, TargetPiccParameters picc)
        {
            // First make sure we have the right mode in the parameters for the PICC only case
            if (mode == TargetModeInitialization.PiccOnly)
            {
                LogInfo.Log($"{nameof(InitAsTarget)} - changing mode for Picc only", LogLevel.Debug);
                ParametersFlags |= ParametersFlags.ISO14443_4_PICC;
            }
            else
            {
                if (ParametersFlags.HasFlag(ParametersFlags.ISO14443_4_PICC))
                {
                    LogInfo.Log($"{nameof(InitAsTarget)} - removing mode for Picc only", LogLevel.Debug);
                    ParametersFlags = ParametersFlags & ~ParametersFlags.ISO14443_4_PICC;
                }
            }

            // Then serialize all buffer and add them
            List<byte> toSend = new List<byte>();
            toSend.Add((byte)mode);
            toSend.AddRange(mifare.Serialize());
            toSend.AddRange(feliCa.Serialize());
            toSend.AddRange(picc.Serialize());
            var ret = WriteCommand(CommandSet.TgInitAsTarget, toSend.ToArray());
            if (ret < 0)
            {
                return (null, null);
            }

            Span<byte> receivedData = stackalloc byte[1024];
            ret = ReadResponse(CommandSet.TgInitAsTarget, receivedData);
            LogInfo.Log($"{nameof(InitAsTarget)}, success: {ret}", LogLevel.Debug);
            if (ret >= 0)
            {
                TargetModeInitialized modeInitialized = new TargetModeInitialized();
                modeInitialized.IsDep = (receivedData[0] & 0b0000_0100) == 0b0000_0100;
                modeInitialized.IsISO14443_4Picc = (receivedData[0] & 0b0000_1000) == 0b0000_1000;
                modeInitialized.TargetFramingType = (TargetFramingType)(receivedData[0] & 0b0000_0011);
                modeInitialized.TargetBaudRate = (TargetBaudRateInialized)(receivedData[0] & 0b0111_0000);
                return (modeInitialized, receivedData.Slice(1, ret - 1).ToArray());
            }

            return (null, null);
        }

        /// <summary>
        /// read data from the reader when PN532 is a target
        /// </summary>
        /// <param name="receivedData">A Span byte array for the read data. Note the first byte contains the status</param>
        /// <returns>Number of byte read</returns>
        public int ReadDataAsTarget(Span<byte> receivedData)
        {
            var ret = WriteCommand(CommandSet.TgGetData);
            if (ret < 0)
            {
                return -1;
            }

            ret = ReadResponse(CommandSet.TgGetData, receivedData);
            LogInfo.Log($"{nameof(InitAsTarget)}, success: {ret}", LogLevel.Debug);
            if (ret > 0)
            {
                LogInfo.Log(
                    $"{nameof(WriteDataAsTarget)} - error: {(ErrorCode)receivedData[0]}, received array: {BitConverter.ToString(receivedData.Slice(1, ret - 1).ToArray())}",
                    LogLevel.Debug);
            }

            return ret;
        }

        /// <summary>
        /// Write data to the reader when PN532 is a target
        /// </summary>
        /// <param name="dataToSend">The data to send</param>
        /// <returns>True if success</returns>
        public bool WriteDataAsTarget(ReadOnlySpan<byte> dataToSend)
        {
            var ret = WriteCommand(CommandSet.TgSetData, dataToSend);
            if (ret < 0)
            {
                return false;
            }

            Span<byte> receivedData = stackalloc byte[1];
            ret = ReadResponse(CommandSet.TgSetData, receivedData);
            LogInfo.Log($"{nameof(InitAsTarget)}, success: {ret}", LogLevel.Debug);
            if (ret > 0)
            {
                LogInfo.Log($"{nameof(WriteDataAsTarget)} - error: {(ErrorCode)receivedData[0]}", LogLevel.Debug);
                return receivedData[0] == (byte)ErrorCode.None;
            }

            return false;
        }

        #endregion

        #region RFConfiguration

        /// <summary>
        /// Set the Radio Frequency Field Mode
        /// </summary>
        /// <param name="rfFieldMode">Radio Frequency Field Mode</param>
        /// <returns>True is success</returns>
        public bool SetRfField(RfFieldMode rfFieldMode)
        {
            return SetRfConfiguration(RfConfigurationMode.RfField, new byte[1] { (byte)rfFieldMode });
        }

        /// <summary>
        /// Set the Various Timing Mode
        /// </summary>
        /// <param name="variousTimingsMode">Various Timing Mode</param>
        /// <returns>True is success</returns>
        public bool SetVariousTimings(VariousTimingsMode variousTimingsMode)
        {
            return SetRfConfiguration(RfConfigurationMode.VariousTimings, variousTimingsMode.Serialize());
        }

        /// <summary>
        /// Set the Maximum Retry in the 2 WriteRead modes
        /// </summary>
        /// <param name="numberRetries">The number of retries</param>
        /// <returns>True is success</returns>
        public bool SetMaxRetryWriteRead(byte numberRetries = 0x00)
        {
            return SetRfConfiguration(RfConfigurationMode.MaxRetryCOM, new byte[1] { numberRetries });
        }

        /// <summary>
        /// Set the MAximu Retries during the various initialization modes
        /// </summary>
        /// <param name="maxRetriesMode">Retry modes</param>
        /// <returns>True is success</returns>
        public bool SetMaxRetriesInitialization(MaxRetriesMode maxRetriesMode)
        {
            return SetRfConfiguration(RfConfigurationMode.MaxRetries, maxRetriesMode.Serialize());
        }

        /// <summary>
        /// Set the specific 106 kbps card Type A modes
        /// </summary>
        /// <param name="analog106Kbps">The mode settings</param>
        /// <returns>True is success</returns>
        public bool SetAnalog106kbpsTypeA(Analog106kbpsTypeAMode analog106Kbps)
        {
            return SetRfConfiguration(RfConfigurationMode.AnalogSettingsB106kbpsTypeA, analog106Kbps.Serialize());
        }

        /// <summary>
        /// Set the specific 212 424 kbps card modes
        /// </summary>
        /// <param name="analog212_424">The mode settings</param>
        /// <returns>True is success</returns>
        public bool SetAnalog212_424Kbps(Analog212_424kbpsMode analog212_424)
        {
            return SetRfConfiguration(RfConfigurationMode.AnalogSettingsB212_424kbps, analog212_424.Serialize());
        }

        /// <summary>
        /// Set the specific 106 kbps card Type B modes
        /// </summary>
        /// <param name="analogSettings">The mode settings</param>
        /// <returns>True is success</returns>
        public bool SetAnalogTypeB(AnalogSettingsTypeBMode analogSettings)
        {
            return SetRfConfiguration(RfConfigurationMode.AnalogSettingsTypeB, analogSettings.Serialize());
        }

        /// <summary>
        /// Configure analog mode
        /// </summary>
        /// <param name="analog212_424_848Kbps">Settings</param>
        /// <returns>True is success</returns>
        public bool SetAnalog212_424_848kbps(Analog212_424_848kbpsMode analog212_424_848Kbps)
        {
            return SetRfConfiguration(RfConfigurationMode.AnalogSettingsB212_424_848ISO_IEC14443_4,
                analog212_424_848Kbps.Serialize());
        }

        private bool SetRfConfiguration(RfConfigurationMode rfConfigurationMode, byte[] configurationData)
        {
            Span<byte> toSend = stackalloc byte[configurationData.Length + 1];
            toSend[0] = (byte)rfConfigurationMode;
            configurationData.AsSpan().CopyTo(toSend.Slice(1));
            var ret = WriteCommand(CommandSet.RFConfiguration, toSend);
            if (ret < 0)
            {
                return false;
            }

            ret = ReadResponse(CommandSet.RFConfiguration, Span<byte>.Empty);
            LogInfo.Log($"{nameof(SetParameters)}: {ret}", LogLevel.Debug);
            return ret >= 0;
        }

        #endregion

        #region Read/Write registers

        /// <summary>
        /// Read an array of SFR registers
        /// </summary>
        /// <param name="registers">Array of register to read</param>
        /// <param name="registerValues">Register read values</param>
        /// <returns>True if success</returns>
        public bool ReadRegisterSfr(SfrRegister[] registers, Span<byte> registerValues)
        {
            Span<byte> toSend = stackalloc byte[registers.Length * 2];
            for (int i = 0; i < registers.Length; i++)
            {
                toSend[i * 2] = 0xFF;
                toSend[i * 2 + 1] = (byte)registers[i];
            }

            return ReadRegisterCore(toSend, registerValues);
        }

        /// <summary>
        /// Read a single register
        /// </summary>
        /// <param name="register">The register to read</param>
        /// <param name="registerValue">The value of the register</param>
        /// <returns>True if success</returns>
        public bool ReadRegister(ushort register, out byte registerValue)
        {
            Span<byte> toRead = stackalloc byte[1];
            var ret = ReadRegister(new ushort[] { register }, toRead);
            registerValue = toRead[0];
            return ret;
        }

        /// <summary>
        /// Read any register from the XRAM
        /// </summary>
        /// <param name="registers">Array of register to read</param>
        /// <param name="registerValues">Register read values</param>
        /// <returns>True if success</returns>
        public bool ReadRegister(ushort[] registers, Span<byte> registerValues)
        {
            Span<byte> toSend = stackalloc byte[registers.Length * 2];
            for (int i = 0; i < registers.Length; i++)
            {
                toSend[i * 2] = (byte)(registers[i] >> 8);
                toSend[i * 2 + 1] = (byte)registers[i];
            }

            return ReadRegisterCore(toSend, registerValues);
        }

        private bool ReadRegisterCore(Span<byte> toSend, Span<byte> registerValues)
        {
            var ret = WriteCommand(CommandSet.ReadRegister, toSend);
            if (ret < 0)
            {
                return false;
            }

            ret = ReadResponse(CommandSet.ReadRegister, registerValues);
            LogInfo.Log($"{nameof(ReadRegister)}: {ret}", LogLevel.Debug);
            return ret >= 0;
        }

        /// <summary>
        /// Write an array of SFR registers
        /// </summary>
        /// <param name="registers">Array of register to write</param>
        /// <param name="registerValue">Register values to write</param>
        /// <returns>True if success</returns>
        public bool WriteRegisterSfr(SfrRegister[] registers, Span<byte> registerValue)
        {
            Span<byte> toSend = stackalloc byte[registers.Length * 3];
            for (int i = 0; i < registers.Length; i++)
            {
                toSend[i * 3] = 0xFF;
                toSend[i * 3 + 1] = (byte)registers[i];
                toSend[i * 3 + 2] = registerValue[i];
            }

            return WriteRegisterCore(toSend, registerValue);
        }

        /// <summary>
        /// Write a single register
        /// </summary>
        /// <param name="register">The register to write</param>
        /// <param name="registerValue">The value of the register</param>
        /// <returns>True if success</returns>
        public bool WriteRegister(ushort register, byte registerValue)
        {
            return WriteRegister(new ushort[] { register }, new byte[] { registerValue });
        }

        /// <summary>
        /// Write an array of register
        /// </summary>
        /// <param name="registers">Array of register to write</param>
        /// <param name="registerValue">Register values to write</param>
        /// <returns></returns>
        public bool WriteRegister(ushort[] registers, Span<byte> registerValue)
        {
            Span<byte> toSend = stackalloc byte[registers.Length * 3];
            for (int i = 0; i < registers.Length; i++)
            {
                toSend[i * 2] = (byte)(registers[i] >> 8);
                toSend[i * 2 + 1] = (byte)registers[i];
                toSend[i * 3 + 2] = registerValue[i];
            }

            return WriteRegisterCore(toSend, registerValue);
        }

        private bool WriteRegisterCore(Span<byte> toSend, Span<byte> registerValue)
        {
            var ret = WriteCommand(CommandSet.ReadRegister, toSend);
            if (ret < 0)
            {
                return false;
            }

            // In theory, nothing is returned but practically you can get some returns
            // For example if you write on a register that is creating an output
            // So this buffer is here only to avoid having an exception when writing to
            // A register that will create an output
            // The maximum amount of data return if 260 but writing specific register can
            // Generate a larger amount
            Span<byte> returnVal = stackalloc byte[1024];
            ret = ReadResponse(CommandSet.ReadRegister, returnVal);
            LogInfo.Log($"{nameof(WriteRegister)}: {ret}", LogLevel.Debug);
            return ret >= 0;
        }

        #endregion

        #region Read/Write GPIO

        /// <summary>
        /// Read the PN532 GPIO
        /// </summary>
        /// <param name="p7">The P7 GPIO</param>
        /// <param name="p3">The P3 GPIO</param>
        /// <param name="l0L1">The specific operation mode register</param>
        /// <returns>True if success</returns>
        public bool ReadGpio(out Port7 p7, out Port3 p3, out OperatingMode l0L1)
        {
            // No flag as default
            p7 = 0;
            p3 = 0;
            l0L1 = 0;
            var ret = WriteCommand(CommandSet.ReadGPIO);
            if (ret < 0)
            {
                return false;
            }

            Span<byte> retGPIO = stackalloc byte[3];
            ret = ReadResponse(CommandSet.ReadGPIO, retGPIO);
            p7 = (Port7)retGPIO[0];
            p3 = (Port3)retGPIO[1];
            l0L1 = (OperatingMode)retGPIO[2];
            return ret >= 0;
        }

        /// <summary>
        /// Write the PN532 GPIO
        /// </summary>
        /// <param name="p7">The P7 GPIO</param>
        /// <param name="p3">The P3 GPIO</param>
        /// <returns>True if success</returns>
        public bool WriteGpio(Port7 p7, Port3 p3)
        {
            Span<byte> toWrite = stackalloc byte[2]
            {
                (byte)p7, (byte)p3
            };
            var ret = WriteCommand(CommandSet.WriteGPIO, toWrite);
            if (ret < 0)
            {
                return false;
            }

            ret = ReadResponse(CommandSet.WriteGPIO, Span<byte>.Empty);
            return ret >= 0;
        }

        #endregion

        #region Power management

        /// <summary>
        /// Power down the PN532
        /// </summary>
        /// <param name="wakeUpEnable">What can wake the PN532</param>
        /// <returns>True if success</returns>
        public bool PowerDown(WakeUpEnable wakeUpEnable)
        {
            Span<byte> toSend = stackalloc byte[1]
            {
                (byte)wakeUpEnable
            };
            var ret = WriteCommand(CommandSet.PowerDown, toSend);
            if (ret < 0)
            {
                return false;
            }

            Span<byte> status = stackalloc byte[1];
            ret = ReadResponse(CommandSet.PowerDown, status);
            LogInfo.Log($"{nameof(PowerDown)}: {ret}, Status {status[0]}", LogLevel.Debug);
            // Time needed to sleep
            Thread.Sleep(1);
            return (status[0] == (byte)ErrorCode.None) && (ret >= 0);
        }

        /// <summary>
        /// Wake Up the PN532
        /// </summary>
        public void WakeUp()
        {
            if (_serialPort != null)
            {
                // Wakeup the device send the magic 0x55 with a long preamble and SAM Command
                LogInfo.Log("Waking up device", LogLevel.Debug);
                // Create a SAM message and add the wake up message before
                byte[] samMessage = CreateWriteMessage(CommandSet.SAMConfiguration,
                    new byte[3] { (byte)_securityAccessModuleMode, (byte)(_virtualCardTimeout), 0x00 });
                byte[] wakeUp = new byte[_serialWakeUp.Length + samMessage.Length];
                _serialWakeUp.CopyTo(wakeUp, 0);
                samMessage.CopyTo(wakeUp, _serialWakeUp.Length);
                _serialPort.Write(wakeUp, 0, wakeUp.Length);
                LogInfo.Log($"Send: {BitConverter.ToString(wakeUp)}", LogLevel.Debug);
                // Wait to make sure it's awake and processed the order
                Thread.Sleep(5);
                // Dump the results
                DumpSerial();
            }
            else if (_spiDevice is object && _controller is object)
            {
                // Wakeup the device by pulling down the pin select of SPI
                LogInfo.Log("Waking up device", LogLevel.Debug);
                _controller.Write(_pin, PinValue.Low);
                Thread.Sleep(4);
                _controller.Write(_pin, PinValue.High);
                byte[] samMessage = CreateWriteMessage(CommandSet.SAMConfiguration,
                    new byte[3] { (byte)_securityAccessModuleMode, (byte)(_virtualCardTimeout), 0x00 });
                byte[] wakeUp = new byte[_i2CWakeUp.Length + samMessage.Length];
                _i2CWakeUp.CopyTo(wakeUp, 0);
                samMessage.CopyTo(wakeUp, _i2CWakeUp.Length);
                _spiDevice.Write(wakeUp);

            }
            else if (_i2cDevice != null)
            {
                LogInfo.Log("Waking up PN522 on I2C mode", LogLevel.Debug);
                byte[] samMessage = CreateWriteMessage(CommandSet.SAMConfiguration,
                    new byte[3] { (byte)_securityAccessModuleMode, (byte)(_virtualCardTimeout), 0x00 });
                byte[] wakeUp = new byte[_i2CWakeUp.Length + samMessage.Length];
                _i2CWakeUp.CopyTo(wakeUp, 0);
                samMessage.CopyTo(wakeUp, _i2CWakeUp.Length);
                int i = 3;
                while (i > 0)
                {
                    try
                    {
                        _i2cDevice.Write(wakeUp);
                        LogInfo.Log($"Send: {BitConverter.ToString(wakeUp)}", LogLevel.Debug);
                        break;
                    }
                    catch (IOException)
                    {
                        i--;
                    }
                }

                byte[] buff = new byte[7];
                do
                {
                    try
                    {
                        _i2cDevice.Read(buff);
                    }
                    catch (IOException)
                    {
                        throw new Exception("PN532 is not ready for I2C communication");
                    }

                }
                while (buff[0] == 0x01);
            }
        }

        #endregion

        #region Communication

        /// <summary>
        /// Setup the baud rate communication when using the HSU Serial Port mode
        /// </summary>
        /// <param name="baudRate">Baud rate</param>
        /// <returns>True if success</returns>
        public bool SetSerialBaudRate(BaudRate baudRate)
        {
            if (_serialPort == null)
            {
                return false;
            }

            Span<byte> toSend = stackalloc byte[1]
            {
                (byte)baudRate
            };
            var ret = WriteCommand(CommandSet.SetSerialBaudRate, toSend);
            if (ret < 0)
            {
                return false;
            }

            ret = ReadResponse(CommandSet.SetSerialBaudRate, Span<byte>.Empty);
            // Need to send an acknowledge
            _serialPort.Write(_ackBuffer, 0, _ackBuffer.Length);
            _serialPort.Close();
            // See page 39 of the documentation for the baud rates
            // It is approximately Math.Round(10252800.0 / BaudRate)
            switch (baudRate)
            {
                case BaudRate.B0009600:
                    _serialPort.BaudRate = 9600;
                    _serialPort.ReadTimeout = 1067;
                    break;
                case BaudRate.B0019200:
                    _serialPort.BaudRate = 19200;
                    _serialPort.ReadTimeout = 533;
                    break;
                case BaudRate.B0038400:
                    _serialPort.BaudRate = 38400;
                    _serialPort.ReadTimeout = 267;
                    break;
                case BaudRate.B0057600:
                    _serialPort.BaudRate = 57600;
                    _serialPort.ReadTimeout = 178;
                    break;
                case BaudRate.B0115200:
                    _serialPort.BaudRate = 115200;
                    _serialPort.ReadTimeout = 89;
                    break;
                case BaudRate.B0230400:
                    _serialPort.BaudRate = 230400;
                    _serialPort.ReadTimeout = 44;
                    break;
                case BaudRate.B0460800:
                    _serialPort.BaudRate = 460800;
                    _serialPort.ReadTimeout = 22;
                    break;
                case BaudRate.B0921600:
                    _serialPort.BaudRate = 921600;
                    _serialPort.ReadTimeout = 11;
                    break;
                case BaudRate.B1288000:
                    _serialPort.BaudRate = 1288000;
                    _serialPort.ReadTimeout = 8;
                    break;
                default:
                    break;
            }

            _serialPort.WriteTimeout = _serialPort.ReadTimeout;
            _serialPort.Open();
            return ret >= 0;
        }

        private void DumpSerial()
        {
            if (_serialPort != null)
            {
                LogInfo.Log($"Serial Available bytes and dumped: {_serialPort.BytesToRead}", LogLevel.Debug);
                while (_serialPort.BytesToRead > 0)
                {
                    _serialPort.ReadByte();
                }
            }
        }

        private int WriteCommand(CommandSet commandSet)
        {
            return WriteCommand(commandSet, Span<byte>.Empty);
        }

        private int WriteCommand(CommandSet commandSet, ReadOnlySpan<byte> writeData)
        {
            LogInfo.Log(
                $"{nameof(WriteCommand)}: {nameof(CommandSet)} {commandSet} Bytes to send: {BitConverter.ToString(writeData.ToArray())}",
                LogLevel.Debug);
            if (_spiDevice != null)
            {
                return WriteCommandSPI(commandSet, writeData);
            }

            if (_i2cDevice != null)
            {
                return WriteCommandI2C(commandSet, writeData);
            }

            if (_serialPort != null)
            {
                return WriteCommandSerial(commandSet, writeData);
            }

            return -1;
        }

        /// <summary>
        /// PREAMBLE 1 byte4
        /// START CODE 2 bytes (0x00 and 0xFF),
        /// LEN 1 byte indicating the number of bytes in the data field
        /// (TFI and PD0 to PDn),
        /// LCS 1 Packet Length Checksum LCS byte that satisfies the relation:
        /// Lower byte of [LEN + LCS] = 0x00,
        /// TFI 1 byte frame identifier, the value of this byte depends
        /// on the way of the message
        /// - D4h in case of a frame from the host controller to the PN532,
        /// - D5h in case of a frame from the PN532 to the host controller.
        /// DATA LEN-1 bytes of Packet Data Information
        /// The first byte PD0 is the Command Code,
        /// DCS 1 Data Checksum DCS byte that satisfies the relation:
        /// Lower byte of [TFI + PD0 + PD1 + … + PDn + DCS] = 0x00,
        /// POSTAMBLE 1 byte2.
        /// The amount of data that can be exchanged using this frame structure is limited to 255
        /// bytes (including TFI).
        /// 00 00 FF LEN LCS TFI PD0 PD1 ……... PDn DCS 00
        /// -- ----- --- --- --- ----------------- --- --
        /// |    |    |   |   |            |         |  |_ Postamble
        /// |    |    |   |   |            |         |____ Packet Data Checksum
        /// |    |    |   |   |            |______________ Packet Data
        /// |    |    |   |   |___________________________ Specific PN532 Frame Identifier
        /// |    |    |   |_______________________________ Packet Length Checksum
        /// |    |    |___________________________________ Packet Length
        /// |    |________________________________________ Start codes
        /// |_____________________________________________ Preamble
        /// If the length is large than 255, then you have LEN and LCS = 0xFF and
        /// 2 additional bytes for the MBS and LBS size of the real length
        /// </summary>
        /// <param name="commandSet">The command to use</param>
        /// <param name="writeData">The additional data to send</param>
        /// <returns></returns>
        private byte[] CreateWriteMessage(CommandSet commandSet, ReadOnlySpan<byte> writeData)
        {
            // 7 bytes + writeData length + 2 bytes if preamble
            int correctionPreamble = 2;
            int correctionIndex = 0;
            int correctionLArgeSizeBuffer = writeData.Length < 250 ? 0 : 2;
            if (!((_parametersFlags & ParametersFlags.RemovePrePostAmble) == ParametersFlags.RemovePrePostAmble))
            {
                correctionPreamble = 0;
                correctionIndex = 1;
            }

            Span<byte> buff = stackalloc byte[7 + writeData.Length + correctionPreamble + correctionLArgeSizeBuffer];
            if (correctionPreamble != 0)
            {
                buff[0] = Preamble;
            }

            buff[1 - correctionIndex] = StartCode1;
            buff[2 - correctionIndex] = StartCode2;
            // Normal frame size
            if (correctionLArgeSizeBuffer == 0)
            {
                // Size for commandSet + size of buffer + 1
                byte length = (byte)(1 + writeData.Length + 1);
                buff[3 - correctionIndex] = length;
                // CRC for length
                buff[4 - correctionIndex] = (byte)(~length + 1);
            }
            else
            {
                // Large frame size
                buff[3 - correctionIndex] = 0xFF;
                buff[4 - correctionIndex] = 0xFF;
                buff[5 - correctionIndex] = (byte)(writeData.Length >> 8);
                buff[6 - correctionIndex] = (byte)(writeData.Length);
            }

            // We are writing from Host
            buff[5 - correctionIndex + correctionLArgeSizeBuffer] = FromHostCheckSumD4;
            // The command and the data
            buff[6 - correctionIndex + correctionLArgeSizeBuffer] = (byte)commandSet;
            if (!writeData.IsEmpty)
            {
                writeData.CopyTo(buff.Slice(7 - correctionIndex + correctionLArgeSizeBuffer));
            }

            // Checksum
            byte checkSum = (byte)(FromHostCheckSumD4 + commandSet);
            for (int i = 0; i < writeData.Length; i++)
            {
                checkSum += writeData[i];
            }

            buff[7 + writeData.Length - correctionIndex + correctionLArgeSizeBuffer] = (byte)(~checkSum + 1);
            if (correctionPreamble != 0)
            {
                buff[8 + writeData.Length + correctionLArgeSizeBuffer] = Postamble;
            }

            LogInfo.Log($"Message to send: {BitConverter.ToString(buff.ToArray())}", LogLevel.Debug);
            return buff.ToArray();
        }

        private int WriteCommandSerial(CommandSet commandSet, ReadOnlySpan<byte> writeData)
        {
            if (_serialPort is null)
            {
                throw new Exception($"{nameof(_serialPort)} is incorrectly configured");
            }

            // Always make sure we don't have anything waiting to be read
            DumpSerial();
            var toWrite = CreateWriteMessage(commandSet, writeData);
            _serialPort.Write(toWrite, 0, toWrite.Length);
            // Check if we have something to read
            var timeout = _serialPort.ReadTimeout;
            while (!IsReady())
            {
                Thread.Sleep(1);
                timeout--;
                if (timeout == 0)
                {
                    return -1;
                }
            }

            if (CheckAckFrame())
            {
                return writeData.Length;
            }

            return -1;
        }

        private int WriteCommandI2C(CommandSet commandSet, ReadOnlySpan<byte> writeData)
        {
            if (_i2cDevice is null)
            {
                throw new Exception($"{nameof(_i2cDevice)} is incorrectly configured");
            }

            var toWrite = CreateWriteMessage(commandSet, writeData);
            try
            {
                _i2cDevice.Write(toWrite);
            }
            catch (Exception)
            {
                // I2C is sensitive, sometimes, it refuses to write or read
                return -1;
            }

            // Check if we have something to read
            var timeout = ReadTimeOut;
            while (!IsReady())
            {
                Thread.Sleep(1);
                timeout--;
                if (timeout == 0)
                {
                    return -1;
                }
            }

            if (CheckAckFrame())
            {
                return writeData.Length;
            }

            return -1;
        }

        private int WriteCommandSPI(CommandSet commandSet, ReadOnlySpan<byte> writeData)
        {
            if (_spiDevice is null)
            {
                throw new Exception($"{nameof(_spiDevice)} is incorrectly configured");
            }

            if (_controller is null)
            {
                throw new Exception($"{nameof(_controller)} is incorrectly configured");
            }

            var message = CreateWriteMessage(commandSet, writeData);
            Span<byte> buff = stackalloc byte[message.Length + 1];
            buff[0] = WriteData;
            message.AsSpan().CopyTo(buff.Slice(1));
            ReverseByte(buff);
            _controller.Write(_pin, PinValue.Low);
            Thread.Sleep(2);
            _spiDevice.Write(buff);
            _controller.Write(_pin, PinValue.High);

            // Check if we have something to read
            var timeout = ReadTimeOut;
            while (!IsReady())
            {
                Thread.Sleep(1);
                timeout--;
                if (timeout == 0)
                {
                    return -1;
                }
            }

            if (CheckAckFrame())
            {
                return writeData.Length;
            }

            return -1;
        }

        private int ReadResponse(CommandSet commandSet, Span<byte> readData)
        {
            if (_spiDevice is object && _controller is object)
            {
                var ret = ReadResponseSPI(commandSet, readData);
                _controller.Write(_pin, PinValue.High);
                return ret;
            }
            else if (_i2cDevice is object)
            {
                return ReadResponseI2C(commandSet, readData);
            }
            else if (_serialPort is object)
            {
                return ReadResponseSerial(commandSet, readData);
            }

            return -1;
        }

        private int ReadResponseSerial(CommandSet commandSet, Span<byte> readData)
        {
            if (_serialPort is null)
            {
                throw new Exception($"{nameof(_serialPort)} is incorrectly configured");
            }

            int timeout = _serialPort.ReadTimeout;
            while (!IsReady())
            {
                Thread.Sleep(1);
                timeout--;
                if (timeout == 0)
                {
                    return -1;
                }
            }

            // PREAMBULE
            if (!((_parametersFlags & ParametersFlags.RemovePrePostAmble) == ParametersFlags.RemovePrePostAmble))
            {
                if (!(_serialPort.ReadByte() == Preamble))
                {
                    return -1;
                }
            }

            // STARTCODE1
            if (!(_serialPort.ReadByte() == StartCode1))
            {
                return -1;
            }

            // STARTCODE2
            if (!(_serialPort.ReadByte() == StartCode2))
            {
                return -1;
            }

            // Checksum length the sum of both should be 0
            int length = _serialPort.ReadByte();
            int length2 = _serialPort.ReadByte();
            if ((byte)(length + length2) != 0)
            {
                // Maybe we have an extended packet?
                if ((length == 0xFF) && (length2 == 0xFF))
                {
                    // Real length MBS
                    length = _serialPort.ReadByte() << 8;
                    // Real length LBS
                    length += _serialPort.ReadByte();
                    length2 = _serialPort.ReadByte();
                    if ((byte)(length + length2) != 0)
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }

            // Is it a device to cloud message?
            if (_serialPort.ReadByte() != ToHostCheckSumD5)
            {
                return -1;
            }

            // The response command should be +1 vs the one sent
            if (_serialPort.ReadByte() != ((byte)commandSet + 1))
            {
                return -1;
            }

            // Finally, we can read the data
            if (length - 2 > 0)
            {
                var buff = new byte[length - 2];
                _serialPort.Read(buff, 0, buff.Length);
                buff.AsSpan().CopyTo(readData);
            }

            // Almost finished, we need to calculate the checksum
            var checkSum = ToHostCheckSumD5 + (byte)commandSet + 1;
            for (int i = 0; i < length - 2; i++)
            {
                checkSum += readData[i];
            }

            var checkSumReal = _serialPort.ReadByte();
            if ((byte)(checkSum + checkSumReal) != 0)
            {
                return -1;
            }

            if (!((_parametersFlags & ParametersFlags.RemovePrePostAmble) == ParametersFlags.RemovePrePostAmble))
            {
                if (!(_serialPort.ReadByte() == Postamble))
                {
                    return -1;
                }
            }

            return length - 2;
        }

        private int ReadResponseSPI(CommandSet commandSet, Span<byte> readData)
        {
            if (_spiDevice is null)
            {
                throw new Exception($"{nameof(_spiDevice)} is incorrectly configured");
            }

            if (_controller is null)
            {
                throw new Exception($"{nameof(_controller)} is incorrectly configured");
            }

            var timeout = ReadTimeOut;
            while (!IsReady())
            {
                Thread.Sleep(1);
                timeout--;
                if (timeout == 0)
                {
                    return -1;
                }
            }

            _controller.Write(_pin, PinValue.Low);
            Thread.Sleep(1);
            _spiDevice.WriteByte(ReverseByte(ReadData));
            // PREAMBULE
            if (!((_parametersFlags & ParametersFlags.RemovePrePostAmble) == ParametersFlags.RemovePrePostAmble))
            {
                if (!(ReverseByte(_spiDevice.ReadByte()) == Preamble))
                {
                    return -1;
                }
            }

            // STARTCODE1
            if (!(ReverseByte(_spiDevice.ReadByte()) == StartCode1))
            {
                return -1;
            }

            // STARTCODE2
            if (!(ReverseByte(_spiDevice.ReadByte()) == StartCode2))
            {
                return -1;
            }

            // Checksum length the sum of both should be 0
            int length = ReverseByte(_spiDevice.ReadByte());
            int length2 = ReverseByte(_spiDevice.ReadByte());
            if ((byte)(length + length2) != 0)
            {
                // Maybe we have an extended packet?
                if ((length == 0xFF) && (length2 == 0xFF))
                {
                    // Real length MBS
                    length = ReverseByte(_spiDevice.ReadByte()) << 8;
                    // Real length LBS
                    length += ReverseByte(_spiDevice.ReadByte());
                    length2 = ReverseByte(_spiDevice.ReadByte());
                    if ((byte)(length + length2) != 0)
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }

            // Is it a device to cloud message?
            if (ReverseByte(_spiDevice.ReadByte()) != ToHostCheckSumD5)
            {
                return -1;
            }

            // The response command should be +1 vs the one sent
            if (ReverseByte(_spiDevice.ReadByte()) != ((byte)commandSet + 1))
            {
                return -1;
            }

            // Finally, we can read the data
            if (length - 2 > 0)
            {
                var buff = new byte[length - 2];
                _spiDevice.Read(buff);
                ReverseByte(buff.AsSpan());
                buff.AsSpan().CopyTo(readData);

            }

            // Almost finished, we need to calculate the checksum
            var checkSum = ToHostCheckSumD5 + (byte)commandSet + 1;
            for (int i = 0; i < length - 2; i++)
            {
                checkSum += readData[i];
            }

            var checkSumReal = ReverseByte(_spiDevice.ReadByte());
            if ((byte)(checkSum + checkSumReal) != 0)
            {
                return -1;
            }

            if (!((_parametersFlags & ParametersFlags.RemovePrePostAmble) == ParametersFlags.RemovePrePostAmble))
            {
                if (!(ReverseByte(_spiDevice.ReadByte()) == Postamble))
                {
                    return -1;
                }
            }

            return length - 2;
        }

        private int ReadResponseI2C(CommandSet commandSet, Span<byte> readData)
        {
            if (_i2cDevice is null)
            {
                throw new Exception($"{nameof(_i2cDevice)} is incorrectly configured");
            }

            var timeout = ReadTimeOut;
            while (!IsReady())
            {
                Thread.Sleep(1);
                timeout--;
                if (timeout == 0)
                {
                    return -1;
                }
            }

            // For I2C, we need to read at least 2 bytes other wise it things we're still trying
            // to check the status
            byte[] preamb = new byte[I2cMaxBuffer];
            _i2cDevice.Read(preamb);
            int idxPreamb = 0;
            // Dropping the first byte, it is 0x01 and read previously in the pooling
            if (preamb[idxPreamb] == 0x01)
            {
                idxPreamb++;
            }

            // PREAMBULE
            if (!((_parametersFlags & ParametersFlags.RemovePrePostAmble) == ParametersFlags.RemovePrePostAmble))
            {
                if (!(preamb[idxPreamb++] == Preamble))
                {
                    return -1;
                }
            }

            // STARTCODE1
            if (!(preamb[idxPreamb++] == StartCode1))
            {
                return -1;
            }

            // STARTCODE2
            if (!(preamb[idxPreamb++] == StartCode2))
            {
                return -1;
            }

            // Checksum length the sum of both should be 0
            int length = preamb[idxPreamb++];
            int length2 = preamb[idxPreamb++];
            if ((byte)(length + length2) != 0)
            {
                // Maybe we have an extended packet?
                if ((length == 0xFF) && (length2 == 0xFF))
                {
                    // Real length MBS
                    length = preamb[idxPreamb++] << 8;
                    // Real length LBS
                    length += preamb[idxPreamb++];
                    length2 = preamb[idxPreamb++];
                    if ((byte)(length + length2) != 0)
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }

            // Is it a device to cloud message?
            if (preamb[idxPreamb++] != ToHostCheckSumD5)
            {
                return -1;
            }

            // The response command should be +1 vs the one sent
            if (preamb[idxPreamb++] != ((byte)commandSet + 1))
            {
                return -1;
            }

            // Finally, we can read the data
            if (length - 2 > 0)
            {
                preamb.AsSpan().Slice(idxPreamb, length - 2).CopyTo(readData);
                idxPreamb += length - 2;
            }

            // Almost finished, we need to calculate the checksum
            var checkSum = ToHostCheckSumD5 + (byte)commandSet + 1;
            for (int i = 0; i < length - 2; i++)
            {
                checkSum += readData[i];
            }

            var checkSumReal = preamb[idxPreamb++];
            if ((byte)(checkSum + checkSumReal) != 0)
            {
                return -1;
            }

            if (!((_parametersFlags & ParametersFlags.RemovePrePostAmble) == ParametersFlags.RemovePrePostAmble))
            {
                if (!(preamb[idxPreamb++] == Postamble))
                {
                    return -1;
                }
            }

            return length - 2;
        }

        private bool CheckAckFrame()
        {
            Span<byte> ackReceived = stackalloc byte[6]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            if (_spiDevice is object && _controller is object)
            {
                _controller.Write(_pin, PinValue.Low);
                _spiDevice.WriteByte(ReadData);
                _spiDevice.Read(ackReceived);
                _controller.Write(_pin, PinValue.High);
            }
            else if (_i2cDevice != null)
            {
                Span<byte> i2cackReceived = stackalloc byte[7];
                _i2cDevice.Read(i2cackReceived);
                i2cackReceived.Slice(1).CopyTo(ackReceived);
            }
            else if (_serialPort != null)
            {
                try
                {
                    var buff = ackReceived.ToArray();
                    _serialPort.Read(buff, 0, buff.Length);
                    buff.AsSpan().CopyTo(ackReceived);

                }
                catch (Exception ex)
                {
                    LogInfo.Log($"Exception: {ex.Message}", LogLevel.Info);
                }
            }

            LogInfo.Log($"ACK: {BitConverter.ToString(ackReceived.ToArray())}", LogLevel.Debug);
            return ackReceived.SequenceEqual(_ackBuffer);
        }

        private bool IsReady()
        {
            byte ret = 0;
            if (_spiDevice is object && _controller is object)
            {
                _controller.Write(_pin, PinValue.Low);
                _spiDevice.WriteByte(ReadStatus);
                ret = _spiDevice.ReadByte();
                _controller.Write(_pin, PinValue.High);
            }
            else if (_i2cDevice != null)
            {
                try
                {
                    ret = _i2cDevice.ReadByte();
                }
                catch (IOException)
                {
                    return false;
                }

            }
            else if (_serialPort != null)
            {
                return _serialPort.BytesToRead > 0;
            }

            return ((ret & 0x01) == 0x01);
        }

        private static readonly byte[] BitReverseTable =
        {
            0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0, 0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0, 0x08,
            0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8, 0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8, 0x04, 0x84,
            0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4, 0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4, 0x0c, 0x8c, 0x4c,
            0xcc, 0x2c, 0xac, 0x6c, 0xec, 0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc, 0x02, 0x82, 0x42, 0xc2,
            0x22, 0xa2, 0x62, 0xe2, 0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2, 0x0a, 0x8a, 0x4a, 0xca, 0x2a,
            0xaa, 0x6a, 0xea, 0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa, 0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6,
            0x66, 0xe6, 0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6, 0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e,
            0xee, 0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe, 0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1,
            0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1, 0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9, 0x19,
            0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9, 0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5, 0x15, 0x95,
            0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5, 0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed, 0x1d, 0x9d, 0x5d,
            0xdd, 0x3d, 0xbd, 0x7d, 0xfd, 0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3, 0x13, 0x93, 0x53, 0xd3,
            0x33, 0xb3, 0x73, 0xf3, 0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb, 0x1b, 0x9b, 0x5b, 0xdb, 0x3b,
            0xbb, 0x7b, 0xfb, 0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7, 0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7,
            0x77, 0xf7, 0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef, 0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f,
            0xff
        };

        private static byte ReverseByte(byte toReverse)
        {
            return BitReverseTable[toReverse];
        }

        private static void ReverseByte(Span<byte> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = ReverseByte(span[i]);
            }
        }

        #endregion

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null!;
            _i2cDevice?.Dispose();
            _i2cDevice = null!;

            if (_serialPort is { IsOpen: true })
            {
                _serialPort.Close();
            }

            _serialPort?.Dispose();
            _serialPort = null!;
        }
    }
}
