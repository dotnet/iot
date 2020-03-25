using System;

namespace Iot.Device.Ad7193
{
    public class AdcValueReceivedEventArgs : EventArgs
    {
        public AdcValueReceivedEventArgs(AdcValue adcValue)
        {
            this.AdcValue = adcValue;
        }

        public AdcValue AdcValue { get; }
    }
}
