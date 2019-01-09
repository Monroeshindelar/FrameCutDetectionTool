using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.FFMPEG;

namespace WindowsFormsApp1 {
    public partial class Form1 : Form {
        Form2 results; //results form
        string queryFileName; //video file name
        List<Bitmap> queryFileFrames; //list of frame bitmaps
        int currentIndex = 0; //for walking the list when the video is playing
        bool videoPlaying = false; //bool to control video stop/start
        public Form1() {
            InitializeComponent();
            queryFileFrames = new List<Bitmap>();
            timer1 = new Timer();
            timer1.Interval = 1000 / 25;
            timer1.Tick += WhenTimerTicks;
        }
        //Search Videos Button.
        //Opens File Dialog Box to Search for a video file
        private void button2_Click(object sender, EventArgs e) {
            //Filter stuff
            openFileDialog1.Filter = "AVI Files|*.avi";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.FileName = "";
            openFileDialog1.ShowDialog();
            //loops through the video file and gets the frame
            if (openFileDialog1.FileName != "") {
                queryFileName = openFileDialog1.FileName;
                VideoFileReader reader = new VideoFileReader();
                reader.Open(queryFileName);
                for (int i = 0; i < 5000; i++) {
                    Bitmap videoFrame = reader.ReadVideoFrame();
                    if(i >= 1000) queryFileFrames.Add(videoFrame);
                }
                pictureBox1.Image = queryFileFrames[0];
            }
        }
        //Does the calculations
        //Opens a new frame
        private void button1_Click(object sender, EventArgs e) {
            if (queryFileFrames.Count > 0) {
                results = new Form2(this);
                results.getResults(queryFileFrames);
                results.displayResults();
                results.Show();
                this.Hide();
            }
        }
        //Controls what happens when the timer ticks
        void WhenTimerTicks(object sender, EventArgs e) {
            if (currentIndex < queryFileFrames.Count()) {
                pictureBox1.Image = queryFileFrames[currentIndex++];
            }
        }
        //Plays the video when the picture box is clicked
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