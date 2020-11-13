// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        DIGITAL_INPUT = (0x00),

        /// <summary>
        /// The pin supports digital output;
        /// </summary>
        DIGITAL_OUTPUT = (0x01),

        /// <summary>
        /// The pin supports analog input
        /// </summary>
        ANALOG_INPUT = (0x02),

        /// <summary>
        /// The pin supports PWM
        /// </summary>
        PWM = (0x03),

        /// <summary>
        /// The pin supports servo motor controls
        /// </summary>
        SERVO = (0x04),

        /// <summary>
        /// Unused
        /// </summary>
        SHIFT = (0x05),

        /// <summary>
        /// The pin supports I2C data transfer
        /// </summary>
        I2C = (0x06),

        /// <summary>
        /// The pin supports one wire communication
        /// </summary>
        ONEWIRE = (0x07),

        /// <summary>
        /// The pin can drive a stepper motor
        /// </summary>
        STEPPER = (0x08),

        /// <summary>
        /// The pin has an encoder
        /// </summary>
        ENCODER = (0x09),

        /// <summary>
        /// The pin can perform UART (TX or RX)
        /// </summary>
        SERIAL = (0x0A),

        /// <summary>
        /// The pin can be set to input-pullup.
        /// </summary>
        INPUT_PULLUP = (0x0B),

        /// <summary>
        /// The pin can be used for SPI transfer (Clock, MOSI, MISO and default CS pin)
        /// For most Arduinos, MOSI=11, MISO=12 and Clock = 13. The default CS pin is 10.
        /// </summary>
        SPI = 0x0C,

        //// Unofficial extensions (only supported by special firmware)

        /// <summary>
        /// HC-SR04
        /// </summary>
        SONAR = 0x0D,

        /// <summary>
        /// Arduino tone library
        /// </summary>
        TONE = 0x0E,

        /// <summary>
        /// DHT type sensors
        /// </summary>
        DHT = 0x0F,
    }
}
