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
            var grabber = new Grabber(spectrumLines: 8);
            var devices = grabber.GetDevices();

            // select device
            for (var i = 0; i < devices.Count; i++)
            {
                Csl.WriteLine($"[{i}] {devices.ElementAt(i).Value}");
            }

            var selectedPos = devices.Count > 1
                ? int.Parse(Csl.ReadKey().KeyChar.ToString())
                : 0;

            // open serial
            //connector.Open();

            // init grabber
            grabber.Init(devices.ElementAt(selectedPos).Key, data =>
            {
                Csl.SetCursorPosition(0, 5);
                Csl.WriteLine($"L: {data.LeftLevel:000} R: {data.RightLevel:000}");

                for (var i = 0; i < data.Spectrum.Length; i++)
                {
                    Csl.WriteLine($"Band {i:00}: {data.Spectrum[i]:000}");
                    RenderBar(data.Spectrum[i] / 255f, 70);
                }

                connector.SetColor(0x0, 0x0, 0x0, (byte)((float)data.Spectrum[0] / 255 * 125));
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
