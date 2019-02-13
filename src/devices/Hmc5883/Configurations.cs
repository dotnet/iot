using System;

namespace Iot.Device.Hmc5883 {

    /// <summary>
    /// Data output rates.
    /// This enum determines the rate at which data is written to all three data output registers.
    /// </summary>
    public enum  OutputRates : byte
    {
        /// <summary>
        /// Data output rate 0.75Hz.
        /// </summary>
        Rate0_75 = 0b00000000,
        /// <summary>
        /// Data output rate 1.5Hz.
        /// </summary>
        Rate1_5 = 0b00000100,
        /// <summary>
        /// Data output rate 3Hz.
        /// </summary>
        Rate3 = 0b00001000,
        /// <summary>
        /// Data output rate 7.5Hz.
        /// </summary>
        Rate7_5 = 0b00001100,
        /// <summary>
        /// Data output rate 15Hz (default).
        /// </summary>
        Rate15 = 0b00010000,
        /// <summary>
        /// Data output rate 30Hz.
        /// </summary>
        Rate30 = 0b00010100,
        /// <summary>
        /// Data output rate 75Hz.
        /// </summary>
        Rate75 = 0b00011000
    }

    /// <summary>
    /// Measurement configuration.
    /// This enum defines the measurement flow of the device, specifically whether or not to incorporate an applied bias to the sensor into the measurement.
    /// </summary>
    public enum  MeasurementModes : byte
    {
        /// <summary>
        /// Normal measurement configuration (default).
        /// In normal measurement configuration the device follows normal measurement flow.
        /// The positive and negative pins of the resistive load are left floating and high impedance.
        /// </summary>
        Normal = 0b00000000,
        /// <summary>
        /// Positive bias configuration for X and Y axes, negative bias configuration for Z axis.
        /// In this configuration, a positive current is forced across the resistive load for X and Y axes, a negative current for Z axis.
        /// </summary>
        PositiveBiasConfiguration = 0b00000001,
        /// <summary>
        /// Negative bias configuration for X and Y axes, positive bias configuration for Z axis.
        /// In this configuration, a negative current is forced across the resistive load for X and Y axes, a positive current for Z axis.
        /// </summary>
        NegativeBias = 0b00000010
    }

    /// <summary>
    /// Gain configuration.
    /// This enum configures the gain for the device. The gain configuration is common for all channels.
    /// </summary>
    public enum  GainConfiguration : byte
    {
        /// <summary>
        /// Gain(counts/Gauss) 1280
        /// </summary>
        Ga0_9 = 0b00000000,
        /// <summary>
        /// Gain(counts/Gauss) 1024 (default).
        /// </summary>
        Ga1_2 = 0b00100000,
        /// <summary>
        /// Gain(counts/Gauss) 768.
        /// </summary>
        Ga1_9 = 0b01000000,
        /// <summary>
        /// Gain(counts/Gauss) 614.
        /// </summary>
        Ga2_5 = 0b01100000,
        /// <summary>
        /// Gain(counts/Gauss) 415.
        /// </summary>
        Ga4_0 = 0b10000000,
        /// <summary>
        /// Gain(counts/Gauss) 361.
        /// </summary>
        Ga4_6 = 0b10100000,
        /// <summary>
        /// Gain(counts/Gauss) 307.
        /// </summary>
        Ga5_5 = 0b11000000,
        /// <summary>
        /// Gain(counts/Gauss) 219.
        /// </summary>
        Ga7_9 = 0b11100000
    }

    /// <summary>
    /// Mode select enum.
    /// This enum select the operation mode of this device.
    /// </summary>
    public enum  OperatingModes : byte
    {
        /// <summary>
        /// Continuous-Measurement Mode.
        /// In continuous-measurement mode, the device continuously performs measurements and places the result in the data register.
        /// </summary>
        ContinuousMeasurementMode = 0b00000000,        
        /// <summary>
        /// Single-Measurement Mode (default).
        /// When single-measurement mode is selected, device performs a single measurement, sets RDY high and returned to sleep mode.
        /// </summary>
        SingleMeasurementMode = 0b00000001,
        /// <summary>
        /// Idle Mode. 
        /// Device is placed in idle mode.
        /// </summary>
        IdleMode = 0b00000010,
        /// <summary>
        /// Sleep Mode. 
        /// Device is placed in sleep mode.
        /// </summary>
        SleepMode = 0b00000011
    }

    /// <summary>
    /// This enum contains device statuses.
    /// </summary>
    public enum  StatusRegisterValues
    {
        /// <summary>
        /// Regulator Enabled Bit. This bit is set when the internal voltage regulator is enabled.
        /// This bit is cleared when the internal regulator is disabled.
        /// </summary>
        Ren,
        /// <summary>
        /// Data output register lock. This bit is set when this some but not all for of the six data output registers have been read.
        /// When this bit is set, the six data output registers are locked and any new data will not be placed in these register until
        /// on of four conditions are met: one, all six have been read or the mode changed, two, a POR is issued, three, the
        /// mode is changed, or four, the measurement is changed.
        /// </summary>
        LOCK,
        /// <summary>
        /// Ready Bit. Set when data is written to all six data registers. Cleared when device initiates a write to the data output
        /// registers, when in off mode, and after one or more of the data output registers are written to. 
        /// When RDY bit is clear it shall remain cleared for a minimum of 5 μs. 
        /// DRDY pin can be used as an alternative to the status register for monitoring the device for measurement data.
        /// </summary>
        RDY
    }
}