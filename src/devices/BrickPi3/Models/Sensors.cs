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
        Port1 = 0x01,
        Port2 = 0x02,
        Port3 = 0x04,
        Port4 = 0x08,
    }

    /// <summary>
    /// MID_CLOCK = 0x01,   Send the clock pulse between reading and writing. Required by the NXT US sensor.
    /// PIN1_9V = 0x02,     9v pullup on pin 1
    /// SAME = 0x04,        Keep performing the same transaction e.g. keep polling a sensor
    /// </summary>
    public enum SensorI2CSettings : byte
    {
        MidClock = 0x01,
        Pin1_9V = 0x02,
        Same = 0x04,
        AllowStretchAck,
        AllowStretchAny,
    }

    /// <summary>
    /// All type of supported sensors
    /// </summary>
    public enum SensorType : byte
    {
        None = 1,
        I2C,
        Custom,
        Touch,
        NXTTouch,
        EV3Touch,
        NXTLightOn,
        NXTLightOff,
        NXTColorRed,
        NXTColorGreen,
        NXTColorBlue,
        NXTColorFull,
        NXTColorOff,
        NXTUltrasonic,
        EV3GyroAbs,
        EV3GyroDps,
        EV3GyroAbsDps,
        EV3ColorReflected,
        EV3ColorAmbient,
        EV3ColorColor,
        EV3ColorRawReflected,
        EV3ColorColorComponents,
        EV3UltrasonicCentimeter,
        EV3UltrasonicInches,
        EV3UltrasonicListen,
        EV3InfraredProximity,
        EV3InfraredSeek,
        EV3InfraredRemote
    }

    /// <summary>
    /// Maind state for data when returned by any of the get_ function
    /// Used internally by the brick engine
    /// </summary>
    public enum SensorState : byte
    {
        ValidData = 0,
        NotConfigured,
        Configuring,
        NoData,
        I2CError
    }

    /// <summary>
    ///     Flags for use with SENSOR_TYPE.CUSTOM
    /// PIN1_9V
    ///     Enable 9V out on pin 1 (for LEGO NXT Ultrasonic sensor).
    /// PIN5_OUT
    ///     Set pin 5 state to output.Pin 5 will be set to input if this flag is not set.
    /// PIN5_STATE
    ///    If PIN5_OUT is set, this will set the state to output high, otherwise the state will
    ///    be output low.If PIN5_OUT is not set, this flag has no effect.
    /// PIN6_OUT
    ///    Set pin 6 state to output.Pin 6 will be set to input if this flag is not set.
    /// PIN6_STATE
    ///    If PIN6_OUT is set, this will set the state to output high, otherwise the state will
    ///    be output low.If PIN6_OUT is not set, this flag has no effect.
    /// PIN1_ADC
    ///    Enable the analog/digital converter on pin 1 (e.g. for NXT analog sensors).
    /// PIN6_ADC
    ///     Enable the analog/digital converter on pin 6.
    /// </summary>
    public enum SensorCustom
    {
        Pin1_9V = 0x0002,
        Pin5_Out = 0x0010,
        Pin5_State = 0x0020,
        Pin6_Out = 0x0100,
        Pin6_State = 0x0200,
        Pin1_ADC = 0x1000,
        Pin6_ADC = 0x4000
    }
}
