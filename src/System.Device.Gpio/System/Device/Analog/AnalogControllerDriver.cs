using System;
using System.Collections.Generic;
using System.Text;
using System.Device.Gpio;

namespace System.Device.Analog
{
    public abstract class AnalogControllerDriver : IDisposable
    {
        public event ValueChangeEventHandler ValueChanged;

        public abstract int PinCount
        {
            get;
        }

        /// <summary>
        /// The reference voltage level, if externally supplied.
        /// Not supported by all boards.
        /// While the Arduino does have an external analog input reference pin, Firmata doesn't allow configuring it.
        /// </summary>
        public double VoltageReference
        {
            get;
            set;
        }

        protected internal abstract int ConvertPinNumberToLogicalNumberingScheme(int pinNumber);

        protected internal abstract int ConvertLogicalNumberingSchemeToPinNumber(int logicalPinNumber);

        public abstract void OpenPin(int pinNumber);
        public abstract void ClosePin(int pinNumber);

        /// <summary>
        /// Return the resolution of an analog input pin.
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <param name="numberOfBits">Returns the resolution of the ADC in number of bits, including the sign bit (if applicable)</param>
        /// <param name="minVoltage">Minimum measurable voltage</param>
        /// <param name="maxVoltage">Maximum measurable voltage</param>
        public abstract void QueryResolution(int pinNumber, out int numberOfBits, out double minVoltage, out double maxVoltage);

        public abstract uint ReadRaw(int pinNumber);

        public virtual double ReadVoltage(int pinNumber)
        {
            uint raw = ReadRaw(pinNumber);
            return ConvertToVoltage(pinNumber, raw);
        }

        public virtual double ConvertToVoltage(int pinNumber, uint rawValue)
        {
            QueryResolution(pinNumber, out int numberOfBits, out double minVoltage, out double maxVoltage);
            if (minVoltage >= 0)
            {
                // The ADC can handle only positive values
                int maxRawValue = (1 << numberOfBits) - 1;
                double voltage = ((double)rawValue / maxRawValue) * maxVoltage;
                return voltage;
            }
            else
            {
                // The ADC also handles negative values. This means that the number of bits includes the sign.
                uint maxRawValue = (uint)((1 << (numberOfBits - 1)) - 1);
                if (rawValue < maxRawValue)
                {
                    double voltage = ((double)rawValue / maxRawValue) * maxVoltage;
                    return voltage;
                }
                else
                {
                    // This is a bitmask which has all the bits 1 that are not used in the data.
                    // We use this to sign-extend our raw value. The mask also includes the sign bit itself,
                    // but we know that this is already 1
                    uint topBits = ~maxRawValue;
                    rawValue |= topBits;
                    int raw2 = (int)rawValue;
                    double voltage = ((double)raw2 / maxRawValue) * maxVoltage;
                    return voltage; // This is now negative
                }
            }
        }

        public abstract bool SupportsAnalogInput(int pinNumber);

        public abstract void EnableAnalogValueChangedEvent(int pinNumber, GpioController masterController, int masterPin);

        public abstract void DisableAnalogValueChangedEvent(int pinNumber);

        protected void FireValueChanged(ValueChangedEventArgs eventArgs)
        {
            ValueChanged?.Invoke(this, eventArgs);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
