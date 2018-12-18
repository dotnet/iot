// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace System.Device.Gpio.Drivers
{
    public class LibgpiodDriver : GpioDriver
    {
        private const string GpioGet = "gpioget";
        private const string GpioChip0 = "gpiochip0 ";
        private const string GpioSet = "gpioset";
        public int TimeoutMilliSeconds { get; set; } = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
        private class MutableTuple<Item1, Item2>
        {
            public Item1 Value1 { get; set; }
            public Item2 Value2 { get; set; }
            public MutableTuple(Item1 item1, Item2 item2) {
                Value1 = item1;
                Value2 = item2;
            }
        }
        private Dictionary<int, MutableTuple<PinMode, Process>> _pinNumberToPinModeAndProcess = new Dictionary<int, MutableTuple<PinMode, Process>>();
        protected internal override int PinCount => throw new NotImplementedException();

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        protected internal override void ClosePin(int pinNumber)
        {
            MutableTuple<PinMode, Process> pinModeAndProcess;
            if (_pinNumberToPinModeAndProcess.TryGetValue(pinNumber, out pinModeAndProcess)) 
            {
                Process existingProcess = pinModeAndProcess.Value2;
                if (existingProcess != null) {
                    existingProcess.Kill();
                    existingProcess.WaitForExit(1);
                    pinModeAndProcess.Value2 = null;
                }
                _pinNumberToPinModeAndProcess.Remove(pinNumber);
            }
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => throw new PlatformNotSupportedException("This driver is generic so it can not perform conversions between pin numbering schemes.");

        protected internal override PinMode GetPinMode(int pinNumber)
        {
            MutableTuple<PinMode, Process> pinModeAndProcess;
            if (_pinNumberToPinModeAndProcess.TryGetValue(pinNumber, out pinModeAndProcess))
             {
                return pinModeAndProcess.Value1;
            }
            return PinMode.Input;
        }

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            // Libgpiod command line tools does not support pull up or pull down resistors.
            return mode != PinMode.InputPullDown && mode != PinMode.InputPullUp;
        }

        protected internal override void OpenPin(int pinNumber)
        {
            // do nothing
        }

        protected internal override PinValue Read(int pinNumber)
        {
            PinValue result = default(PinValue);
            Process gpioGetCommand = new Process();
            gpioGetCommand.StartInfo.FileName = GpioGet;
            gpioGetCommand.StartInfo.Arguments = $"{GpioChip0} {pinNumber}";
            gpioGetCommand.StartInfo.RedirectStandardOutput = true;
            gpioGetCommand.Start();
            if (WaitForProcessToExit(gpioGetCommand, $"{GpioGet} {GpioChip0} {pinNumber}", TimeoutMilliSeconds))
            {
                string valueContents = gpioGetCommand.StandardOutput.ReadToEnd();
                result = ConvertMachineValueToPinValue(valueContents);
            }
            return result;
        }

        private PinValue ConvertMachineValueToPinValue(string value)
        {
            return value != null && value.IndexOf('1') != -1 ? PinValue.High : PinValue.Low;
        }

        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            if (mode == PinMode.InputPullDown || mode == PinMode.InputPullUp)
            {
                throw new PlatformNotSupportedException("This driver is generic so it does not support Input Pull Down or Input Pull Up modes.");
            }
            MutableTuple<PinMode, Process> pinModeAndProcess;
            if (_pinNumberToPinModeAndProcess.TryGetValue(pinNumber, out pinModeAndProcess))
            {
                pinModeAndProcess.Value1 = mode;
            }
            else
            {
                _pinNumberToPinModeAndProcess.Add(pinNumber, new MutableTuple<PinMode, Process>(mode, null));
            }
        }

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();  
        }

        protected internal override void Write(int pinNumber, PinValue value)
        {
            string stringValue = ConvertPinValueToMachineValue(value);
            // --mode=signal would tell the process set the value and wait for SIGINT or SIGTERM 
            string command = $"--mode=signal {GpioChip0} {pinNumber}={stringValue}";

            try
            {
                MutableTuple<PinMode, Process> pinModeAndProcess;
                Process gpioSetCommand;
                if (PinValue.Low == value)
                {
                    if (_pinNumberToPinModeAndProcess.TryGetValue(pinNumber, out pinModeAndProcess))
                    {
                        gpioSetCommand = pinModeAndProcess.Value2;
                        if (gpioSetCommand != null)
                        {
                            gpioSetCommand.Kill();
                            gpioSetCommand.WaitForExit(1);
                            pinModeAndProcess.Value2 = null;
                        }
                    }
                }
                else
                {
                    gpioSetCommand = new Process();
                    gpioSetCommand.StartInfo.FileName = GpioSet;
                    gpioSetCommand.StartInfo.Arguments = command;
                    gpioSetCommand.Start();
                    if (_pinNumberToPinModeAndProcess.TryGetValue(pinNumber, out pinModeAndProcess))
                    {
                        pinModeAndProcess.Value2= gpioSetCommand;
                    }
                    else
                    {
                        _pinNumberToPinModeAndProcess[pinNumber]= new MutableTuple<PinMode, Process>(PinMode.Output, gpioSetCommand);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                    throw new UnauthorizedAccessException("Reading a pin value requires root permissions.", e);
            }
        }

        private static bool WaitForProcessToExit(Process process, string command, int waitMilliseconds)
        {
            process.WaitForExit(waitMilliseconds);
            if (process.HasExited)
            {
                if (process.ExitCode != 0)
                {
                    throw new Exception($"Unknown error occured while running command: {command}");
                }
            }
            else
            {
                throw new Exception($"Unable to finish process, timed out while running command: {command}");
            }
            return true;
        }

        private string ConvertPinValueToMachineValue(PinValue value)
        {
            return (value == PinValue.High) ? "1" : "0";
        }

        protected override void Dispose(bool disposing)
        {
            foreach (MutableTuple<PinMode, Process> pair in _pinNumberToPinModeAndProcess.Values)
            {
                if (pair.Value2 != null)
                {
                    pair.Value2.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
