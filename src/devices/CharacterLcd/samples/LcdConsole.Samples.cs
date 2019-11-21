using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Iot.Device.CharacterLcd;

namespace Iot.Device.CharacterLcd.Samples
{
    /// <summary>
    /// Sample code for using the <see cref="LcdConsole"/> class.
    /// </summary>
    public class LcdConsoleSamples
    {
        /// <summary>
        /// Write stuff to the display.
        /// </summary>
        /// <param name="lcd">The display driver</param>
        public static void WriteTest(ICharacterLcd lcd)
        {
            LcdConsole console = new LcdConsole(lcd, "A00", false);
            console.LineFeedMode = LineFeedMode.Disable;
            Console.WriteLine("Nowrap test:");
            console.Write("This is a long text that should not wrap and just extend beyond the display");
            console.WriteLine("This has CRLF\r\nin it and should \r\n wrap.");
            console.Write("This goes to the last line of the display");
            console.WriteLine("This isn't printed, because it's off the screen");
            Console.ReadLine();
            Console.WriteLine("Autoscroll test:");
            console.LineFeedMode = LineFeedMode.ScrollUp;
            console.WriteLine();
            console.WriteLine("Now the display should move up.");
            console.WriteLine("And more up.");
            for (int i = 0; i < 20; i++)
            {
                console.WriteLine($"This is line {i + 1}/{20}, but longer than the screen");
                Thread.Sleep(10);
            }
            console.LineFeedMode = LineFeedMode.UntilFull | LineFeedMode.ScrollUp;
            console.WriteLine("Same again, this time with full wrapping.");
            for (int i = 0; i < 20; i++)
            {
                console.Write($"This is string {i + 1}/{20} longer than the screen");
                Thread.Sleep(10);
            }
            Console.ReadLine();
            Console.WriteLine("Intelligent wrapping test");
            console.LineFeedMode = LineFeedMode.Intelligent | LineFeedMode.ScrollUp;
            console.WriteLine("Now intelligent wrapping should wrap this long sentence at word borders and ommit spaces at the start of lines.");
            Console.WriteLine("Not wrappable test");
            Console.ReadLine();
            console.WriteLine("NowThisIsOneSentenceInOneWordThatCannotBeWrapped");
            Console.ReadLine();
            Console.WriteLine("Individual line test");
            console.Clear();
            console.LineFeedMode = LineFeedMode.UntilFull;
            console.Write(0, "This is all garbage that will be replaced");
            console.Write(0, "Running clock test");
            int left = console.Size.Width;
            // Let the current time move trought the display on line 1
            while (!Console.KeyAvailable)
            {
                String time = String.Format(CultureInfo.CurrentCulture, "{0}", DateTime.Now.ToLongTimeString());
                string printTime = time;
                if (left > 0)
                {
                    printTime = new string(' ', left) + time;
                }
                else if (left < 0)
                {
                    printTime = time.Substring(-left);
                }
                console.Write(1, printTime);
                left--;
                Thread.Sleep(500);
                // Restart when the time string has left the display
                if (left < -time.Length)
                {
                    left = console.Size.Width;
                }
            }
            console.Write(0, "!! ALERTING !!");
            console.BlinkDisplay(4);
            Console.ReadKey();
            Console.WriteLine("Culture Info Test");
            console.LoadCulture(CultureInfo.CreateSpecificCulture("de-CH"), '?');
            console.Clear();
            console.NewLineSleepTime = TimeSpan.FromSeconds(1);
            console.LineFeedMode = LineFeedMode.Intelligent | LineFeedMode.ScrollUp;
            //console.WriteLine(@"Die Ratten im Gemäuer, englischer Originaltitel ""The Rats in the Walls"" " +
            //    "ist eine phantastische Kurzgeschichte des amerikanischen Schriftstellers H. P. Lovecraft. Das etwa " +
            //    "8000 Wörter umfassende Werk wurde zwischen August und September 1923 verfasst und erschien erstmals " +
            //    "im März 1924 im Pulp-Magazin Weird Tales. Der Titel bezieht sich auf das Rascheln von Ratten in den " +
            //    "Gemäuern des Familienanwesens, das der Erzähler Delapore nach 300 Jahren auf den Ruinen des Stammsitzes " +
            //    "seiner Vorfahren neu errichtet hat. Im Verlauf der Erzählung führen die Ratten Delapore zur Entdeckung " +
            //    "des grausigen Geheimnisses der Gruft seines Anwesens und der finsteren Vergangenheit seiner Familie. " +
            //    "Nach Lovecraft entstand die Grundidee für die Geschichte, als eines späten Abends seine Tapete zu knistern begann. " +
            //    "(von https://de.wikipedia.org/wiki/Die_Ratten_im_Gem%C3%A4uer, CC-BY-SA 3.0)");
            console.WriteLine("From A00 default map: ");
            console.WriteLine("Code: [{|}]^_\\");
            console.WriteLine("Greek: Ωαβεπθμ");
            console.WriteLine("Others: @ñ¢");
            console.Write("Math stuff: ∑÷×∞");

            console.WriteLine("German code page");
            console.WriteLine("Umlauts: äöüßÄÜÖ");
            console.WriteLine("Äußerst ölige, überflüssige Ölfässer im Großhandel von Ützhausen.");
            console.WriteLine("Currency: ¥€£$");

            console.LoadCulture(CultureInfo.CreateSpecificCulture("fr-fr"), '?');
            console.Clear();
            console.WriteLine("Le français est une langue indo-européenne de la famille des langues romanes. " +
                "Le français s'est formé en France. Le français est déclaré langue officielle en France en 1539. " +
                "Après avoir été sous l'Ancien Régime la langue des cours royales et princières, " +
                "des tsars de Russie aux rois d'Espagne et d'Angleterre en passant par les princes de l'Allemagne, " +
                "il demeure une langue importante de la diplomatie internationale aux côtés de l'anglais. ");

            console.LoadCulture(CultureInfo.CreateSpecificCulture("da-da"), '?');
            console.Clear();
            console.WriteLine("Dansk er et nordgermansk sprog af den østnordiske (kontinentale) gruppe, " +
                "der tales af ca. seks millioner mennesker. Det er stærkt påvirket af plattysk. Dansk tales " +
                "også i Sydslesvig (i Flensborg ca. 20 %) samt på Færøerne og Grønland.");
            console.WriteLine("PÅ FÆRØER");

            Console.ReadLine();
            Console.WriteLine("Japanese test");
            console.LoadCulture(CultureInfo.CreateSpecificCulture("ja-ja"));
            console.WriteLine("What about some japanese?");
            console.WriteLine("イロハニホヘト");
            console.WriteLine("チリヌルヲ");
            console.WriteLine("ワカヨタレソ");
            console.WriteLine("ツネナラム");
            console.WriteLine("ウヰノオクヤマ");
            console.WriteLine("ケフコエテ");
            console.WriteLine("アサキユメミシ");
            console.WriteLine("ヱヒモセス");
            console.Clear();
            console.Write("Test finished");
            console.Dispose();
        }
    }
}
