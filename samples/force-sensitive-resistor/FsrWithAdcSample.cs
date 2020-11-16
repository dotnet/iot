// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Adc;
using Iot.Device.Spi;

namespace force_sensitive_resistor
{
    class FsrWithAdcSample
    {
        private int _resistance = 10_000; // kOhm
        private int _voltageSupplied = 3_300;  // 3300mV = 3.3V
        private Mcp3008 _adcConvertor;

        public FsrWithAdcSample()
        {
            // Create a ADC convertor instance you are using depending how you wired ADC pins to controller
            // in this example used ADC Mcp3008 with software spi method. If you want to do hardware spi, then call SoftwareSpi.Create()
            _adcConvertor = new Mcp3008(new SoftwareSpi(18, 23, 24, 25));
        }

        public double CalculateVoltage(int readValue)
        {
            // This sample used Mcp3008 ADC which analog voltage read output ranges from 0 to 1023 (10 bit) 
            // mapping it to corresponding milli voltage, update output range if you use different ADC
            return _voltageSupplied * readValue / 1023;
        }

        internal int Read(int v)
        {
            return _adcConvertor.Read(0);
        }

        public double CalculateFsrResistance(double fsrVoltage)
        {
            // Formula: FSR = ((Vcc - V) * R) / V
            if (fsrVoltage > 0)
            {
                return (_voltageSupplied - fsrVoltage) * _resistance / fsrVoltage;
            }
            return 0;
        }

        public double CalculateForce(double resistance)
        {
            if (resistance > 0)
            {
                double force;
                double fsrConductance = 1_000_000 / resistance; 

                // Use the two FSR guide graphs to approximate the force
                if (fsrConductance <= 1000)
                {
                    force = fsrConductance / 80;
                }
                else
                {
                    force = fsrConductance - 1000;
                    force /= 30;
                }
                return force;
            }
            return 0;
        }
    }
}
