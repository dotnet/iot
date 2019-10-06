// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Units
{
    /// <summary>
    /// Structure representing pressure
    /// </summary>
    public struct Pressure
    {
        private const double MbarRatio = 0.01;
        private const double KpaRatio = 0.001;
        private const double HpaRatio = 0.01;
        private const double InhgRatio = 0.000295301;
        private const double MmhgRatio = 0.00750062;
        private double _pa;
        
        private Pressure(double pa)
        {
            _pa = pa;
        }
        
        /// <summary>
        /// The mean sea-level pressure (MSLP) is the average atmospheric pressure at mean sea level.
        /// </summary>
        public static Pressure MeanSeaLevel => Pressure.FromPa(101325);
        
        /// <summary>
        /// Pressure in Pa
        /// </summary>
        public double Pa => _pa;
        
        /// <summary>
        /// Pressure in mbar
        /// </summary>
        public double Mbar => MbarRatio * _pa;
        
        /// <summary>
        /// Pressure in kPa
        /// </summary>
        public double Kpa => KpaRatio * _pa;
        
        /// <summary>
        /// Pressure in hPa
        /// </summary>
        public double Hpa => HpaRatio * _pa;
        
        /// <summary>
        /// Pressure in inHg
        /// </summary>
        public double Inhg => InhgRatio * _pa;
        
        /// <summary>
        /// Pressure in mmHg
        /// </summary>
        public double Mmhg => MmhgRatio * _pa;
        
        /// <summary>
        /// Creates Pressure instance from pressure in Pa
        /// </summary>
        /// <param name="value">Pressure value in Pa</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromPa(double value)
        {
            return new Pressure(value);
        }
        
        /// <summary>
        /// Creates Pressure instance from pressure in mbar
        /// </summary>
        /// <param name="value">Pressure value in mbar</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromMbar(double value)
        {
            return new Pressure(value / MbarRatio);
        }
        
        /// <summary>
        /// Creates Pressure instance from pressure in kPa
        /// </summary>
        /// <param name="value">Pressure value in kPa</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromKpa(double value)
        {
            return new Pressure(value / KpaRatio);
        }
        
        /// <summary>
        /// Creates Pressure instance from pressure in hPa
        /// </summary>
        /// <param name="value">Pressure value in hPa</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromHpa(double value)
        {
            return new Pressure(value / HpaRatio);
        }
        
        /// <summary>
        /// Creates Pressure instance from pressure in inHg
        /// </summary>
        /// <param name="value">Pressure value in inHg</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromInhg(double value)
        {
            return new Pressure(value / InhgRatio);
        }
        
        /// <summary>
        /// Creates Pressure instance from pressure in mmHg
        /// </summary>
        /// <param name="value">Pressure value in mmHg</param>
        /// <returns>Pressure instance</returns>
        public static Pressure FromMmhg(double value)
        {
            return new Pressure(value / MmhgRatio);
        }
    }
}
