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
        private GpioController _controller;
        private int _pinNumber = 18;

        public Mcp3008.Mcp3008 AdcConverter { get; set; }
        public int Resistance { get; set; } = 10000; // 10k ohm
        public int PowerSupplied { get; set; } = 5000;  // 5 mV

        public Fsr408()
        {
            _controller = new GpioController();
            _controller.OpenPin(_pinNumber);
        }
        public Fsr408(int pinNumber)
        {
            _pinNumber = pinNumber;
            _controller = new GpioController();
            _controller.OpenPin(_pinNumber);
        }


        public int ReadFromMcp3008()
        {
            if (AdcConverter != null)
            {
                return AdcConverter.Read(0);
            }
            else
            {
                throw new NotSupportedException("ADC converter not set");
            }
        }

        public int ReadCapacitorChargingDuration()
        {
            
            // set pin to low
            _controller.SetPinMode(_pinNumber, PinMode.Output);
            _controller.Write(_pinNumber, PinValue.Low);

            // Prepare pin for input and ...
            _controller.SetPinMode(_pinNumber, PinMode.Input);
            Stopwatch timeElapsed = Stopwatch.StartNew();
            while (_controller.Read(_pinNumber) == PinValue.Low)
            { // count until read high
                
                if (timeElapsed.ElapsedMilliseconds == 30000)
                {   // if count goes too high it means FSR resustance is highest which means no pressure, don't need to count more  
                    break;
                }
            }
            return (int)timeElapsed.ElapsedMilliseconds;
        }

        public int ReadVotlageUsingMcp3008()
        {
            if (AdcConverter != null)
            {
                int readValue = AdcConverter.Read(_pinNumber);
                return CalculateVoltage(readValue);
            }
            else
            {
                throw new NotSupportedException("ADC converter not set");
            }
        }

        public int CalculateVoltage(int readValue)
        {
            // Mcp3008 analog voltage reading ranges from 0 to 1023 (10 bit) 
            // mapping it to corresponding milli voltage 
            // which ranging from 0mV to PowerSupplied mV 
            return PowerSupplied * readValue / 1023;
        }

        public int ReadFsrResistanceUsingMcp3008()
        {
            if (AdcConverter != null)
            {
                int readValue = AdcConverter.Read(_pinNumber);
                return CalculateFsrResistance(CalculateVoltage(readValue));
            }
            else
            {
                throw new NotSupportedException("ADC converter not set");
            }
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
            if (AdcConverter != null)
            {
                int readValue = AdcConverter.Read(_pinNumber);
                return CalculateForce(CalculateFsrResistance(CalculateVoltage(readValue)));
            }
            else
            {
                throw new NotSupportedException("ADC converter not set");
            }
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
            if (_controller != null)
            {
                _controller.Dispose();
                _controller = null;
            }
        }
    }
}
