// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//Ported from https://github.com/Depau/python-apds9930/blob/master/apds9930/__init__.py

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.apds9930
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
        ///  proximity in mm
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
        ///  ambient light in lux
        /// </returns>
        public double GetAmbientLight(){
            int ch0 = GetCh0Light();
            int ch1 = GetCh1Light();
            
            return GetAmbientToLux(ch0, ch1);
        }

        /// <summary>
        ///  Reads the ch0 light data from the sensor
        /// </summary>
        /// <returns>
        ///  ADC count
        /// </returns>
        public int GetCh0Light(){
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
        public int GetCh1Light(){
            byte l = (byte)ReadByteData((byte)Register.Ch1DATAL);
            byte h = (byte)ReadByteData((byte)Register.Ch1DATAH);

            return l + (h << 8);
        }

        /// <summary>
        ///  Accepts data from both channels and returns a value in lux
        /// </summary>
        /// <returns>
        ///  ambient light in lux
        /// </returns>
        public double GetAmbientToLux(int ch0, int ch1){     
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
        ///  Value  - Led Current
        ///  0      -  100 mA
        ///  1      -  50 mA
        ///  2      -  25 mA
        ///  3      -  12.5 mA
        /// </summary>
        private void SetLedDrive(byte ledValue){
            byte regValue = (byte)ReadByteData((byte)Register.CONTROL);
            ledValue &= 3;
            ledValue = (byte)(ledValue << 6);
            regValue &= 0x3F;
            regValue |= ledValue;
                
            WriteByteData((byte)Register.CONTROL, (byte)regValue);
        }

        /// <summary>
        ///  Set Receiver gain for proximity detection
        ///  Value  - Gain
        ///  0      -  1x
        ///  1      -  2x
        ///  2      -  4x
        ///  3      -  8x
        /// </summary>
        private void SetProxmityGain(byte Value){
            byte regValue = (byte)ReadByteData((byte)Register.CONTROL);

            Value &= 3;
            Value = (byte)(Value << 2);
            regValue &= 0xF3;
            regValue |= Value;
                
            WriteByteData((byte)Register.CONTROL, (byte)regValue);
        }

        /// <summary>
        ///  Set Receiver gain for ambient light sensor
        ///  Value  - Gain
        ///  0      -  1x
        ///  1      -  4x
        ///  2      -  16x
        ///  3      -  64x
        /// </summary>
        private void SetAmbientLightGain(byte Value){
            byte regValue = (byte)ReadByteData((byte)Register.CONTROL);
            Value &= 3;           
            regValue &= 0xFC;
            regValue |= Value;
                
            WriteByteData((byte)Register.CONTROL, (byte)regValue);
        }

        private byte GetAmbientLightGain(){
            byte regValue = (byte)ReadByteData((byte)Register.CONTROL);
            regValue &= 3;           
                
            return regValue;
        }
       
        /// <summary>
        ///  Set gain for Diode used for proximity sensor
        ///  Value  - Gain
        ///  0      -  Reserved
        ///  1      -  Reserved
        ///  2      -  Use Ch1 diode
        ///  3      -  Reserved
        /// </summary>
        private void SetProximityDiode(byte Value){
            byte regValue = (byte)ReadByteData((byte)Register.CONTROL);
            Value &= 3;     
            Value = (byte)(Value << 4);      
            regValue &= 0xCF;
            regValue |= Value;
                
            WriteByteData((byte)Register.CONTROL, (byte)regValue);
        }
        
        private void SetProximityIntLowThreshold(byte Value){
            byte h = (byte)(Value >> 8);
            byte l = (byte)(Value & 0x00FF);                            
            WriteByteData((byte)Register.PILTL, (byte)l);
            WriteByteData((byte)Register.PILTH, (byte)h);
        }

        private void SetProximityIntHighThreshold(byte Value){
            byte h = (byte)(Value >> 8);
            byte l = (byte)(Value & 0x00FF);
                            
            WriteByteData((byte)Register.PIHTL, (byte)l);
            WriteByteData((byte)Register.PIHTH, (byte)h);
        }

        public void EnableProximitySensor(bool interrupt = false){
            SetProxmityGain((byte)Register.DEFAULT_PGAIN);
            SetLedDrive((byte)Register.DEFAULT_PDRIVE);
            SetProximityDiode((byte)Register.DEFAULT_PDIODE);
            EnableProximityInterrupt(interrupt);
            SetPower(true);
            SetProximitySensor(true);
        }

        public void EnableLightSensor(bool interrupt = false)
        {
            SetAmbientLightGain((byte)Register.DEFAULT_AGAIN);
            SetAmbientLightInterreupt(interrupt);
            SetPower(true);
            SetAmbientLightSensor(true);
        }

        private void SetAmbientLightInterreupt(bool interrupt = false){
            if(!interrupt){                
                _i2cDevice.WriteByte((byte)(Register.CLEAR_ALS_INT));                
            }            
        }

        private void SetAmbientLightSensor(bool value = false){
            SetMode((byte)Register.AMBIENT_LIGHT, value);
        }

        private void EnableProximityInterrupt(bool interrupt = false){
            SetMode((byte)Register.PROXIMITY_INT, interrupt);
        }

        private void SetPower(bool value){
            SetMode((byte)Register.POWER, value);
        }

        private void SetProximitySensor(bool value){
            SetMode((byte)Register.PROXIMITY, value);
        }

        private void SetMode(byte Mode, bool enable){
            byte regValue = (byte)ReadByteData((byte)Register.ENABLE);

            if(Mode >= 0 && Mode <=6){
                if(enable){
                    regValue |= (byte)(1 << Mode);
                }else{
                    regValue &= (byte)(1 << Mode);
                }
            }else if(Mode == (byte)Register.ALL){
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
