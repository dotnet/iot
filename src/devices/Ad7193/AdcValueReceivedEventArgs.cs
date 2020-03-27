using System;

namespace Iot.Device.Ad7193
{
    /// <summary>
    /// Contains data for the ADC value received event
    /// </summary>
    public class AdcValueReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the AdcValueReceivedEventArgs class
        /// </summary>
        /// <param name="adcValue">The raw ADC value that was received</param>
        public AdcValueReceivedEventArgs(AdcValue adcValue)
        {
            AdcValue = adcValue;
        }

        /// <summary>
        /// Read-only accessor of the ADC value that was received
        /// </summary>
        public AdcValue AdcValue { get; }
    }
}
