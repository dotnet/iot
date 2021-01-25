namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// Modes of Qmc5883l
    /// </summary>
    public enum Mode : byte
    {
        /// <summary>
        /// Standby mode is the default state of QMC5883L upon POR and soft reset, only few function blocks are activated
        /// in this mode, which keeps power consumption as low as possible. In this state, register values are hold on by an
        /// ultra-low power LDO, I2C interface can be woken up by reading or writing any registers.There is no
        /// magnetometer measurement in the Standby mode.Internal clocking is also halted.
      /// </summary>
      STAND_BY = 0x00,

        /// <summary>
        /// During the continuous-measurement mode (mode bits= 01), the magnetic sensor continuously makes
        /// measurements and places measured data in data output registers. The field range (or sensitivity) and data output
        /// rate registers are also located in the control register (09H), they should be set up properly for your applications in
        /// the continuous-measurement mode.
        /// </summary>
      CONTINUOUS = 0x01
    }
}
