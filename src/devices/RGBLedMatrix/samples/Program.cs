// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.LEDMatrix;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;

bool play = false;
int scenario = 2;
string weatherKey = "Please request a key from openweathermap.org";

CityData[] citiesData = new CityData[]
{
    new("New York", "US", "America/New_York"),
    new("Redmond", "US", "America/Los_Angeles"),
    new("Toronto", "CA", "America/Toronto"),
    new("Mexico", "MX", "America/Mexico_City"),
    new("Madrid", "ES", "Europe/Madrid"),
    new("London", "UK", "Europe/London"),
    new("Paris", "FR", "Europe/Paris"),
    new("Rome", "IT", "Europe/Rome"),
    new("Moscow", "RU", "Europe/Moscow"),
    new("Casablanca", "MA", "Africa/Casablanca"),
    new("Cairo", "EG", "Africa/Cairo"),
    new("Riyadh", "SA", "Asia/Riyadh")
};

SkiaSharpAdapter.Register();

Console.WriteLine($"Hello Matrix World!");

// If using 64x64 with Bonnet (https://www.adafruit.com/product/3211), you can just do
// PinMapping mapping = PinMapping.MatrixBonnetMapping64;

// If using 32x32 with Bonnet (https://www.adafruit.com/product/3211), you can just do
PinMapping mapping = PinMapping.MatrixBonnetMapping32;

// If not using Bonnet, will need to provide the manual GPIO mapping using PinMapping

// To create RGBLedMatrix for 32x32 panel, do the following
RGBLedMatrix matrix = new RGBLedMatrix(mapping, 32, 32);

// To create RGBLedMatrix for 64x64 panel, do the following
// RGBLedMatrix matrix = new RGBLedMatrix(mapping, 64, 64);

// PinMapping mapping = PinMapping.MatrixBonnetMapping64;
// RGBLedMatrix matrix = new RGBLedMatrix(mapping, 64, 64);

// If you chain 4 32x32 panels serially, you can do
// RGBLedMatrix matrix = new RGBLedMatrix(mapping, 128, 32);

// If you chain 4 32x32 panels having 2 rows chaining (2 panels in first row an d2 panels in second row).
// RGBLedMatrix matrix = new RGBLedMatrix(mapping, 64, 64, 2, 2);
_ = Task.Run(() =>
{
    matrix.StartRendering();

    while (scenario != 0)
    {
        switch (scenario)
        {
            case 1: Demo1(matrix); break;
            case 2: Demo2(matrix); break;
            case 3: Demo3(matrix); break;
            case 4: Demo4(matrix); break;
            case 5: Demo5(matrix); break;
            case 6: Demo6(matrix); break;
            case 7: Demo7(matrix); break;
            case 8: Demo8(matrix); break;
            default:
                scenario = 2;
                break;
        }
    }
});

ConsoleKeyInfo cki;
Console.WriteLine($"Press q to exit.");
Interop.ThreadHelper.SetCurrentThreadHighPriority();

do
{
    cki = Console.ReadKey();

    if (cki.KeyChar == '+')
    {
        matrix.PWMDuration = matrix.PWMDuration + 100;
        Console.WriteLine($"     ({matrix.PWMDuration})");
    }

    if (cki.KeyChar == '-')
    {
        matrix.PWMDuration = matrix.PWMDuration - 100;
        Console.WriteLine($"     ({matrix.PWMDuration})");
    }

    if (cki.KeyChar == 'f')
    {
        Console.WriteLine($"Frame Time: {matrix.FrameTime} \u00B5s");
        Console.WriteLine($"Duration : {matrix.PWMDuration}");
    }

    if (cki.KeyChar >= '1' && cki.KeyChar <= '9')
    {
        play = false;
        scenario = cki.KeyChar - '0';
        Thread.Sleep(1000);
    }
}
while (cki.KeyChar != 'q');

play = false;
scenario = 0;
Thread.Sleep(1000);
matrix.Dispose();

unsafe void Demo1(RGBLedMatrix matrix)
{
    play = true;

    try
    {
        BdfFont font = BdfFont.Load(@"fonts/10x20.bdf");
        BdfFont font1 = BdfFont.Load(@"fonts/8x13B.bdf");
        matrix.Fill(0, 0, 0);
        Thread.Sleep(100);

        int x = matrix.Width - 1;
        string text = "Hello .NET IoT";
        int fullTextWidth = text.Length * font.Width;

        while (play)
        {
            matrix.DrawText(x, 0, text, font, 0, 0, 255, 0, 0, 0);
            x--;
            if (x == -fullTextWidth)
            {
                x = matrix.Width - 1;
            }

            string d = DateTime.Now.ToString("hh:mm:ss");
            matrix.DrawText(0, font.Height + 1, d, font1, 128, 128, 0, 0, 0, 0);

            Thread.Sleep(25);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

void Demo2(RGBLedMatrix matrix)
{
    int length = matrix.Width / 4;
    int height = matrix.Height / 4;

    play = true;
    while (play)
    {
        matrix.FillRectangle(0, 0, length, length, 255, 0, 0);
        matrix.FillRectangle(length, 0, length, length, 0, 255, 0);
        matrix.FillRectangle(2 * length, 0, length, length, 0, 0, 255);
        matrix.FillRectangle(3 * length, 0, length, length, 255, 255, 0);

        matrix.FillRectangle(0, height, length, length, 255, 0, 255);
        matrix.FillRectangle(length, height, length, length, 255, 255, 255);
        matrix.FillRectangle(2 * length, height, length, length, 0, 130, 0);
        matrix.FillRectangle(3 * length, height, length, length, 130, 0, 0);

        matrix.FillRectangle(0, 2 * height, length, length, 0, 0, 128);
        matrix.FillRectangle(length, 2 * height, length, length, 192, 192, 192);
        matrix.FillRectangle(2 * length, 2 * height, length, length, 128, 128, 0);
        matrix.FillRectangle(3 * length, 2 * height, length, length, 128, 128, 128);

        matrix.FillRectangle(0, 3 * height, length, length, 40, 40, 40);
        matrix.FillRectangle(length, 3 * height, length, length, 80, 80, 80);
        matrix.FillRectangle(2 * length, 3 * height, length, length, 120, 120, 120);
        matrix.FillRectangle(3 * length, 3 * height, length, length, 0, 120, 120);

        Thread.Sleep(5000);
    }
}

void Demo3(RGBLedMatrix matrix)
{
    try
    {
        play = true;

        byte blue = 0x10;
        matrix.Fill(0, 0, blue);

        TimeZoneInfo[] zones = new TimeZoneInfo[citiesData.Length];
        string[] weatherUrls = new string[citiesData.Length];
        for (int i = 0; i < citiesData.Length; i++)
        {
            weatherUrls[i] = String.Format("http://api.openweathermap.org/data/2.5/weather?q={0},{1}&mode=xml&units=imperial&APPID={2}", citiesData[i].City, citiesData[i].CountryCode, weatherKey);
            zones[i] = TimeZoneInfo.FindSystemTimeZoneById(citiesData[i].ZoneId);
        }

#pragma warning disable SYSLIB0014 // Type or member is obsolete
        using (WebClient client = new WebClient())
        {
            BdfFont font = BdfFont.Load(@"fonts/6x12.bdf");
            BdfFont font1 = BdfFont.Load(@"fonts/5x7.bdf");
            BdfFont font2 = BdfFont.Load(@"fonts/4x6.bdf");

            int cityIndex = 0;

            while (play)
            {
                string xml = client.DownloadString(weatherUrls[cityIndex]);
                XDocument doc = XDocument.Parse(xml);
                XElement? root = doc.Root;
                XElement? element = root?.Element("temperature");
                string tempString = element?.Attribute("value")?.Value ?? "0";
                int tempValue = ((int)Math.Round(Double.Parse(tempString)));
                string temperature = tempValue.ToString(CultureInfo.InvariantCulture);

                element = doc.Root?.Element("weather");
                string description = element?.Attribute("value")?.Value + "                                           ";

                element = doc.Root?.Element("humidity");
                string humidity = element?.Attribute("value")?.Value + element?.Attribute("unit")?.Value + "             ";
                element = doc.Root?.Element("city")?.Element("sun");
                string rise = element?.Attribute("rise")?.Value ?? DateTime.UtcNow.ToString();
                string set = element?.Attribute("set")?.Value ?? DateTime.UtcNow.ToString();
                string sunRise = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(rise, CultureInfo.InvariantCulture), zones[cityIndex]).ToString("hh:mm tt");
                string sunSet = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(set, CultureInfo.InvariantCulture), zones[cityIndex]).ToString("hh:mm tt");

                int pos = Math.Max(0, (matrix.Width - (citiesData[cityIndex].City.Length * font.Width))) / 2;
                ScrollText(
                    matrix,
                    citiesData[cityIndex].City,
                    font,
                    matrix.Width - 1,
                    pos,
                    128, 128, 128, 0, 0, blue);

                int y = font.Height;
                matrix.DrawText((matrix.Width - (temperature.Length + 1) * font1.Width) / 2, y, temperature + "\u00B0", font1, 255, 255, 0, 0, 0, blue);

                y += font1.Height + 2;
                matrix.DrawText(2, y, description, font2, 128, 128, 128, 0, 0, blue);

                y += font2.Height + 2;
                matrix.DrawText(2, y, "humidity: ", font2, 128, 128, 128, 0, 0, blue);
                matrix.DrawText(font2.Width * "humidity: ".Length + 2, y, humidity, font2, 255, 255, 0, 0, 0, blue);

                y += font2.Height;
                string localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zones[cityIndex]).ToString("hh:mm tt");
                matrix.DrawText((matrix.Width - localTime.Length * font.Width) / 2, y, localTime, font, 255, 255, 0, 0, 0, blue);

                y += font.Height + 2;
                matrix.DrawText(2, y, "Sun Rise: ", font2, 128, 128, 128, 0, 0, blue);
                matrix.DrawText(2 + "Sun Rise: ".Length * font2.Width, y, sunRise, font2, 255, 255, 0, 0, 0, blue);

                y += font2.Height + 2;
                matrix.DrawText(2, y, "Sun Set:  ", font2, 128, 128, 128, 0, 0, blue);
                matrix.DrawText(2 + "Sun Set:  ".Length * font2.Width, y, sunSet, font2, 255, 255, 0, 0, 0, blue);

                Thread.Sleep(4000);

                ScrollText(
                    matrix,
                    citiesData[cityIndex].City,
                    font,
                    pos,
                    -(pos + citiesData[cityIndex].City.Length * font.Width),
                    128, 128, 128, 0, 0, blue);

                y = font.Height;
                matrix.DrawText((matrix.Width - (temperature.Length + 1) * font1.Width) / 2, y, temperature + "\u00B0", font1, 0, 0, blue, 0, 0, blue);

                y += font1.Height + 2;
                matrix.DrawText(2, y, description, font2, 0, 0, blue, 0, 0, blue);

                y += font2.Height + 2;
                matrix.DrawText(2, y, "humidity: ", font2, 0, 0, blue, 0, 0, blue);
                matrix.DrawText(font2.Width * "humidity: ".Length + 2, y, humidity, font2, 0, 0, blue, 0, 0, blue);

                y += font2.Height;
                matrix.DrawText((matrix.Width - localTime.Length * font.Width) / 2, y, localTime, font, 0, 0, blue, 0, 0, blue);

                y += font.Height + 2;
                matrix.DrawText(2, y, "Sun Rise: ", font2, 0, 0, blue, 0, 0, blue);
                matrix.DrawText(2 + "Sun Rise: ".Length * font2.Width, y, sunRise, font2, 0, 0, blue, 0, 0, blue);

                y += font2.Height + 2;
                matrix.DrawText(2, y, "Sun Set:  ", font2, 0, 0, blue, 0, 0, blue);
                matrix.DrawText(2 + "Sun Set:  ".Length * font2.Width, y, sunSet, font2, 0, 0, blue, 0, 0, blue);

                cityIndex = (cityIndex + 1) % citiesData.Length;
            }
        }
#pragma warning restore SYSLIB0014 // Type or member is obsolete
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

unsafe void Demo4(RGBLedMatrix matrix)
{
    play = true;

    byte blue = 0x15;
    matrix.Fill(0, 0, blue);

#pragma warning disable SYSLIB0014 // Type or member is obsolete
    using WebClient client = new WebClient();
#pragma warning restore SYSLIB0014 // Type or member is obsolete
    int lastMinute = -1;
    string temperature = string.Empty;

    BdfFont font = BdfFont.Load(@"fonts/6x12.bdf");
    BdfFont font1 = BdfFont.Load(@"fonts/5x7.bdf");

    BitmapImage? weatherIcon = null;
    BitmapImage defaultIcon = BitmapImage.CreateFromFile("bitmaps/01d.bmp");
    string? lastIcon = null;
    string description = string.Empty;

    while (play)
    {
        DateTime time = DateTime.Now;
        if (Math.Abs(time.Minute - lastMinute) > 4)
        {
            lastMinute = time.Minute;
            string xml = client.DownloadString("http://api.openweathermap.org/data/2.5/weather?q=Redmond,US&mode=xml&units=imperial&APPID=" + weatherKey);

            XDocument doc = XDocument.Parse(xml);
            XElement? element = doc.Root?.Element("temperature");
            string tempValue = element?.Attribute("value")?.Value ?? "0";
            temperature = ((int)Math.Round(Double.Parse(tempValue))).ToString(CultureInfo.InvariantCulture);

            element = doc.Root?.Element("weather");
            string? icon = element?.Attribute("icon")?.Value;
            description = element?.Attribute("value")?.Value ?? "Missing description";

            if (lastIcon != icon)
            {
                weatherIcon = BitmapImage.CreateFromFile("bitmaps/" + icon ?? defaultIcon + ".bmp");
            }

            matrix.DrawBitmap(20, 2, weatherIcon ?? defaultIcon, 255, 255, 255, 0, 0, blue);
            matrix.DrawText(Math.Max(0, matrix.Width - description.Length * font1.Width), 42, description, font1, 128, 128, 128, 0, 0, blue);
            matrix.DrawText(2, 2 + font.Height, temperature + "\u00B0", font, 128, 128, 128, 0, 0, blue);
        }

        matrix.DrawText(2, 2, time.ToString("ddd"), font, 128, 128, 128, 0, 0, blue);
        matrix.DrawText(2, matrix.Height - font.Height, time.ToString("hh:mm:sstt"), font, 128, 128, 128, 0, 0, blue);

        Thread.Sleep(200);
    }
}

void Demo5(RGBLedMatrix matrix)
{
    play = true;

    try
    {
        var sw = Stopwatch.StartNew();
        while (play)
        {
            float time = sw.ElapsedMilliseconds / 1000f;
            for (int ix = 0; ix < matrix.Width; ix++)
            {
                for (int iy = 0; iy < matrix.Height; iy++)
                {
                    Vector2 uv = new Vector2(ix / (float)(matrix.Width - 1), iy / (float)(matrix.Height - 1));
                    Vector3 cv = HSV(uv, time);
                    Color c = ColorFromVec3(cv);
                    matrix.SetPixel(ix, iy, c.R, c.G, c.B);
                }
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

unsafe void Demo6(RGBLedMatrix matrix)
{
    play = true;

    try
    {
        matrix.Fill(0, 0, 0);

        BitmapImage[] bitmaps = new BitmapImage[]
        {
            BitmapImage.CreateFromFile(@"bitmaps/dotnet-bot-branded-32x32.bmp"),
            BitmapImage.CreateFromFile(@"bitmaps/i-love-dotnet.bmp")
        };

        int x = matrix.Width - 1;
        int bitmapIndex = 0;
        while (play)
        {
            matrix.DrawBitmap(x, 0, bitmaps[bitmapIndex]);

            if (x + bitmaps[bitmapIndex].Width < matrix.Width)
            {
                matrix.FillRectangle(x + bitmaps[bitmapIndex].Width, 0, matrix.Width - x - bitmaps[bitmapIndex].Width, matrix.Height, 0, 0, 0);
            }

            x--;

            if (x == -bitmaps[bitmapIndex].Width)
            {
                bitmapIndex = (bitmapIndex + 1) % bitmaps.Length;
                x = matrix.Width - 1;
            }

            Thread.Sleep(25);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

unsafe void Demo7(RGBLedMatrix matrix)
{
    play = true;

    try
    {
        matrix.Fill(0, 0, 0);

        while (play)
        {
            matrix.SetPixel(matrix.Width / 2, matrix.Height / 2, 255, 0, 0);
            matrix.DrawCircle(matrix.Width / 2, matrix.Height / 2, 14, 255, 0, 0);
            matrix.DrawCircle(matrix.Width / 2, matrix.Height / 2, 9, 0, 255, 0);
            matrix.DrawCircle(matrix.Width / 2, matrix.Height / 2, 6, 0, 0, 255);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

void Demo8(RGBLedMatrix matrix)
{
    play = true;

    try
    {
        var sw = Stopwatch.StartNew();
        while (play)
        {
            float time = sw.ElapsedMilliseconds / 1000f;
            for (int ix = 0; ix < matrix.Width; ix++)
            {
                for (int iy = 0; iy < matrix.Height; iy++)
                {
                    Vector2 uv = new Vector2(ix / (float)(matrix.Width - 1), iy / (float)(matrix.Height - 1));
                    Vector3 cv = Star(uv, time);
                    Color c = ToSRGB(cv.X, cv.Y, cv.Z);
                    matrix.SetPixel(ix, iy, c.R, c.G, c.B);
                }
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

Vector3 Clamp(Vector3 c, float a, float b)
{
    return new Vector3(Math.Clamp(c.X, a, b), Math.Clamp(c.Y, a, b), Math.Clamp(c.Z, a, b));
}

float ModF(float a, float b)
{
    float ret = a % b;
    if (ret < 0)
    {
        ret += b;
    }

    return ret;
}

Vector3 Mod(Vector3 a, float b)
{
    return new Vector3(ModF(a.X, b), ModF(a.Y, b), ModF(a.Z, b));
}

Vector3 Add(Vector3 v, float s)
{
    return new Vector3(v.X + s, v.Y + s, v.Z + s);
}

Vector3 Abs(Vector3 vector)
{
    return new Vector3(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));
}

Vector3 Mix(Vector3 a, Vector3 b, float f)
{
    return a * (1 - f) + b * f;
}

Vector3 Hsv2rgb_smooth(Vector3 c)
{
    float c1 = c.X + 6.0f;

    Vector3 v1 = Add(new Vector3(0.0f, 4.0f, 2.0f), c.X * 6.0f);
    Vector3 rgb = Clamp(Add(Abs(Add(Mod(v1, 6.0f), -3.0f)), -1.0f), 0.0f, 1.0f);

    rgb = rgb * rgb * (Add(-2.0f * rgb, 3.0f)); // cubic smoothing

    return c.Z * Mix(new Vector3(1.0f, 1.0f, 1.0f), rgb, c.Y);
}

float Smoothstep(float edge0, float edge1, float x)
{
    // Scale, bias and saturate x to 0..1 range
    x = Math.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
    // Evaluate polynomial
    return x * x * (3 - 2 * x);
}

Vector3 Star(Vector2 uv, float time)
{
    Vector2 p = uv - new Vector2(0.5f, 0.5f);
    float a = (float)(Math.Atan2(p.Y, p.X) / 2f / Math.PI);

    a = ModF(a + time / 10.0f, 1.0f);

    float r = p.Length();
    float n = 5.0f;

    float s1 = 0.2f;
    float s2 = 0.5f;
    float srange = s2 - s1;
    float x = Math.Abs(ModF(a, 1.0f / n) * n - 0.5f) * srange / 0.5f + s1;

    float blur = 4.0f + 3.0f * (float)Math.Sin(time * 2f * Math.PI / 5f);
    float c = Smoothstep(0.0f, 1.0f, (x - r) * blur); // x <= r ? 0.0 : 1.0;

    float ha = a;
    float h = ModF(ha, 1.0f);

    float v = c;

    float s = 1.0f;
    return Hsv2rgb_smooth(new Vector3(h, s, v));
}

Vector3 HSV(Vector2 uv, float time)
{
    Vector2 p = uv - new Vector2(0.5f, 0.5f);
    float a = (float)(Math.Atan2(p.Y, p.X) / 2f / Math.PI);

    a = ModF(a + time / 10.0f, 1.0f);

    float r = p.Length();

    float ha = a;
    float h = ModF(ha, 1.0f);

    float s = 1.0f;
    float v = r * 2.0f;

    return Hsv2rgb_smooth(new Vector3(a, s, v));
}

byte ColF(float x)
{
    x *= 255f;
    x = Math.Clamp(x, 0f, 255f);
    return (byte)x;
}

byte Col(double x, double d, double e)
{
    x *= e;
    x = Math.Pow(x, d);
    x = Math.Clamp(x, 0.0f, 1.0f);
    return (byte)(x * 255);
}

byte ColR(double x)
{
    return Col(x, 1.9, 0.95);
}

byte ColG(double x)
{
    return Col(x, 1.9, 0.95);
}

byte ColB(double x)
{
    return Col(x, 1.9, 0.95);
}

Color ToSRGB(double x, double y, double z)
{
    return Color.FromArgb(
        ColR(x),
        ColG(y),
        ColB(z));
}

Color ColorFromVec3(Vector3 v)
{
    return Color.FromArgb(ColF(v.X), ColF(v.Y), ColF(v.Z));
}

void ScrollText(
                        RGBLedMatrix matrix,
                        string text,
                        BdfFont font,
                        int startPos,
                        int endPos,
                        byte red, byte green, byte blue, byte bkRed, byte bkGreen, byte bkBlue)
{
    if (startPos < endPos)
    {
        return;
    }

    text = text + " "; // to clear the text when scrolling
    int fullTextWidth = text.Length * font.Width;

    while (startPos >= endPos)
    {
        matrix.DrawText(startPos, 0, text, font, red, green, blue, bkRed, bkGreen, bkBlue);
        startPos--;
        Thread.Sleep(20);
    }
}

internal record CityData(string City, string CountryCode, string ZoneId);
