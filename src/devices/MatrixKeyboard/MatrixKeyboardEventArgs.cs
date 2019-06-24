// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Timers;

namespace Iot.Device.MatrixKeyboard
{
    /// <summary>
    /// 按钮事件
    /// </summary>
    public class MatrixKeyboardEventArgs : EventArgs
    {
        /// <summary>
        /// 按钮事件类型。Rising 为按钮按下，Falling 为按钮弹起
        /// </summary>
        public PinEventTypes EventType;

        /// <summary>
        /// 按钮所在行下标
        /// </summary>
        public int Row;

        /// <summary>
        /// 按钮所在列下标
        /// </summary>
        public int Column;

        internal MatrixKeyboardEventArgs(PinEventTypes eventType, int row, int column)
        {
            EventType = eventType;
            Row = row;
            Column = column;
        }
    }
}
