//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

namespace Iot.Device.QwiicButton
{
    public partial class QwiicButton
    {
        /// <summary>
        /// Returns whether the queue of button press timestamps is full.
        /// </summary>
        public bool IsPressedQueueFull()
        {
            var pressedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.PressedQueueStatus));
            return pressedQueue.IsFull;
        }

        /// <summary>
        /// Returns whether the queue of button press timestamps is empty.
        /// </summary>
        public bool IsPressedQueueEmpty()
        {
            var pressedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.PressedQueueStatus));
            return pressedQueue.IsEmpty;
        }

        /// <summary>
        /// Returns how many milliseconds it has been since the last button press.
        /// Since this returns a 32-bit unsigned int, it will roll over about every 50 days.
        /// </summary>
        public uint TimeSinceLastPress()
        {
            return _i2cBus.ReadQuadRegister(Register.PressedQueueFront);
        }

        /// <summary>
        /// Returns how many milliseconds it has been since the first button press.
        /// Since this returns a 32-bit unsigned int, it will roll over about every 50 days.
        /// </summary>
        public uint TimeSinceFirstPress()
        {
            return _i2cBus.ReadQuadRegister(Register.PressedQueueBack);
        }

        /// <summary>
        /// Returns the oldest value in the queue (milliseconds since first button press), and then removes it.
        /// </summary>
        public uint PopPressedQueue()
        {
            var timeSinceFirstPress = TimeSinceFirstPress(); // Take the oldest value on the queue

            var pressedQueue =
                new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.PressedQueueStatus))
                {
                    PopRequest = true
                };

            // Remove the oldest value from the queue
            _i2cBus.WriteSingleRegister(Register.PressedQueueStatus, pressedQueue.QueueStatusValue);

            return timeSinceFirstPress; // Return the value we popped
        }

        /// <summary>
        /// Returns whether the queue of button click timestamps is full.
        /// </summary>
        public bool IsClickedQueueFull()
        {
            var clickedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.ClickedQueueStatus));
            return clickedQueue.IsFull;
        }

        /// <summary>
        /// Returns whether the queue of button click timestamps is empty.
        /// </summary>
        public bool IsClickedQueueEmpty()
        {
            var clickedQueue = new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.ClickedQueueStatus));
            return clickedQueue.IsEmpty;
        }

        /// <summary>
        /// Returns how many milliseconds it has been since the last button click.
        /// Since this returns a 32-bit unsigned int, it will roll over about every 50 days.
        /// </summary>
        public uint TimeSinceLastClick()
        {
            return _i2cBus.ReadQuadRegister(Register.ClickedQueueFront);
        }

        /// <summary>
        /// Returns how many milliseconds it has been since the first button click.
        /// Since this returns a 32-bit unsigned int, it will roll over about every 50 days.
        /// </summary>
        public uint TimeSinceFirstClick()
        {
            return _i2cBus.ReadQuadRegister(Register.ClickedQueueBack);
        }

        /// <summary>
        /// Returns the oldest value in the queue (milliseconds since first button click), and then removes it.
        /// </summary>
        public uint PopClickedQueue()
        {
            var timeSinceFirstClick = TimeSinceFirstClick();

            var clickedQueue =
                new QueueStatusBitField(_i2cBus.ReadSingleRegister(Register.ClickedQueueStatus))
                {
                    PopRequest = true
                };
            _i2cBus.WriteSingleRegister(Register.ClickedQueueStatus, clickedQueue.QueueStatusValue);

            return timeSinceFirstClick;
        }
    }
}
