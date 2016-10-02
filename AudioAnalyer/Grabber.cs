using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Dsp;

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
            public double[] Spectrum { get;set; }
        }

        public IWaveIn InputDevice = new WasapiLoopbackCapture();

        public void Init(Action<Data> process)
        {
            var waveProvider = new BufferedWaveProvider(InputDevice.WaveFormat);
            var meteringProvider = new MeteringSampleProvider(waveProvider.ToSampleProvider(), InputDevice.WaveFormat.SampleRate / 20);
            var aggregator = new SampleAggregator(waveProvider.ToSampleProvider()) { PerformFFT = true };
            var bytsPerSample = InputDevice.WaveFormat.BitsPerSample / 8;
            var specAnalyser = new SpectrumAnalyser();
            var data = new Data();

            //var longBufferLength = 48000;
            //var longBuffer = new FixedSizedQueue<float>(longBufferLength);

            InputDevice.DataAvailable += (sender, args) =>
            {
                waveProvider.AddSamples(args.Buffer, 0, args.BytesRecorded);
                
                //var meterBuffer = new float[waveProvider.BufferedBytes / bytsPerSample];
                //meteringProvider.Read(meterBuffer, 0, waveProvider.BufferedBytes / bytsPerSample);

                var aggBuffer = new float[waveProvider.BufferedBytes / bytsPerSample];
                aggregator.Read(aggBuffer, 0, waveProvider.BufferedBytes / bytsPerSample);

                //for (int i = 0; i < buffer.Length; i++)
                //{
                //    longBuffer.Enqueue(buffer[i]);
                //}
                //longBuffer.Enqueue()

                //data.Band1.Current = (float)GoertzelFilter(longBuffer.ToArray(), 250, 0, buffer.Length, InputDevice.WaveFormat.SampleRate);
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

            aggregator.FftCalculated += (object sender, FftEventArgs e) =>
            {
                specAnalyser.Update(e.Result);

                data.Spectrum = specAnalyser.Output;

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
