
using System;
using System.Device.Gpio;
using System.Diagnostics;

namespace Iot.Device.DHTxx
{
    public class DelayMS
    {

        private Stopwatch stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Wait asyncrhonous way for a specific number of milliseconds
        /// 
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to wait</param>
        /// <remarks>
        /// This function doesn't work if you want to wait for less than 100 microseconds.
        /// </remarks>
        public void Wait(double milliseconds)
        {
            long initialTick = stopwatch.ElapsedTicks;
            long initialElapsed = stopwatch.ElapsedMilliseconds;
            double desiredTicks = milliseconds / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                //nothing than waiting
            }
        }

    }

    public class DHTSensor
    {
        private byte[] dht11_val = new byte[5];
        private int pin;
        private DhtType dhtType;
        const int MAX_TIME = 85;
        const uint MAX_WAIT = 255;
        
        /// <summary>
        /// How last read went
        /// </summary>
        public bool LastRead { get; internal set; }
        
        /// <summary>
        /// Get the last read temperature
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public double Temperature
        {
            get {
                if (dhtType == DhtType.Dht11)
                    return GetTempDht11();
                else
                    return GetTempDht22();
            }
        }
        
        /// <summary>
        /// Get the last read humidity
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public double Humidity
        {
            get {
                if (dhtType == DhtType.Dht11)
                    return GetHumidityDht11();
                else
                    return GetHumidityDht22();
            }
        }

        /// <summary>
        /// Create a DHT sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        /// <param name="dhtType">The DHT Type, eitherDht11 or Dht22</param>
        public DHTSensor(int pin, DhtType dhtType)
        {
            this.pin = pin;
            this.dhtType = dhtType;
        }

        /// <summary>
        /// Start a reading
        /// </summary>
        /// <returns>
        /// <c>true</c> if read is successfull, otherwise <c>false</c>.
        /// </returns>
        public bool ReadData()
        {
            using (GpioController controller = new GpioController())
            {                
                // Set the max value for waiting micro second
                // 27 = debug
                // 99 = release
                byte waitMS = 99;
                #if DEBUG
                    waitMS = 27;
                 #endif
                controller.OpenPin(pin);

                DelayMS delayMS= new DelayMS();
                PinValue lststate = PinValue.High; 
                uint counter = 0; 
                byte j = 0, i; 
                for (i = 0; i < 5; i++) 
                    dht11_val[i] = 0; 

                // write on the pin
                controller.SetPinMode(pin, PinMode.Output);
                controller.Write(pin, PinValue.Low); 
                //wait 18 milliseconds
                delayMS.Wait(18); 
                controller.Write(pin, PinValue.High); 
                delayMS.Wait(0.02); 
                controller.SetPinMode(pin, PinMode.Input);
                for (i = 0; i < MAX_TIME; i++) 
                { 
                    counter = 0; 
                    while (controller.Read(pin) == lststate){ 
                        counter++; 
                        // This wait about 1 micro second
                        // No other way to do it for such a precision
                        for(byte wt=0; wt< waitMS; wt++)
                            ; 
                        if (counter == MAX_WAIT) 
                            break; 
                    } 
                    lststate = controller.Read(pin); 
                    if (counter == MAX_WAIT)                     
                        break; 
                    
                    // top 3 transistions are ignored   
                    if ((i >= 4) && (i % 2 == 0)){ 
                        dht11_val[j / 8] <<= 1; 
                        if (counter>16) 
                            dht11_val[j / 8] |= 1; 
                        j++;                         
                    } 
                } 

                if ((j >= 40) && (dht11_val[4] == ((dht11_val[0] + dht11_val[1] + dht11_val[2] + dht11_val[3]) & 0xFF))) 
                { 
                    if ((dht11_val[0] == 0) && (dht11_val[2] == 0)) 
                        LastRead = false;                        
                    else
                        LastRead = true;

                } else
                    LastRead = false;
                
                controller.ClosePin(pin);
                return LastRead; 
            }
        }


        public string ByteToBinaryString(byte number)
        {
            const byte mask = 1;
            var binary = string.Empty;
            for(int i=0; i<8; i++)
            {
                // Logical AND the number and prepend it to the result string
                binary = (number & mask) + binary;
                number = (byte)(number >> 1);
            }
            binary = binary.PadLeft(8, '0');
            return binary;
        }
        // Convertion for DHT11
        private double GetTempDht11() 
        { 
            if(LastRead)
                return (double)(dht11_val[2] + dht11_val[3] / 10); 
            else
                return(double.MaxValue);
        } 

        private double GetHumidityDht11() 
        { 
            if(LastRead)
                return (double)(dht11_val[0] + dht11_val[1] / 10); 
            else
                return(double.MaxValue);
        } 

        // convertion for DHT22
        private double GetTempDht22() 
        { 
            if(LastRead)
            {
                var temp = (((dht11_val[2] & 0x7F) << 8) | dht11_val[3]) * 0.1F;
                // if MSB = 1 we have negative temperature
                return ((dht11_val[2] & 0x80) == 0 ? temp : -temp); 
            }
            else
                return(double.MaxValue);
        } 

        private double GetHumidityDht22() 
        { 
            if(LastRead)
                return (double)((dht11_val[0] << 8) | dht11_val[1]) * 0.1F; 
            else
                return(double.MaxValue);
        } 

    }
}