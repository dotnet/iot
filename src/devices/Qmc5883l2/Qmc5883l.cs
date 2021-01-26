using System;
using System.Device.Model;
using System.Device

namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// 3-Axis Digital Compass QMC5883L
    /// </summary>
    [Interface("3-Axis Digital Compass QMC5883L")]
    public class Qmc5883l : IDisposable
    {
        /// <summary>
        /// QMC5883L Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x0D;

        private readonly byte _mode;
        private readonly byte _outputRate;
        private readonly byte _fieldRange;
        private readonly byte _oversampling;

        private I2cDevice

        public void Dispose()
        {
            
        }
    }
}
