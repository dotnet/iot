// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//Ported from https://github.com/Depau/python-apds9930/blob/master/apds9930/__init__.py

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Apds9930
{
    public class Apds9930 : IDisposable
    {
        private I2cDevice _i2cDevice;   
        public const byte DefaultI2cAddress = 0x39;
                   
        public Apds9930(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;        

            //Turn off all features (set ENABLE to 0x00)
            SetMode((byte)Register.ALL, false);

            //Set default values for ambient light and proximity registers
            WriteByteData((byte)Register.ATIME, (byte)Register.DEFAULT_ATIME );            
            WriteByteData((byte)Register.WTIME, (byte)Register.DEFAULT_WTIME);
            WriteByteData((byte)Register.PPULSE, (byte)Register.DEFAULT_PPULSE);
            WriteByteData((byte)Register.POFFSET, (byte)Register.DEFAULT_POFFSET);
            WriteByteData((byte)Register.CONFIG, (byte)Register.DEFAULT_CONFIG);
        
            SetLedDrive((byte)Register.DEFAULT_PDRIVE);          
            SetProxmityGain((byte)Register.DEFAULT_PGAIN);            
            SetAmbientLightGain((byte)Register.DEFAULT_AGAIN);
            SetProximityDiode((byte)Register.DEFAULT_PDIODE);
            SetProximityIntLowThreshold((byte)Register.DEFAULT_PILT);            
            SetProximityIntHighThreshold((byte)Register.DEFAULT_PIHT);            
            WriteByteData((byte)Register.PERS, (byte)Register.DEFAULT_PERS);            
        }

        /// <summary>
        ///  Reads the proximity from the sensor
        /// </summary>
        /// <returns>
        ///  proximity
        /// </returns>
        public int GetProximity()
        {
            byte l = (byte)ReadByteData((byte)Register.PDATAL);
            byte h = (byte)ReadByteData((byte)Register.PDATAH);

            return l + (h << 8);
        }

        /// <summary>
        ///  Reads the ambient light from the sensor
        /// </summary>
        /// <returns>
        ///  Ambient light in lux
        /// </returns>
        public double GetAmbientLight()
        {
            int ch0 = GetCh0Light();
            int ch1 = GetCh1Light();

            // Constants according to the datasheet Page 9                     
            //Device Factor
            double DF = 52.0; 
            //Glass (or Lens) Attenuation Factor 
            double GA = 0.49;
            // Coefficients in open air            
            double B  = 1.862;
            double C  = 0.746;
            double D  = 1.291;  

            double ALSIT = 2.73 * (256.00 - (double)Register.DEFAULT_ATIME);
            double iac = Math.Max(ch0 - B * ch1, C * ch0 - D * ch1);
            double lpc = GA * DF / (ALSIT * GetAmbientLightGain());

            return iac * lpc;                
        }

        /// <summary>
        ///  Reads the ch0 light data from the sensor
        /// </summary>
        /// <returns>
        ///  ADC count
        /// </returns>
        public int GetCh0Light()
        {
            byte l = (byte)ReadByteData((byte)Register.Ch0DATAL);
            byte h = (byte)ReadByteData((byte)Register.Ch0DATAH);

            return l + (h << 8);
        }

        /// <summary>
        ///  Reads the ch1 light data from the sensor
        /// </summary>
        /// <returns>
        ///  ADC count
        /// </returns>
        public int GetCh1Light()
        {
            byte l = (byte)ReadByteData((byte)Register.Ch1DATAL);
            byte h = (byte)ReadByteData((byte)Register.Ch1DATAH);

            return l + (h << 8);
        }

        /// <summary>
        ///  Accepts data from both channels and returns a value in lux
        /// </summary>
        /// <returns>
        ///  Ambient light in lux
        /// </returns>
        public double GetAmbientToLux(int ch0, int ch1)
        {     
            // Constants according to the datasheet Page 9                     
            //Device Factor
            double DF = 52.0; 
            //Glass (or Lens) Attenuation Factor 
            double GA = 0.49;
            // Coefficients in open air            
            double B  = 1.862;
            double C  = 0.746;
            double D  = 1.291;  

            double ALSIT = 2.73 * (256.00 - (double)Register.DEFAULT_ATIME);
            double iac = Math.Max(ch0 - B * ch1, C * ch0 - D * ch1);
            double lpc = GA * DF / (ALSIT * GetAmbientLightGain());

            return iac * lpc;
        }

        /// <summary>
        ///  Set Led drive strength for proximity and ALS
        ///  <para> Value - Gain  </para>
        ///  <para>  0    -  100 mA   </para>
        ///  <para>  1    -  50 mA   </para>
        ///  <para>  2    -  25 mA  </para>
        ///  <para>  3    -  12.5 mA  </para>
        /// </summary>
        private void SetLedDrive(byte ledValue)
        {
            byte regValue = (byte)ReadByteData((byte)Register.CONTROL);
            ledValue &= 0b011;
            ledValue = (byte)(ledValue << 6);            
            regValue &= 0b0011_1111;
            regValue |= ledValue;                
            WriteByteData((byte)Register.CONTROL, (byte)regValue);
        }

        /// <summary>
        ///  Set Receiver gain for proximity detection
        ///  <para> Value - Gain  </para>
        ///  <para>  0    -  1x   </para>
        ///  <para>  1    -  4x   </para>
        ///  <para>  2    -  16x  </para>
        ///  <para>  3    -  64x  </para>
        /// </summary>
        private void SetProxmityGain(byte Value)
        {
            byte regValue = (byte)ReadByteData((byte)Register.CONTROL);
            Value &= 0b011;
            Value = (byte)(Value << 2);            
            regValue &= 0b1111_0011;
            regValue |= Value;                
            WriteByteData((byte)Register.CONTROL, (byte)regValue);
        }

        /// <summary>
        ///  Set Receiver gain for ambient light sensor
        ///  <para> Value - Gain  </para>
        ///  <para>  0    -  1x   </para>
        ///  <para>  1    -  4x   </para>
        ///  <para>  2    -  16x  </para>
        ///  <para>  3    -  64x  </para>
        /// </summary>
        private void SetAmbientLightGain(byte Value)
        {
            byte regValue = (byte)ReadByteData((byte)Register.CONTROL);
            Value &= 0b011;                       
            regValue &= 0b1111_1100;
            regValue |= Value;                
            WriteByteData((byte)Register.CONTROL, (byte)regValue);
        }

        /// <summary>
        ///  Get ambient light gain
        /// </summary>
        /// <return>
        ///  <para> Value - Gain  </para>
        ///  <para>  0    -  1x   </para>
        ///  <para>  1    -  4x   </para>
        ///  <para>  2    -  16x  </para>
        ///  <para>  3    -  64x  </para>
        /// </return>
        private byte GetAmbientLightGain()
        {
            byte regValue = (byte)ReadByteData((byte)Register.CONTROL);
            regValue &= 0b011;           
                
            return regValue;
        }
       
        /// <summary>
        ///  Set gain for Diode used for proximity sensor
        ///  <para> Value - Gain  </para>
        ///  <para>  0   -  Reserved   </para>
        ///  <para>  1   -  Reserved   </para>
        ///  <para>  2   -  Use Ch1 diode  </para>
        ///  <para>  3   -  Reserved  </para>
        /// </summary>
        private void SetProximityDiode(byte Value)
        {
            byte regValue = (byte)ReadByteData((byte)Register.CONTROL);
            Value &= 0b011;     
            Value = (byte)(Value << 4);                  
            regValue &= 0b1100_1111;
            regValue |= Value;                
            WriteByteData((byte)Register.CONTROL, (byte)regValue);
        }
        
        /// <summary>
        ///  Set proximity low threshold
        /// </summary>
        private void SetProximityIntLowThreshold(byte Value)
        {
            byte h = (byte)(Value >> 8);
            byte l = (byte)(Value & 0x00FF);                            
            WriteByteData((byte)Register.PILTL, (byte)l);
            WriteByteData((byte)Register.PILTH, (byte)h);
        }

        /// <summary>
        ///  Set proximity high threshold
        /// </summary>
        private void SetProximityIntHighThreshold(byte Value)
        {
            byte h = (byte)(Value >> 8);
            byte l = (byte)(Value & 0x00FF);                            
            WriteByteData((byte)Register.PIHTL, (byte)l);
            WriteByteData((byte)Register.PIHTH, (byte)h);
        }

        /// <summary>
        ///  Set all the needed values to turn on the proximity sensor and turn it on.
        /// </summary>
        public void EnableProximitySensor(bool interrupt = false)
        {
            SetProxmityGain((byte)Register.DEFAULT_PGAIN);
            SetLedDrive((byte)Register.DEFAULT_PDRIVE);
            SetProximityDiode((byte)Register.DEFAULT_PDIODE);
            //Enable proximity interrupt
            SetMode((byte)Register.PROXIMITY_INT, interrupt);
            SetPower(true);
            //Enable proximity sensor
            SetMode((byte)Register.PROXIMITY, true);            
        }

        /// <summary>
        ///  Set all the needed values to turn on the ambient light sensor and turn it on.
        /// </summary>
        public void EnableLightSensor(bool interrupt = false)
        {
            SetAmbientLightGain((byte)Register.DEFAULT_AGAIN);
            //Enable ambient light interreupt
            if(!interrupt)
            {                
                _i2cDevice.WriteByte((byte)(Register.CLEAR_ALS_INT));                
            }                        
            // Enable oscillator
            SetPower(true);                        
            //Enable ambient light sensor
            SetMode((byte)Register.AMBIENT_LIGHT, true);
        }

        /// <sumary>
        /// Enable or disbale internal oscillator 
        /// </summary>
        private void SetPower(bool value)
        {
            SetMode((byte)Register.POWER, value);
        }
        
        /// <summary>
        ///  Enable or disable the proximity interrupt.
        /// </summary>
        private void SetMode(byte Mode, bool enable)
        {
            byte regValue = (byte)ReadByteData((byte)Register.ENABLE);

            if(Mode >= (byte)Register.POWER && Mode <= (byte)Register.SLEEP_AFTER_INT)
            {
                if(enable)
                {
                    regValue |= (byte)(1 << Mode);
                }
                else
                {
                    regValue &= (byte)(1 << Mode);
                }
            }
            else if(Mode == (byte)Register.ALL)
            {
                regValue = enable ? (byte)0x7F : (byte)0x00;
            }

            WriteByteData((byte)Register.ENABLE, (byte)regValue);
        }

        internal ushort ReadByteData(byte register, Register mode = Register.AUTO_INCREMENT)
        {           
            _i2cDevice.WriteByte((byte)(register | (byte)mode));
            byte value = _i2cDevice.ReadByte();
        
            return value;
        }

        internal void WriteByteData(byte register, byte value, Register mode = Register.AUTO_INCREMENT)        
        {            
            Span<byte> bytes =  stackalloc byte[2];
            bytes[0] = (byte)(register |  (byte)mode);
            bytes[1] = value;
            _i2cDevice.Write(bytes);
        }

        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}