// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Common
{
    /// <summary>
    /// This class implements a histeresis filter on a boolean value.
    /// The output value will depend on the input and a configurable delay.
    /// This is useful to e.g. trigger an error only after an input value
    /// is high for at least a certain time.
    /// </summary>
    public class HysteresisFilter
    {
        private bool _currentValue;
        private bool _lastInputValue;
        private Stopwatch _tickCountLastChange;

        /// <summary>
        /// Creates a new histeresis filter with the given initial value
        /// </summary>
        /// <param name="initialValue">The initial value of the output</param>
        public HysteresisFilter(bool initialValue)
        {
            _currentValue = initialValue;
            _lastInputValue = initialValue;
            _tickCountLastChange = Stopwatch.StartNew();
            RisingDelayTime = TimeSpan.Zero;
            FallingDelayTime = TimeSpan.Zero;
        }

        /// <summary>
        /// The delay time for a raising input
        /// </summary>
        public TimeSpan RisingDelayTime
        {
            get;
            set;
        }

        /// <summary>
        /// The delay time for a falling input
        /// </summary>
        public TimeSpan FallingDelayTime
        {
            get;
            set;
        }

        /// <summary>
        /// The current output value. This will not change unless <see cref="Update"/> is called regularly.
        /// </summary>
        public bool Output => _currentValue;

        /// <summary>
        /// Updates the input value.
        /// This method must be called regularly (typically significantly more often than any of <see cref="FallingDelayTime"/> and
        /// <see cref="RisingDelayTime"/>) to update the input. The output will change when the input has changed for more
        /// than the delay time in either direction.
        /// </summary>
        /// <param name="newValue">The new input value</param>
        /// <returns>The current output value</returns>
        public bool Update(bool newValue)
        {
            // Reset clock when input value is different from output and we have not already started a wait
            if (newValue != _currentValue && newValue != _lastInputValue)
            {
                _tickCountLastChange.Restart();
            }

            _lastInputValue = newValue;

            if (newValue)
            {
                if (_tickCountLastChange.Elapsed >= RisingDelayTime)
                {
                    _currentValue = true;
                }
            }
            else
            {
                if (_tickCountLastChange.Elapsed >= FallingDelayTime)
                {
                    _currentValue = false;
                }
            }

            return _currentValue;
        }
    }
}
