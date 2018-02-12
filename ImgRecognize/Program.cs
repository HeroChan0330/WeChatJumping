using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImgRecognize
{
    class Program
    {


        static int hoverTime = 0;
        static int swipe_X1, swipe_X2, swipe_Y1, swipe_Y2;

        static void WriteCMD()
        {
            string str = string.Format("adb shell input swipe {0} {1} {2} {3} {4}", swipe_X1, swipe_Y1, swipe_X2, swipe_Y2, hoverTime);
            // string str = "adb shell input swipe 540 1600 580 1600 " + hoverTime.ToString();
            StreamWriter writer = new StreamWriter("adbtest.bat", false, Encoding.Default);
            writer.WriteLine(str);
            writer.Close();
        }

        static void Jump()
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

        static void SetSwipe()
        {
            Random random = new Random();
            swipe_X1 = ImgRecognize.width / 2 + random.Next(-ImgRecognize.width * 3 / 8, ImgRecognize.width * 3 / 8);
            swipe_Y1 = random.Next(-100, 100) + 1600*ImgRecognize.height/1920;
            swipe_X2 = swipe_X1; swipe_Y2 = swipe_Y1;
        }

        static void SetButton()
        {
                Random random = new Random();
                int left = (ImgRecognize.width / 2);
                int top = 1584 *ImgRecognize.height/1920;
                left +=random.Next(-50,50);
                top +=random.Next(-10,10);
                swipe_X1 = left; swipe_X2 = left;
                swipe_Y1 = top; swipe_Y2 = top;
        }

        static void PullScreenShot()
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
                    
        }


        static void Main(string[] args)
        {
            ImgRecognize.SetScreen(1080, 1920);
            /*
            DirectoryInfo dir = new DirectoryInfo("train_data");
            foreach (FileInfo file in dir.GetFiles())
            {

                Bitmap bitmap = (Bitmap)Image.FromFile(file.FullName);

                ImgRecognize.FindBoard_1(bitmap).Save(@"OutputData\" + file.Name);
            }*/
          /*  Bitmap bitmap = (Bitmap)Image.FromFile("091819.png");
            ImgRecognize.FindBoard_1(bitmap).Save("save.png");*/
           
            while (true)
            {
                PullScreenShot();
                Thread.Sleep(500);
                Bitmap bitmap = (Bitmap)Image.FromFile(@"temp.png");
                ImgRecognize.Result res=ImgRecognize.FindBoard_1(bitmap);
                bitmap.Dispose();
                if(res!=null){
                    hoverTime = (int)(1.45 * (double)res.GetDistance());
                    Console.WriteLine("HoverTime:" + hoverTime.ToString()+"\n");
                    SetSwipe();
                }
                else
                {
                    hoverTime = 100;
                    Console.WriteLine("Restart");
                    SetButton();
                }
                Jump();
                Thread.Sleep(hoverTime+500);
            }

            while (true) ;
        }
    }
}
