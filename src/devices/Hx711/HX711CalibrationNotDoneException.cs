namespace Iot.Device.HX711
{
    public class HX711CalibrationNotDoneException : Exception
    {
        private const string MESSAGE = "HX711 component need a calibration process first.";

        public HX711CalibrationNotDoneException()
        : base(message: MESSAGE) { }

        public HX711CalibrationNotDoneException(Exception inner)
            : base(message: MESSAGE, inner) { }
    }
}
