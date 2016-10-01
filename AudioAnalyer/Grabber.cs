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
        public struct Data
        {
            public float Volume { get; set; }
        }

        public IWaveIn InputDevice = new WasapiLoopbackCapture();

        public void Init(Action<Data> process)
        {
            var waveProvider = new BufferedWaveProvider(InputDevice.WaveFormat);
            //var volumeProvier = new VolumeSampleProvider(waveProvider.ToSampleProvider());
            var meteringProvider = new MeteringSampleProvider(waveProvider.ToSampleProvider());

            InputDevice.DataAvailable += (sender, args) =>
            {
                waveProvider.AddSamples(args.Buffer, 0, args.BytesRecorded);
                
                var buffer = new float[args.BytesRecorded];
                meteringProvider.Read(buffer, 0, args.BytesRecorded);
            };

            meteringProvider.StreamVolume += (sender, args) =>
            {
                process.Invoke(new Data
                {
                    Volume = args.MaxSampleValues.Max()
                });
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
    }
}
