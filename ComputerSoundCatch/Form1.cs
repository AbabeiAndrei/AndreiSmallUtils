using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSCore.Codecs.WAV;
using CSCore.SoundIn;

namespace ComputerSoundCatch
{
    public partial class Form1 : Form
    {
        private SemaphoreSlim semaphore;
        private bool _recording;

        public Form1()
        {
            InitializeComponent();
            semaphore = new SemaphoreSlim(1);
            _recording = false;
        }

        private async void btnRecord_Click(object sender, EventArgs e)
        {
            if (_recording)
            {
                semaphore.Release();
                _recording = false;
                btnRecord.Text = "Record";
                return;
            }

            _recording = true;
            btnRecord.Text = "Stop";
            await Task.Factory.StartNew(StartRecord);
        }

        private void StartRecord()
        {
            using (WasapiCapture capture = new WasapiLoopbackCapture())
            {
                //if nessesary, you can choose a device here
                //to do so, simply set the device property of the capture to any MMDevice
                //to choose a device, take a look at the sample here: http://cscore.codeplex.com/

                //initialize the selected device for recording
                capture.Initialize();

                //create a wavewriter to write the data to
                using (var stream = new FileStream("dump.wav", FileMode.CreateNew))
                {
                    using (var w = new WaveWriter(stream, capture.WaveFormat))
                    {
                        //setup an eventhandler to receive the recorded data
                        capture.DataAvailable += (s, args) =>
                        {
                            //save the recorded audio
                            w.Write(args.Data, args.Offset, args.ByteCount);
                        };

                        //start recording
                        capture.Start();

                        SpinWait.SpinUntil(() => !_recording);

                        //stop recording
                        capture.Stop();
                    }
                    stream.Flush(true);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

        }
    }
}
