//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.QwiicButton.RegisterMapping
{
    internal struct QueueStatusBitField
    {
        [Flags]
        private enum QueueStatusBits
        {
            PopRequest = 1,
            IsEmpty = 2,
            IsFull = 4,
        }

        private QueueStatusBits _queueStatusValue;

        public QueueStatusBitField(byte queueStatusValue)
        {
            _queueStatusValue = (QueueStatusBits)queueStatusValue;
        }

        public byte QueueStatusValue => (byte)_queueStatusValue;

        /// <summary>
        /// Set to true to pop from the queue.
        /// After the value is popped from the queue, this flag is set back to false.
        /// </summary>
        public bool PopRequest
        {
            get { return FlagsHelper.IsSet(_queueStatusValue, QueueStatusBits.PopRequest); }
            set { FlagsHelper.SetValue(ref _queueStatusValue, QueueStatusBits.PopRequest, value); }
        }

        /// <summary>
        /// Returns whether the queue is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return FlagsHelper.IsSet(_queueStatusValue, QueueStatusBits.IsEmpty); }
        }

        /// <summary>
        /// Returns whether the queue is full.
        /// </summary>
        public bool IsFull
        {
            get { return FlagsHelper.IsSet(_queueStatusValue, QueueStatusBits.IsFull); }
        }
    }
}
