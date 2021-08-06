namespace Iot.Device.MCP960X
{
    /// <summary>
    /// An enumeration representing the thermocouple type
    /// </summary>
    public enum ThermocoupleType : byte
    {
        /// <summary>
        /// Type K thermocouple
        /// </summary>
        K = 0b0000_0000,

        /// <summary>
        /// Type J thermocouple
        /// </summary>
        J = 0b0001_0000,

        /// <summary>
        /// Type T thermocouple
        /// </summary>
        T = 0b0010_0000,

        /// <summary>
        /// Type N thermocouple
        /// </summary>
        N = 0b0011_0000,

        /// <summary>
        /// Type S thermocouple
        /// </summary>
        S = 0b0100_0000,

        /// <summary>
        /// Type E thermocouple
        /// </summary>
        E = 0b0101_0000,

        /// <summary>
        /// Type B thermocouple
        /// </summary>
        B = 0b0110_0000,

        /// <summary>
        /// Type R thermocouple
        /// </summary>
        R = 0b0111_0000,
    }

    /// <summary>
    /// An enumeration representing the digitial filter type
    /// </summary>
    public enum DigitalFilterCoefficientsType : byte
    {
        /// <summary>
        /// Type Filter off
        /// </summary>
        N0 = 0b0000_0000,

        /// <summary>
        /// Type Filter minimum
        /// </summary>
        N1 = 0b0000_0001,

        /// <summary>
        /// Type Filter minimum  + 1
        /// </summary>
        N2 = 0b0000_0010,

        /// <summary>
        /// Type Filter minimum  + 1
        /// </summary>
        N3 = 0b0000_0011,

        /// <summary>
        /// Type Filter Mid
        /// </summary>
        N4 = 0b0000_0100,

        /// <summary>
        /// Type Filter Mid + 1
        /// </summary>
        N5 = 0b0000_0101,

        /// <summary>
        /// Type Filter Mid + 2
        /// </summary>
        N6 = 0b0000_0110,

        /// <summary>
        /// Type Filter maximum
        /// </summary>
        N7 = 0b0000_0111,
    }

    /// <summary>
    /// An enumeration representing the cold junction/ambient sensor resolution type
    /// </summary>
    public enum ColdJunctionResolutionType : byte
    {
        /// <summary>
        /// Type 0.0625°C
        /// </summary>
        N_0_0625 = 0b0000_0000,

        /// <summary>
        /// Type 0.25°C
        /// </summary>
        N_0_25 = 0b1000_0000,
    }

    /// <summary>
    /// An enumeration representing the ADC measurement resolution type
    /// </summary>
    public enum ADCMeasurementResolutionType : byte
    {
        /// <summary>
        /// Type 18-bit Resolution
        /// </summary>
        R18 = 0b0000_0000,

        /// <summary>
        /// Type 16-bit Resolution
        /// </summary>
        R16 = 0b0010_0000,

        /// <summary>
        /// Type 14-bit Resolution
        /// </summary>
        R14 = 0b0100_0000,

        /// <summary>
        /// Type 12-bit Resolution
        /// </summary>
        R12 = 0b0110_0000,
    }

    /// <summary>
    /// An enumeration representing the burst mode temperature samples type
    /// </summary>
    public enum BurstModeTemperatureSamplesType : byte
    {
        /// <summary>
        /// Type 1 sample
        /// </summary>
        S1 = 0b0000_0000,

        /// <summary>
        /// Type 2 samples
        /// </summary>
        S2 = 0b0000_0100,

        /// <summary>
        /// Type 4 samples
        /// </summary>
        S4 = 0b0000_1000,

        /// <summary>
        /// Type 8 samples
        /// </summary>
        S8 = 0b0000_1100,

        /// <summary>
        /// Type 16 samples
        /// </summary>
        S16 = 0b0001_0000,

        /// <summary>
        /// Type 32 samples
        /// </summary>
        S32 = 0b0001_0100,

        /// <summary>
        /// Type 64 samples
        /// </summary>
        S64 = 0b0001_1000,

        /// <summary>
        /// Type 128 samples
        /// </summary>
        S128 = 0b0001_1100,
    }

    /// <summary>
    /// An enumeration representing the shutdown mode type
    /// </summary>
    public enum ShutdownModesType : byte
    {
        /// <summary>
        /// Type Normal operation
        /// </summary>
        Normal = 0b0000_0000,

        /// <summary>
        /// Type Shutdown mode
        /// </summary>
        Shutdown = 0b0000_0001,

        /// <summary>
        /// Type Burst mode
        /// </summary>
        Burst = 0b0000_0010,
    }

    /// <summary>
    /// An enumeration representing the shutdown mode type
    /// </summary>
    public enum DeviceIDType : byte
    {
        /// <summary>
        /// Type device MCP9600/L00/RL00
        /// </summary>
        MCP9600 = 0x40,

        /// <summary>
        /// Type device MCP9601/L01/RL01
        /// </summary>
        MCP9601 = 0x41,
    }
}
