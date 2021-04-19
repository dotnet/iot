using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino.Runtime
{
    [ArduinoReplacement(typeof(System.Math), false)]
    internal class MiniMath
    {
        [ArduinoImplementation(NativeMethod.MathCeiling)]
        public static double Ceiling(double a)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.MathFloor)]
        public static double Floor(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.MathPow)]
        public static double Pow(double x, double y)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.MathLog)]
        public static double Log(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.MathLog2)]
        public static double Log2(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.MathLog10)]
        public static double Log10(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.MathSin)]
        public static double Sin(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.MathCos)]
        public static double Cos(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.MathTan)]
        public static double Tan(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.MathSqrt)]
        public static double Sqrt(double d)
        {
            throw new NotImplementedException();
        }
    }
}
