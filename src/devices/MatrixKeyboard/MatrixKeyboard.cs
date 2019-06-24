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
    /// GPIO Matrix Keyboard Driver
    /// </summary>
    public class MatrixKeyboard : IDisposable
    {
        /// <summary>
        /// Get row pins
        /// </summary>
        public IEnumerable<int> RowPins => rowPins;

        /// <summary>
        /// Get column pins
        /// </summary>
        public IEnumerable<int> ColPins => colPins;

        /// <summary>
        /// Get or set scanning frequency in Hertz
        /// </summary>
        public double ScanFreq
        {
            get => 1000 / (scanTimer.Interval / colPins.Length);
            set => scanTimer.Interval = 1000 / (value * rowPins.Length);
        }

        /// <summary>
        /// Initialize keyboard
        /// </summary>
        /// <param name="gpioController">GPIO controller</param>
        /// <param name="rowPins">Row pins</param>
        /// <param name="colPins">Column pins</param>
        /// <param name="scanFreq">Scanning frequency</param>
        public MatrixKeyboard(IGpioController gpioController, IEnumerable<int> rowPins, IEnumerable<int> colPins, double scanFreq = 50)
        {
            gpio = gpioController;

            this.rowPins = rowPins.ToArray();
            this.colPins = colPins.ToArray();
            buttonValues = new PinValue[this.rowPins.Length * this.colPins.Length];

            scanTimer = new Timer(1000 / (scanFreq * this.rowPins.Length));
            scanTimer.Elapsed += ScanTimerElapsed;
        }

        /// <summary>
        /// Open GPIO pins then start keyboard scanning
        /// </summary>
        public void StartScan()
        {
            for (var i = 0; i < rowPins.Length; i++)
            {
                gpio.OpenPin(rowPins[i], PinMode.Output);
            }
            for (var i = 0; i < colPins.Length; i++)
            {
                gpio.OpenPin(colPins[i], PinMode.Input);
            }
            scanTimer.Start();
        }

        /// <summary>
        /// Stop keyboard scanning then close GPIO pins
        /// </summary>
        public void StopScan()
        {
            scanTimer.Stop();
            for (var i = 0; i < rowPins.Length; i++)
            {
                gpio.ClosePin(rowPins[i]);
            }
            for (var i = 0; i < colPins.Length; i++)
            {
                gpio.ClosePin(colPins[i]);
            }
        }

        /// <summary>
        /// Get all buttons' values
        /// </summary>
        public ReadOnlySpan<PinValue> Values => buttonValues.AsSpan();

        /// <summary>
        /// Get buttons' values by row
        /// </summary>
        /// <param name="row">Row index</param>
        public ReadOnlySpan<PinValue> RowValues(int row) => buttonValues.AsSpan(row * rowPins.Length, colPins.Length);

        /// <summary>
        /// Keyboard event
        /// </summary>
        public event PinChangeEventHandler PinChangeEvent;

        /// <summary>
        /// Delegate of keyboard event
        /// </summary>
        public delegate void PinChangeEventHandler(object sender, MatrixKeyboardEventArgs pinValueChangedEventArgs);

        public void Dispose()
        {
            scanTimer?.Dispose();
            scanTimer = null;

            gpio?.Dispose();
            gpio = null;
        }

        private readonly int[] rowPins;
        private readonly int[] colPins;
        private int currentRow;
        private readonly PinValue[] buttonValues;

        private IGpioController gpio;
        private Timer scanTimer;
        private void ScanTimerElapsed(object sender, ElapsedEventArgs e)
        {
            for (var i = 0; i < colPins.Length; i++)
            {
                var index = currentRow * colPins.Length + i;

                var oldValue = buttonValues[index];
                var newValue = gpio.Read(colPins[i]);

                buttonValues[index] = newValue;
                if (newValue != oldValue)
                {
                    var args = new MatrixKeyboardEventArgs(newValue == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling, currentRow, i);
                    PinChangeEvent(this, args);
                }
            }

            gpio.Write(rowPins[currentRow], PinValue.Low);
            currentRow = (currentRow + 1) % rowPins.Length;
            gpio.Write(rowPins[currentRow], PinValue.High);
        }
    }
}
