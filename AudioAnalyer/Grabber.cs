using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AudioAnalyer
{
    public class Grabber : IDisposable
    {
        public class Data
        {
            public DataSet Volume { get; } = new DataSet(30);
            public DataSet Band1 { get; } = new DataSet(30);
            public DataSet Band2 { get; } = new DataSet(30);
            public DataSet Band3 { get; } = new DataSet(30);
            public DataSet Band4 { get; } = new DataSet(30);
        }

        public IWaveIn InputDevice = new WasapiLoopbackCapture();

        public void Init(Action<Data> process)
        {
            var waveProvider = new BufferedWaveProvider(InputDevice.WaveFormat);
            var samplingProvider = waveProvider.ToSampleProvider();
            var meteringProvider = new MeteringSampleProvider(samplingProvider, InputDevice.WaveFormat.SampleRate / 20);
            var data = new Data();
            var longBuffer = new Queue<float>(48000);

            InputDevice.DataAvailable += (sender, args) =>
            {
                waveProvider.AddSamples(args.Buffer, 0, args.BytesRecorded);
                
                var buffer = new float[waveProvider.BufferedBytes / 4];
                meteringProvider.Read(buffer, 0, waveProvider.BufferedBytes / 4);


                //for (int i = 0; i < waveProvider.BufferedBytes; i++)
                //{

                //}
                //longBuffer.Enqueue()

                //data.Band1.Current = (float)GoertzelFilter(buffer, 250, 0, buffer.Length, InputDevice.WaveFormat.SampleRate);
                //data.Band2.Current = (float)GoertzelFilter(buffer, 500, 0, buffer.Length, InputDevice.WaveFormat.SampleRate);
                //data.Band3.Current = (float)GoertzelFilter(buffer, 750, 0, buffer.Length, InputDevice.WaveFormat.SampleRate);
                //data.Band4.Current = (float)GoertzelFilter(buffer, 1000, 0, buffer.Length, InputDevice.WaveFormat.SampleRate);
            };

            meteringProvider.StreamVolume += (sender, args) =>
            {
                data.Volume.Current = args.MaxSampleValues.Max();

                //if (data.Volume.Current == 0)
                //    return;

                process.Invoke(data);
            };

            InputDevice.StartRecording();
        }

        public void Stop()
        {
            InputDevice.StopRecording();
        }

        public void Dispose()
        {
            InputDevice.Dispose();
        }

        private double GoertzelFilter(float[] samples, double freq, int start, int end, float sampleRate)
        {
            double sPrev = 0.0;
            double sPrev2 = 0.0;
            int i;
            double normalizedfreq = freq / sampleRate;
            double coeff = 2 * Math.Cos(2 * Math.PI * normalizedfreq);
            for (i = start; i < end; i++)
            {
                double s = samples[i] + coeff * sPrev - sPrev2;
                sPrev2 = sPrev;
                sPrev = s;
            }
            double power = sPrev2 * sPrev2 + sPrev * sPrev - coeff * sPrev * sPrev2;
            return power;
        }
    }
}
