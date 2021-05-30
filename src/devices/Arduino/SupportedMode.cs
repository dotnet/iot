// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Mode bits for the Firmata protocol.
    /// These are used both for capability reporting as well as to set a mode
    /// </summary>
    public enum SupportedMode
    {
        /// <summary>
        /// The pin supports digital input
        /// </summary>
        DigitalInput = (0x00),

        /// <summary>
        /// The pin supports digital output;
        /// </summary>
        DigitalOutput = (0x01),

        /// <summary>
        /// The pin supports analog input
        /// </summary>
        AnalogInput = (0x02),

        /// <summary>
        /// The pin supports PWM
        /// </summary>
        Pwm = (0x03),

        /// <summary>
        /// The pin supports servo motor controls
        /// </summary>
        Servo = (0x04),

        /// <summary>
        /// Unused
        /// </summary>
        Shift = (0x05),

        /// <summary>
        /// The pin supports I2C data transfer
        /// </summary>
        I2c = (0x06),

        /// <summary>
        /// The pin supports one wire communication
        /// </summary>
        OneWire = (0x07),

        /// <summary>
        /// The pin can drive a stepper motor
        /// </summary>
        Stepper = (0x08),

        /// <summary>
        /// The pin has an encoder
        /// </summary>
        Encoder = (0x09),

        /// <summary>
        /// The pin can perform UART (TX or RX)
        /// </summary>
        Serial = (0x0A),

        /// <summary>
        /// The pin can be set to input-pullup.
        /// </summary>
        InputPullup = (0x0B),

        //// Unofficial extensions (only supported by special firmware)

        /// <summary>
        /// The pin can be used for SPI transfer (Clock, MOSI, MISO and default CS pin)
        /// For most Arduinos, MOSI=11, MISO=12 and Clock = 13. The default CS pin is 10.
        /// </summary>
        Spi = 0x0C,

        /// <summary>
        /// HC-SR04
        /// </summary>
        Sonar = 0x0D,

        /// <summary>
        /// Arduino tone library
        /// </summary>
        Tone = 0x0E,

        /// <summary>
        /// DHT type sensors
        /// </summary>
        Dht = 0x0F,

        /// <summary>
        /// Frequency measurement
        /// </summary>
        Frequency = 0x10,
    }
}
