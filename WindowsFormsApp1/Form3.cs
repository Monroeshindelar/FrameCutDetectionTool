using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1 {
    public partial class Form3 : Form {
        int startIndex;
        int saveIndex;
        List<Bitmap> frames;
        bool videoPlaying;
        public Form3(int startFrame, List<Bitmap> video) {
            InitializeComponent();
            startIndex = startFrame;
            saveIndex = startIndex;
            frames = video;
            timer1 = new Timer();
            timer1.Interval = 1000 / 25;
            timer1.Tick += WhenTimerTicks;
            pictureBox1.Image = video[startFrame];
            textBox1.Text = "Starting from frame: " + (startIndex + 1000);
            videoPlaying = false;
        }

        void WhenTimerTicks(object sender, EventArgs e) {
            if (startIndex < frames.Count()) {
                pictureBox1.Image = frames[startIndex++];
            } else if(startIndex == frames.Count()) {
                timer1.Stop();
                startIndex = saveIndex;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            if (!videoPlaying) {
                timer1.Start();
                videoPlaying = true;
            } else {
                timer1.Stop();
                videoPlaying = false;
            }
        }
    }
}
