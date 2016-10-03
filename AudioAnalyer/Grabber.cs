using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.BassWasapi;

namespace AudioAnalyer
{
    public class Grabber
    {
        public struct Data
        {
            public byte[] Spectrum { get; set; }
            public byte LeftLevel { get; set; }
            public byte RightLevel { get; set; }
            public double BPM { get; set; }
        }

        private readonly int _grabIntervall;
        private bool _running;
        private readonly float[] _fft = new float[1024];
        private readonly int _lines;                                    // number of spectrum lines
        private Action<Data> _callback;
        private BPMCounter _bpmCounter;

        public Grabber(int grabIntervall = 25, int spectrumLines = 16)
        {
            _grabIntervall = grabIntervall;
            _lines = spectrumLines;
        }

        public IDictionary<int, string> GetDevices()
        {
            var devices = new Dictionary<int, string>();

            for (var i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
            {
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);

                if (device.IsEnabled && device.IsLoopback)
                {
                    devices.Add(i, device.name);
                }
            }

            return devices;
        }

        public void Init(int deviceIndex, Action<Data> callback)
        {
            _callback = callback;

            // config
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);

            // init bass
            var result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            if (!result)
                throw new Exception("Init Error");

            // init wasapi
            result = BassWasapi.BASS_WASAPI_Init(deviceIndex, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, (buffer, length, user) => length, IntPtr.Zero);

            if (!result)
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());

            // bpm counter
            _bpmCounter = new BPMCounter(_grabIntervall, 44100);
            
            // start
            BassWasapi.BASS_WASAPI_Start();

            _running = true;

            Task.Run(ProcessData);
        }

        public void Stop()
        {
            _running = false;

            BassWasapi.BASS_WASAPI_Stop(true);
        }

        private async Task ProcessData()
        {
            while (_running)
            {
                //get channel fft data
                var ret = BassWasapi.BASS_WASAPI_GetData(_fft, (int)BASSData.BASS_DATA_FFT2048); 

                if (ret >= -1)
                {
                    //computes the spectrum data, the code is taken from a bass_wasapi sample.
                    var b0 = 0;
                    var spectrum = new byte[_lines];

                    for (var x = 0; x < _lines; x++)
                    {
                        var peak = 0f;
                        var b1 = (int)Math.Pow(2, x * 10.0 / (_lines - 1));
                        if (b1 > 1023) b1 = 1023;
                        if (b1 <= b0) b1 = b0 + 1;
                        for (; b0 < b1; b0++)
                        {
                            if (peak < _fft[1 + b0]) peak = _fft[1 + b0];
                        }
                        var y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);
                        if (y > 255) y = 255;
                        if (y < 0) y = 0;
                        spectrum[x] = (byte)y;
                    }

                    // channel level
                    var level = BassWasapi.BASS_WASAPI_GetLevel();

                    // bpm
                    //_bpmCounter.ProcessAudio(0, false);
                    
                    // callback
                    _callback.Invoke(new Data
                    {
                        Spectrum = spectrum,
                        LeftLevel = (byte)((float)Utils.LowWord32(level) / ushort.MaxValue * byte.MaxValue),
                        RightLevel = (byte)((float)Utils.HighWord32(level) / ushort.MaxValue * byte.MaxValue),
                        BPM = _bpmCounter.BPM
                    });
                }

                await Task.Delay(_grabIntervall);
            }
        }
    }
}
