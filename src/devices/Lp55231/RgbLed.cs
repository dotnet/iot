// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Lp55231
{
    /// <summary>
    /// Represents an RgbLed on the Lp55231
    /// </summary>
    public class RgbLed
    {
        private readonly Action<byte, byte> _setIntensity;
        private readonly byte _instance;

        private byte _red;
        private byte _green;
        private byte _blue;

        internal RgbLed(byte instance, Action<byte, byte> setIntensity)
        {
            if (instance < 0 || instance > 2)
            {
                throw new IndexOutOfRangeException("Channel must be between 0 and 2");
            }

            if (setIntensity == null)
            {
                throw new ArgumentNullException(nameof(setIntensity));
            }

            _instance = instance;
            _setIntensity = setIntensity;
        }

        /// <summary>
        /// Gets/sets the red component of the RgbLed
        /// </summary>
        public byte Red
        {
            get => _red;
            set
            {
                if (_red != value)
                {
                    _red = value;

                    _setIntensity(Lp55231.RedChannel(_instance), _red);
                }
            }
        }

        /// <summary>
        /// Gets/sets the green component of the RgbLed
        /// </summary>
        public byte Green
        {
            get => _green;
            set
            {
                if (_green != value)
                {
                    _green = value;

                    _setIntensity(Lp55231.GreenChannel(_instance), _green);
                }
            }
        }

        /// <summary>
        /// Gets/sets the blue component of the RgbLed
        /// </summary>
        public byte Blue
        {
            get => _blue;
            set
            {
                if (_blue != value)
                {
                    _blue = value;

                    _setIntensity(Lp55231.BlueChannel(_instance), _blue);
                }
            }
        }
    }
}
