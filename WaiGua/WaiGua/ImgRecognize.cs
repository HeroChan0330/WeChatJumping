using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaiGua
{
    static class ImgRecognize
    {
        /*public static Bitmap img;
        public static byte[,] bwArray = new byte[190, 360];
        //为了提高速度 横向和纵向像素点除三
        public static void Blackize()
        {
            BitmapData bitmapData = img.LockBits(new Rectangle(0, 700, 1080, 570), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                for (int j = 0; j < 190; j++)
                {
                    for (int i = 0; i < 360; i++)
                    {
                        int sum = ptr[0] + ptr[1] + ptr[2];
                        bwArray[j, i] = (byte)(sum / 3);
                        ptr += 9;
                    }
                    ptr += 3 * bitmapData.Stride - bitmapData.Width * 3;
                }
            }
        }
        public static Point startPoint, destinationPoint;

        public static Bitmap Analyze()
        {
            Bitmap bitmap = GetImg();
            byte bgColor = (byte)((bwArray[0, 0] + bwArray[0, 1]) / 2);
            int topCenterX = 0, topCenterY = 0;
            int px1 = 0, py1 = 0;
            int lEst = 360, rEst = 0;
            int lrEstY = 0;
            for (int j = 0; j < 110; j++)
            {
                int lEst_tmp = 360, rEst_tmp = 0;
                for (int i = 0; i < 360; i++)
                {
                    if (Math.Abs(bwArray[j, i] - bgColor) > 3)
                    {
                        if (topCenterX == 0 && topCenterY == 0)
                        {
                            topCenterX = i; topCenterY = j;
                            goto nextLine;
                        }
                        if (i < lEst_tmp) lEst_tmp = i;
                        if (i > rEst_tmp) rEst_tmp = i;
                        px1 = i; py1 = j;
                        //bitmap.SetPixel(i, j, Color.FromArgb(255, 0, 0));
                        //Console.WriteLine(string.Format("x={0},y={1}", px1, py1));
                        //goto endloop;
                    }
                }

                if (lEst != 360 && rEst != 0)
                {
                    double angle1 = Math.Atan2(lEst - topCenterX, lrEstY - topCenterY);
                    double angle2 = Math.Atan2(lEst_tmp - lEst, j - lrEstY);
                    Console.WriteLine(Math.Abs(angle1 - angle2));
                    if (Math.Abs(angle1 - angle2) > 0.523) break;
                }


                lEst = lEst_tmp; rEst = rEst_tmp;
                lrEstY = j;
                if (lEst != 360 && rEst != 0)
                {
                    bitmap.SetPixel(lEst_tmp, j, Color.FromArgb(255, 0, 0));
                    bitmap.SetPixel(rEst_tmp, j, Color.FromArgb(255, 0, 0));
                }
            nextLine:
                px1 = 0;
            }
        endloop:
            Console.WriteLine(string.Format("lEst={0},rEst={1}", lEst, rEst));
            return bitmap;
            // int sumL=(byte)(bwArray[)

        }
        public static Bitmap GetImg()
        {
            Bitmap res = new Bitmap(360, 190);
            //  BitmapData bitmapData = res.LockBits(new Rectangle(0, 0, 360, 150), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            for (int j = 0; j < 190; j++)
            {
                for (int i = 0; i < 360; i++)
                {
                    res.SetPixel(i, j, Color.FromArgb(bwArray[j, i], bwArray[j, i], bwArray[j, i]));
                }
            }
            return res;
        }

        */

        public struct PB
        {
            public int piece_x;
            public int piece_y;
            public int board_x;
            public int board_y;
            public PB(int px,int py,int bx,int by){
                piece_x=px;
                piece_y=py;
                board_x=bx;
                board_y=by;
            }
        }



        public static Color[,] im_pixel;
        //# 二分之一的棋子底座高度，可能要调节
        public static int piece_base_height_1_2=20;
        public static int piece_body_width=70;

       public static void SetScreen(int width, int height)
        {
            im_pixel=new Color[width,height];
        }

        static void GetPixels(Bitmap img)
        {
            BitmapData bitmapData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            unsafe{
                byte*ptr=(byte*)bitmapData.Scan0;
                for(int j=0;j<img.Height;j++){
                    for(int i=0;i<img.Width;i++){
                        im_pixel[i, j] = Color.FromArgb(ptr[0], ptr[1], ptr[2]);
                        ptr+=3;
                    }
                    ptr+=bitmapData.Stride-img.Width*3;
                }
            }
            img.UnlockBits(bitmapData);
        }
             public static PB find_piece_and_board(Bitmap bitmap)
             {
        
                 
                 
                int  w=bitmap.Width, h = bitmap.Height;
                int piece_x_sum = 0;
                int piece_x_c = 0;
                int piece_y_max = 0;
                int board_x = 0;
                int board_y = 0;
                int scan_x_border = w / 8;  
                int scan_start_y = 0; 
                int i=0,j=0;
                 Color pixel;
                    Color last_pixel;
                    GetPixels(bitmap);
                    for(i=h/3;i<=h*2/3;i+=50){//以 50px 步长，尝试探测 scan_start_y
                        last_pixel=im_pixel[0,i];
                        for( j=1;j<w;j++){
                             pixel=im_pixel[j,i];
                            if(!pixel.Equals(last_pixel)){//不是纯色的线，则记录 scan_start_y 的值，准备跳出循环
                                scan_start_y=i-50;
                                break;
                            }  
                        }
                        if(scan_start_y!=0)break;
                    }
                    Console.WriteLine("scan_start_y:"+scan_start_y.ToString());
            // 从 scan_start_y 开始往下扫描，棋子应位于屏幕上半部分，这里暂定不超过 2/3
                 for( i=scan_start_y;i<=h*2/3;i++){
                     for( j=scan_x_border;j<=w-scan_x_border;j++){
                          pixel=im_pixel[j,i];
                        /*# 根据棋子的最低行的颜色判断，找最后一行那些点的平均值，这个颜
                        # 色这样应该 OK，暂时不提出来*/
                         if( (pixel.R>50&&pixel.R<60)||(pixel.G>53&&pixel.G<63)||(pixel.B>95&&pixel.B<110)){
                            piece_x_sum+=j;
                             piece_x_c+=1;
                             piece_y_max=(i>piece_y_max?i:piece_y_max);
                         }
                     }
                  }
                 if(piece_x_sum==0||piece_x_c==0){
                     return new PB(0,0,0,0);
                 }
                 int piece_x=piece_x_sum/piece_x_c;
                 int piece_y=piece_y_max-piece_base_height_1_2;
                 //# 限制棋盘扫描的横坐标，避免音符 bug
                 int borad_x_start,board_x_end;
                 if(piece_x<w/2){
                     borad_x_start=piece_x;
                     board_x_end=w;
                 }
                 else{
                     borad_x_start=0;
                     board_x_end=piece_x;
                 }
                 int board_x_sum=0,board_x_c=0;
                 for( i=h/3;i<=h*2/3;i++){
                     last_pixel=im_pixel[0,i];
                     if(board_x!=0||board_y!=0){
                         break;
                     }
                     board_x_sum=0;
                     board_x_c=0;
                     for( j=borad_x_start;j<board_x_end;j++){
                          pixel=im_pixel[j,i];
                         if(Math.Abs(j-piece_x)<piece_body_width){
                             continue;
                         }
                         if(Math.Abs(pixel.R-last_pixel.R)+Math.Abs(pixel.R-last_pixel.R)+Math.Abs(pixel.G-last_pixel.G)>10){
                             board_x_sum+=j;
                             board_x_c+=1;
                         }
                     }
                     if(board_x_sum!=0){
                         board_x=board_x_sum/board_x_c;
                     }
                 }
                 last_pixel=im_pixel[board_x,i];
                      /*
                        # 从上顶点往下 +274 的位置开始向上找颜色与上顶点一样的点，为下顶点
                        # 该方法对所有纯色平面和部分非纯色平面有效，对高尔夫草坪面、木纹桌面、
                        # 药瓶和非菱形的碟机（好像是）会判断错误
                    */
                 int k;
                 for(k=i+274;k>=i;k--){
                     pixel=im_pixel[board_x,j];
                     if(Math.Abs(pixel.R-last_pixel.R)+Math.Abs(pixel.R-last_pixel.G)+Math.Abs(pixel.B-last_pixel.B)<10){
                         board_y=j+10;
                         break;
                     }
                 }
                 board_y=(i+k)/2;
                 /*
                    # 如果上一跳命中中间，则下个目标中心会出现 r245 g245 b245 的点，利用这个
                    # 属性弥补上一段代码可能存在的判断错误
                    # 若上一跳由于某种原因没有跳到正中间，而下一跳恰好有无法正确识别花纹，则有
                    # 可能游戏失败，由于花纹面积通常比较大，失败概率较低
                */
                 for(j=i;j<i+200;j++){
                     pixel=im_pixel[board_x,j];
                     if (Math.Abs(pixel.R - 245) + Math.Abs(pixel.G - 245) + Math.Abs(pixel.B - 245) ==0)
                     {
                        board_y=j+10;
                         break;
                     }

                 }
                 if(board_y==0||board_y==0){
                     return new PB(0,0,0,0);
                 }

                 return new PB(piece_x,piece_y,board_x,board_y);
             }

             public static PB Analyze(Bitmap img)
             {
                 return find_piece_and_board(img);
             }
             public static double GetDistance(Bitmap img)
             {
                 PB data = find_piece_and_board(img);
                 double dis = Math.Sqrt((data.board_x - data.piece_x) * (data.board_x - data.piece_x) + (data.board_y - data.piece_y) * (data.board_y - data.piece_y));
                 return dis;
             }
            
    }

        /*

def find_piece_and_board(im):
    """
    寻找关键坐标
    """
    w, h = im.size

    piece_x_sum = 0
    piece_x_c = 0
    piece_y_max = 0
    board_x = 0
    board_y = 0
    scan_x_border = int(w / 8)  # 扫描棋子时的左右边界
    scan_start_y = 0  # 扫描的起始 y 坐标
    im_pixel = im.load()
    # 以 50px 步长，尝试探测 scan_start_y
    for i in range(int(h / 3), int(h*2 / 3), 50):
        last_pixel = im_pixel[0, i]
        for j in range(1, w):
            pixel = im_pixel[j, i]
            # 不是纯色的线，则记录 scan_start_y 的值，准备跳出循环
            if pixel != last_pixel:
                scan_start_y = i - 50
                break
        if scan_start_y:
            break
    print('scan_start_y: {}'.format(scan_start_y))

    # 从 scan_start_y 开始往下扫描，棋子应位于屏幕上半部分，这里暂定不超过 2/3
    for i in range(scan_start_y, int(h * 2 / 3)):
        # 横坐标方面也减少了一部分扫描开销
        for j in range(scan_x_border, w - scan_x_border):
            pixel = im_pixel[j, i]
            # 根据棋子的最低行的颜色判断，找最后一行那些点的平均值，这个颜
            # 色这样应该 OK，暂时不提出来
            if (50 < pixel[0] < 60) \
                    and (53 < pixel[1] < 63) \
                    and (95 < pixel[2] < 110):
                piece_x_sum += j
                piece_x_c += 1
                piece_y_max = max(i, piece_y_max)

    if not all((piece_x_sum, piece_x_c)):
        return 0, 0, 0, 0
    piece_x = int(piece_x_sum / piece_x_c)
    piece_y = piece_y_max - piece_base_height_1_2  # 上移棋子底盘高度的一半

    # 限制棋盘扫描的横坐标，避免音符 bug
    if piece_x < w/2:
        board_x_start = piece_x
        board_x_end = w
    else:
        board_x_start = 0
        board_x_end = piece_x

    for i in range(int(h / 3), int(h * 2 / 3)):
        last_pixel = im_pixel[0, i]
        if board_x or board_y:
            break
        board_x_sum = 0
        board_x_c = 0

        for j in range(int(board_x_start), int(board_x_end)):
            pixel = im_pixel[j, i]
            # 修掉脑袋比下一个小格子还高的情况的 bug
            if abs(j - piece_x) < piece_body_width:
                continue

            # 修掉圆顶的时候一条线导致的小 bug，这个颜色判断应该 OK，暂时不提出来
            if abs(pixel[0] - last_pixel[0]) \
                    + abs(pixel[1] - last_pixel[1]) \
                    + abs(pixel[2] - last_pixel[2]) > 10:
                board_x_sum += j
                board_x_c += 1
        if board_x_sum:
            board_x = board_x_sum / board_x_c
    last_pixel = im_pixel[board_x, i]

    # 从上顶点往下 +274 的位置开始向上找颜色与上顶点一样的点，为下顶点
    # 该方法对所有纯色平面和部分非纯色平面有效，对高尔夫草坪面、木纹桌面、
    # 药瓶和非菱形的碟机（好像是）会判断错误
    for k in range(i+274, i, -1):  # 274 取开局时最大的方块的上下顶点距离
        pixel = im_pixel[board_x, k]
        if abs(pixel[0] - last_pixel[0]) \
                + abs(pixel[1] - last_pixel[1]) \
                + abs(pixel[2] - last_pixel[2]) < 10:
            break
    board_y = int((i+k) / 2)

    # 如果上一跳命中中间，则下个目标中心会出现 r245 g245 b245 的点，利用这个
    # 属性弥补上一段代码可能存在的判断错误
    # 若上一跳由于某种原因没有跳到正中间，而下一跳恰好有无法正确识别花纹，则有
    # 可能游戏失败，由于花纹面积通常比较大，失败概率较低
    for j in range(i, i+200):
        pixel = im_pixel[board_x, j]
        if abs(pixel[0] - 245) + abs(pixel[1] - 245) + abs(pixel[2] - 245) == 0:
            board_y = j + 10
            break

    if not all((board_x, board_y)):
        return 0, 0, 0, 0
    return piece_x, piece_y, board_x, board_y
        */


    

}
