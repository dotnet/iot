using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Math), false)]
    internal class MiniMath
    {
        [ArduinoImplementation("MathCeiling")]
        public static double Ceiling(double a)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("MathFloor")]
        public static double Floor(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("MathPow")]
        public static double Pow(double x, double y)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("MathLog")]
        public static double Log(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("MathLog2")]
        public static double Log2(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("MathLog10")]
        public static double Log10(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("MathSin")]
        public static double Sin(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("MathCos")]
        public static double Cos(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("MathTan")]
        public static double Tan(double d)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("MathSqrt")]
        public static double Sqrt(double d)
        {
            return Math.Sqrt(d);
        }

        [ArduinoImplementation("MathExp")]
        public static double Exp(double d)
        {
            return Math.Exp(d);
        }

        [ArduinoImplementation("MathAbs")]
        public static double Abs(double d)
        {
            return Math.Abs(d);
        }
    }
}
