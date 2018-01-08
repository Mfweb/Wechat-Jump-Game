using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using System.IO;
using System.Drawing.Imaging;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Controls;
using AForge.Imaging.ColorReduction;
using AForge.Video;
using System.IO.Ports;

namespace 定位
{
    public partial class MainForm : Form
    {
        ColorFiltering colorFilter = new ColorFiltering();
        BlobCounter blobCounter = new BlobCounter();


        private FilterInfoCollection videoDevices;
        VideoCaptureDevice videoSource;
        byte[,] GrayScale;
       
        public MainForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            SetStyle(ControlStyles.UserPaint,true);
            SetStyle(ControlStyles.AllPaintingInWmPaint,true); //禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer,true);         //双缓冲
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint,true);
            this.UpdateStyles();
        }
        private void Get_All_Devices()
        {
            try
            {
                DevicesSelect.Items.Clear();
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                    throw new ApplicationException();
                foreach (FilterInfo device in videoDevices)
                {
                    DevicesSelect.Items.Add(device.Name);
                }
                if(DevicesSelect.Items.Count>=3)
                {
                    DevicesSelect.SelectedIndex = 2;
                }
                else
                {
                    DevicesSelect.SelectedIndex = 0;
                }
            }
            catch (ApplicationException)
            {
                DevicesSelect.Items.Add("No Devices");
                videoDevices = null;
            }
        }

        private void Open_Devices(int pix)
        {

            videoSource = new VideoCaptureDevice(videoDevices[DevicesSelect.SelectedIndex].MonikerString);
            videoSource.VideoResolution = videoSource.VideoCapabilities[pix];
            info_A.Text = "H:" + videoSource.VideoCapabilities[pix].FrameSize.Height.ToString() + " W:" + videoSource.VideoCapabilities[pix].FrameSize.Width.ToString();
            Show.VideoSource = videoSource;
            Show.Start();
            GrayScale = new byte[videoSource.VideoCapabilities[pix].FrameSize.Width, videoSource.VideoCapabilities[pix].FrameSize.Height];
        }

        private void Close_Devices()
        {
            Show.SignalToStop();
            Show.WaitForStop();
            videoSource = null;
            videoDevices.Clear();
            Get_All_Devices();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            cap = new ScreenCaptureStream(new Rectangle(2000, 200, 320, 480),10);
            cap.NewFrame += cap_NewFrame;
            Get_All_Devices();
            colorFilter.Red = new IntRange(10, 255);
            colorFilter.Green = new IntRange(10, 255);
            colorFilter.Blue = new IntRange(10, 255);            
        }

        private void loop_thread()
        {
            while (true)
            {
                pictureHandle(Show.GetCurrentVideoFrame());
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(button1.Text == "打开")
            {
                Open_Devices(0);
                Get_Pix_List();
                //timer1.Enabled = true;
                timer2.Enabled = true;
                button1.Text = "关闭";
                System.Threading.Thread lp = new System.Threading.Thread(loop_thread);
                lp.Start();
            }
            else
            {
                button1.Text = "打开";
                Close_Devices();
                //timer1.Enabled = false;
                timer2.Enabled = false;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            cap.Stop();
            Close_Devices();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Get_All_Devices();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = PICA.Image;
            for(int i= 0;i<1000;i++)
            {
                string path = @"D:\pictures\" + i.ToString() + ".png";
                if (File.Exists(path))
                    continue;
                else
                {
                    pictureBox1.Image.Save(path);
                    Text = path;
                    return;
                }
            }
        }

        int run_thread = 0;
        int main_x = 0;
        int main_y = 0;
        int main_w = 0;
        int main_h = 0;

        int detal = 0;


        float scale = 6f;//6S
        //float scale = 5.20843f;//4S
        private bool findColor(Color pix,Color pix2)
        {
            int g = (pix.R + pix.G + pix.B) / 3;
            int g2 = (pix2.R + pix2.G + pix2.B) / 3;
            if (Math.Abs(g - g2) < 20)
                return true;
            else
                return false;
        }

        private void pictureHandle2(object bt)
        {
            if (run_thread > 0) return;
            run_thread++;
            Bitmap bt2 = (Bitmap)bt;
            //Text = bt2.Width.ToString() + " " + bt2.Height.ToString();



            //画笔
            //OilPainting filterDilatation = new OilPainting(15);
            //filterDilatation.ApplyInPlace(bt2);

            //SusanCornersDetector scd = new SusanCornersDetector();

            //bt2 = Grayscale.CommonAlgorithms.BT709.Apply(bt2);
            //BradleyLocalThresholding filterDilatation = new BradleyLocalThresholding();
            //filterDilatation.ApplyInPlace(bt2);

            //FilterIterator filterN = new FilterIterator(filterDilatation, 1);
            //bt2 = filterN.Apply(bt2);





            //Erosion filterDilatation = new Erosion();
            //filterDilatation.ApplyInPlace(bt2);

            Color back_ground_color = bt2.GetPixel(200, 30);
            int max_min = 20;

            IntRange red = new IntRange(back_ground_color.R- max_min, back_ground_color.R+ max_min);
            IntRange green = new IntRange(back_ground_color.G- max_min, back_ground_color.G+ max_min);
            IntRange blue = new IntRange(back_ground_color.B- max_min, back_ground_color.B+ max_min);
            ColorFiltering colorFilter = new ColorFiltering(red, green, blue);
            colorFilter.FillOutsideRange = false;//替换选择部分还是其他部分
            colorFilter.FillColor = new RGB(Color.Black);
            colorFilter.ApplyInPlace(bt2);

            //int[,] kernel = {
            //{ -2, -1,  0},
            //{ -1, 1,  1 },
            //{ 0,  1,  2 } };
            //卷积
            //Convolution filter = new Convolution(kernel);
            //filter.ApplyInPlace(bt2);
            //ColorImageQuantizer ciq = new ColorImageQuantizer(new MedianCutQuantizer());
            //Color[] colorTable = ciq.CalculatePalette(bt2, 128);
            //BurkesColorDithering dithering = new BurkesColorDithering();
            //dithering.ColorTable = colorTable;
            //bt2 = dithering.Apply(bt2);

            ////转灰度
            bt2 = Grayscale.CommonAlgorithms.BT709.Apply(bt2);
            
            //OtsuThreshold filter2 = new OtsuThreshold();//二值化
            //filter2.ApplyInPlace(bt2);

            //bt2 = new BlobsFiltering(50, 50, bt2.Width, bt2.Height).Apply(bt2);//去噪点

            //DifferenceEdgeDetector filtere = new DifferenceEdgeDetector();
            //filtere.ApplyInPlace(bt2);

            //HoughLineTransformation hFilter = new HoughLineTransformation();
            //hFilter.ProcessImage(bt2);
            //bt2 = hFilter.ToBitmap();


            //画笔
            //OilPainting filterDilatation = new OilPainting(15);
            //filterDilatation.ApplyInPlace(bt2);

            //BlobCounter filter3 = new BlobCounter();
            //filter3.ProcessImage(bt2);
            //Rectangle[] rects = filter3.GetObjectsRectangles();

            //BitmapData data = bt2.LockBits(new Rectangle(new System.Drawing.Point(0, 0), bt2.Size), ImageLockMode.ReadWrite, bt2.PixelFormat);
            //if (rects.Length > 0)
            //{
            //    Drawing.Rectangle(data, rects[0], Color.White);//为独立对象绘制包围盒
            //    Bitmap td = bt2.Clone(rects[0],bt2.PixelFormat);

            //    //查找四边形 三角形
            //    QuadrilateralFinder qf = new QuadrilateralFinder();
            //    List<IntPoint> corners = qf.ProcessImage(td);
            //    BitmapData data2 = td.LockBits(new Rectangle(0, 0, td.Width, td.Height),
            //                ImageLockMode.ReadWrite, td.PixelFormat);
            //    Drawing.Polygon(data2, corners, Color.Red);
            //    for (int i = 0; i < corners.Count; i++)
            //    {
            //        Drawing.FillRectangle(data2,
            //            new Rectangle(corners[i].X - 2, corners[i].Y - 2, 5, 5),
            //            Color.FromArgb(i * 32 + 127 + 32, i * 64, i * 64));
            //    }
            //    td.UnlockBits(data2);
            //}

            // bt2.UnlockBits(data);

            //Edges filterDilatation = new Edges();
            //filterDilatation.ApplyInPlace(bt2);


            //HoughLineTransformation lineTransform = new HoughLineTransformation();
            //lineTransform.ProcessImage(bt2);
            //Bitmap houghLineImage = lineTransform.ToBitmap();
            //HoughLine[] lines = lineTransform.GetLinesByRelativeIntensity(0.5);


            //BitmapData data = bt2.LockBits(new Rectangle(0, 0, bt2.Width, bt2.Height),
            //           ImageLockMode.ReadWrite, bt2.PixelFormat);
            //foreach (HoughLine line in lines)
            //{
            //    // get line's radius and theta values
            //    int r = line.Radius;
            //    double t = line.Theta;

            //    if (t-180 > 62 || t - 180<-62) continue;
            //    if (t - 180 < 58 || t - 180 > -58) continue;
            //    //if (t-180 < -62) continue;

            //    // check if line is in lower part of the image
            //    if (r < 0)
            //    {
            //        t += 180;
            //        r = -r;
            //    }

            //    // convert degrees to radians
            //    t = (t / 180) * Math.PI;

            //    // get image centers (all coordinate are measured relative
            //    // to center)
            //    int w2 = bt2.Width / 2;
            //    int h2 = bt2.Height / 2;

            //    double x0 = 0, x1 = 0, y0 = 0, y1 = 0;

            //    if (line.Theta != 0)
            //    {
            //        // none-vertical line
            //        x0 = -w2; // most left point
            //        x1 = w2;  // most right point

            //        // calculate corresponding y values
            //        y0 = (-Math.Cos(t) * x0 + r) / Math.Sin(t);
            //        y1 = (-Math.Cos(t) * x1 + r) / Math.Sin(t);
            //    }
            //    else
            //    {
            //        // vertical line
            //        x0 = line.Radius;
            //        x1 = line.Radius;

            //        y0 = h2;
            //        y1 = -h2;
            //    }

            //    // draw line on the image
            //    Drawing.Line(data,
            //        new IntPoint((int)x0 + w2, h2 - (int)y0),
            //        new IntPoint((int)x1 + w2, h2 - (int)y1),
            //        Color.Red);
            //}
            //bt2.UnlockBits(data);
            //苏珊角落探测器。
            //SusanCornersDetector scd = new SusanCornersDetector();
            //List<IntPoint> corners = scd.ProcessImage(bt2);
            //BitmapData data = bt2.LockBits(new Rectangle(0, 0, bt2.Width, bt2.Height),
            //            ImageLockMode.ReadWrite, bt2.PixelFormat);
            //foreach (IntPoint corner in corners)
            //{
            //    Drawing.FillRectangle(data,
            //        new Rectangle(corner.X - 2, corner.Y - 2, 5, 5),
            //        Color.Red);
            //}
            //bt2.UnlockBits(data);
            //查找四边形 三角形
            //QuadrilateralFinder qf = new QuadrilateralFinder();
            //List<IntPoint> corners = qf.ProcessImage(bt2);
            //BitmapData data = bt2.LockBits(new Rectangle(0, 0, bt2.Width, bt2.Height),
            //            ImageLockMode.ReadWrite, bt2.PixelFormat);
            //Drawing.Polygon(data, corners, Color.Red);
            //for (int i = 0; i < corners.Count; i++)
            //{
            //    Drawing.FillRectangle(data,
            //        new Rectangle(corners[i].X - 2, corners[i].Y - 2, 5, 5),
            //        Color.FromArgb(i * 32 + 127 + 32, i * 64, i * 64));
            //}

            //bt2.UnlockBits(data);


            BlobCounter filter3 = new BlobCounter();
            filter3.MinWidth = 50;
            filter3.MinHeight = 50;
            filter3.MaxWidth = 200;
            filter3.MaxHeight = 200;
            filter3.ProcessImage(bt2);
            Rectangle[] rects = filter3.GetObjectsRectangles();

            Bitmap colorfor = bt2.Clone(new Rectangle(0,0,bt2.Width,bt2.Height),PixelFormat.Format24bppRgb);

            Rectangle findRec = new Rectangle();
            bool isfindRec = false;
            foreach (var item in rects)
            {
                if (item.Width < 30 || item.Height < 30) continue;
                //if (item.Top > main_x) continue;
                findRec = item;
                isfindRec = true;
                break;
            }


            if (isfindRec)
            {
                int start_x = 0;
                int start_y = 0;

                int temp_start_x = 0;
                int temp_end_x = 0;
                for (int x = findRec.Width - 1; x >= 0; x--)
                {
                    Color pix = bt2.GetPixel(findRec.X + x, findRec.Y+2);
                    if (temp_start_x == 0)
                    {
                        Color pix2 = bt2.GetPixel(findRec.X + x, findRec.Y + 3);
                        if (!findColor(pix,Color.FromArgb(0,0,0)) && !findColor(pix2, Color.FromArgb(0, 0, 0)) &&//两行都不是黑色
                            ((findRec.X + x) > (main_x + main_w+5) || (findRec.X + x) < (main_x-5))//不在当前位置方框内
                            )
                        {
                            temp_start_x = x;
                        }
                    }
                    else
                    {
                        if ((findColor(pix, Color.FromArgb(0, 0, 0))))//找到黑色
                        {
                            temp_end_x = x;
                            break;
                        }
                    }
                }
                start_x = findRec.X + temp_end_x + (temp_start_x - temp_end_x) / 2;

                //查找Y坐标值
                //for (int y = 0; y < findRec.Height; y++)
                //{
                //    Color pix = bt2.GetPixel(findRec.X + findRec.Width - 2, findRec.Y + y);
                //    if (findColor(pix))
                //    {
                //        start_y = findRec.Y + y;
                //        break;
                //    }

                //}

                List<IntPoint> allLinePoint = new List<IntPoint>();

                int l_count = 0;
                int last_l = 0;
                for (int y = 5; y < findRec.Height; y++)
                {
                    int left_x = 0;//左侧距离
                    int right_x = 0;//右侧距离
                    

                    Color comp = bt2.GetPixel(start_x, findRec.Y + y);
                    while (true)//找左侧边界
                    {
                        Color pix = bt2.GetPixel(start_x - left_x, findRec.Y + y);
                        if (!findColor(pix, comp))//找到了黑色
                            break;
                        allLinePoint.Add(new IntPoint(start_x - left_x, findRec.Y + y));
                        left_x++;
                        if (start_x - left_x <= 0) break;
                    }
                    while (true)//找右侧
                    {
                        Color pix = bt2.GetPixel(start_x + right_x, findRec.Y + y);
                        if (!findColor(pix, comp))//找到了黑色
                            break;
                        allLinePoint.Add(new IntPoint(start_x + right_x, findRec.Y + y));
                        right_x++;
                        if (start_x + right_x >= bt2.Width) break;
                    }

                    int now_l = right_x + left_x;//获得白场宽度
                    if (last_l != 0)
                    {
                        if (last_l >= now_l)
                        {
                            l_count++;
                            if (l_count >= 2)
                            {
                                start_y = findRec.Y + y - 2;
                                break;
                            }
                        }
                        else
                            l_count = 0;
                    }
                    last_l = now_l;
                }

                //计算长度
                float length = (float)Math.Sqrt((main_x - start_x) * (main_x - start_x) + (main_y - start_y) * (main_y - start_y));
                label2.Text = length.ToString() + "px";
                detal = (int)(length * scale);//计算预计跳跃时间
                label10.Text = detal.ToString() + "ms";
                label8.Text = "X:" + start_x.ToString() + ",Y" + start_y.ToString();





                BitmapData data = colorfor.LockBits(new Rectangle(new System.Drawing.Point(0, 0), colorfor.Size), ImageLockMode.ReadWrite, colorfor.PixelFormat);
                if(allLinePoint.Count > 0)
                Drawing.Polygon(data, allLinePoint, Color.Green);
                //画十字线
                Drawing.Line(data, new AForge.IntPoint(findRec.X, start_y),
                    new AForge.IntPoint(findRec.X + findRec.Width, start_y), Color.Red);

                Drawing.Line(data, new AForge.IntPoint(start_x, findRec.Y),
                        new AForge.IntPoint(start_x, findRec.Y + findRec.Height), Color.Red);
                //画人物与目标之间的线
                Drawing.Line(data, new AForge.IntPoint(start_x, start_y),
                       new AForge.IntPoint(main_x, main_y), Color.Red);

                Drawing.Rectangle(data, findRec, Color.Blue);//为独立对象绘制包围盒
                colorfor.UnlockBits(data);

                //画落点圆环
                Graphics gra = Graphics.FromImage(colorfor);
                Pen pen = new Pen(Color.Red);
                gra.DrawEllipse(pen, main_x - length, main_y - length, length * 2, length * 2);
            }
            else
            {
                label8.Text = "找不到目标";
                label2.Text = "找不到目标";
            }


            //BlobCounter filter3 = new BlobCounter();
            //filter3.MinWidth = 50;
            //filter3.MinHeight = 50;
            //filter3.ProcessImage(bt2);
            ////Rectangle[] rects = filter3.GetObjectsRectangles();

            //BitmapData data = bt2.LockBits(new Rectangle(new System.Drawing.Point(0, 0), bt2.Size), ImageLockMode.ReadWrite, bt2.PixelFormat);
            //foreach (Rectangle rec in filter3.GetObjectsRectangles())
            //{
            //    Drawing.Rectangle(data, rec, Color.White);//为独立对象绘制包围盒
            //}
            //bt2.UnlockBits(data);






            //Bitmap[] find_block = new Bitmap[2];
            //find_block[0] = Grayscale.CommonAlgorithms.BT709.Apply(new Bitmap(@"D:\find\block\1.png"));
            //find_block[1] = Grayscale.CommonAlgorithms.BT709.Apply(new Bitmap(@"D:\find\block\2.png"));


            try
            {
                if (checkBox1.Checked == true)//画预测圆环
                {
                    float rd = (float)(float.Parse(textBox1.Text) / scale);
                    Graphics gra = Graphics.FromImage(colorfor);
                    Pen pen = new Pen(Color.Pink);//画笔颜色
                    gra.DrawEllipse(pen, main_x - rd, main_y-rd, rd * 2, rd * 2);
                }
                pictureBox2.Image = colorfor;
                //pictureBox3.Image = bt2;
                //bool is_find = false;
                //ExhaustiveTemplateMatching templateMatching = new ExhaustiveTemplateMatching(0.95f);//图像查找
                //for (int i = 0; i < 11; i++)
                //{
                //    Bitmap find_bitmap = Grayscale.CommonAlgorithms.BT709.Apply(new Bitmap(@"D:\find\block\" + (i + 1).ToString() + ".bmp"));
                //    TemplateMatch[] matchings_block = templateMatching.ProcessImage(bt2, find_bitmap);
                //    BitmapData tm2 = bt2.LockBits(new Rectangle(0, 0, bt2.Width, bt2.Height), ImageLockMode.ReadWrite, bt2.PixelFormat);

                //    foreach (var item in matchings_block)
                //    {
                //        Drawing.Rectangle(tm2, item.Rectangle, Color.Black);
                //    }
                //    bt2.UnlockBits(tm2);

                //    if (matchings_block.Length > 0)
                //    {
                //        pictureBox2.Image = bt2;
                //        Text = Text + " find " + (i + 1).ToString();
                //        is_find = true;
                //        break;
                //    }
                //}
                //if (is_find == false)
                //{
                //    pictureBox2.Image = bt2;
                //}
            }
            catch (Exception)
            {

                
            }

            run_thread--;
            //
            ////;
            //

        }

//IntRange(55, 70);
        private void pictureHandle(Bitmap temp_map2)
        {
            if (temp_map2 != null)
            {
                //剪裁图片
                Bitmap temp_map = temp_map2.Clone(new Rectangle(0,0, temp_map2.Width, temp_map2.Height), temp_map2.PixelFormat);

                //过滤掉没用的颜色
                int range = 10;
                ColorFiltering filter = new ColorFiltering();
                filter.Red   = new IntRange(61 - range, 61 + range);
                filter.Green = new IntRange(49 - range, 49 + range);
                filter.Blue  = new IntRange(91 - range, 91 + range);
                filter.FillOutsideRange = true;
                filter.ApplyInPlace(temp_map);

                //temp_map2.Dispose();
                temp_map = Grayscale.CommonAlgorithms.BT709.Apply(temp_map);//转换成灰度
                temp_map = new BlobsFiltering(5, 5, temp_map.Width, temp_map.Height).Apply(temp_map);//去噪点


                //查找物体
                BlobCounter filter3 = new BlobCounter();
                filter3.ProcessImage(temp_map);
                Rectangle[] rects = filter3.GetObjectsRectangles();

                Rectangle findRec = new Rectangle();
                bool isfindRec = false;
                foreach (var item in rects)
                {
                    if (item.Top < 100) continue;
                    if (item.Height > 20 || item.Width > 20) continue;
                    //if (item.Height < 20) continue;
                    //if (item.Height < 31 || item.Height > 33) continue;
                    //if (item.Width < 16 || item.Width > 18) continue;
                    findRec = item;
                    isfindRec = true;
                    break;
                }

                Bitmap bt1 = temp_map2.Clone(new Rectangle(new System.Drawing.Point(0, 0), temp_map.Size), PixelFormat.Format24bppRgb);

                if (isfindRec)//找到了
                {
                    //Text = findRec.Width.ToString() + " " + findRec.Height.ToString();
                    //因为只可能出现在上面  所以剪切掉下面部分不再用
                    if (temp_map2.Width > 200 && temp_map2.Height > 300)
                    {
                        try
                        {
                            Bitmap transBT = temp_map2.Clone(new Rectangle(50, 120, temp_map2.Width - 100, temp_map2.Height - 50 - findRec.Top + findRec.Height), temp_map2.PixelFormat);
                            System.Threading.Thread handle2 = new System.Threading.Thread(pictureHandle2);
                            handle2.IsBackground = true;
                            handle2.Start(transBT);
                        }
                        catch (Exception)
                        {
                        }

                    }

                    //Edges filter2 = new Edges();
                    //bt2 = filter2.Apply(bt2);
                    //用来画框
                    BitmapData tm = bt1.LockBits(new Rectangle(0, 0, bt1.Width, bt1.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    Drawing.Rectangle(tm, findRec, Color.Red);//画框
                    bt1.UnlockBits(tm);

                    try
                    {
                        main_x = findRec.X - 50 + findRec.Width / 2;
                        main_y = findRec.Y - 120 + findRec.Height;
                        main_w = findRec.Width;
                        main_h = findRec.Height;

                        label4.Text = "X:" + findRec.X.ToString() + ",Y:" + findRec.Y.ToString();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                }




                //ExhaustiveTemplateMatching templateMatching = new ExhaustiveTemplateMatching(0.7f);//图像查找
                //TemplateMatch[] matchings = templateMatching.ProcessImage(temp_map, find_main);
                ////转换成彩色图片
                //if (matchings.Length > 0)//找到了
                //{


                    
                //}
                try
                {
                   
                    PICA.Image = bt1;
                    temp_map.Dispose();
                }
                catch (Exception)
                {

                }

                //bt1.Dispose();
            }
        }
        Screen c = Screen.PrimaryScreen;
        ScreenCaptureStream cap;

        void cap_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            //Bitmap temp = eventArgs.Frame.Clone(new Rectangle(1000,100,100,100), eventArgs.Frame.PixelFormat);
            pictureHandle(eventArgs.Frame);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {

            //pictureHandle(gitScreen());
            //Application.DoEvents();
            pictureHandle(Show.GetCurrentVideoFrame());
        }

        private void Get_Pix_List()
        {
            DevicesPix.Items.Clear();
            for (int i = 0; i < videoSource.VideoCapabilities.Length; i++)
            {
                DevicesPix.Items.Add(videoSource.VideoCapabilities[i].FrameSize.Width + "x" + videoSource.VideoCapabilities[i].FrameSize.Height.ToString());
            }
            DevicesPix.SelectedIndex = 0;

        }

        private void DevicesPix_A_SelectedIndexChanged(object sender, EventArgs e)
        {
            Show.SignalToStop();
            Show.WaitForStop();
            videoSource = null;
            Open_Devices(DevicesPix.SelectedIndex);
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                label3.Text = "FPS:" + videoSource.FramesReceived.ToString();
            }
            catch (Exception)
            {
            }

        }

        private void Show_A_Click(object sender, EventArgs e)
        {

        }

        private void DevicesSelect_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            //timer1.Enabled = true;

            cap.Start();
            
        }

        private void button5_Click(object sender, EventArgs e)
        {

            //try
            //{
            string[] temp_list;
            comboBox1.Items.Clear();
            temp_list = SerialPort.GetPortNames();
            if (temp_list.Count() > 0)
            {
                for (int i = 0; i < temp_list.Count(); i++)
                {
                    comboBox1.Items.Add(temp_list[i]);
                }
                comboBox1.Text = comboBox1.Items[0].ToString();
            }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(this, ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

        }

        private void button6_Click(object sender, EventArgs e)
        {
            rec:
            if (button6.Text == "连接")
            {
                try
                {
                    COM.BaudRate = 115200;
                    COM.PortName = comboBox1.Text;
                    COM.DataBits = 8;
                    COM.StopBits = StopBits.One;
                    COM.Parity = Parity.None;
                    COM.ReadBufferSize = 8;
                    COM.Open();
                    button6.Text = "断开";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    COM.Close();
                    button6.Text = "连接";
                }
                catch (Exception)
                {
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                COM.Write(detal.ToString() + "\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                COM.Write(textBox1.Text + "\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            textBox1.Text = trackBar1.Value.ToString();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Width = pictureBox1.Left + pictureBox1.Width + 20;
            Height = groupBox3.Top + groupBox3.Height + 50;
        }

        private void PICA_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            
            float length = (float)Math.Sqrt((main_x - e.X) * (main_x - e.X) + (main_y - e.Y) * (main_y - e.Y));
            textBox1.Text = ((int)(length * scale)).ToString();
            try
            {
                COM.Write(textBox1.Text + "\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PICA_MouseDown(object sender, MouseEventArgs e)
        {
            
            
        }
    }
}
