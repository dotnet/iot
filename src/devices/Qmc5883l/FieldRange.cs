using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qmc5883l
{
    /// <summary>
    /// Field ranges of the magnetic sensor can be selected through the register RNG. The full scale field range is
    /// determined by the application environments.For magnetic clear environment, low field range such as +/- 2gauss
    /// can be used.The field range goes hand in hand with the sensitivity of the magnetic sensor. The lowest field range
    /// has the highest sensitivity, therefore, higher resolution.
    /// </summary>
    [Flags]
    public enum FieldRange : byte
    {
       /// <summary>
       /// Field range of 2 Gauss
       /// </summary>
        GAUSS_2 = 0x00,

        /// <summary>
        /// Field range of 8 Gauss
        /// </summary>
        GAUSS_8 = 0x10
    }
}
