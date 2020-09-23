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

    Qwiic Button is an I2C based button that records any button presses to a queue.
    Qwiic Button maintains a queue of events. To remove events from the queue write
    the appropriate register (timeSinceLastButtonClicked or timeSinceLastButtonPressed)
    to zero. The register will then be filled with the next available event time.

******************************************************************************/
        private const int DefaultAddress = 0x6F; // default I2C address of the button
        private I2cBusAccess _i2cBus;

        /*-------------------------------- Device Status ------------------------*/

        /// <summary>
        /// Initializes a new instance of the <see cref="QwiicButton"/> class.
        /// </summary>
        /// <param name="i2cBusId">I2C bus ID the button is connected to.</param>
        /// <param name="i2cAddress">I2C bus address of the button (default=0x6F).</param>
        public QwiicButton(int i2cBusId, byte i2cAddress = DefaultAddress)
        {
            I2cBusId = i2cBusId;
            I2cAddress = i2cAddress;
            var settings = new I2cConnectionSettings(i2cBusId, i2cAddress);
            var device = I2cDevice.Create(settings);
            _i2cBus = new I2cBusAccess(device);
        }

        /// <summary>
        /// I2C bus ID the button is connected to.
        /// </summary>
        public int I2cBusId { get; set; }

        /// <summary>
        /// I2C bus address of the button.
        /// </summary>
        public byte I2cAddress { get; set; }

        /// <summary>
        /// Returns the 8-bit device ID of the button.
        /// </summary>
        public byte GetDeviceId()
        {
            return _i2cBus.ReadSingleRegister(Register.ID);
        }

        /// <summary>
        /// Returns the firmware version of the button as a 16-bit integer.
        /// The leftmost (high) byte is the major revision number, and the rightmost (low) byte is the minor revision number.
        /// </summary>
        public ushort GetFirmwareVersionAsInteger()
        {
            ushort version = (ushort)(_i2cBus.ReadSingleRegister(Register.FIRMWARE_MAJOR) << 8);
            version |= _i2cBus.ReadSingleRegister(Register.FIRMWARE_MINOR);
            return version;
        }

        /// <summary>
        /// Returns the firmware version of the button as a [major revision].[minor revision] string.
        /// </summary>
        public string GetFirmwareVersionAsString()
        {
            var major = _i2cBus.ReadSingleRegister(Register.FIRMWARE_MAJOR);
            var minor = _i2cBus.ReadSingleRegister(Register.FIRMWARE_MINOR);
            return major + "." + minor;
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
                I2cAddress = address;
                return true;
            }

            Console.WriteLine("Could not set I2C address");
            return false;
        }

        /*------------------------------ Button Status ---------------------- */

        /// <summary>
        /// Returns whether the button is pressed.
        /// </summary>
        public bool IsPressed()
        {
            var status = new StatusRegisterBitField(_i2cBus.ReadSingleRegister(Register.BUTTON_STATUS));
            return status.IsPressed;
        }

        /// <summary>
        /// Returns whether the button is clicked.
        /// </summary>
        public bool HasBeenClicked()
        {
            var status = new StatusRegisterBitField(_i2cBus.ReadSingleRegister(Register.BUTTON_STATUS));
            return status.HasBeenClicked;
        }

        /// <summary>
        /// Returns whether a new button status event has occurred.
        /// </summary>
        public bool EventAvailable()
        {
            var status = new StatusRegisterBitField(_i2cBus.ReadSingleRegister(Register.BUTTON_STATUS));
            return status.EventAvailable;
        }

        /// <summary>
        /// Sets <see cref="IsPressed"/>, <see cref="HasBeenClicked"/> and <see cref="EventAvailable"/> to false.
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
        /// Returns the time in milliseconds that the button waits for the mechanical contacts to settle.
        /// </summary>
        public ushort GetDebounceTime()
        {
            return _i2cBus.ReadDoubleRegister(Register.BUTTON_DEBOUNCE_TIME);
        }

        /// <summary>
        /// Sets the time in milliseconds that the button waits for the mechanical contacts to settle and checks if the register was set properly.
        /// Returns 0 on success, 1 on register I2C write fail, and 2 if the value didn't get written into the register properly.
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

        #region Queue Manipulation

        /// <summary>
        /// Returns whether the queue of button press timestamps is full.
        /// </summary>
        public bool IsPressedQueueFull()
        {
            var pressedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.PRESSED_QUEUE_STATUS));
            return pressedQueue.IsFull;
        }

        /// <summary>
        /// Returns whether the queue of button press timestamps is empty.
        /// </summary>
        public bool IsPressedQueueEmpty()
        {
            var pressedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.PRESSED_QUEUE_STATUS));
            return pressedQueue.IsEmpty;
        }

        /// <summary>
        /// Returns how many milliseconds it has been since the last button press.
        /// Since this returns a 32-bit unsigned int, it will roll over about every 50 days.
        /// </summary>
        public uint TimeSinceLastPress()
        {
            return _i2cBus.ReadQuadRegister(Register.PRESSED_QUEUE_FRONT);
        }

        /// <summary>
        /// Returns how many milliseconds it has been since the first button press.
        /// Since this returns a 32-bit unsigned int, it will roll over about every 50 days.
        /// </summary>
        public uint TimeSinceFirstPress()
        {
            return _i2cBus.ReadQuadRegister(Register.PRESSED_QUEUE_BACK);
        }

        /// <summary>
        /// Returns the oldest value in the queue (milliseconds since first button press), and then removes it.
        /// </summary>
        public uint PopPressedQueue()
        {
            var timeSinceFirstPress = TimeSinceFirstPress(); // Take the oldest value on the queue

            var pressedQueue =
                new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.PRESSED_QUEUE_STATUS))
                {
                    PopRequest = true
                };

            // Remove the oldest value from the queue
            _i2cBus.WriteSingleRegister(Register.PRESSED_QUEUE_STATUS, pressedQueue.QueueStatusValue);

            return timeSinceFirstPress; // Return the value we popped
        }

        /// <summary>
        /// Returns whether the queue of button click timestamps is full.
        /// </summary>
        public bool IsClickedQueueFull()
        {
            var clickedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.CLICKED_QUEUE_STATUS));
            return clickedQueue.IsFull;
        }

        /// <summary>
        /// Returns whether the queue of button click timestamps is empty.
        /// </summary>
        public bool IsClickedQueueEmpty()
        {
            var clickedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.CLICKED_QUEUE_STATUS));
            return clickedQueue.IsEmpty;
        }

        /// <summary>
        /// Returns how many milliseconds it has been since the last button click.
        /// Since this returns a 32-bit unsigned int, it will roll over about every 50 days.
        /// </summary>
        public uint TimeSinceLastClick()
        {
            return _i2cBus.ReadQuadRegister(Register.CLICKED_QUEUE_FRONT);
        }

        /// <summary>
        /// Returns how many milliseconds it has been since the first button click.
        /// Since this returns a 32-bit unsigned int, it will roll over about every 50 days.
        /// </summary>
        public uint TimeSinceFirstClick()
        {
            return _i2cBus.ReadQuadRegister(Register.CLICKED_QUEUE_BACK);
        }

        /// <summary>
        /// Returns the oldest value in the queue (milliseconds since first button click), and then removes it.
        /// </summary>
        public uint PopClickedQueue()
        {
            var timeSinceFirstClick = TimeSinceFirstClick();

            var clickedQueue =
                new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.CLICKED_QUEUE_STATUS))
                {
                    PopRequest = true
                };
            _i2cBus.WriteSingleRegister(Register.CLICKED_QUEUE_STATUS, clickedQueue.QueueStatusValue);

            return timeSinceFirstClick;
        }

        #endregion

        #region LED Configuration

        /// <summary>
        /// Turns the onboard LED on with the specified brightness.
        /// <param name="brightness">LED brightness value between 0 (off) and 255 (max). Defaults to max.</param>
        /// </summary>
        public bool LedOn(byte brightness = 255)
        {
            return LedConfig(brightness, 0, 0);
        }

        /// <summary>
        /// Turns the onboard LED off.
        /// </summary>
        public bool LedOff()
        {
            return LedConfig(0, 0, 0);
        }

        /// <summary>
        /// Configures the onboard LED with the given max brightness, granularity, cycle time, and off time.
        /// <param name="brightness">LED brightness value between 0 (off) and 255 (max). Default is 255.</param>
        /// <param name="cycleTime">Total pulse cycle time (in ms). Does not include off time.</param>
        /// <param name="offTime">Off time between pulses (in ms). Default is 500 ms.</param>
        /// <param name="granularity">Amount of steps it takes to get to the set brightness level (1 is fine for most applications).</param>
        /// </summary>
        public bool LedConfig(byte brightness, ushort cycleTime, ushort offTime, byte granularity = 1)
        {
            bool success = _i2cBus.WriteSingleRegister(Register.LED_BRIGHTNESS, brightness);
            success &= _i2cBus.WriteSingleRegister(Register.LED_PULSE_GRANULARITY, granularity);
            success &= _i2cBus.WriteDoubleRegister(Register.LED_PULSE_CYCLE_TIME, cycleTime);
            success &= _i2cBus.WriteDoubleRegister(Register.LED_PULSE_OFF_TIME, offTime);
            return success;
        }

        #endregion

        /// <inheritdoc />
        public void Dispose()
        {
            _i2cBus?.Dispose();
            _i2cBus = null;
        }
    }
}
