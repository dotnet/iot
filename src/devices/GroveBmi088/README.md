# Grove 6-Axis Accelerometer & Gyroscope
The Grove - 6-Axis Accelerometer & Gyroscope (BMI088) is a 6 DoF (degrees of freedom) High-performance Inertial Measurement Unit (IMU).
This sensor is based on BOSCH BMI088, which is a high-performance IMU with high vibration suppression. The 6-axis sensor combines a 16 bit 
triaxial gyroscope and a 16 bit triaxial accelerometer.

## Usage
```C#
using (var sensor = new GroveBmi088())
{
    // read acceleration
    Vector3 acceleration = sensor.GetAccelerometer();
    // read rotation
    Vector3 rotation = sensor.GetGyroscope();
}

```

## References
Based on the C++ implementation on https://github.com/Seeed-Studio/Grove_6Axis_Accelerometer_And_Gyroscope_BMI088
Product page: https://www.seeedstudio.com/Grove-6-Axis-Accelerometer-Gyroscope-BMI088.html
Data sheet: https://github.com/SeeedDocument/Grove-6-Axis_Accelerometer-Gyroscope-BMI088/raw/master/res/BMI088.pdf
