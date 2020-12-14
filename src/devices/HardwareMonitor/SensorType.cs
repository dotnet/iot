namespace Iot.Device.HardwareMonitor
{
    /// <summary>
    /// Designates a sensor type
    /// </summary>
    public enum SensorType
    {
        /// <summary>
        /// Unknown sensor type
        /// </summary>
        Unknown,

        /// <summary>
        /// The sensor delivers a voltage
        /// </summary>
        Voltage,

        /// <summary>
        /// The sensor delivers a clock speed (frequency)
        /// </summary>
        Clock,

        /// <summary>
        /// The sensor delivers a temperature
        /// </summary>
        Temperature,

        /// <summary>
        /// The sensor delivers the load percentage of a component
        /// </summary>
        Load,

        /// <summary>
        /// The sensor is a fan
        /// </summary>
        Fan,

        /// <summary>
        /// The sensor measures flow (typically in a water cooling system)
        /// </summary>
        Flow,

        /// <summary>
        /// The sensor is used to control a device (i.e. fan speed)
        /// </summary>
        Control,

        /// <summary>
        /// The sensor measures the usage level (i.e. disk usage)
        /// </summary>
        Level,

        /// <summary>
        /// The sensor reports power
        /// </summary>
        Power,

        /// <summary>
        /// The sensor reports energy used
        /// </summary>
        Energy,

        /// <summary>
        /// The sensor reports heat flux (thermal heat dissipation)
        /// </summary>
        HeatFlux,

        /// <summary>
        /// The sensor reports electric current
        /// </summary>
        Current,
    }
}
