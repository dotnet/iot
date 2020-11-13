using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;
using Iot.Device.Arduino;

#pragma warning disable CS1591
namespace Arduino.Samples
{
    /// <summary>
    /// These are simple methods to test the IL execution engine on the Arduino
    /// </summary>
    public class ArduinoCompilerSampleMethods
    {
        public class SimpleLedBinding
        {
            private readonly GpioController _controller;
            private int _ledPin;
            private int _delay;

            public SimpleLedBinding(GpioController controller, int pin, int delay)
            {
                _controller = controller;
                _controller.SetPinMode(pin, PinMode.Output);
                _ledPin = pin;
                _delay = delay;
            }

            public void Loop()
            {
                for (int i = 0; i < 20; i++)
                {
                    _controller.Write(_ledPin, 1);
                    ArduinoRuntimeCore.Sleep(null!, _delay);
                    _controller.Write(_ledPin, 0);
                    ArduinoRuntimeCore.Sleep(null!, _delay);
                }
            }

            public static void RunBlink(GpioController gpioController, int pin, int delay)
            {
                SimpleLedBinding blink = new SimpleLedBinding(gpioController, pin, delay);
                blink.Loop();
            }
        }

        public static int AddInts(int a, int b)
        {
            return a + b;
        }

        public static int SubtactInts(int a, int b)
        {
            return a - b;
        }

        public static int Max(int a, int b)
        {
            if (a > b)
            {
                return a;
            }
            else
            {
                return b;
            }
        }

        public static bool Equal(int a, int b)
        {
            return a == b;
        }

        public static bool Unequal(int a, int b)
        {
            return !(a == b);
        }

        public static bool Smaller(int a, int b)
        {
            return a < b;
        }

        public static void Blink(IArduinoHardwareLevelAccess hw, int pin, int delay)
        {
            hw.SetPinMode(pin, PinMode.Output);
            for (int i = 0; Smaller(i, 10); i++)
            {
                hw.WritePin(pin, 1);
                ArduinoRuntimeCore.Sleep(hw, delay);
                hw.WritePin(pin, 0);
                ArduinoRuntimeCore.Sleep(hw, delay);
            }

            hw.SetPinMode(pin, PinMode.Input);
        }

        public static UInt32 ReadDht11(IArduinoHardwareLevelAccess controller, int pin)
        {
            uint count;
            uint resultLow = 0;
            uint result = 0;
            uint checksum = 0;
            int debugPin = 10; // Using a scope to measure performance on this pin

            uint loopCount = 5000;

            UInt32 watchStart = controller.GetMicroseconds();
            // controller.SleepMicroseconds(1);
            UInt32 elapsed = controller.GetMicroseconds() - watchStart;
            controller.DebugValue((int)elapsed);

            controller.SetPinMode(debugPin, PinMode.Output);
            controller.WritePin(debugPin, 0);

            // keep data line HIGH
            // We need to write the pin before changing the mode, this ensures we're not tripping a low pulse here
            controller.WritePin(pin, 1);

            controller.SetPinMode(pin, PinMode.Output);
            controller.WritePin(pin, 1);
            ArduinoRuntimeCore.Sleep(controller, 20);

            // send trigger signal
            controller.WritePin(pin, 0);
            // wait at least 18 milliseconds
            // here wait for 18 milliseconds will cause sensor initialization to fail
            ArduinoRuntimeCore.Sleep(controller, 20);

            // pull up data line
            // TODO: This causes the signal to be disturbed for about 5ms. Do we need to always write a 0 before setting the mode to InputPullUp?
            // controller.WritePin(pin, 1);
            // wait 20 - 40 microseconds
            // controller.SleepMicroseconds(20);
            controller.SetPinMode(pin, PinMode.InputPullUp);
            controller.WritePin(debugPin, 1);
            // controller.SleepMicroseconds(55);
            // DHT corresponding signal - LOW - about 80 microseconds
            count = loopCount;
            while (controller.ReadPin(pin) == 0)
            {
                if (count-- == 0)
                {
                    controller.DebugValue(0xAAAA);
                    return 0;
                }
            }

            // HIGH - about 80 microseconds
            count = loopCount;
            while (controller.ReadPin(pin) == 1)
            {
                if (count-- == 0)
                {
                    controller.DebugValue(0xBBBB);
                    return 0;
                }
            }

            // the read data contains 40 bits
            for (int i = 0; i < 40; i++)
            {
                // beginning signal per bit, about 50 microseconds
                count = loopCount;

                while (controller.ReadPin(pin) == 0)
                {
                    if (count-- == 0)
                    {
                        controller.DebugValue(i);
                        return 0;
                    }

                }

                // 26 - 28 microseconds represent 0
                // 70 microseconds represent 1
                count = loopCount;
                while (controller.ReadPin(pin) == 1)
                {
                    if (count-- == 0)
                    {
                        controller.DebugValue(i);
                        return 0;
                    }
                }

                UInt32 loopUsed = loopCount - count;
                loopUsed *= elapsed;

                // bit to byte
                // less than 40 microseconds can be considered as 0, not necessarily less than 28 microseconds
                // here take 30 microseconds
                resultLow <<= 1;
                if (loopUsed > 30)
                {
                    resultLow |= 1;
                    controller.WritePin(debugPin, 1);
                }
                else
                {
                    controller.WritePin(debugPin, 0);
                }

                if (i == 31)
                {
                    // When 32 bits have been read, save them away - what follows is the checksum
                    result = resultLow;
                    resultLow = 0;
                }
            }

            checksum = resultLow;
            ////_lastMeasurement = Environment.TickCount;

            ////if ((_readBuff[4] == ((_readBuff[0] + _readBuff[1] + _readBuff[2] + _readBuff[3]) & 0xFF)))
            ////{
            ////    IsLastReadSuccessful = (_readBuff[0] != 0) || (_readBuff[2] != 0);
            ////}
            ////else
            ////{
            ////    IsLastReadSuccessful = false;
            ////}

            return result;
        }
    }
}
