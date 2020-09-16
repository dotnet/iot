//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.QwiicButton
{
    /// <summary>
    /// TODO
    /// </summary>
    public class QwiicButton : IDisposable
    {
        /******************************************************************************
SparkFun Qwiic Button/Switch Library Source File
Fischer Moseley @ SparkFun Electronics
Original Creation Date: July 24, 2019

Development environment specifics:
    Qwiic Button Version: 1.0.0
    Qwiic Switch Version: 1.0.0

******************************************************************************/
        private const int DefaultAddress = 0x6F; // default I2C address of the button
        private const int DeviceId = 0x5D;          // device ID of the Qwiic Button

        // private TwoWire* _i2cPort;      //Generic connection to user's chosen I2C port
        private byte _deviceAddress; // I2C address of the button/switch
        private I2cBusAccess _i2cBus;

        /*-------------------------------- Device Status ------------------------*/

        /// <summary>
        /// TODO
        /// </summary>
        public QwiicButton(int busId, byte deviceAddress = DefaultAddress)
        {
            var settings = new I2cConnectionSettings(busId, deviceAddress);
            var device = I2cDevice.Create(settings);
            _i2cBus = new I2cBusAccess(device);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public byte GetDeviceId()
        {
            return _i2cBus.ReadSingleRegister(Register.ID);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool CheckDeviceId()
        {
            // return ((DeviceID() == DEV_ID_SW) || (DeviceID() == DEV_ID_BTN)); //Return true if the device ID matches either the button or the switch
            return GetDeviceId() == DeviceId;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public byte GetDeviceType()
        {
            if (GetDeviceId() == DeviceId)
            {
                return 1;
            }

            return 0;

            // if (IsConnected())
            // {
            // only try to get the device ID if the device will acknowledge
            // if (GetDeviceId() == DeviceId)
            //    {
            //        return 1;
            //    }

            // if (id == DEV_ID_SW)
            //    //     return 2;
            // // }
            // return 0;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ushort GetFirmwareVersion()
        {
            ushort version = (ushort)(_i2cBus.ReadSingleRegister(Register.FIRMWARE_MAJOR) << 8);
            version |= _i2cBus.ReadSingleRegister(Register.FIRMWARE_MINOR);
            return version;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool SetI2cAddress(byte address)
        {
            if (address < 0x08 || address > 0x77)
            {
                Console.WriteLine("I2C input address is out of range");
                return false;
            }

            var success = _i2cBus.WriteSingleRegister(Register.I2C_ADDRESS, address);
            if (success)
            {
                _deviceAddress = address;
                return true;
            }

            Console.WriteLine("Could not set I2C address");
            return false;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public byte I2cAddress() => _deviceAddress;

        /*------------------------------ Button Status ---------------------- */

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsPressed()
        {
            var status = new StatusRegisterBitField(_i2cBus.ReadSingleRegister(Register.BUTTON_STATUS));
            // Console.WriteLine($"StatusRegisterValue: {Convert.ToString(status.StatusRegisterValue, toBase: 2)}");
            return status.IsPressed;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool HasBeenClicked()
        {
            var status = new StatusRegisterBitField(_i2cBus.ReadSingleRegister(Register.BUTTON_STATUS));
            return status.HasBeenClicked;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool Available()
        {
            var status = new StatusRegisterBitField(_i2cBus.ReadSingleRegister(Register.BUTTON_STATUS));
            return status.EventAvailable;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public byte ClearEventBits()
        {
            var status = new StatusRegisterBitField(_i2cBus.ReadSingleRegister(Register.BUTTON_STATUS))
            {
                EventAvailable = false,
                HasBeenClicked = false,
                IsPressed = false
            };
            return _i2cBus.WriteSingleRegisterWithReadback(Register.BUTTON_STATUS, status.StatusRegisterValue);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ushort GetDebounceTime()
        {
            return _i2cBus.ReadDoubleRegister(Register.BUTTON_DEBOUNCE_TIME);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public byte SetDebounceTime(ushort time)
        {
            return (byte)_i2cBus.WriteDoubleRegisterWithReadback(Register.BUTTON_DEBOUNCE_TIME, time);
        }

        /*------------------- Interrupt Status/Configuration ---------------- */

        /// <summary>
        /// TODO
        /// </summary>
        public byte EnablePressedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_i2cBus.ReadSingleRegister(Register.INTERRUPT_CONFIG))
                {
                    PressedEnable = true
                };
            return _i2cBus.WriteSingleRegisterWithReadback(Register.INTERRUPT_CONFIG, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public byte DisablePressedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_i2cBus.ReadSingleRegister(Register.INTERRUPT_CONFIG))
                {
                    PressedEnable = false
                };
            return _i2cBus.WriteSingleRegisterWithReadback(Register.INTERRUPT_CONFIG, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public byte EnableClickedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_i2cBus.ReadSingleRegister(Register.INTERRUPT_CONFIG))
                {
                    ClickedEnable = true
                };

            return _i2cBus.WriteSingleRegisterWithReadback(Register.INTERRUPT_CONFIG, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public byte DisableClickedInterrupt()
        {
            var interrupt =
                new InterruptConfigBitField(_i2cBus.ReadSingleRegister(Register.INTERRUPT_CONFIG))
                {
                    ClickedEnable = false
                };

            return _i2cBus.WriteSingleRegisterWithReadback(Register.INTERRUPT_CONFIG, interrupt.InterruptConfigValue);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public byte ResetInterruptConfig()
        {
            var interrupt = new InterruptConfigBitField
            {
                PressedEnable = true,
                ClickedEnable = true
            };
            var interruptValue = _i2cBus.WriteSingleRegisterWithReadback(Register.INTERRUPT_CONFIG, interrupt.InterruptConfigValue);

            var status = new StatusRegisterBitField
            {
                EventAvailable = false
            };
            _i2cBus.WriteSingleRegisterWithReadback(Register.BUTTON_STATUS, status.StatusRegisterValue);

            return interruptValue;
        }

        /*------------------------- Queue Manipulation ---------------------- */
        // pressed queue manipulation

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsPressedQueueFull()
        {
            var pressedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.PRESSED_QUEUE_STATUS));
            return pressedQueue.IsFull;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsPressedQueueEmpty()
        {
            var pressedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.PRESSED_QUEUE_STATUS));
            return pressedQueue.IsEmpty;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ulong TimeSinceLastPress()
        {
            return _i2cBus.ReadQuadRegister(Register.PRESSED_QUEUE_FRONT);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ulong TimeSinceFirstPress()
        {
            return _i2cBus.ReadQuadRegister(Register.PRESSED_QUEUE_BACK);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ulong PopPressedQueue()
        {
            ulong tempData = TimeSinceFirstPress(); // Take the oldest value on the queue

            var pressedQueue =
                new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.PRESSED_QUEUE_STATUS))
                {
                    PopRequest = true
                };

            // Remove the oldest value from the queue
            _i2cBus.WriteSingleRegister(Register.PRESSED_QUEUE_STATUS, pressedQueue.QueueStatusValue);

            return tempData; // Return the value we popped
        }

        /// <summary>
        /// Clicked queue manipulation.
        /// </summary>
        public bool IsClickedQueueFull()
        {
            var clickedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.CLICKED_QUEUE_STATUS));
            return clickedQueue.IsFull;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsClickedQueueEmpty()
        {
            var clickedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.CLICKED_QUEUE_STATUS));
            return clickedQueue.IsEmpty;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ulong TimeSinceLastClick()
        {
            return _i2cBus.ReadQuadRegister(Register.CLICKED_QUEUE_FRONT);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ulong TimeSinceFirstClick()
        {
            return _i2cBus.ReadQuadRegister(Register.CLICKED_QUEUE_BACK);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public ulong PopClickedQueue()
        {
            ulong tempData = TimeSinceFirstClick();

            var clickedQueue =
                new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.CLICKED_QUEUE_STATUS))
                {
                    PopRequest = true
                };
            _i2cBus.WriteSingleRegister(Register.CLICKED_QUEUE_STATUS, clickedQueue.QueueStatusValue);

            return tempData;
        }

        /*------------------------ LED Configuration ------------------------ */

        /// <summary>
        /// TODO
        /// </summary>
        public bool LedConfig(byte brightness, ushort cycleTime, ushort offTime, byte granularity = 1)
        {
            bool success = _i2cBus.WriteSingleRegister(Register.LED_BRIGHTNESS, brightness);
            success &= _i2cBus.WriteSingleRegister(Register.LED_PULSE_GRANULARITY, granularity);
            success &= _i2cBus.WriteDoubleRegister(Register.LED_PULSE_CYCLE_TIME, cycleTime);
            success &= _i2cBus.WriteDoubleRegister(Register.LED_PULSE_OFF_TIME, offTime);
            return success;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool LedOff()
        {
            return LedConfig(0, 0, 0);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool LedOn(byte brightness = 255)
        {
            return LedConfig(brightness, 0, 0);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _i2cBus?.Dispose();
            _i2cBus = null;
        }
    }
}
