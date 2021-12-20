// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BuildHat.Models
{
    /// <summary>
    /// All type of supported sensors
    /// </summary>
    public enum SensorType : byte
    {
        // Those ones are passive

        /// <summary>None</summary>
        None = 0,

        /// <summary>System medium motor</summary>
        SystemMediumMotor = 1,

        /// <summary>System train motor</summary>
        SystemTrainMotor = 2,

        /// <summary>System turntable motor</summary>
        SystemTurntableMotor = 3,

        /// <summary>General PWM/third party</summary>
        GeneralPwm = 4,

        /// <summary>Button/touch sensor</summary>
        ButtonOrTouchSensor = 5,

        /// <summary>Technic large motor (some have active ID)</summary>
        TechnicLargeMotor = 6,

        /// <summary>Technic XL motor (some have active ID)</summary>
        TechnicXLMotor = 7,

        /// <summary>Simple lights</summary>
        SimpleLights = 8,

        /// <summary>Future lights 1</summary>
        FutureLights1 = 9,

        /// <summary>Future lights 2</summary>
        FutureLights2 = 10,

        /// <summary>System future actuator (train points)</summary>
        SystemFutureActuator = 11,

        // Following ones are active

        /// <summary>WeDo tilt sensor</summary>
        WeDoTiltSensor = 0x22,

        /// <summary>Wido motion sensor</summary>
        WeDoDistanceSensor = 0x23,

        /// <summary>Colour and distance sensor</summary>
        ColourAndDistanceSensor = 0x25,

        /// <summary>Medium linear motor</summary>
        MediumLinearMotor = 0x26,

        /// <summary>Technic large motor</summary>
        TechnicLargeMotorId = 0x2E,

        /// <summary>Technic XL motor</summary>
        TechnicXLMotorId = 0x2F,

        /// <summary>SPIKE Prime medium motor</summary>
        SpikePrimeMediumMotor = 0x30,

        /// <summary>SPIKE Prime large motor</summary>
        SpikePrimeLargeMotor = 0x31,

        /// <summary>SPIKE Prime colour sensor</summary>
        SpikePrimeColorSensor = 0x3D,

        /// <summary>SPIKE Prime ultrasonic distance sensor</summary>
        SpikePrimeUltrasonicDistanceSensor = 0x3E,

        /// <summary>SPIKE Prime force sensor</summary>
        SpikePrimeForceSensor = 0x3F,

        /// <summary>SPIKE Essential 3x3 colour light matrix</summary>
        SpikeEssential3x3ColorLightMatrix = 0x40,

        /// <summary>SPIKE Essential small angular motor</summary>
        SpikeEssentialSmallAngularMotor = 0x41,

        /// <summary>Technic medium motor</summary>
        TechnicMediumAngularMotor = 0x4B,

        /// <summary>Techni motor</summary>
        TechnicMotor = 0x4C,
    }
}
