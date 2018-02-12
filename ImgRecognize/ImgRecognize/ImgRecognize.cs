using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgRecognize
{
    static class ImgRecognize
    {
        public const double PI = 3.14159;
        public static int width;
        public static int height;
        public static Color[,] pixels;
        public const int CircleBoardMaxRadius = 300;
        static Color BackgroundColor;
        static bool[,] possiblePoint;

        public struct Result
        {
            public int boardX;
            public int boardY;
            public int pieceX;
            public int pieceY;
            public Result(int px, int py, int bx, int by)
            {
                pieceX = px;
                pieceY = py;
                boardX = bx;
                boardY = by;
            }
            public double GetDistance()
            {
                return Math.Sqrt((boardX - pieceX) * (boardX - pieceX) + (boardY - pieceY) * (boardY - pieceY));
            }
        }


        public static void SetScreen(int w,int h)
        {
            width = w; height = h;
            pixels = new Color[height, width];
            possiblePoint = new bool[height, width];
        }
        public static void GetImgPixels(Bitmap img)
        {
            BitmapData bitmapdata = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* ptr = (byte*)bitmapdata.Scan0;
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        pixels[j, i] = Color.FromArgb(ptr[0], ptr[1], ptr[2]);
                        ptr += 3;
                    }
                    ptr += bitmapdata.Stride - bitmapdata.Width * 3;
                }
            }
            img.UnlockBits(bitmapdata);
        }
        public static int FindScanStartY()
        {
            int y=0;
            for (y = height/3; y < height * 2 / 3; y+=10)//10为步长寻找起始点
            {
                float lastHue = pixels[y, 0].GetHue();
                
                for (int x = 1; x < width; x++)
                {
                    if (Math.Abs(pixels[y, x].GetHue() - lastHue) >= 5) goto stop;
                }
            }
            stop:
            y -= 10;
            Console.WriteLine("scan start y:" + y.ToString());

            BackgroundColor = pixels[y, width / 2];

            return y;
        }


        public static bool PieceColor(Color c)
        {
            float hue = c.GetHue();
            float bri = c.GetBrightness();
            if (hue > 330 && hue < 365 && bri < 0.4) return true;
            return false;
        }


        public static Point FindPiece(Bitmap bitmap)
        {
            Bitmap bm = new Bitmap(bitmap);
            GetImgPixels(bitmap);
            int scanStartX = width / 8;
            int scanStartY=FindScanStartY();
            int xMin=width, xMax=0, yMin=height, yMax=0;
            for (int y = scanStartY; y < height * 2 / 3; y++)
            {
                for (int x = scanStartX; x < width - scanStartX; x++)
                {
                    if (PieceColor(pixels[y, x]))
                    {
                        if (x < xMin) xMin = x;
                        if (x > xMax) xMax = x;
                        if (y < yMin) yMin = y;
                        if (y > yMax) yMax = y;
                        bm.SetPixel(x, y, Color.Orange);
                        possiblePoint[y, x] = true;
                    }
                }
            }

            if (xMax - xMin > 85)//出错了 从中间分割寻找真正的piece
            {
                int mid = (xMax + xMin) / 2;
                int lCnt = 0, rCnt = 0;
                for (int i = xMin; i < mid; i++)
                {
                    for (int j = yMin; j < yMax; j++)
                    {
                        if (possiblePoint[j, i]) lCnt++;
                    }
                }
                for (int i = mid; i < xMax; i++)
                {
                    for (int j = yMin; j < yMax; j++)
                    {
                        if (possiblePoint[j, i]) rCnt++;
                    }
                }
                int scanx, scanxEnd;
                
                if (lCnt < rCnt) { 
                    scanx = mid; 
                    scanxEnd = xMax;
                    xMin = xMax;
                }
                else
                {
                    scanx = xMin;
                    scanxEnd = mid;
                    xMax = xMin ;
                }
                int oldYMin = yMin, oldYMax = yMax;
                yMin = oldYMax; yMax = oldYMin;
                for (int y = oldYMin; y < oldYMax; y++)
                {
                    for (int x = scanx; x < scanxEnd; x++)
                    {
                        if (PieceColor(pixels[y, x]))
                        {
                            if (x < xMin) xMin = x;
                            if (x > xMax) xMax = x;
                            if (y < yMin) yMin = y;
                            if (y > yMax) yMax = y;
                        }
                    }
                }
            }

            Graphics p = Graphics.FromImage(bm);
            p.DrawRectangle(new Pen(new SolidBrush(Color.Red), 2), new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin));
            //bm.Save("save.png");
            //return bm;
            return new Point((xMin+xMax)/2,yMax);
        }


        

        static int GetColorDis(Color a, Color b)
        {
            return Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
        }
        static float GetHueDel(Color a, Color b)
        {
            return Math.Abs(a.GetHue() - b.GetHue());
        }
        static double GetDis(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        static int GetLineSumDel(int cx, int cy, double angle)//获取一条倾斜直线上的像素的H与背景色的H差值绝对值之和 步长为2
        {
            int sum = 0;
            for (int x = 0; x < 50; x+=2)
            {
                int y = (int)((double)x * Math.Tan(angle));
                sum += (int)Math.Abs(pixels[cy-y, cx+x].GetHue() - BackgroundColor.GetHue());
                sum += (int)Math.Abs(pixels[cy + y, cx - x].GetHue() - BackgroundColor.GetHue());
            }
            return sum;
        }

        static int GetLineSumDel2(int cx, int cy, double angle)//获取一条倾斜直线上的点和背景色的RGB差值绝对值之和
        {
            int sum = 0;
            for (int x = 0; x < 50; x += 2)
            {
                int y = (int)((double)x * Math.Tan(angle));
                sum += GetColorDis(pixels[cy - y, cx + x], BackgroundColor);
                sum += GetColorDis(pixels[cy + y, cx - x], BackgroundColor);
            }
            return sum;
        }
        static Point GetLimPoint(int cx, int cy, double angle)
        {
            if (GetHueDel(pixels[cy, cx], BackgroundColor) <=5)
            {
                for (int dx = 0; dx < 50; dx++)
                {
                    int dy = (int)((double)dx*Math.Tan(angle));
                    if (GetHueDel(pixels[cy-dy, cx+dx], BackgroundColor) >=5)
                    {
                        return new Point(cx+dx, cy-dy);
                    }
                    else if (GetHueDel(pixels[cy + dy, cx - dx], BackgroundColor) >= 5)
                    {
                        return new Point(cx - dx, cy + dy);
                    }
                }
            }
            return new Point(cx,cy);
        }
/*
        public static Bitmap FindBoard_0(Bitmap bitmap)
        {
            Bitmap bm = new Bitmap(bitmap);
            GetImgPixels(bitmap);
            int scanStartX = width / 8;
            int scanStartY = FindScanStartY();
            for (int y = scanStartY; y < height * 2 / 3; y++)
            {
                for (int x = scanStartX; x < width - scanStartX; x++)
                {
                    float hueDel = Math.Abs(pixels[y, x].GetHue() - pixels[y-1, x].GetHue()) + Math.Abs(pixels[y+1, x-1].GetHue() - pixels[y, x].GetHue());
                    int rgbDel = GetColorDis(pixels[y, x], pixels[y + 1, x]) + GetColorDis(pixels[y, x], pixels[y + 1, x-1]);
                    if (hueDel < 2 && rgbDel > 100 && GetColorDis(pixels[y, x], pixels[y, x-1])<1)
                    {
                        
                        bm.SetPixel(x-1, y, Color.OrangeRed);
                        bm.SetPixel(x - 1, y+1, Color.OrangeRed);
                        bm.SetPixel(x, y, Color.OrangeRed);
                        bm.SetPixel(x, y + 1, Color.OrangeRed);
                    }
                    float hueDel = Math.Abs(pixels[y, x].GetHue() - pixels[y - 1, x].GetHue());
                    float rgbDel = GetColorDis(pixels[y, x], pixels[y - 1, x]);
                    if (Math.Abs(pixels[y,x].GetHue()-BackgroundColor.GetHue())>5 && hueDel < 3 && rgbDel > 20)
                    {
                        bm.SetPixel(x, y, Color.OrangeRed);
                    }
                }
            }
            return bm;
        }
        */
        public static Result FindBoard_1(Bitmap bitmap)
        {
            Point piecePos=FindPiece(bitmap);
            Bitmap bm = new Bitmap(bitmap);
            int xMin1 = piecePos.X, yMin1 = piecePos.Y;
            int y=piecePos.Y;
            for (int x = piecePos.X; x > 60&&y>height/3; x--)//向左寻找极限点
            {
              /*   y=piecePos.Y-(int)((double)(piecePos.X-x) / 1.7321);
                 bm.SetPixel(x, y, Color.OrangeRed);
                if (GetHueDel(pixels[y, x], BackgroundColor) > 5)
                {
                    xMin1 = x; yMin1 = y;
                }*/
                y = piecePos.Y - (int)((double)(piecePos.X - x) / 1.7321);
                bm.SetPixel(x, y, Color.OrangeRed);
                int sum = GetLineSumDel2(x, y, PI / 6);
               // Console.WriteLine(string.Format("（{0},{1}） sum={2}", x, y, sum));
                if (sum >= 500)
                {
                    xMin1 = x;
                    yMin1 = y;
                }
            }
            Point pt = GetLimPoint(xMin1, yMin1, PI / 6);
            xMin1 = pt.X; yMin1 = pt.Y;




            int xMax2=piecePos.X,yMin2=piecePos.Y;
            y=piecePos.Y;
            for(int x=piecePos.X;x<width-60&&y>height/3;x++){//向右寻找极限点
               /* y=piecePos.Y-(int)((double)(x-piecePos.X) / 1.7321);
                bm.SetPixel(x, y, Color.OrangeRed);
                if (GetHueDel(pixels[y, x], BackgroundColor) > 5)
                {
                    xMax2 = x; yMin2 = y;
                }*/
                y = piecePos.Y - (int)((double)(x - piecePos.X) / 1.7321);
                bm.SetPixel(x, y, Color.OrangeRed);
                int sum = GetLineSumDel2(x, y, -PI / 6);
                //Console.WriteLine(string.Format("（{0},{1}） sum={2}", x, y, sum));
                if (sum >= 500)
                {
                    xMax2 = x;
                    yMin2 = y;
                }

            }
            pt = GetLimPoint(xMax2, yMin2, -PI / 6);
            xMax2 = pt.X; yMin2 = pt.Y;





            double angle = PI / 6;//沿着滑块边沿的角度
            int xLim = xMin1, yLim = yMin1;
            
            if (GetDis(new Point(xMin1, yMin1), piecePos) < GetDis(new Point(xMax2, yMin2), piecePos))
            {
                xLim = xMax2;
                yLim = yMin2;
                angle = -PI / 6;
            }

            //xLim yLin为所求极限点
            //下面就是要从极限点入手找到Board
            //先试探一下是圆形还是矩形
            int cnt = 0;
            for (int x = -15; x < 15; x+=3)
            {
                y=yLim-(int)((double)x*Math.Tan(angle));
                if (GetHueDel(pixels[y + 1, x + xLim], BackgroundColor) > 10 && GetHueDel(pixels[y - 1, x + xLim], BackgroundColor)<3) cnt++;
            }
            //if (cnt <= 8)//可能圆是比较好处理的吧
            //{
                Console.WriteLine("Working!!!");
                Color boardColor = pixels[yLim+2, xLim + (angle > 0 ? 2 : -2)];
                int x2=xLim,y2=yLim;
                if (angle > 0)
                {
                    int temp = 0;
                    for (int i = 0; i < CircleBoardMaxRadius; i++)
                    {
                        if (GetHueDel(boardColor, pixels[yLim, xLim + i]) < 3)
                        {
                            x2 = xLim + i;
                            temp = 0;
                        }
                        else
                        {
                            temp++;
                        }
                        if (temp > 40) break;
                    }
                }
                else
                {
                    int temp = 0;
                    for (int i = 0; i >- CircleBoardMaxRadius; i--)
                    {
                        if (GetHueDel(boardColor, pixels[yLim, xLim + i]) < 3)
                        {
                            x2 = xLim + i;
                            temp = 0;
                        }
                        else
                        {
                            temp++;
                        }
                        if (temp > 40) break;
                    }
                }
                for (int i = 0; i < CircleBoardMaxRadius / 2; i++)
                {
                    if (GetHueDel(boardColor, pixels[yLim + i, xLim]) < 3&&GetColorDis(boardColor, pixels[yLim + i, xLim])<20) y2 = yLim + i;
                    else if (GetHueDel(boardColor, pixels[yLim - i, xLim]) < 3 && GetColorDis(boardColor, pixels[yLim - i, xLim]) < 20) y2 = yLim - i;
                }
                Graphics p = Graphics.FromImage(bm);
                p.DrawLine(new Pen(new SolidBrush(Color.Red), 2), xLim, yLim, xLim + 200, yLim - 115);
                p.DrawLine(new Pen(new SolidBrush(Color.Red), 2), xLim, yLim, xLim - 200, yLim - 115);

                int xMid = (xLim + x2) / 2, yMid = (yLim + y2) / 2;
                //p.DrawRectangle(new Pen(new SolidBrush(Color.Red), 2), xLim, yLim, Math.Abs(x2 - xLim), Math.Abs(y2 - yLim));
                p.DrawRectangle(new Pen(new SolidBrush(Color.Red), 2), xMid-1, yMid-1, 2, 2);
                p.DrawRectangle(new Pen(new SolidBrush(Color.Green), 2), xLim - 1, yLim - 1, 2, 2);
            //}
            Result res = new Result(piecePos.X, piecePos.Y, xMid, yMid);
           //    return bm;
            Console.WriteLine("Target Found!!!");
            Console.WriteLine(string.Format("Piece pos({0},{1}) BoradPos({2},{3})",piecePos.X,piecePos.Y,xMid,yMid));
            
            return res;
        }
    }
}
