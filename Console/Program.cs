using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioAnalyer;
using PixelServerConnector;
using System.Threading;
using Csl = System.Console;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var connector = new SerialConnector("COM3", 115200);
            var grabber = new Grabber(spectrumLines: 6);

            // open serial
            connector.Open();

            //connector.SetColor(0x0, 0x0, 0x0, 0xA0);

            // select device
            var devices = grabber.GetDevices();

            for (var i = 0; i < devices.Count; i++)
            {
                Csl.WriteLine($"[{i}] {devices.ElementAt(i).Value}");
            }

            var selectedPos = devices.Count > 1
                ? int.Parse(Csl.ReadKey().KeyChar.ToString())
                : 0;

            // init audio grabber
            grabber.Init(devices.ElementAt(selectedPos).Key, data =>
            {
                Csl.SetCursorPosition(0, 5);

                Csl.WriteLine($"L: {data.LeftLevel:000} R: {data.RightLevel:000}");

                Csl.WriteLine($"BPM: {data.BPM}");

                for (var i = 0; i < data.Spectrum.Length; i++)
                {
                    Csl.WriteLine($"Band {i:00}: {data.Spectrum[i]:000}");
                    RenderBar(data.Spectrum[i] / 255f, 70);
                }

                // set color
                var level = (byte)((float)data.Spectrum[0] / 255 * 125);

                connector.SetColor(level, level, level, level);
            });

            Csl.ReadLine();

            grabber.Stop();
            connector.Close();
        }

        static void RenderBar(float normalizedValue, int size, string prefix = null)
        {
            var calcSize = Math.Max((int)(normalizedValue * size), 1);
            Csl.WriteLine($"{prefix}{new string('█', calcSize)}{new string(' ', size - calcSize)}");
        }
    }
}
