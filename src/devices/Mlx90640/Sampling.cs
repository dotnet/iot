using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Mlx90640
{
    /// <summary>
    /// MLX90640 sampling
    /// </summary>
    public enum Sampling : byte
    {
        /// <summary>
        /// 0.5 Hz
        /// </summary>
        Sampling_0_5_Hz = 0x00,

        /// <summary>
        /// 1 Hz
        /// </summary>
        Sampling_01_Hz = 0x01,

        /// <summary>
        /// 2 Hz
        /// </summary>
        Sampling_02_Hz = 0x02,

        /// <summary>
        /// 4 Hz
        /// </summary>
        Sampling_04_Hz = 0x03,

        /// <summary>
        /// 8 Hz
        /// </summary>
        Sampling_08_Hz = 0x04,

        /// <summary>
        /// 16 Hz
        /// </summary>
        Sampling_16_Hz = 0x05,

        /// <summary>
        /// 32 Hz
        /// </summary>
        Sampling_32_Hz = 0x06,

        /// <summary>
        /// 64 Hz
        /// </summary>
        Sampling_64_Hz = 0x07,
    }
}
