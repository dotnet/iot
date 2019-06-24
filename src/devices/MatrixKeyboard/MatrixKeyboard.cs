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
    /// GPIO 矩阵键盘驱动
    /// </summary>
    public class MatrixKeyboard : IDisposable
    {
        /// <summary>
        /// 获取行扫描针脚
        /// </summary>
        public IEnumerable<int> RowPins => rowPins;

        /// <summary>
        /// 获取列扫描针脚
        /// </summary>
        public IEnumerable<int> ColPins => colPins;

        /// <summary>
        /// 获取或设置扫描频率
        /// </summary>
        public double ScanFreq
        {
            get => 1000 / (scanTimer.Interval / colPins.Length);
            set => scanTimer.Interval = 1000 / (value * rowPins.Length);
        }

        /// <summary>
        /// 创建矩阵键盘实例
        /// </summary>
        /// <param name="gpioController">GPIO 控制器</param>
        /// <param name="rowPins">行扫描针脚</param>
        /// <param name="colPins">列扫描针脚</param>
        /// <param name="scanFreq">扫描频率</param>
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
        /// 开启 GPIO 针脚并开始键盘扫描
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
        /// 停止键盘扫描并关闭 GPIO 针脚
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
        /// 获取键盘所有按钮的状态
        /// </summary>
        /// <returns>按钮状态列表，按行下标升序输出每一行</returns>
        public ReadOnlySpan<PinValue> Values => buttonValues.AsSpan();

        /// <summary>
        /// 获取指定行按钮的状态
        /// </summary>
        /// <param name="row">行下标</param>
        /// <returns>按钮的状态列表，按列下标升序输出选中的行</returns>
        public ReadOnlySpan<PinValue> RowValues(int row) => buttonValues.AsSpan(row * rowPins.Length, colPins.Length);

        /// <summary>
        /// 按钮事件
        /// </summary>
        public event PinChangeEventHandler PinChangeEvent;

        /// <summary>
        /// 按钮事件委托
        /// </summary>
        /// <param name="sender">事件发起方（本实例）</param>
        /// <param name="pinValueChangedEventArgs">事件参数</param>
        public delegate void PinChangeEventHandler(object sender, MatrixKeyboardEventArgs pinValueChangedEventArgs);

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            scanTimer.Dispose();
            gpio.Dispose();

            scanTimer = null;
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
