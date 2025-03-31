// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Bmp180;

namespace Iot.Device.Bmp180
{
    /// <summary>
    /// Implements the Bmm150 magnetic field data (off-chip) temperature compensation functions
    /// https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bmm150-ds001.pdf
    /// Page 15
    /// </summary>
    public class Bmm150Compensation
    {
        private const int Bmm150OverflowAdcvalXYaxesFlip = -4096;
        private const int Bmm150OverflowAdcvalZaxisHall = -16384;
        private const int Bmm150NegativeSaturationZ = -32767;
        private const int Bmm150PositiveSaturationZ = 32767;

        /// <summary>
        /// Returns the compensated magnetometer x axis data(micro-tesla) in float.
        /// More details, permalink: https://github.com/BoschSensortec/BMM150-Sensor-API/blob/a20641f216057f0c54de115fe81b57368e119c01/bmm150.c#L1614
        /// </summary>
        /// <param name="x">axis raw value</param>
        /// <param name="rhall">temperature compensation value (RHALL) </param>
        /// <param name="trimData">trim registers values</param>
        /// <returns>compensated magnetometer x axis data(micro-tesla) in float</returns>
        public static double CompensateX(int x, uint rhall, Bmm150TrimRegisterData trimData)
        {
            int retval = 0;
            int process_comp_x0 = 0;
            int process_comp_x1;
            int process_comp_x2;
            int process_comp_x3;
            int process_comp_x4;
            int process_comp_x5;
            int process_comp_x6;
            int process_comp_x7;
            int process_comp_x8;
            int process_comp_x9;
            int process_comp_x10;

            // Overflow condition check
            if (x != Bmm150OverflowAdcvalXYaxesFlip)
            {
                if (rhall != 0)
                {
                    /* Availability of valid data*/
                    // rhall is always > 0 and at most 16 bits. In fact, it should be in the order of DigXyz1 (around 6000)
                    process_comp_x0 = (int)rhall;
                }
                else if (trimData.DigXyz1 != 0)
                {
                    process_comp_x0 = trimData.DigXyz1;
                }
                else
                {
                    process_comp_x0 = 0;
                }

                if (process_comp_x0 != 0)
                {
                    /* Processing compensation equations*/
                    process_comp_x1 = (trimData.DigXyz1) * 16384; // ~ 100'000'000
                    process_comp_x2 = ((process_comp_x1 / process_comp_x0)) - (0x4000); // ~0
                    retval = (process_comp_x2);
                    process_comp_x3 = ((retval) * (retval));
                    process_comp_x4 = ((trimData.DigXy2) * (process_comp_x3 / 128));
                    process_comp_x5 = ((trimData.DigXy1) * 128);
                    process_comp_x6 = retval * process_comp_x5;
                    process_comp_x7 = (((process_comp_x4 + process_comp_x6) / 512) + (0x100000));
                    process_comp_x8 = (((trimData.DigX2) + (0xA0)));
                    process_comp_x9 = ((process_comp_x7 * process_comp_x8) / 4096);
                    process_comp_x10 = (x) * process_comp_x9;
                    retval = ((process_comp_x10 / 8192));
                    return (retval + ((trimData.DigX1) * 8)) / 16.0;
                }
                else
                {
                    return Double.NaN;
                }
            }
            else
            {
                // Overflow, set output to 0.0f
                return Double.NaN;
            }
        }

        /// <summary>
        /// Returns the compensated magnetometer y axis data(micro-tesla) in float.
        /// More details, permalink: https://github.com/BoschSensortec/BMM150-Sensor-API/blob/a20641f216057f0c54de115fe81b57368e119c01/bmm150.c#L1648
        /// </summary>
        /// <param name="y">axis raw value</param>
        /// <param name="rhall">temperature compensation value (RHALL) </param>
        /// <param name="trimData">trim registers values</param>
        /// <returns>compensated magnetometer y axis data(micro-tesla) in float</returns>
        public static double CompensateY(int y, uint rhall, Bmm150TrimRegisterData trimData)
        {
            int retval;
            int process_comp_y0 = 0;
            int process_comp_y1;
            int process_comp_y2;
            int process_comp_y3;
            int process_comp_y4;
            int process_comp_y5;
            int process_comp_y6;
            int process_comp_y7;
            int process_comp_y8;
            int process_comp_y9;

            // Overflow condition check
            if (y != Bmm150OverflowAdcvalXYaxesFlip)
            {
                if (rhall != 0)
                {
                    /* Availability of valid data*/
                    process_comp_y0 = (int)rhall;
                }
                else if (trimData.DigXyz1 != 0)
                {
                    process_comp_y0 = trimData.DigXyz1;
                }
                else
                {
                    process_comp_y0 = 0;
                }

                if (process_comp_y0 != 0)
                {
                    /*Processing compensation equations*/
                    process_comp_y1 = ((trimData.DigXyz1) * 16384) / process_comp_y0;
                    process_comp_y2 = (process_comp_y1) - (0x4000);
                    retval = (process_comp_y2);
                    process_comp_y3 = retval * retval;
                    process_comp_y4 = (trimData.DigXy2) * (process_comp_y3 / 128);
                    process_comp_y5 = (((trimData.DigXy1) * 128));
                    process_comp_y6 = ((process_comp_y4 + (retval * process_comp_y5)) / 512);
                    process_comp_y7 = trimData.DigY2 + 0xA0;
                    process_comp_y8 = (((process_comp_y6 + 0x100000) * process_comp_y7) / 4096);
                    process_comp_y9 = (y * process_comp_y8);
                    retval = (process_comp_y9 / 8192);
                    return (retval + ((trimData.DigY1) * 8)) / 16.0;
                }
                else
                {
                    return double.NaN;
                }
            }
            else
            {
                // Overflow, set output to 0.0f
                return double.NaN;
            }
        }

        /// <summary>
        /// Returns the compensated magnetometer z axis data(micro-tesla) in float.
        /// More details, permalink: https://github.com/BoschSensortec/BMM150-Sensor-API/blob/a20641f216057f0c54de115fe81b57368e119c01/bmm150.c#L1682
        /// </summary>
        /// <param name="z">axis raw value</param>
        /// <param name="rhall">temperature compensation value (RHALL) </param>
        /// <param name="trimData">trim registers values</param>
        /// <returns>compensated magnetometer z axis data(micro-tesla) in float</returns>
        public static double CompensateZ(int z, uint rhall, Bmm150TrimRegisterData trimData)
        {
            int retval;
            int process_comp_z0;
            int process_comp_z1;
            int process_comp_z2;
            int process_comp_z3;
            int process_comp_z4;

            // Overflow condition check
            if (z != Bmm150OverflowAdcvalZaxisHall)
            {
                if ((trimData.DigZ2 != 0) && (trimData.DigZ1 != 0)
                                            && (rhall != 0) && (trimData.DigXyz1 != 0))
                {
                    /*Processing compensation equations*/
                    process_comp_z0 = ((int)rhall) - (trimData.DigXyz1);
                    process_comp_z1 = ((trimData.DigZ3) * ((process_comp_z0))) / 4;
                    process_comp_z2 = (((z - trimData.DigZ4)) * 32768);
                    process_comp_z3 = (trimData.DigZ1) * ((int)rhall * 2);
                    process_comp_z4 = ((process_comp_z3 + (32768)) / 65536);
                    retval = ((process_comp_z2 - process_comp_z1) / (trimData.DigZ2 + process_comp_z4));

                    /* saturate result to +/- 2 micro-tesla */
                    if (retval > Bmm150PositiveSaturationZ)
                    {
                        retval = Bmm150PositiveSaturationZ;
                    }
                    else
                    {
                        if (retval < Bmm150NegativeSaturationZ)
                        {
                            retval = Bmm150NegativeSaturationZ;
                        }
                    }

                    /* Conversion of LSB to micro-tesla*/
                    return retval / 16.0;
                }
                else
                {
                    return Double.NaN;

                }
            }
            else
            {
                /* Overflow condition*/
                return double.NaN;
            }
        }
    }
}
