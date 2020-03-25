using System;

namespace BBD.Mars.Iot.Device.Ad7193
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
