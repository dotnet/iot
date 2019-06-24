// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Linq;

namespace Iot.Device.MatrixKeyboard.Samples
{
    /// <summary>
    /// This sample is for Raspberry Pi Model 3B+
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            //  获取初始化参数
            Console.Write("input row pins(eg. 27,23,24,10) ");
            var r = Console.ReadLine().Split(',').Select(m => int.Parse(m));
            Console.Write("input column pins(eg. 15,14,3,2) ");
            var c = Console.ReadLine().Split(',').Select(m => int.Parse(m));
            Console.Write("input scaning frequency(eg. 50) ");
            var f = double.Parse(Console.ReadLine());

            //  初始化矩阵键盘
            var gpio = new GpioController();
            var mk = new MatrixKeyboard(gpio, r, c, f);
            mk.PinChangeEvent += Mk_PinChangeEvent;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press any key to start. Then stop. Then dispose. Then exit. ");

            //  启动键盘扫描
            Console.ReadKey();
            mk.StartScan();
            Console.WriteLine("MatrixKeyboard.StartScan() ");

            //  终止键盘扫描
            Console.ReadKey();
            mk.StopScan();
            Console.WriteLine("MatrixKeyboard.StopScan() ");

            //  释放资源
            Console.ReadKey();
            mk.Dispose();
            Console.WriteLine("MatrixKeyboard.Dispose() ");

            //  退出程序
            Console.ReadKey();
        }

        /// <summary>
        /// 键盘事件处理
        /// </summary>
        /// <param name="sender">事件发起方（键盘实例）</param>
        /// <param name="pinValueChangedEventArgs">事件参数</param>
        private static void Mk_PinChangeEvent(object sender, MatrixKeyboardEventArgs pinValueChangedEventArgs)
        {
            //  清空屏幕
            Console.Clear();

            //  输出事件
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")} {pinValueChangedEventArgs.Row}, {pinValueChangedEventArgs.Column}, {pinValueChangedEventArgs.EventType}");
            Console.WriteLine();

            //  输出键盘状态
            var s = (MatrixKeyboard)sender;
            for (var r = 0; r < s.RowPins.Count(); r++)
            {
                var rv = s.RowValues(r);
                for (var c = 0; c < s.ColPins.Count(); c++)
                {
                    Console.Write(rv[c] == PinValue.Low ? " ." : " #");
                }
                Console.WriteLine();
            }
        }
    }
}
