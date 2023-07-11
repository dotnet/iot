// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Spi;
using System.Drawing;
using Iot.Device.Ws28xx;
using LEDStripSample;

Console.WriteLine("Attach debugger if you want to, press any key to continue");
Console.ReadKey();
Console.Clear();

SpiConnectionSettings settings = new(0, 0)
{
    ClockFrequency = 2_400_000,
    Mode = SpiMode.Mode0,
    DataBitLength = 8
};

using SpiDevice spi = SpiDevice.Create(settings);

var ledCount = -1;
MenuId menuId = MenuId.Root;
Animations? effects = null;

while (ledCount == -1)
{
    Console.Write("Number of LEDs (0 to exit): ");
    var input = Console.ReadLine();
    if (int.TryParse(input, out ledCount))
    {
        break;
    }
}

if (ledCount == 0)
{
    return;
}

while (true)
{
    Console.WriteLine("1: WS2812b");
    Console.WriteLine("2: WS2815b");
    Console.WriteLine("3: SK68012");
    Console.WriteLine("0: Exit");
    Console.Write("Type of Strip: ");
    var ledType = Console.ReadLine()!.Trim();

    switch (ledType)
    {
        case "0":
            return;

        case "1":
            effects = new Animations(new Ws2812b(spi, ledCount), ledCount);
            break;

        case "2":
            effects = new Animations(new Ws2815b(spi, ledCount), ledCount);
            break;

        case "3":
            effects = new Animations(new Sk6812(spi, ledCount), ledCount) { SupportsSeparateWhite = true };
            break;

        default:
            Console.WriteLine("Unsupported selection.");
            break;
    }

    if (effects != null)
    {
        break;
    }
}

var isActive = true;

effects!.SwitchOffLeds();

var colorPercentage = 1.0f;

while (isActive)
{
    DrawMenu();
}

Console.WriteLine("Exit application.");

void DrawMenu()
{
    Console.Clear();
    switch (menuId)
    {
        case MenuId.Root:
            isActive = DrawMainMenu();
            break;

        case MenuId.WhiteLevel:
            DrawWhiteLevelMenu();
            break;

        case MenuId.TheatreChase:
            DrawTheatreChaseMenu();
            break;

        case MenuId.Wipe:
            DrawWipeMenu();
            break;

        case MenuId.Special:
            DrawSpecialMenu();
            break;
    }
}

void DrawSpecialMenu()
{
    Console.WriteLine("1. Knight Rider");
    Console.WriteLine("0: Back");
    Console.Write("Selection: ");

    var choice = Console.ReadLine();
    switch (choice)
    {
        case "0":
            menuId = MenuId.Root;
            break;

        case "1":
            StartKnightRider();
            break;

        default:
            Console.WriteLine("Unknown Selection. Any key to continue.");
            Console.ReadKey();
            break;
    }
}

void DrawWipeMenu()
{
    Console.WriteLine("1. Wipe black");
    Console.WriteLine("2. Wipe red");
    Console.WriteLine("3. Wipe green");
    Console.WriteLine("4. Wipe blue");
    Console.WriteLine("5. Wipe yellow");
    Console.WriteLine("6. Wipe turqoise");
    Console.WriteLine("7. Wipe purple");
    Console.WriteLine("8. Wipe cold white");
    if (effects.SupportsSeparateWhite)
    {
        Console.WriteLine("9: Separate white");
    }

    Console.WriteLine("0: Back");
    Console.Write("Selection: ");

    var choice = Console.ReadLine();
    switch (choice)
    {
        case "0":
            menuId = MenuId.Root;
            break;

        case "1":
            effects.ColorWipe(effects.FilterColor(Color.Black));
            break;

        case "2":
            effects.ColorWipe(effects.FilterColor(Color.Red));
            break;

        case "3":
            effects.ColorWipe(effects.FilterColor(Color.Green));
            break;

        case "4":
            effects.ColorWipe(effects.FilterColor(Color.Blue));
            break;

        case "5":
            effects.ColorWipe(effects.FilterColor(Color.Yellow));
            break;

        case "6":
            effects.ColorWipe(effects.FilterColor(Color.Turquoise));
            break;

        case "7":
            effects.ColorWipe(effects.FilterColor(Color.Purple));
            break;

        case "8":
            effects.ColorWipe(effects.FilterColor(Color.White));
            break;

        case "9":
            effects.ColorWipe(Color.FromArgb(255, 0, 0, 0));
            break;

        default:
            Console.WriteLine("Unbekannter Eintrag. Beliebige Taste zum fortfahren.");
            Console.ReadKey();
            break;
    }
}

void DrawTheatreChaseMenu()
{
    Console.WriteLine("1: All LED off");
    Console.WriteLine("2: Chase in red");
    Console.WriteLine("3: Chase in green");
    Console.WriteLine("4: Chase in blue");
    Console.WriteLine("5: Christmas Chase");

    if (effects.SupportsSeparateWhite)
    {
        Console.WriteLine("6: White Chase");
    }

    Console.WriteLine("0: Back");
    Console.Write("Selection: ");
    var choice = Console.ReadLine();
    switch (choice)
    {
        case "0":
            menuId = MenuId.Root;
            break;

        case "1":
            effects.SwitchOffLeds();
            break;

        case "2":
            StartChase(effects.FilterColor(Color.Red), Color.FromArgb(0, 0, 0, 0));
            break;

        case "3":
            StartChase(effects.FilterColor(Color.Green), Color.FromArgb(0, 0, 0, 0));
            break;

        case "4":
            StartChase(effects.FilterColor(Color.Blue), Color.FromArgb(0, 0, 0, 0));
            break;

        case "5":
            StartChase(effects.FilterColor(Color.Red), effects.FilterColor(Color.Green));
            break;

        case "6":
            StartChase(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(0, 0, 0, 0));
            break;

        default:
            Console.WriteLine("Unbekannter Eintrag. Beliebige Taste zum fortfahren.");
            Console.ReadKey();
            break;
    }
}

bool DrawMainMenu()
{
    Console.WriteLine("1: All LED off");
    Console.WriteLine("2: All LED to white (percentual)");
    Console.WriteLine("3: Rainbow");
    Console.WriteLine("4: TheatreChase");
    Console.WriteLine("5: Wipe");
    Console.WriteLine("6: Special");

    Console.WriteLine("0: Back");
    Console.Write("Selection: ");
    var choice = Console.ReadLine();
    var keepActive = true;
    switch (choice)
    {
        case "0":
            keepActive = false;
            break;

        case "1":
            effects.SwitchOffLeds();
            keepActive = true;
            break;

        case "2":
            keepActive = true;
            menuId = MenuId.WhiteLevel;
            break;

        case "3":
            StartRainbow();
            break;

        case "4":
            menuId = MenuId.TheatreChase;
            break;

        case "5":
            menuId = MenuId.Wipe;
            break;

        case "6":
            menuId = MenuId.Special;
            break;

        default:
            Console.WriteLine("Unknown Selection. Any key to continue.");
            Console.ReadKey();
            break;
    }

    return keepActive;
}

void DrawWhiteLevelMenu()
{
    Console.WriteLine($"1: Change percentag (currently: {colorPercentage * 100}%)");
    Console.WriteLine("2: LEDs white (RGB)");
    if (effects.SupportsSeparateWhite)
    {
        Console.WriteLine("3: Separate White");
    }

    Console.WriteLine("0: Back");
    Console.Write("Selection: ");
    var choice = Console.ReadLine();
    switch (choice)
    {
        case "0":
            menuId = MenuId.Root;
            break;

        case "1":
            RequestPercentage();
            break;

        case "2":
            effects.SetWhiteValue(colorPercentage);
            break;

        case "3":
            effects.SetWhiteValue(colorPercentage, true);
            break;

        default:
            Console.WriteLine("Unknown Selection. Any key to continue.");
            Console.ReadKey();
            break;
    }
}

void RequestPercentage()
{
    Console.Write("Please enter the percentage between 0 and 100 in in whole numbers:");
    var percentage = Console.ReadLine();
    if (int.TryParse(percentage, out int parsedValue))
    {
        if (parsedValue < 0 || parsedValue > 100)
        {
            Console.WriteLine("Invalid entry. Value not acceptable.");
        }
        else
        {
            colorPercentage = parsedValue / 100.0f;
        }
    }

    Console.WriteLine("Any key to return");
    Console.ReadKey();
}

void StartRainbow()
{
    Console.WriteLine("Any key to stop");
    var cancellationTokenSource = new CancellationTokenSource();

    var rainbowTask = Task.Run(() => effects.Rainbow(cancellationTokenSource.Token));
    Console.ReadKey();
    cancellationTokenSource.Cancel();
    rainbowTask.Wait();
    effects.SwitchOffLeds();
}

void StartKnightRider()
{
    Console.WriteLine("Any key to stop");
    var cancellationTokenSource = new CancellationTokenSource();

    var knightRiderTask = Task.Run(() => effects.KnightRider(cancellationTokenSource.Token));
    Console.ReadKey();
    cancellationTokenSource.Cancel();
    knightRiderTask.Wait();
    effects.SwitchOffLeds();
}

void StartChase(Color color, Color blankColor)
{
    Console.WriteLine("Any key to stop");
    var cancellationTokenSource = new CancellationTokenSource();

    var rainbowTask = Task.Run(() => effects.TheatreChase(color, blankColor, cancellationTokenSource.Token));
    Console.ReadKey();
    cancellationTokenSource.Cancel();
    rainbowTask.Wait();
    effects.SwitchOffLeds();
}
