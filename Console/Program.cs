using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioAnalyer;
using PixelServerConnector;
using System.Threading;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var grabber = new Grabber();
            var connector = new SerialConnector("COM3", 115200);

            connector.Open();

            grabber.Init(data =>
            {
                var col = 0x127 * data.Volume.Normalized;

                System.Console.SetCursorPosition(0, 8);

                RenderBar(data.Volume.Normalized, 80, $"Volume: {data.Volume.Normalized:0.000} ");
                //RenderBar(data.Band1.Normalized, 80,  $"Band1 : {data.Band1.Normalized:0.000} ");
                //RenderBar(data.Band2.Normalized, 80, $"Band2 : {data.Band2.Normalized:0.000} ");
                //RenderBar(data.Band3.Normalized, 80, $"Band3 : {data.Band3.Normalized:0.000} ");
                //RenderBar(data.Band4.Normalized, 80, $"Band4 : {data.Band4.Normalized:0.000} ");

                connector.SetColor(0x0, 0x0, 0x0, (byte)col);
            });

            var format = grabber.InputDevice.WaveFormat;

            System.Console.WriteLine($"AverageBytesPerSecond: {format.AverageBytesPerSecond}");
            System.Console.WriteLine($"BitsPerSample: {format.BitsPerSample}");
            System.Console.WriteLine($"BlockAlign: {format.BlockAlign}");
            System.Console.WriteLine($"Channels: {format.Channels}");
            System.Console.WriteLine($"Encoding: {format.Encoding}");
            System.Console.WriteLine($"ExtraSize: {format.ExtraSize}");
            System.Console.WriteLine($"SampleRate: {format.SampleRate}");


            System.Console.ReadLine();

            grabber.Stop();

            

            
            //connector.SetColor(0x0, 0x0, 0x0, 0x0);
            //connector.SetColor(0x0, 0x0, 0x0, 0x0);

            //for (int i = 0; i < 128; i++)
            //{
            //    connector.SetColor(0x0, 0x0, 0x0, (byte)i);

            //    Thread.Sleep(10);
            //}

            //connector.SetColor(0x0, 0x0, 0x0, 0x0);

            connector.Close();
        }

        static void RenderBar(float normalizedValue, int size, string prefix = null)
        {
            var calcSize = Math.Max((int)(normalizedValue * size), 1);
            System.Console.WriteLine($"{prefix}{new string('█', calcSize)}{new string(' ', size - calcSize)}");
        }

        static void Audio()
        {

        }
            

    }
}
