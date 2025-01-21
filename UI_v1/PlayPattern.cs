using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using WpfApp1.plugin;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;


namespace UI_v1
{
    public partial class MainWindow : Window
    {
        //{trackNumber : {sampleName : {}}}
        public Dictionary<string, SampleData> patternDictionary = new Dictionary<string, SampleData>
        {
            //    patternDictionary<music name, SampleData>
            //    {
            //            "sample1", 
            //            List<PatternEQ>{
            //                Patterns = new List<PatternEQ>
            //                {
            //                    new PatternEQ { IsActive = true, Bass = 0.0, Mid = 0.0, Treble = 0.0 ,  pitch = 1},
            //                    new PatternEQ { IsActive = true, Bass = 0.0, Mid = 0.0, Treble = 0.0 ,  pitch = 2},
            //                    new PatternEQ { IsActive = true, Bass = 0.0, Mid = 0.0, Treble = 0.0 ,  pitch = 3}
            //                }
            //                other information...
            //            },
            //            "sample2", 
            //            List<PatternEQ>{
            //                Patterns = new List<PatternEQ>
            //                {
            //                    new PatternEQ { IsActive = true, Bass = 0.0, Mid = 0.0, Treble = 0.0 ,  pitch = 1},
            //                    new PatternEQ { IsActive = true, Bass = 0.0, Mid = 0.0, Treble = 0.0 ,  pitch = 2},
            //                    new PatternEQ { IsActive = true, Bass = 0.0, Mid = 0.0, Treble = 0.0 ,  pitch = 3}
            //                }
            //                other information...
            //            },
            //    }
        };

        public Dictionary<string, string> TrackNumberToSampleName = new Dictionary<string, string> { };
        public Dictionary<string, bool> muteTrackList = new Dictionary<string, bool> { };
        public List<bool> PlayableTrack = new List<bool> { };

        public DrumMachine drumMachine;

        private DispatcherTimer scrollTimer;

        private void StartMusicWithScrolling(double bpm, double sourceWidth)
        {
            double offsetPerSecond = bpm / 15 * sourceWidth;

            scrollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(5)
            };

            scrollTimer.Tick += (sender, e) =>
            {
                SmoothScrollTrackContainer(offsetPerSecond);
            };

            drumMachine.PlayPattern(patternDictionary, muteTrackList);
            scrollTimer.Start();
        }

        private void SmoothScrollTrackContainer(double offsetPerSecond)
        {
            double targetOffset = trackContainerScroll.HorizontalOffset + offsetPerSecond;

            trackContainerScroll.ScrollToHorizontalOffset(targetOffset);
        }



        //play
        private void PlayMusic(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(Bpm.Text.Substring(0, Bpm.Text.Length - 3), out int bpm) && bpm > 0)
            {
                drumMachine.SetBPM(bpm);

                if(int.TryParse(StartTime.Text.Substring(0, StartTime.Text.Length - 2), out int _startTime) && _startTime >= 0)
                {
                    drumMachine.StartFromTime((_startTime-1) * 1000 * 60 / bpm * 4);
                    drumMachine.PlayPattern(patternDictionary, muteTrackList);
                    //
                    //StartMusicWithScrolling(bpm, SourceWidth);
                }
                else
                {
                    MessageBox.Show("Invalid Start Time!");
                }
                
            }
            else
            {
                MessageBox.Show("Invalid BPM!");
            }
        }

        public class DrumMachine
        {

            string sourcepath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "source\\");

            private Dictionary<string, WaveOutEvent> outputs; //key有副檔名
            private Dictionary<string, AudioFileReader> samples;
            private readonly object releaseLock = new object();
            private readonly object outputsampleLock = new object();
            private int bpm;
            private int startTime;
            Stopwatch stopWatch = new Stopwatch();
            private Thread playbackThread;

            private bool isPlaying;
            public bool IsPlaying => isPlaying;

            public DrumMachine()
            {
                outputs = new Dictionary<string, WaveOutEvent>();
                samples = new Dictionary<string, AudioFileReader>();
                bpm = 120;
            }
            //public void SetTotalThreads(int threadCount)
            //{
            //    lock (threadLock)
            //    {
            //        totalThreads = threadCount;
            //        completedThreads = 0;
            //    }
            //}
            //private void ThreadCompleted()
            //{
            //    lock (threadLock)
            //    {
            //        completedThreads++;

            //        if (completedThreads == totalThreads)
            //        {
            //            ReleaseWaveOutEvent();
            //        }
            //    }
            //}
            public void StartFromTime(int milliseconds)
            {
                startTime = milliseconds;
            }
            public void LoadSamples(List<string> samplePaths)
            {
                foreach (var path in samplePaths)
                {
                    if (!samples.ContainsKey(path))
                    {
                        //Debug.WriteLine("HEY YOU LOOK HERE!!!!path: " + sourcepath + path);
                        var sample = new AudioFileReader(sourcepath + path);
                        samples.Add(path, sample);
                    }
                }
            }
            public void OutputSample(float vol, float bass, float mid, float treble, float tone, bool SCActive)
            {
                lock(outputsampleLock)
                {
                    foreach (var sample in samples)
                    {
                        var path = sample.Key;
                        var auido = sample.Value;

                        var semitone = (float)Math.Pow(2, 1.0 / 12);
                        var a1Pitched = new SmbPitchShiftingSampleProvider(auido.ToSampleProvider());

                        //Debug.WriteLine("tone: " + tone);
                        if (tone > 1)
                        {
                            a1Pitched.PitchFactor = (float)Math.Pow(semitone, tone);//tone * 
                        }
                        else
                        {
                            a1Pitched.PitchFactor = tone; // * (float)Math.Pow(semitone, tone - 1)
                        }
                        //Debug.WriteLine("tone*(float)Math.Pow(semitone, tone-1) = " + a1Pitched.PitchFactor);
                        auido.Volume = vol;
                        //Debug.WriteLine("path112233 = " + path);
                        if (!outputs.ContainsKey(path))
                        {
                            var output = new WaveOutEvent();
                            Equalizer equalizer = new Equalizer();
                            SideChain sideChain;

                            equalizer.Init(a1Pitched);
                            equalizer.SetBandGain(0, bass);
                            equalizer.SetBandGain(1, mid);
                            equalizer.SetBandGain(2, treble);

                            if (SCActive)
                            {
                                sideChain = new SideChain(equalizer, bpm, isActive: SCActive);
                                output.Init(sideChain);
                            }
                            else
                            {
                                output.Init(equalizer);
                            }
                            outputs.Add(path, output);
                        }
                    }
                }
            }
            public void SetBPM(int bpm)
            {
                this.bpm = bpm;
            }
            public void PlayPattern(Dictionary<string, SampleData> patternDictionary, Dictionary<string, bool> muteTrackList)
            {
                isPlaying = true;
                playbackThread = new Thread(() =>
                    PlayPatternThread(
                        patternDictionary,
                        muteTrackList
                    )
                );
                playbackThread.Start();
            }
            private void PlayPatternThread(Dictionary<string, SampleData> instrumentPatterns, Dictionary<string, bool> muteTrackList)
            {
                int interval = 60000 / (bpm * 4); // bpm*4
                int startStep = Math.Max(0, startTime / interval);
                var availableFiles = Directory.GetFiles(sourcepath).Select(Path.GetFileName).ToHashSet();

                for (int step = startStep; step < SectionOfLatestNote + 1; step++)
                {
                    //start
                    if (step == 0)
                        stopWatch.Start();
                    //end
                    else
                        stopWatch.Restart();
                    foreach (var instrumentPattern in instrumentPatterns)
                    {
                        string instrumentName = instrumentPattern.Key;//music name
                        List<PatternEQ> pattern = instrumentPattern.Value.Patterns;
                        bool active = pattern[step] == null ? false : pattern[step].IsActive; //System.ArgumentOutOfRangeException: '索引超出範圍。必須為非負數且小於集合的大小。
                        bool SCisActive = instrumentPattern.Value.sideChainActive;

                        float bass = (float)pattern[step].Bass;
                        float mid = (float)pattern[step].Mid;
                        float treble = (float)pattern[step].Treble;
                        float tone = (float)pattern[step].tone;
                        float vol = (float)instrumentPattern.Value.Volume;

                        string wavFile = instrumentName + ".wav";
                        string mp3File = instrumentName + ".mp3";
                        string selectedFile = availableFiles.Contains(wavFile) ? wavFile
                                            : availableFiles.Contains(mp3File) ? mp3File
                                            : null;
                        if (muteTrackList[instrumentName] && active)
                        {
                            if (selectedFile != null)
                                PlaySample(selectedFile, vol, bass, mid, treble, tone, SCisActive);
                            //else
                            //    PlaySample(instrumentName + ".mp3", vol, bass, mid, treble, tone, SCisActive);
                        }
                    }
                    stopWatch.Stop();
                    Debug.WriteLine("stopWatch.ElapsedMilliseconds = " + (int)stopWatch.ElapsedMilliseconds);
                    Debug.WriteLine("DDD = " + Math.Min(interval - (int)stopWatch.ElapsedMilliseconds, interval));

                    //Thread.Sleep(Math.Min(interval - (int)stopWatch.ElapsedMilliseconds, interval));
                    Thread.Sleep(interval); //Hello I'm 9ay. I am 張智堯. 我要 shampoo.
                }
            }
            private void PlaySample(string instrumentName, float vol, float bass, float mid, float treble, float tone, bool SCisActive)
            {
                if (outputs.ContainsKey(instrumentName))
                {
                    var output = outputs[instrumentName];
                    if (output.PlaybackState == PlaybackState.Playing)
                    {
                        output.Stop();
                    }
                    output.Dispose();
                    outputs.Remove(instrumentName);
                }

                OutputSample(vol, bass, mid, treble, tone,SCisActive);
                if (outputs.ContainsKey(instrumentName))
                {
                    samples[instrumentName].Position = 0;
                    outputs[instrumentName].Play();
                }
            }

            public void StopMusic()
            {
                isPlaying = false;
                playbackThread.Join();
                foreach (var output in outputs)
                {
                    output.Value.Stop();
                    output.Value.Dispose();
                }
                outputs.Clear();
            }
        }
    }
}