using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaiGua
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        Thread updateThread;
        int ox=0, oy=0;
        private void Form1_Load(object sender, EventArgs e)
        {
            updateThread = new Thread(ScreenUpdate);
            updateThread.Start();
        }

        void ScreenUpdate()
        {
            while (true)
            {
                try
                {
                    
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = @"adb.exe"; //启动的应用程序名称  
                    startInfo.Arguments = "shell screencap -p /sdcard/temp.png";
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    Process adbProgram = new Process();
                    adbProgram.StartInfo = startInfo;
                    adbProgram.Start();
                    adbProgram.WaitForExit();


                    startInfo.Arguments = "pull /sdcard/temp.png";
                    adbProgram.StartInfo = startInfo;
                    adbProgram.Start();
                    adbProgram.WaitForExit();
                    //MessageBox.Show("Exit");
                    Stream s = File.Open("temp.png", FileMode.Open);
                    pictureBox1.Image = Image.FromStream(s);
                    s.Close();
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    //throw;
                    Console.WriteLine("Fail");
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (hoverTime != 0)
            {
             /*   try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = @"D:\Android Sdk\platform-tools\adb.exe"; //启动的应用程序名称  
                    startInfo.Arguments = "adb shell input swipe 540 1600 540 1600 "
                        + (hoverTime).ToString()+"\n";


                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    Process adbProgram = new Process();
                    adbProgram.StartInfo = startInfo;
                    adbProgram.Start();
                    Console.WriteLine(startInfo.Arguments);
                    adbProgram.WaitForExit();
                   // hoverTime = 0;
                }
                catch { }*/
                Jump();
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {

            
        }
        int hoverTime=0;
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                timer1.Enabled = false;
                ox = e.X;
                oy = e.Y;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                double dis = Math.Sqrt((e.X - ox) * (e.X - ox) + (e.Y - oy) * (e.Y - oy)) * 3;
                hoverTime = (int)(dis * 1.4286);
               // hoverTime = (int)(dis * 1.35);
                Console.WriteLine(dis);
                // pictureBox1.Refresh();
                Graphics p = pictureBox1.CreateGraphics();
                p.DrawLine(new Pen(new SolidBrush(Color.Red), 2), ox, oy, e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (hoverTime != 0)
                {
                    Jump();
                }
            }
        }


        private void WriteCMD()
        {
            Random random = new Random();
            int posX = random.Next() % 720 + 200;
            int posY = random.Next() % 1500 + 300;
            string str = string.Format("adb shell input swipe {0} {1} {2} {3} {4}", posX, posY, posX, posY, hoverTime);
           // string str = "adb shell input swipe 540 1600 580 1600 " + hoverTime.ToString();
            StreamWriter writer = new StreamWriter("adbtest.bat",false,Encoding.Default);
            writer.WriteLine(str);
            writer.Close();
        }

        void Jump()
        {
            if (hoverTime == 0) return;
            WriteCMD();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"adbtest.bat"; //启动的应用程序名称  

            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process adbProgram = new Process();
            adbProgram.StartInfo = startInfo;
            adbProgram.Start();
            adbProgram.WaitForExit();
        }
        private void button2_Click(object sender, EventArgs e)
        {


        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int radius = (int)Math.Sqrt((e.X - ox) * (e.X - ox) + (e.Y - oy) * (e.Y - oy));
                pictureBox1.Refresh();
                Graphics p = pictureBox1.CreateGraphics();
                p.DrawEllipse(new Pen(new SolidBrush(Color.Red), 2), ox - radius, oy - radius, 2 * radius, 2 * radius);
            }
        }
    }
}
