//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace Iot.Device.QwiicButton
{
    [Flags]
    internal enum StatusRegister
    {
        None = 0,
        EventAvailable = 1,
        HasBeenClicked = 2,
        IsPressed = 4,
    }

    internal class StatusRegisterBitField2
    {
        public StatusRegisterBitField2(byte statusRegisterValue)
        {
            StatusRegisterValue = statusRegisterValue;
        }

        public byte StatusRegisterValue { get; }

        public bool EventAvailable
        {
            get { return (((StatusRegister)StatusRegisterValue) & StatusRegister.EventAvailable) != StatusRegister.None; }
        }

        public bool IsPressed
        {
            get { return (((StatusRegister)StatusRegisterValue) & StatusRegister.IsPressed) != StatusRegister.None; }
        }

        public bool HasBeenClicked
        {
            get { return (((StatusRegister)StatusRegisterValue) & StatusRegister.HasBeenClicked) != StatusRegister.None; }
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct StatusRegisterBitField
    {
        /// <summary>
        /// User mutable, gets set to 1 when a new event occurs. User is expected to write 0 to clear the flag.
        /// </summary>
        [FieldOffset(0)]
        public bool EventAvailable;

        /// <summary>
        /// Defaults to zero on POR. Gets set to one when the button gets clicked. Must be cleared by the user.
        /// </summary>
        [FieldOffset(1)]
        public bool HasBeenClicked;

        /// <summary>
        /// Gets set to one if button is pushed.
        /// </summary>
        [FieldOffset(2)]
        public bool IsPressed;

        /// <summary>
        /// TODO
        /// </summary>
        [FieldOffset(0)]
        public byte ByteWrapped;
    }

    /// <summary>
    /// TODO
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct InterruptConfigBitField
    {
        /// <summary>
        /// User mutable, set to 1 to enable an interrupt when the button is clicked.
        /// </summary>
        [FieldOffset(0)]
        public bool ClickedEnable;

        /// <summary>
        /// User mutable, set to 1 to enable an interrupt when the button is pressed.
        /// </summary>
        [FieldOffset(1)]
        public bool PressedEnable;

        /// <summary>
        /// TODO
        /// </summary>
        [FieldOffset(0)]
        public byte ByteWrapped;
    }

    /// <summary>
    /// TODO
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct QueueStatusBitField
    {
        /// <summary>
        /// User mutable, user sets to 1 to pop from queue, we pop from queue and set the bit back to zero.
        /// </summary>
        [FieldOffset(0)]
        public bool PopRequest;

        /// <summary>
        /// User immutable, returns 1 or 0 depending on whether or not the queue is empty
        /// </summary>
        [FieldOffset(1)]
        public bool IsEmpty;

        /// <summary>
        /// User immutable, returns 1 or 0 depending on whether or not the queue is full
        /// </summary>
        [FieldOffset(2)]
        public bool IsFull;

        /// <summary>
        /// TODO
        /// </summary>
        [FieldOffset(0)]
        public byte ByteWrapped;
    }
}
