// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using Iot.Device.Board;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Mode bits for the Firmata protocol.
    /// These are used both for capability reporting as well as to set a mode
    /// </summary>
    public record SupportedMode
    {
        /// <summary>
        /// The pin supports digital input
        /// </summary>
        public static SupportedMode DigitalInput = new SupportedMode(0x00, "Digital Input", PinUsage.Gpio);

        /// <summary>
        /// The pin supports digital output;
        /// </summary>
        public static SupportedMode DigitalOutput = new SupportedMode(0x01, "Digital Output", PinUsage.Gpio);

        /// <summary>
        /// The pin supports analog input
        /// </summary>
        public static SupportedMode AnalogInput = new SupportedMode(0x02, "Analog Input", PinUsage.AnalogIn);

        /// <summary>
        /// The pin supports PWM
        /// </summary>
        public static SupportedMode Pwm = new SupportedMode(0x03, "PWM", PinUsage.AnalogOut);

        /// <summary>
        /// The pin supports servo motor controls
        /// </summary>
        public static SupportedMode Servo = new SupportedMode(0x04, "Servo");

        /// <summary>
        /// Unused
        /// </summary>
        public static SupportedMode Shift = new SupportedMode(0x05, "Shift registers");

        /// <summary>
        /// The pin supports I2C data transfer
        /// </summary>
        public static SupportedMode I2c = new SupportedMode(0x06, "I2C", PinUsage.I2c);

        /// <summary>
        /// The pin supports one wire communication
        /// </summary>
        public static SupportedMode OneWire = new SupportedMode(0x07, "One Wire");

        /// <summary>
        /// The pin can drive a stepper motor
        /// </summary>
        public static SupportedMode Stepper = new SupportedMode(0x08, "Stepper Motor");

        /// <summary>
        /// The pin has an encoder
        /// </summary>
        public static SupportedMode Encoder = new SupportedMode(0x09, "Encoder");

        /// <summary>
        /// The pin can perform UART (TX or RX)
        /// </summary>
        public static SupportedMode Serial = new SupportedMode(0x0A, "Serial", PinUsage.Uart);

        /// <summary>
        /// The pin can be set to input-pullup.
        /// </summary>
        public static SupportedMode InputPullup = new SupportedMode(0x0B, "Input Pullup", PinUsage.Gpio);

        //// Unofficial extensions (only supported by special firmware)

        /// <summary>
        /// The pin can be used for SPI transfer (Clock, MOSI, MISO and default CS pin)
        /// For most Arduinos, MOSI=11, MISO=12 and Clock = 13. The default CS pin is 10.
        /// </summary>
        public static SupportedMode Spi = new SupportedMode(0x0C, "SPI", PinUsage.Spi);

        /// <summary>
        /// HC-SR04
        /// </summary>
        public static SupportedMode Sonar = new SupportedMode(0x0D, "HC-SR04");

        /// <summary>
        /// Arduino tone library
        /// </summary>
        public static SupportedMode Tone = new SupportedMode(0x0E, "Tone");

        /// <summary>
        /// DHT type sensors
        /// </summary>
        public static SupportedMode Dht = new SupportedMode(0x0F, "DHT");

        /// <summary>
        /// Frequency measurement
        /// </summary>
        public static SupportedMode Frequency = new SupportedMode(0x10, "Frequency");

        /// <summary>
        /// Declares a new pin mode
        /// </summary>
        /// <param name="value">The pin mode value</param>
        /// <param name="name">The user-readable name for the mode</param>
        /// <param name="pinUsage">Pin usage for this mode, if applicable</param>
        public SupportedMode(byte value, string name, PinUsage pinUsage)
        {
            Value = value;
            Name = name;
            PinUsage = pinUsage;
        }

        /// <summary>
        /// Declares a new pin mode
        /// </summary>
        /// <param name="value">The pin mode value</param>
        /// <param name="name">The user-readable name for the mode</param>
        public SupportedMode(byte value, string name)
        {
            Value = value;
            Name = name;
            PinUsage = PinUsage.Unknown;
        }

        /// <summary>
        /// The value for the pin mode
        /// </summary>
        public byte Value
        {
            get;
        }

        /// <summary>
        /// The name of the pin mode
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// The <see cref="Device.Board.PinUsage"/> for this internal mode, if applicable
        /// </summary>
        public PinUsage PinUsage
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} (0x{Value:X2})";
        }
    }
}
