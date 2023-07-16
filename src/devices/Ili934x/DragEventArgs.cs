// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Ili934x
{
    /// <summary>
    /// Event arguments for dragging (moving the finger over the screen)
    /// </summary>
    public class DragEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs a new instance of <see cref="DragEventArgs"/>
        /// </summary>
        public DragEventArgs(bool isDragBegin, bool isDragEnd, Point lastPoint, Point currentPoint)
        {
            IsDragBegin = isDragBegin;
            IsDragEnd = isDragEnd;
            LastPoint = lastPoint;
            CurrentPoint = currentPoint;
        }

        /// <summary>
        /// True if the dragging is starting
        /// </summary>
        public bool IsDragBegin
        {
            get;
            init;
        }

        /// <summary>
        /// True if the user has stopped dragging (no longer touching the screen)
        /// </summary>
        public bool IsDragEnd
        {
            get;
            init;
        }

        /// <summary>
        /// The previous point
        /// </summary>
        public Point LastPoint
        {
            get;
            init;
        }

        /// <summary>
        /// The current point. When <see cref="IsDragEnd"/> is true, this is equal to the last point
        /// </summary>
        public Point CurrentPoint
        {
            get;
            init;
        }
    }
}
