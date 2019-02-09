// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Diagnostics;

namespace Iot.Device.Fsr408
{
    public class Fsr408 : IDisposable
    {
        private int _pinNumber = 0;

        private Mcp3008.Mcp3008 _adcConverter;
        public int Resistance { get; set; } = 10000; // 10k ohm
        public int PowerSupplied { get; set; } = 5000;  // 5 mV

        public Fsr408(Mcp3008.Mcp3008 adcConverter)
        {
            _adcConverter = adcConverter;
        }

        public Fsr408(int pinNumber, Mcp3008.Mcp3008 adcConverter)
        {
            _adcConverter = adcConverter;
            _pinNumber = pinNumber;
        }

        public int Read()
        {
            return _adcConverter.Read(_pinNumber);
        }

        public int ReadVoltage()
        {
            return CalculateVoltage(_adcConverter.Read(_pinNumber));
        }

        public int CalculateVoltage(int readValue)
        {
            // Mcp3008 analog voltage reading ranges from 0 to 1023 (10 bit) 
            // mapping it to corresponding milli voltage 
            // which ranging from 0mV to PowerSupplied mV 
            return PowerSupplied * readValue / 1023;
        }

        public int ReadFsrResistance()
        {
            return CalculateFsrResistance(CalculateVoltage(_adcConverter.Read(_pinNumber)));
        }

        public int CalculateFsrResistance(int fsrVoltage)
        {
            // Formula: FSR = ((Vcc - V) * R) / V
            if (fsrVoltage > 0)
            {
                return (PowerSupplied - fsrVoltage) * Resistance / fsrVoltage;
            }
            return 0;
        }

        public int ReadPressureForceUsingMcp3008()
        {
            return CalculateForce(CalculateFsrResistance(CalculateVoltage(_adcConverter.Read(_pinNumber))));
        }

        public int CalculateForce(int resistance)
        {
            if (resistance > 0)
            {
                int force;
                int fsrConductance = 1000000 / resistance; // in micro ohms

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

        public void Dispose()
        {
            if (_adcConverter != null)
            {
                _adcConverter.Dispose();
                _adcConverter = null;
            }
        }
    }
}
