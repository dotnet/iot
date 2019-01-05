// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Iot.Device.Max7219
{

    /// <summary>
    /// Matrix text writer can be used to write 
    /// </summary>
    public class MatrixTextWriter
    {
        readonly Max7219 _device;

        public MatrixTextWriter(Max7219 device, Font font)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (font == null)
                throw new ArgumentNullException(nameof(font));
            _device = device;
            Font = font;
        }

        public Font Font { get; set; }

        /// <summary>
        /// Writes a char to the given device with the specified font.
        /// </summary>
        public void WriteLetter(int deviceId, char chr, bool flush = true)
        {
            var bitmap = Font[chr];
            var end = Math.Min(bitmap.Length, Max7219.NumDigits);
            for (int col = 0; col < end; col++)
            {
                _device[deviceId, col] = bitmap[col];
            }
            if (flush)
            {
                _device.Flush();
            }
        }

        /// <summary>
        ///  Scrolls the underlying buffer (for all cascaded devices) up one pixel
        /// </summary>
        public void ScrollUp(bool flush = true)
        {
            for (var i = 0; i < _device.Length; i++)
                _device[i] = (byte)(_device[i] >> 1);
            if (flush)
            {
                _device.Flush();
            }
        }

        /// <summary>
        /// Scrolls the underlying buffer (for all cascaded devices) down one pixel
        /// </summary>
        public void ScrollDown(bool flush = true)
        {
            for (var i = 0; i < _device.Length; i++)
            {
                _device[i] = (byte)((_device[i] << 1) & 0xff);
            }
            if (flush)
            {
                _device.Flush();
            }
        }

        /// <summary>
        /// Scrolls the underlying buffer (for all cascaded devices) to the left
        /// </summary>
        public void ScrollLeft(byte value, bool flush = true)
        {
            for (var i = 1; i < _device.Length; i++)
            {
                _device[i - 1] = _device[i];
            }
            _device[_device.Length - 1] = value;
            if (flush)
            {
                _device.Flush();
            }
        }

        /// <summary>
        /// Scrolls the underlying buffer (for all cascaded devices) to the right
        /// </summary>
        public void ScrollRight(byte value, bool flush = true)
        {
            for (var i = _device.Length - 1; i > 0; i--)
            {
                _device[i] = _device[i - 1];
            }
            _device[0] = value;
            if (flush)
            {
                _device.Flush();
            }
        }

        /// <summary>
        /// Shows a message on the device. 
        /// If it's longer then the total width (or <see paramref="alwaysScroll"/> == true), 
        /// it transitions the text message across the devices from right-to-left.
        /// </summary>
        public void ShowMessage(string text, int delayInMilliseconds = 50, bool alwaysScroll = false)
        {
            IEnumerable<byte[]> textCharBytes = text.Select(chr => Font[chr]);
            int textBytesLength = textCharBytes.Sum(x => x.Length) + text.Length - 1;

            bool scroll = alwaysScroll || textBytesLength > _device.Length;
            if (scroll)
            {
                var pos = _device.Length - 1;
                _device.ClearAll(false);
                foreach (byte[] arr in textCharBytes)
                {
                    foreach (byte b in arr)
                    {
                        ScrollLeft(b, true);
                        Thread.Sleep(delayInMilliseconds);

                    }
                    ScrollLeft(0, true);
                    Thread.Sleep(delayInMilliseconds);

                }
                for (; pos > 0; pos--)
                {
                    ScrollLeft(0, true);
                    Thread.Sleep(delayInMilliseconds);
                }
            }
            else
            {
                //calculate margin to display text centered
                var margin = (_device.Length - textBytesLength) / 2;
                _device.ClearAll(false);
                var pos = margin;
                foreach (byte[] arr in textCharBytes)
                {
                    foreach (byte b in arr)
                    {
                        _device[pos++] = b;
                    }
                    pos++;
                }
                _device.Flush();
            }
        }
    }
}