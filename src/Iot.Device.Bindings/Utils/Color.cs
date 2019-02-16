using System.Runtime.InteropServices;

namespace Iot.Device.Bindings.Utils
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Color
    {
        public Color(byte r, byte g, byte b, byte a = 0xff) : this()
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public Color(byte level) : this(level, level, level) { }

        public Color(uint argb) : this() { ARGB = argb; }

        public static explicit operator Color(uint value) => new Color(value);
        public static explicit operator uint(Color c) => c.ARGB;

        public static Color operator /(Color c, int div)
            => new Color((byte)(c.R / div), (byte)(c.G / div), (byte)(c.B / div), c.A);

        public static Color operator *(Color c, int mul)
            => new Color((byte)(c.R * mul), (byte)(c.G * mul), (byte)(c.B * mul), c.A);

        public static Color operator <<(Color c, int shift)
            => new Color((byte)(c.R << shift), (byte)(c.G << shift), (byte)(c.B << shift), c.A);

        public static Color operator >>(Color c, int shift)
            => new Color((byte)(c.R >> shift), (byte)(c.G >> shift), (byte)(c.B >> shift), c.A);

        [FieldOffset(0)]
        public byte B;
        [FieldOffset(1)]
        public byte G;
        [FieldOffset(2)]
        public byte R;
        [FieldOffset(3)]
        public byte A;
        [FieldOffset(0)]
        public uint ARGB;

        public static readonly Color Black   = new Color(0, 0, 0);
        public static readonly Color White   = new Color(255, 255, 255);
        public static readonly Color Red     = new Color(255, 0, 0);
        public static readonly Color Green   = new Color(0, 255, 0);
        public static readonly Color Blue    = new Color(0, 0, 255);
        public static readonly Color Yellow  = new Color(255, 255, 0);
        public static readonly Color Cyan    = new Color(0, 255, 255);
        public static readonly Color Magenta = new Color(255, 0, 255);

        public override string ToString() => ARGB.ToString("x8");
    }
}
