// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.QwiicButton.RegisterMapping;

namespace Iot.Device.QwiicButton
{
    public sealed partial class QwiicButton
    {
        /// <summary>
        /// Returns whether the queue of button press timestamps is full.
        /// </summary>
        public bool IsPressedQueueFull()
        {
            var pressedQueue = new QueueStatusBitField(_registerAccess.ReadRegister<byte>(Register.PressedQueueStatus));
            return pressedQueue.IsFull;
        }

        /// <summary>
        /// Returns whether the queue of button press timestamps is empty.
        /// </summary>
        public bool IsPressedQueueEmpty()
        {
            var pressedQueue = new QueueStatusBitField(_registerAccess.ReadRegister<byte>(Register.PressedQueueStatus));
            return pressedQueue.IsEmpty;
        }

        /// <summary>
        /// Returns interval of time since the last button press.
        /// Since this returns a <see cref="TimeSpan"/> based on a 32-bit unsigned int,
        /// it will roll over about every 50 days.
        /// If called when queue is empty then returns interval of time since the button was powered on.
        /// </summary>
        public TimeSpan GetTimeSinceLastPress()
        {
            var timeSinceLastPressInMs = _registerAccess.ReadRegister<uint>(Register.PressedQueueFront);
            return TimeSpan.FromMilliseconds(timeSinceLastPressInMs);
        }

        /// <summary>
        /// Returns interval of time since the first button press.
        /// Since this returns a <see cref="TimeSpan"/> based on a 32-bit unsigned int,
        /// it will roll over about every 50 days.
        /// If called when queue is empty then returns interval of time since the button was powered on.
        /// </summary>
        public TimeSpan GetTimeSinceFirstPress()
        {
            var timeSinceFirstPressInMs = _registerAccess.ReadRegister<uint>(Register.PressedQueueBack);
            return TimeSpan.FromMilliseconds(timeSinceFirstPressInMs);
        }

        /// <summary>
        /// Returns the oldest value in the queue of button press timestamps,
        /// i.e. the interval of time since the first button press, and then removes it.
        /// If called when queue is empty then returns interval of time since the button was powered on.
        /// </summary>
        public TimeSpan PopPressedQueue()
        {
            var timeSinceFirstPress = GetTimeSinceFirstPress();

            var pressedQueue =
                new QueueStatusBitField(_registerAccess.ReadRegister<byte>(Register.PressedQueueStatus))
                {
                    PopRequest = true
                };
            _registerAccess.WriteRegister(Register.PressedQueueStatus, pressedQueue.QueueStatusValue);

            return timeSinceFirstPress;
        }

        /// <summary>
        /// Returns whether the queue of button click timestamps is full.
        /// </summary>
        public bool IsClickedQueueFull()
        {
            var clickedQueue = new QueueStatusBitField(_registerAccess.ReadRegister<byte>(Register.ClickedQueueStatus));
            return clickedQueue.IsFull;
        }

        /// <summary>
        /// Returns whether the queue of button click timestamps is empty.
        /// </summary>
        public bool IsClickedQueueEmpty()
        {
            var clickedQueue = new QueueStatusBitField(_registerAccess.ReadRegister<byte>(Register.ClickedQueueStatus));
            return clickedQueue.IsEmpty;
        }

        /// <summary>
        /// Returns interval of time since the last button click.
        /// Since this returns a <see cref="TimeSpan"/> based on a 32-bit unsigned int,
        /// it will roll over about every 50 days.
        /// If called when queue is empty then returns interval of time since the button was powered on.
        /// </summary>
        public TimeSpan GetTimeSinceLastClick()
        {
            var timeSinceLastClickInMs = _registerAccess.ReadRegister<uint>(Register.ClickedQueueFront);
            return TimeSpan.FromMilliseconds(timeSinceLastClickInMs);
        }

        /// <summary>
        /// Returns interval of time since the first button click.
        /// Since this returns a <see cref="TimeSpan"/> based on a 32-bit unsigned int,
        /// it will roll over about every 50 days.
        /// If called when queue is empty then returns interval of time since the button was powered on.
        /// </summary>
        public TimeSpan GetTimeSinceFirstClick()
        {
            var timeSinceFirstClickInMs = _registerAccess.ReadRegister<uint>(Register.ClickedQueueBack);
            return TimeSpan.FromMilliseconds(timeSinceFirstClickInMs);
        }

        /// <summary>
        /// Returns the oldest value in the queue of button click timestamps,
        /// i.e. the interval of time since the first button click, and then removes it.
        /// If called when queue is empty then returns interval of time since the button was powered on.
        /// </summary>
        public TimeSpan PopClickedQueue()
        {
            var timeSinceFirstClick = GetTimeSinceFirstClick();

            var clickedQueue =
                new QueueStatusBitField(_registerAccess.ReadRegister<byte>(Register.ClickedQueueStatus))
                {
                    PopRequest = true
                };
            _registerAccess.WriteRegister(Register.ClickedQueueStatus, clickedQueue.QueueStatusValue);

            return timeSinceFirstClick;
        }
    }
}
