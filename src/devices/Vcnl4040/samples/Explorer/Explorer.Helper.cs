// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;

internal partial class Explorer
{
    private static bool PromptEnum<T>(string prompt, out T selectedValue, T currentValue = default)
        where T : struct
    {
        Array enumValues = Enum.GetValues(typeof(T));

        Console.WriteLine(prompt + ": ");
        int n = 0;
        foreach (var x in enumValues)
        {
            Console.WriteLine($"  ({n}) {x}");
            n++;
        }

        Console.Write($"({currentValue}) => ");

        string? input = Console.ReadLine();
        if (string.IsNullOrEmpty(input))
        {
            selectedValue = currentValue;
            Console.WriteLine($"  => {currentValue}\n");
            return true;
        }

        bool result = int.TryParse(input, out int choice);
        if (!result)
        {
            selectedValue = default;
            Console.WriteLine();
            return false;
        }

        if (choice < 0 || choice >= n)
        {
            Console.WriteLine($"\nINVALID CHOICE ({choice})\n");
            selectedValue = default;
            return false;
        }

        object? chosenEnumValue = enumValues.GetValue(choice);
        if (chosenEnumValue != null)
        {
            selectedValue = (T)chosenEnumValue!;
            Console.WriteLine();
            return true;
        }

        Console.WriteLine($"\nINVALID CHOICE ({choice})\n");
        selectedValue = default;
        return false;
    }

    private static bool PromptValue(string prompt, out ushort enteredValue, ushort? currentValue = null, ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
    {
        if (currentValue != null)
        {
            Console.Write($"{prompt} ({currentValue}): ");
        }
        else
        {
            Console.Write($"{prompt}: ");
        }

        string? input = Console.ReadLine();
        if (input == string.Empty && currentValue != null)
        {
            enteredValue = currentValue.Value;
            Console.WriteLine($"  => {currentValue}");
        }
        else
        {
            bool result = ushort.TryParse(input, out enteredValue);
            if (!result)
            {
                Console.WriteLine("\nINVALID INPUT\n");
                return false;
            }
        }

        if (enteredValue < min || enteredValue > max)
        {
            Console.WriteLine($"\nINPUT OUT OF RANGE ({min}-{max})\n");
            return false;
        }

        Console.WriteLine();
        return true;
    }

    private static bool PromptYesNoChoice(string prompt, out bool value, bool current)
    {
        bool result = PromptEnum(prompt, out YesNoChoice choice, current ? YesNoChoice.Yes : YesNoChoice.No);
        if (!result)
        {
            value = false;
            return false;
        }

        value = choice == YesNoChoice.Yes;
        return true;
    }

    private static void PrintBarGraph(double value, double maxValue, string addInfo = "")
    {
        Console.CursorLeft = 0;

        string addInfoPart = $" ({value,8:0.000}){(!string.IsNullOrEmpty(addInfo) ? " [" + addInfo + "]" : string.Empty)}";

        StringBuilder line = new();
        int barWidth = Console.WindowWidth - addInfoPart.Length - 2;
        int progressBarWidth = (int)((double)Math.Min(value, maxValue) / maxValue * barWidth);
        int spaces = barWidth - progressBarWidth;

        line.Append('[');

        for (int i = 0; i < progressBarWidth; i++)
        {
            line.Append('#');
        }

        for (int i = 0; i < spaces; i++)
        {
            line.Append(' ');
        }

        line.Append(']');
        line.Append(addInfoPart);

        Console.Write(line.ToString());
        Console.Write(new string(' ', Console.WindowWidth - line.Length));
        Console.CursorLeft = 0;
    }

    private enum YesNoChoice
    {
        No,
        Yes
    }

    private class Command
    {
        public string Section { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public Action Action { get; init; } = () => { };
        public bool ShowConfiguration { get; init; } = true;
        public string Id { get; set; } = string.Empty;
    }
}
