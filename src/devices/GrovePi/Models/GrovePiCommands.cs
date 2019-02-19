// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.GrovePiDevice.Models
{
    /// <summary>
    /// GroovePi commands to read, write, setup pins and access special sensors
    /// Note that those commands are supports in most of the recent firmware with version higher than 1.2.1
    /// </summary>
    public enum GrovePiCommands
    {
        // Digital read a pin, equivalent of digitalRead on Arduino
        DigitalRead = 1,
        // Digital write a pin, equivalent of digitalWrite on Arduino
        DigitalWrite = 2,
        // Analog read a pin, equivalent of analogRead on Arduino
        AnalogRead = 3,
        // Analog write a pin, equivalent of analogRead on Arduino
        AnalogWrite = 4,
        // Set the Pin moden, equivalent of pinMode on Arduino
        PinMode = 5,
        // Ultrasonic sensor
        UltrasonicRead = 7,
        // Get the version number
        Version = 8,
        // DHT22 sensor
        DhtTemp = 40,
        // Initialize the Led bar
        LedBarInitialization = 50,
        // Set orientiation
        LedBarOrientation = 51,
        // Set level
        LedBarLevel = 52,
        // Set an individual led
        LedBarSetOneLed = 53,
        // Toggle an individual led
        LedBarToggleOneLed = 54,
        // Set all leds
        LedBarSet = 55,
        // Get the led status
        LetBarGet = 56,
        // Initialize na 4 digit display
        FourDigitInit = 70,
        // Set brightness, not visible until next cmd
        FourDigitBrightness = 71,
        // Set numeric value without leading zeros
        FourDigitValue = 72,
        // Set numeric value with leading zeros
        FourDigitValueZeros = 73,
        // Set individual digit
        FourDigitIndividualDigit = 74,
        // Set individual leds of a segment
        FourDigitIndividualLeds = 75,
        // Set left and right values with colon
        FourDigitScore = 76,
        // Analog read for n seconds
        FourDigitAnalogRead = 77,
        // Entire display on
        FourDigitAllOn = 78,
        // Entire display off
        FourDigitAllOff = 79,  
    }
}
