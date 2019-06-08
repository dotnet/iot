using System.Device.I2c;

namespace Iot.Device.Bmx280
{
    /// <summary>
    /// Represents a BME680 temperature, pressure, relative humidity and VOC gas sensor.
    /// </summary>
    public class Bme680 : BmxBase
    {
        /// <summary>
        /// Default I2C bus address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x76;

        /// <summary>
        /// Secondary I2C bus address.
        /// </summary>
        public const byte SecondaryI2cAddress = 0x77;

        /// <summary>
        /// The expected chip ID of the BME68x product family.
        /// </summary>
        private readonly byte DeviceId = 0x61;

        /// <summary>
        /// Initialize a new instance of the <see cref="Bme680"/> class.
        /// </summary>
        /// <param name="i2cDevice">The <see cref="I2cDevice"/> to create with.</param>
        public Bme680(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _deviceId = DeviceId;
            _calibrationData = new Bmx280CalibrationData();
            _communicationProtocol = CommunicationProtocol.I2c;
        }
    }
}
