using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Diagnostics;

namespace UI_v1
{
    public class Equalizer : ISampleProvider
    {
        private ISampleProvider sourceProvider;
        private BiQuadFilter[] filters;
        private int channels;

        public Equalizer(ISampleProvider source = null)
        {
            filters = new BiQuadFilter[3]; // 初始化
            //Debug.WriteLine("nameof(source) " + nameof(source));

            if (source != null)
            {
                Debug.WriteLine("nameof(source) " + nameof(source));
                Init(source);
            }
        }

        public void Init(ISampleProvider source)
        {
            sourceProvider = source ?? throw new ArgumentNullException(nameof(source)); // If source is Null then source, else throw...
            channels = sourceProvider.WaveFormat.Channels;

            filters = new BiQuadFilter[3 * channels];
            InitFilters();
        }

        private void InitFilters()
        {
            for (int i = 0; i < channels; i++)
            {
                filters[3 * i] = BiQuadFilter.PeakingEQ(sourceProvider.WaveFormat.SampleRate, 100, 0.8f, 0);  // 低
                filters[3 * i + 1] = BiQuadFilter.PeakingEQ(sourceProvider.WaveFormat.SampleRate, 1000, 0.8f, 0); // 中
                filters[3 * i + 2] = BiQuadFilter.PeakingEQ(sourceProvider.WaveFormat.SampleRate, 10000, 0.8f, 0); // 高
            }
        }

        public void SetBandGain(int band, float gain)
        {
            if (sourceProvider == null) throw new InvalidOperationException("Source provider is not initialized.");

            for (int i = 0; i < channels; i++)
            {
                switch (band)
                {
                    case 0: // 低
                        filters[3 * i] = BiQuadFilter.PeakingEQ(sourceProvider.WaveFormat.SampleRate, 100, 0.8f, gain*4);
                        break;
                    case 1: // 中
                        filters[3 * i + 1] = BiQuadFilter.PeakingEQ(sourceProvider.WaveFormat.SampleRate, 1000, 0.8f, gain*4);
                        break;
                    case 2: // 高
                        filters[3 * i + 2] = BiQuadFilter.PeakingEQ(sourceProvider.WaveFormat.SampleRate, 10000, 0.8f, gain*4);
                        break;
                }
            }
        }

        public WaveFormat WaveFormat => sourceProvider?.WaveFormat ?? throw new InvalidOperationException("Source provider is not initialized.");

        public int Read(float[] buffer, int offset, int count)
        {
            if (sourceProvider == null) throw new InvalidOperationException("Source provider is not initialized.");

            int samplesRead = sourceProvider.Read(buffer, offset, count);
            for (int n = 0; n < samplesRead; n++)
            {
                int ch = n % channels;
                buffer[offset + n] = filters[ch * 3].Transform(buffer[offset + n]);      // 低
                buffer[offset + n] = filters[ch * 3 + 1].Transform(buffer[offset + n]);  // 中
                buffer[offset + n] = filters[ch * 3 + 2].Transform(buffer[offset + n]);  // 高
            }
            return samplesRead;
        }
    }
}
