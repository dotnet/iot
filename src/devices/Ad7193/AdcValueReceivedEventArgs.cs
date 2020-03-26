namespace Iot.Device.Ad7193
{
    using System;

    public class AdcValueReceivedEventArgs : EventArgs
    {
        public AdcValueReceivedEventArgs(AdcValue adcValue)
        {
            AdcValue = adcValue;
        }

        public AdcValue AdcValue { get; }
    }
}
