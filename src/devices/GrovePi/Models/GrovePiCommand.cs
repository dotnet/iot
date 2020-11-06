// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.GrovePiDevice.Models
{
    /// <summary>
    /// GroovePi commands to read, write, setup pins and access special sensors
    /// Note that those commands are supported in most of the recent firmware with version higher than 1.2.1
    /// </summary>
    public enum GrovePiCommand
    {
        /// <summary>
        /// Digital read a pin, equivalent of digitalRead on Arduino
        /// </summary>
        DigitalRead = 1,

        /// <summary>
        /// Digital write a pin, equivalent of digitalWrite on Arduino
        /// </summary>
        DigitalWrite = 2,

        /// <summary>
        /// Analog read a pin, equivalent of analogRead on Arduino
        /// </summary>
        AnalogRead = 3,

        /// <summary>
        /// Analog write a pin, equivalent of analogRead on Arduino
        /// </summary>
        AnalogWrite = 4,

        /// <summary>
        /// Set the Pin moden, equivalent of pinMode on Arduino
        /// </summary>
        PinMode = 5,

        /// <summary>
        /// Ultrasonic sensor
        /// </summary>
        UltrasonicRead = 7,

        /// <summary>
        /// Get the version number
        /// </summary>
        Version = 8,

        /// <summary>
        /// DHT22 sensor
        /// </summary>
        DhtTemp = 40,

        /// <summary>
        /// Initialize the Led bar
        /// </summary>
        LedBarInitialization = 50,

        /// <summary>
        /// Set orientiation
        /// </summary>
        LedBarOrientation = 51,

        /// <summary>
        /// Set level
        /// </summary>
        LedBarLevel = 52,

        /// <summary>
        /// Set an individual led
        /// </summary>
        LedBarSetOneLed = 53,

        /// <summary>
        /// Toggle an individual led
        /// </summary>
        LedBarToggleOneLed = 54,

        /// <summary>
        /// Set all leds
        /// </summary>
        LedBarSet = 55,

        /// <summary>
        /// Get the led status
        /// </summary>
        LetBarGet = 56,

        /// <summary>
        /// Initialize na 4 digit display
        /// </summary>
        FourDigitInit = 70,

        /// <summary>
        /// Set brightness, not visible until next cmd
        /// </summary>
        FourDigitBrightness = 71,

        /// <summary>
        /// Set numeric value without leading zeros
        /// </summary>
        FourDigitValue = 72,

        /// <summary>
        /// Set numeric value with leading zeros
        /// </summary>
        FourDigitValueZeros = 73,

        /// <summary>
        /// Set individual digit
        /// </summary>
        FourDigitIndividualDigit = 74,

        /// <summary>
        /// Set individual leds of a segment
        /// </summary>
        FourDigitIndividualLeds = 75,

        /// <summary>
        /// Set left and right values with colon
        /// </summary>
        FourDigitScore = 76,

        /// <summary>
        /// Analog read for n seconds
        /// </summary>
        FourDigitAnalogRead = 77,

        /// <summary>
        /// Entire display on
        /// </summary>
        FourDigitAllOn = 78,

        /// <summary>
        /// Entire display off
        /// </summary>
        FourDigitAllOff = 79,
    }
}
