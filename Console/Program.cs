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

            grabber.Init(data =>
            {
                System.Console.WriteLine($"Volume: {data.Volume}");

                var col = 0x128 * data.Volume;

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

        static void Audio()
        {

        }
            

    }
}
