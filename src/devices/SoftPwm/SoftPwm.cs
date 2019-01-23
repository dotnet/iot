using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Pwm;
using System.Device.Gpio;
using System.Threading;

namespace System.Device.Pwm.Drivers
{
    public class SoftPwm: PwmDriver
    {
        // use to determine the freqncy of the PWM
        // PulseFrequency = total frenquency
        // curent pulse width = when the signal is hi
        private double currentPulseWidth;
        private double pulseFrequency;
        // Use to determine the length of the pulse
        // 100 % = full output. 0%= nothing as output
        private double percentage;
        // Use to determine if we are using a high precision timer or not
        private bool precisionPWM = false;

        private bool isRunning;
        private bool istopped = true;
        private int servoPin = -1;

        private Stopwatch stopwatch = Stopwatch.StartNew();

        private Thread runningThread;
        private GpioController controller;
        private bool runThread = true;

        private void RunSoftPWM()
        {
            if(precisionPWM)
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
            while (runThread)
            {
                //Write the pin high for the appropriate length of time
                if (isRunning)
                {
                    if (currentPulseWidth != 0)
                    {
                        controller.Write(servoPin, PinValue.High);
                    }
                    //Use the wait helper method to wait for the length of the pulse
                    if (precisionPWM)
                        Wait(currentPulseWidth);
                    else
                        Task.Delay(TimeSpan.FromMilliseconds(currentPulseWidth)).Wait();
                    //The pulse if over and so set the pin to low and then wait until it's time for the next pulse
                    controller.Write(servoPin, PinValue.Low);
                    if (precisionPWM)
                        Wait(pulseFrequency - currentPulseWidth);
                    else
                        Task.Delay(TimeSpan.FromMilliseconds(pulseFrequency - currentPulseWidth)).Wait();
                }
                else
                {
                    if (!istopped)
                    {
                        controller.Write(servoPin, PinValue.Low);
                        istopped = true;
                    }
                }
            }
        }

        //A synchronous wait is used to avoid yielding the thread 
        //This method calculates the number of CPU ticks will elapse in the specified time and spins
        //in a loop until that threshold is hit. This allows for very precise timing.
        private void Wait(double milliseconds)
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


        public SoftPwm()
        {
            controller = new GpioController();
            if (controller == null)
            {
                Debug.WriteLine("GPIO does not exist on the current system.");
                return;
            }
        }

        public SoftPwm(bool preceisionTimer) : this()
        {
            precisionPWM = preceisionTimer;
        }

        private void UpdateRange()
        {
            currentPulseWidth = percentage * pulseFrequency / 100;
        }

        private void ValidatePWMChannel(int pinNumber)
        {
            if(servoPin != pinNumber)
            {
                throw new ArgumentException($"Soft PWM on pin {pinNumber} not initialized");
            }
        }

        protected override void OpenChannel(int pinNumber, int pwmChannel)
        {
            servoPin = pinNumber;
            controller.OpenPin(servoPin);
            controller.SetPinMode(servoPin, PinMode.Output);
            runningThread = new Thread(RunSoftPWM);
            runningThread.Start();

        }

        protected override void CloseChannel(int pinNumber, int pwmChannel)
        {
            ValidatePWMChannel(pinNumber);
            controller.ClosePin(servoPin);
            servoPin = -1;
            isRunning = false;
        }

        protected override void ChangeDutyCycle(int pinNumber, int pwmChannel, double dutyCycleInPercentage)
        {
            ValidatePWMChannel(pinNumber);        
            percentage = dutyCycleInPercentage;
            UpdateRange();    
        }

        protected override void StartWriting(int pinNumber, int pwmChannel, double frequencyInHertz, double dutyCycleInPercentage)
        {
            ValidatePWMChannel(pinNumber);
            if (frequencyInHertz > 0)
                pulseFrequency = 1 / frequencyInHertz * 1000.0;
            else
                pulseFrequency = 0.0;
            percentage = dutyCycleInPercentage;
            UpdateRange();
            isRunning = true;
        }

        protected override void StopWriting(int pinNumber, int pwmChannel)
        {
            ValidatePWMChannel(pinNumber); 
            isRunning = false;
        }
    }
}
