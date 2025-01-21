using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;



namespace Wpf_playInTheBeat
{
    public partial class MainWindow : Window
    {
        string Path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "..\\..\\source\\";
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] fileNames = new string[10];
            fileNames[0] = "Kick.wav";
            fileNames[1] = "Hat.wav";

            // on startup:
            var zap = new CachedSound(Path + fileNames[0]);
            var boom = new CachedSound(Path + fileNames[1]);

            // later in the app...
            AudioPlaybackEngine.Instance.PlaySound(zap);
            AudioPlaybackEngine.Instance.PlaySound(boom);
        }

        class AudioPlaybackEngine : IDisposable
        {
            private readonly IWavePlayer outputDevice;
            private readonly MixingSampleProvider mixer;

            public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
            {
                outputDevice = new WaveOutEvent();
                mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
                mixer.ReadFully = true;
                outputDevice.Init(mixer);
                outputDevice.Play();
            }

            public void PlaySound(string fileName)
            {
                var input = new AudioFileReader(fileName);
                AddMixerInput(new AutoDisposeFileReader(input));
            }

            private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
            {
                if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
                {
                    return input;
                }
                if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
                {
                    return new MonoToStereoSampleProvider(input);
                }
                throw new NotImplementedException("Not yet implemented this channel count conversion");
            }

            public void PlaySound(CachedSound sound)
            {
                AddMixerInput(new CachedSoundSampleProvider(sound));
            }

            private void AddMixerInput(ISampleProvider input)
            {
                mixer.AddMixerInput(ConvertToRightChannelCount(input));
            }

            public void Dispose()
            {
                outputDevice.Dispose();
            }

            public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
        }

        class CachedSound
        {
            public float[] AudioData { get; private set; }
            public WaveFormat WaveFormat { get; private set; }
            public CachedSound(string audioFileName)
            {
                using (var audioFileReader = new AudioFileReader(audioFileName))
                {
                    // TODO: could add resampling in here if required
                    WaveFormat = audioFileReader.WaveFormat;
                    var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                    var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                    int samplesRead;
                    while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    {
                        wholeFile.AddRange(readBuffer.Take(samplesRead));
                    }
                    AudioData = wholeFile.ToArray();
                }
            }
        }

        class CachedSoundSampleProvider : ISampleProvider
        {
            private readonly CachedSound cachedSound;
            private long position;

            public CachedSoundSampleProvider(CachedSound cachedSound)
            {
                this.cachedSound = cachedSound;
            }

            public int Read(float[] buffer, int offset, int count)
            {
                var availableSamples = cachedSound.AudioData.Length - position;
                var samplesToCopy = Math.Min(availableSamples, count);
                Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);
                position += samplesToCopy;
                return (int)samplesToCopy;
            }

            public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
        }

        class AutoDisposeFileReader : ISampleProvider
        {
            private readonly AudioFileReader reader;
            private bool isDisposed;
            public AutoDisposeFileReader(AudioFileReader reader)
            {
                this.reader = reader;
                this.WaveFormat = reader.WaveFormat;
            }

            public int Read(float[] buffer, int offset, int count)
            {
                if (isDisposed)
                    return 0;
                int read = reader.Read(buffer, offset, count);
                if (read == 0)
                {
                    reader.Dispose();
                    isDisposed = true;
                }
                return read;
            }

            public WaveFormat WaveFormat { get; private set; }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            outputDevice?.Stop();
        }


        public static class WavFileUtils
        {
            [Obsolete]
            public static void TrimWavFile(string inPath, string outPath, TimeSpan cutFromStart, TimeSpan cutFromEnd)
            {
                using (WaveFileReader reader = new WaveFileReader(inPath))
                {
                    using (WaveFileWriter writer = new WaveFileWriter(outPath, reader.WaveFormat))
                    {
                        int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                        int startPos = (int)cutFromStart.TotalMilliseconds * bytesPerMillisecond;
                        startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

                        int endBytes = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
                        endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
                        int endPos = (int)reader.Length - endBytes;

                        TrimWavFile(reader, writer, startPos, endPos);
                    }
                }
            }

            [Obsolete]
            private static void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
            {
                reader.Position = startPos;
                byte[] buffer = new byte[1024];
                while (reader.Position < endPos)
                {
                    int bytesRequired = (int)(endPos - reader.Position);
                    if (bytesRequired > 0)
                    {
                        int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                        int bytesRead = reader.Read(buffer, 0, bytesToRead);
                        if (bytesRead > 0)
                        {
                            writer.WriteData(buffer, 0, bytesRead);
                        }
                    }
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string[] fileNames = new string[10];
            fileNames[0] = "Kick.wav";
            fileNames[1] = "Hat.wav";
            fileNames[2] = "Snares.wav";
            fileNames[3] = "vocal.wav";

            var a1 = new AudioFileReader(Path + fileNames[0]);
            var a2 = new AudioFileReader(Path + fileNames[1]);
            var a3 = new AudioFileReader(Path + fileNames[2]);
            var a4 = new AudioFileReader(Path + fileNames[3]);

            var vocal = new AudioFileReader(Path + fileNames[3]);

            vocal.CurrentTime = TimeSpan.FromSeconds(3);
            var trimmed = vocal.Take(TimeSpan.FromSeconds(3));
            WaveFileWriter.CreateWaveFile16(Path + "vocalTrimmed.wav", trimmed);

            var vocalTrimmed = new AudioFileReader(Path + "vocalTrimmed.wav");

            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (audioFile == null)
            {
                audioFile = vocalTrimmed;
                outputDevice.Init(audioFile);
            }
            outputDevice.Play();
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            outputDevice?.Dispose();
            outputDevice = null;
            audioFile?.Dispose();
            audioFile = null;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string[] fileNames = new string[10];
            fileNames[0] = "Kick.wav";
            fileNames[1] = "Hat.wav";
            fileNames[2] = "Snares.wav";
            fileNames[3] = "vocal.wav";

            var a1 = new AudioFileReader(Path + fileNames[0]);
            var a2 = new AudioFileReader(Path + fileNames[1]);
            var a3 = new AudioFileReader(Path + fileNames[2]);
            var a4 = new AudioFileReader(Path + fileNames[1]);

            //pitch
            var semitone = Math.Pow(2, 1.0 / 12);
            var upOneTone = semitone * semitone;
            var downOneTone = 1.0 / upOneTone;

            var source = new AudioFileReader(Path + fileNames[0]);
            source.CurrentTime = TimeSpan.FromSeconds(0);
            var trimmed = source.Take(TimeSpan.FromSeconds(2));
            WaveFileWriter.CreateWaveFile16(Path + "trimmedKick.wav", trimmed);

            var source2 = new AudioFileReader(Path + fileNames[1]);
            source.CurrentTime = TimeSpan.FromSeconds(0);
            var trimmed2 = source2.Take(TimeSpan.FromSeconds(2));
            WaveFileWriter.CreateWaveFile16(Path + "trimmedHat1.wav", trimmed2);

            var source3 = new AudioFileReader(Path + fileNames[2]);
            source.CurrentTime = TimeSpan.FromSeconds(0);
            var trimmed3 = source3.Take(TimeSpan.FromSeconds(2));
            WaveFileWriter.CreateWaveFile16(Path + "trimmedSnare.wav", trimmed3);

            var source4 = new AudioFileReader(Path + fileNames[1]);
            source.CurrentTime = TimeSpan.FromSeconds(0);
            var trimmed4 = source4.Take(TimeSpan.FromSeconds(2));
            WaveFileWriter.CreateWaveFile16(Path + "trimmedHat2.wav", trimmed4);

            var kick = new AudioFileReader(Path + "trimmedKick.wav");
            var hat1 = new AudioFileReader(Path + "trimmedHat1.wav");
            var snare = new AudioFileReader(Path + "trimmedSnare.wav");
            var hat2 = new AudioFileReader(Path + "trimmedHat2.wav");

            var playlist = new ConcatenatingSampleProvider(new[] { kick, hat1, snare, hat2 });

            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (audioFile == null)
            {
                outputDevice.Init(playlist);
            }
            outputDevice.Play();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            string[] fileNames = new string[10];
            fileNames[0] = "Kick.wav";
            fileNames[1] = "Hat.wav";
            fileNames[2] = "Snares.wav";
            fileNames[3] = "vocal.wav";

            var t = new AudioFileReader(Path + fileNames[0]);
            var t1 = new AudioFileReader(Path + fileNames[1]);
            var first = new AudioFileReader(Path + fileNames[0]);
            var second = new AudioFileReader(Path + fileNames[1]);
            var playlist = first.FollowedBy(second);


            var tempPlaylist = t.FollowedBy(t1);
            WaveFileWriter.CreateWaveFile16(Path + "law2.wav", tempPlaylist);

            var fuck = new AudioFileReader(Path + "law2.wav");


            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (audioFile == null)
            {
                audioFile = fuck;
                outputDevice.Init(audioFile);
            }
            outputDevice.Play();
        }

        private void pitchControll(object sender, RoutedEventArgs e)
        {
            //pitch semitone隔壁一個鍵 往上就是一直往上乘 反之
            var semitone = Math.Pow(2, 1.0 / 12);
            var currTone = 1;

            string[] fileNames = new string[10];
            fileNames[0] = "Kick.wav";
            fileNames[1] = "Hat.wav";
            fileNames[2] = "Snares.wav";
            fileNames[3] = "vocal.wav";

            var a1 = new AudioFileReader(Path + fileNames[3]);
            var a2 = new AudioFileReader(Path + fileNames[1]);
            var a3 = new AudioFileReader(Path + fileNames[2]);
            var a4 = new AudioFileReader(Path + fileNames[1]);

            //a1.Volume = 0.3f;
            var a1Pitched = new SmbPitchShiftingSampleProvider(a1.ToSampleProvider());
            a1Pitched.PitchFactor = 1;//(float)currTone/ (float)semitone


            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            outputDevice.Init(a1Pitched);
            outputDevice.Play();
        }

        private void volume(object sender, RoutedEventArgs e)
        {
            string fileNames = "vocal.wav";
            var audio1 = new AudioFileReader(Path + fileNames);
            audio1.Volume = 0.2f;

            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            outputDevice.Init(audio1);
            outputDevice.Play();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            string fileNames = "vocal.wav";
            var audio1 = new AudioFileReader(Path + fileNames);
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;

            }
            outputDevice.Init(audio1);
            outputDevice.Play();
        }
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            outputDevice?.Stop();
        }
    }
}
