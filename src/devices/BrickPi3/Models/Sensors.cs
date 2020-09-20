// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.BrickPi3.Models
{
    /// <summary>
    /// Sensor ports 1, 2, 3 and 4
    /// </summary>
    public enum SensorPort : byte
    {
        // Used to select the ports for sensors

        /// <summary>Port 1</summary>
        Port1 = 0x01,

        /// <summary>Port 2</summary>
        Port2 = 0x02,

        /// <summary>Port 3</summary>
        Port3 = 0x04,

        /// <summary>Port 4</summary>
        Port4 = 0x08,
    }

    /// <summary>
    /// Sensor I2C settings
    /// </summary>
    public enum SensorI2CSettings : byte
    {
        /// <summary>Send the clock pulse between reading and writing. Required by the NXT US sensor.</summary>
        MidClock = 0x01,

        /// <summary>9v pullup on pin 1</summary>
        Pin1_9V = 0x02,

        /// <summary>Keep performing the same transaction e.g. keep polling a sensor</summary>
        Same = 0x04,

        /// <summary>Allow ACK stretching</summary>
        AllowStretchAck,

        /// <summary>Allow any stretching</summary>
        AllowStretchAny,
    }

    /// <summary>
    /// All type of supported sensors
    /// </summary>
    public enum SensorType : byte
    {
        /// <summary>None</summary>
        None = 1,

        /// <summary>I2C sensor</summary>
        I2C,

        /// <summary>Custom sensor</summary>
        Custom,

        /// <summary>Touch sensor</summary>
        Touch,

        /// <summary>NXT touch sensor</summary>
        NXTTouch,

        /// <summary>EV3 touch sensor</summary>
        EV3Touch,

        /// <summary>NXT light on sensor</summary>
        NXTLightOn,

        /// <summary>NXT light off sensor</summary>
        NXTLightOff,

        /// <summary>NXT color red sensor</summary>
        NXTColorRed,

        /// <summary>NXT color green sensor</summary>
        NXTColorGreen,

        /// <summary>NXT color blue sensor</summary>
        NXTColorBlue,

        /// <summary>NXT color full sensor</summary>
        NXTColorFull,

        /// <summary>NXT color off sensor</summary>
        NXTColorOff,

        /// <summary>NXT ultrasonic sensor</summary>
        NXTUltrasonic,

        /// <summary>EV3 gyro - absolute angular speed sensor</summary>
        EV3GyroAbs,

        /// <summary>EV3 gyro - angular speed sensor</summary>
        EV3GyroDps,

        /// <summary>EV3 gyro - absolute angular speed sensor</summary>
        EV3GyroAbsDps,

        /// <summary>EV3 color reflected sensor</summary>
        EV3ColorReflected,

        /// <summary>EV3 color ambient sensor</summary>
        EV3ColorAmbient,

        /// <summary>EV3 color sensor</summary>
        EV3ColorColor,

        /// <summary>EV3 color raw reflected sensor</summary>
        EV3ColorRawReflected,

        /// <summary>EV3 color components sensor</summary>
        EV3ColorColorComponents,

        /// <summary>EV3 ultrasonic centimeter sensor</summary>
        EV3UltrasonicCentimeter,

        /// <summary>EV3 ultrasonic inches sensor</summary>
        EV3UltrasonicInches,

        /// <summary>EV3 ultrasonic listen sensor</summary>
        EV3UltrasonicListen,

        /// <summary>EV3 infrared proximity sensor</summary>
        EV3InfraredProximity,

        /// <summary>EV3 infrared seek sensor</summary>
        EV3InfraredSeek,

        /// <summary>EV3 infrared remote sensor</summary>
        EV3InfraredRemote
    }

    /// <summary>
    /// Maind state for data when returned by any of the get_ function
    /// Used internally by the brick engine
    /// </summary>
    public enum SensorState : byte
    {
        /// <summary>Valid data</summary>
        ValidData = 0,

        /// <summary>Not configured</summary>
        NotConfigured,

        /// <summary>Configuring</summary>
        Configuring,

        /// <summary>No data</summary>
        NoData,

        /// <summary>I2C error</summary>
        I2CError
    }

    /// <summary>
    /// Flags for use with SENSOR_TYPE.CUSTOM
    /// </summary>
    public enum SensorCustom
    {
        /// <summary>Enable 9V out on pin 1 (for LEGO NXT Ultrasonic sensor).</summary>
        Pin1_9V = 0x0002,

        /// <summary>Set pin 5 state to output.Pin 5 will be set to input if this flag is not set.</summary>
        Pin5_Out = 0x0010,

        /// <summary>
        /// If PIN5_OUT is set, this will set the state to output high, otherwise the state will
        /// be output low. If PIN5_OUT is not set, this flag has no effect.
        /// </summary>
        Pin5_State = 0x0020,

        /// <summary>Set pin 6 state to output. Pin 6 will be set to input if this flag is not set.</summary>
        Pin6_Out = 0x0100,

        /// <summary>
        /// If PIN6_OUT is set, this will set the state to output high, otherwise the state will
        /// be output low. If PIN6_OUT is not set, this flag has no effect.
        /// </summary>
        Pin6_State = 0x0200,

        /// <summary>Enable the analog/digital converter on pin 1 (e.g. for NXT analog sensors).</summary>
        Pin1_ADC = 0x1000,

        /// <summary>Enable the analog/digital converter on pin 6.</summary>
        Pin6_ADC = 0x4000
    }
}
