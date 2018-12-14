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
        public int _waitMilliSeconds { get; set;}
        private Dictionary<int, PinMode> _pinNumperToPinModes;
        private Dictionary<int, Process> _pinNumperToProcess;
        protected internal override int PinCount => throw new PlatformNotSupportedException("This driver is generic so it can not enumerate how many pins are available.");

        public LibgpiodDriver() {
            _pinNumperToPinModes = new Dictionary<int, PinMode>();
            _pinNumperToProcess = new Dictionary<int, Process>();
            _waitMilliSeconds = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
        }
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        protected internal override void ClosePin(int pinNumber)
        {
            // do nothing_pinNumperToProcess
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => throw new PlatformNotSupportedException("This driver is generic so it can not perform conversions between pin numbering schemes.");

        protected internal override PinMode GetPinMode(int pinNumber)
        {
            if (_pinNumperToPinModes.ContainsKey(pinNumber))
             {
                return _pinNumperToPinModes[pinNumber];
            }
            else
            {
                throw new InvalidOperationException("There was an attempt to get a mode to a pin that is not yet open.");
            }
        }

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            // Libgpiod driver does not support pull up or pull down resistors.
            if (mode == PinMode.InputPullDown || mode == PinMode.InputPullUp)
                return false;
            return true;
        }

        protected internal override void OpenPin(int pinNumber)
        {
            // do nothing
        }

        protected internal override PinValue Read(int pinNumber)
        {
            PinValue result = default(PinValue);
            try
            {
                Process myProc = new Process();
                myProc.StartInfo.FileName = GpioGet;
                myProc.StartInfo.Arguments = $" {GpioChip0} {pinNumber}";
                myProc.StartInfo.RedirectStandardOutput = true;
                myProc.Start();
                if (HandlProcessExit(myProc, $"{GpioGet} {GpioChip0} {pinNumber}", _waitMilliSeconds))
                {
                    string valueContents = myProc.StandardOutput.ReadToEnd();
                    result = ConvertSysFsValueToPinValue(valueContents);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                throw new UnauthorizedAccessException("Reading a pin value requires root permissions.", e);
            }
            return result;
        }
        private PinValue ConvertSysFsValueToPinValue(string value)
        {
            PinValue result;
            value = value.Trim();

            switch (value)
            {
                case "0":
                    result = PinValue.Low;
                    break;
                case "1":
                    result = PinValue.High;
                    break;
                default:
                    throw new ArgumentException($"Invalid Gpio pin value {value}");
            }

            return result;
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
            _pinNumperToPinModes[pinNumber] = mode;
        }

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();  // TODO
        }

        protected internal override void Write(int pinNumber, PinValue value)
        {
            string stringValue = ConvertPinValueToSysFs(value);
            string command = $"--mode=signal {GpioChip0} {pinNumber}={stringValue}";
            try
            {
                Process myProc;
                if (PinValue.Low == value)
                {
                    if (_pinNumperToProcess.ContainsKey(pinNumber))
                    {
                        myProc = _pinNumperToProcess[pinNumber];
                        myProc.Kill();
                        myProc.WaitForExit((int)TimeSpan.FromMilliseconds(1).TotalMilliseconds);
                        HandlProcessExit(myProc, $"{GpioSet} {command}", 1);
                        _pinNumperToProcess.Remove(pinNumber);
                    }
                }
                else
                {
                    myProc = new Process();
                    myProc.StartInfo.FileName = GpioSet;
                    myProc.StartInfo.Arguments = command;
                    myProc.Start();
                    _pinNumperToProcess[pinNumber] = myProc;
                }
            }
            catch (UnauthorizedAccessException e)
            {
                    throw new UnauthorizedAccessException("Reading a pin value requires root permissions.", e);
            }
        }

        private static bool HandlProcessExit(Process myProc, string command, int waitMilliseconds)
        {
            myProc.WaitForExit(waitMilliseconds);
            if (myProc.HasExited)
            {
                if (myProc.ExitCode != 0)
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

        private string ConvertPinValueToSysFs(PinValue value)
        {
            string result = string.Empty;
            switch (value)
            {
                case PinValue.High:
                    result = "1";
                    break;
                case PinValue.Low:
                    result = "0";
                    break;
                default:
                    throw new ArgumentException($"Invalid pin value {value}");
            }
            return result;
        }
    }
}
