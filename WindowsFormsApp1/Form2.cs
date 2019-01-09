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
    public partial class Form2 : Form {
        Form1 originalForm; //First form
        Form3 display; //Form to display specific videos
        List<Bitmap> frames; //list of bitmaps of video frames
        double Ts = 0; //Threshold for transitions
        double Tb = 0; //Threshold for cuts
        double[] sd; //array of frame differences
        List<int> cuts; //list of the cuts
        List<int> fades; //list of the fades
        int tor = 2; //threshold for frames lower than the Ts threshold
        List<int[]> frameIntensityHistograms; //list of the intensity histograms for each frame
        List<int> tempList; //list to store sorted cuts and gradual transitions
        int currentIndex = 0; //for walking the frames when video is playing.
        int page; //keep track of the page
        int totalPages; //keep track of total pages
        bool videoPlaying;

        public Form2(Form1 form) {
            originalForm = form;
            frameIntensityHistograms = new List<int[]>();
            cuts = new List<int>();
            fades = new List<int>();
            InitializeComponent();
            timer1 = new Timer();
            timer1.Interval = 1000 / 25;
            timer1.Tick += WhenTimerTicks;
            videoPlaying = false;
        }
        //Gets the cut and gradual trainsition results from the file
        public void getResults(List<Bitmap> queryVideoFrames) {
            frames = queryVideoFrames;
            //init intensity values for every frame
            System.Diagnostics.Debug.WriteLine(queryVideoFrames.Count());
            for(int i = 0; i < queryVideoFrames.Count(); i++) {
                System.Diagnostics.Debug.WriteLine(i);
                Bitmap currentFrame = queryVideoFrames[i];
                frameIntensityHistograms.Add(intensity(currentFrame));
            }
            //set SD
            int[] current;
            int[] next;
            sd = new double[queryVideoFrames.Count()];
            int res = queryVideoFrames[0].Width * queryVideoFrames[0].Height;
            for (int i = 0; i < frameIntensityHistograms.Count() - 1; i++) {
                current = frameIntensityHistograms[i];
                next = frameIntensityHistograms[i + 1];
                sd[i] = manhattan(current, next, res, res);
            }
            //setting thresholds for frame cuts and gradual transitions
            double mean = 0;
            double std = 0;
            for (int i = 0; i < sd.Length; i++) mean += sd[i];
            mean = mean / sd.Length;
            for(int i = 0; i < sd.Length; i++) std += Math.Pow(sd[i] - mean, 2);
            std /= queryVideoFrames.Count();
            std = Math.Sqrt(std);
            Tb = mean + std * 11;
            Ts = mean * 2;
            //finding the frame cuts and gradual transitions
            int Fs_candi = -1;
            int Fe_candi = -1;
            int consecutive = 0;
            double sum_candi = 0;
            for(int i = 0; i < sd.Length; i++) {
                sum_candi = 0;
                if (sd[i] >= Tb) cuts.Add(i);
                if(sd[i] >= Ts && sd[i] < Tb) {
                    Fs_candi = i;
                    consecutive = 0;
                    for(int j = Fs_candi; consecutive < tor && j < sd.Length; j++) {
                        if (sd[j] >= Tb) {
                            cuts.Add(j);
                            Fe_candi = j;
                            i = Fe_candi + 1;
                            break;
                        } else if (sd[j] >= Ts && sd[j] < Tb) {
                            consecutive = 0;
                            Fe_candi = j;
                        } else if (sd[j] < Ts) {
                            consecutive++;
                        }
                    }
                    if (consecutive == tor) {
                        for (int k = Fs_candi; k < Fe_candi + 1; k++) sum_candi += sd[k];
                        if (sum_candi >= Tb) fades.Add(Fs_candi);
                        i = Fe_candi + 2;
                    }
                }
            }
            //calculating number of pages
            double numberPerPage = ((cuts.Count() + fades.Count()) / 20.0);
            totalPages = (int)Math.Ceiling(numberPerPage);
            textBox1.Text = "Page 1 /Out of " + totalPages;
            page = 0;
        }
        //calculates the difference between the frames
        private double manhattan(int[] frame1, int[] frame2, int frame1Res, int frame2Res) {
            double retVal = 0;
            for(int i = 0; i < frame1.Length; i++) {
                retVal += Math.Abs((frame1[i] / (double)frame1Res) - (frame2[i] / (double)frame2Res));
            }
            return retVal; 
        }
        //calculates the intensity of a single frame, represented by a histogram
        private int[] intensity(Bitmap frame) {
            int r, g, b = 0;
            double intensity = 0;
            int[] intensityHistogram = new int[25];
            for(int i = 0; i < frame.Width; i++) {
                for(int j = 0; j < frame.Height; j++) {
                    Color current = frame.GetPixel(i, j);
                    r = current.R;
                    g = current.G;
                    b = current.B;
                    intensity = (0.299 * r) + (0.587 * g) + (0.114 * b);
                    if (intensity < 250) intensityHistogram[(int)intensity / 10]++;
                    else intensityHistogram[24]++;
                }
            }
            return intensityHistogram;
        }
        //change the frame when the timer ticks
        void WhenTimerTicks(object sender, EventArgs e) {
            if (currentIndex < frames.Count()) {
                pictureBox21.Image = frames[currentIndex++];
            } else if(currentIndex == frames.Count()) {
                timer1.Stop();
                currentIndex = 0;
            }
        }
        //displays the cuts and transitions in the second frame
        public void displayResults() {
            tempList = new List<int>();
            pictureBox21.Image = frames[0];
            int offset = page * 20;
            for (int i = 0; i < cuts.Count(); i++) tempList.Add(cuts[i]);
            for (int i = 0; i < fades.Count(); i++) tempList.Add(fades[i]);
            tempList.Sort();
            if(0 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[0 + offset]];
                pictureBox1.Image = image;
            } else {
                pictureBox1.Image = null;
            }
            if (1 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[1 + offset]];
                pictureBox2.Image = image;
            } else {
                pictureBox2.Image = null;
            }
            if (2 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[2 + offset]];
                pictureBox3.Image = image;
            } else {
                pictureBox3.Image = null;
            }
            if (3 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[3 + offset]];
                pictureBox4.Image = image;
            } else {
                pictureBox4.Image = null;
            }
            if (4 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[4 + offset]];
                pictureBox5.Image = image;
            } else {
                pictureBox5.Image = null;
            }
            if (5 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[5 + offset]];
                pictureBox6.Image = image;
            } else {
                pictureBox6.Image = null;
            }
            if (6 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[6 + offset]];
                pictureBox7.Image = image;
            } else {
                pictureBox7.Image = null;
            }
            if (7 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[7 + offset]];
                pictureBox8.Image = image;
            } else {
                pictureBox8.Image = null;
            }
            if (8 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[8 + offset]];
                pictureBox9.Image = image;
            } else {
                pictureBox9.Image = null;
            }
            if (9 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[9 + offset]];
                pictureBox10.Image = image;
            } else {
                pictureBox10.Image = null;
            }
            if (10 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[10 + offset]];
                pictureBox11.Image = image;
            } else {
                pictureBox11.Image = null;
            }
            if (11 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[11 + offset]];
                pictureBox12.Image = image;
            } else {
                pictureBox12.Image = null;
            }
            if (12 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[12 + offset]];
                pictureBox13.Image = image;
            } else {
                pictureBox13.Image = null;
            }
            if (13 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[13 + offset]];
                pictureBox14.Image = image;
            } else {
                pictureBox14.Image = null;
            }
            if (14 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[14 + offset]];
                pictureBox15.Image = image;
            } else {
                pictureBox15.Image = null;
            }
            if (15 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[15 + offset]];
                pictureBox16.Image = image;
            } else {
                pictureBox16.Image = null;
            }
            if (16 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[16 + offset]];
                pictureBox17.Image = image;
            } else {
                pictureBox17.Image = null;
            }
            if (17 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[17 + offset]];
                pictureBox18.Image = image;
            } else {
                pictureBox18.Image = null;
            }
            if (18 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[18 + offset]];
                pictureBox19.Image = image;
            } else {
                pictureBox19.Image = null;
            }
            if (19 + offset < tempList.Count()) {
                Bitmap image = frames[tempList[19 + offset]];
                pictureBox20.Image = image;
            } else {
                pictureBox20.Image = null;
            }
        }
        //sets up all the picture boxes so that if you click them the video plays
        private void pictureBox21_Click(object sender, EventArgs e) {
            if (!videoPlaying) {
                timer1.Start();
                videoPlaying = true;
            } else if(videoPlaying) {
                timer1.Stop();
                videoPlaying = false;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[0 + offset], frames);
            display.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[1 + offset], frames);
            display.Show();
        }

        private void pictureBox3_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[2 + offset], frames);
            display.Show();
        }

        private void pictureBox4_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[3 + offset], frames);
            display.Show();
        }

        private void pictureBox5_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[4 + offset], frames);
            display.Show();
        }

        private void pictureBox6_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[5 + offset], frames);
            display.Show();
        }

        private void pictureBox7_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[6 + offset], frames);
            display.Show();
        }

        private void pictureBox8_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[7 + offset], frames);
            display.Show();
        }

        private void pictureBox9_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[8 + offset], frames);
            display.Show();
        }

        private void pictureBox10_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[9 + offset], frames);
            display.Show();
        }

        private void pictureBox11_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[10 + offset], frames);
            display.Show();
        }

        private void pictureBox12_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[11 + offset], frames);
            display.Show();
        }

        private void pictureBox13_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[12 + offset], frames);
            display.Show();
        }

        private void pictureBox14_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[13 + offset], frames);
            display.Show();
        }

        private void pictureBox15_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[14 + offset], frames);
            display.Show();
        }

        private void pictureBox16_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[15 + offset], frames);
            display.Show();
        }

        private void pictureBox17_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[16 + offset], frames);
            display.Show();
        }

        private void pictureBox18_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[17 + offset], frames);
            display.Show();
        }

        private void pictureBox19_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[18 + offset], frames);
            display.Show();
        }

        private void pictureBox20_Click(object sender, EventArgs e) {
            int offset = page * 20;
            display = new Form3(tempList[19 + offset], frames);
            display.Show();
        }
        //moves to the next page
        private void button1_Click(object sender, EventArgs e) {
            if (page < totalPages - 1) {
                page += 1;
                textBox1.Text = "Page " + (page + 1) + "/Out of " + totalPages;
                displayResults();
            }
        }
        //moves to the previous page
        private void button2_Click(object sender, EventArgs e) {
            if(page > 0) {
                page -= 1;
                textBox1.Text = "Page " + (page + 1) + "/Out of " + totalPages;
                displayResults();
            } else {
                page = totalPages - 1;
                textBox1.Text = "Page " + (page + 1) + "/Out of " + totalPages;
                displayResults();
            }
        }
    }
}
