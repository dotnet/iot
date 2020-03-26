namespace Iot.Device.Ad7193
{
    /// <summary>
    /// AD7193 Register Map
    /// </summary>
    public enum Register : byte
    {
        Communications = 0,     // Communications Register (WO, 8-bit)
        Status = 0,             // Status Register         (RO, 8-bit)
        Mode = 1,               // Mode Register           (RW, 24-bit)
        Configuration = 2,      // Configuration Register  (RW, 24-bit)
        Data = 3,               // Data Register           (RO, 24/32-bit)
        ID = 4,                 // ID Register             (RO, 8-bit)
        GPOCON = 5,             // GPOCON Register         (RW, 8-bit)
        Offset = 6,             // Offset Register         (RW, 24-bit)
        FullScale = 7,          // Full-Scale Register     (RW, 24-bit)
    }
}
