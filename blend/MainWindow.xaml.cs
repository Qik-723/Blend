using OpenCvSharp;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rect = OpenCvSharp.Rect;

namespace blend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public void Stitch()
        {
            Mat top = Cv2.ImRead("F:\\-1.jpg");
            Mat bottom = Cv2.ImRead("F:\\0.jpg");

            Mat back = new Mat(top.Height + bottom.Height - 457, top.Width + 14 * 2, MatType.CV_8UC3);

            Rect rect = new Rect(14, 0, top.Width, top.Height);

            Mat m1 = new Mat(back, rect);

            top.CopyTo(m1);
            back.SaveImage("F:\\result5.jpg");
            rect = new Rect(0, top.Height - 457, top.Width, top.Height);
            Mat m2 = new Mat(back, rect);
            bottom.CopyTo(m2);

            back.SaveImage("F:\\result6.jpg");
        }


        public void caijian()
        {
            Mat top = Cv2.ImRead("F:\\1.jpg");
            Mat bottom = Cv2.ImRead("F:\\2.jpg");

            Mat temp1 = new Mat(top, new Rect(0, 1975 - 100, top.Width, 100));
            Mat temp2 = new Mat(bottom, new Rect(0, 0, top.Width, 100));

            temp1.SaveImage("F:\\temp1.png");
            temp2.SaveImage("F:\\temp2.png");
        }

        public void SmoothSeamGaussianTest()
        {
            Mat image = Cv2.ImRead(@"F:\\result6.jpg", ImreadModes.Color);

            //// 从上下图像中提取重叠部分的区域
            //Mat Top = Cv2.ImRead(@"Top.jpg", ImreadModes.Color);
            //Mat Bottom = Cv2.ImRead(@"Bottom.jpg", ImreadModes.Color);

            // 从上下图像中提取重叠部分的区域
            Mat Top = new Mat(Cv2.ImRead(@"F:\\result5.jpg", ImreadModes.Color), new Rect(0, 1975, image.Width, 100));
            Mat Bottom = new Mat(Cv2.ImRead(@"F:\\result6.jpg", ImreadModes.Color), new Rect(0, 1975, image.Width, 100));

            // 保存提取的Top和Bottom图像（仅用于调试）
            //Cv2.ImWrite("Top.jpg", Top);
            //Cv2.ImWrite("Bottom.jpg", Bottom);

            int seamPositionTop = 0;  // 上部分拼接起始位置
            int seamPositionBottom = 50; // 下部分拼接起始位置
            int blendHeight = 50; // 羽化区域高度
            int imageHeight = image.Height;

            // 创建输出图像，存放羽化后的结果
            Mat blendedImage = new Mat(100, image.Width, MatType.CV_8UC3);




            // 处理上部羽化区域 (1975 to 2025)，权重从 1 到 0.5
            for (int y = seamPositionTop; y < seamPositionBottom; y++)
            {
                // 计算羽化权重 (从 1 到 0.5)
                float alpha = 1.0f - ((float)(y - seamPositionTop) / (seamPositionBottom - seamPositionTop)) * 0.5f;
                Console.WriteLine(alpha.ToString());
                for (int x = 0; x < image.Cols; x++)
                {
                    Vec3b topPixel = Top.At<Vec3b>(y - seamPositionTop, x); // 从Top图像中获取像素
                    Vec3b bottomPixel = Bottom.At<Vec3b>(y - seamPositionTop, x); // 从Bottom图像中获取像素

                    // 计算混合后的像素
                    Vec3b blendedPixel = new Vec3b
                    {
                        Item0 = (byte)((alpha) * topPixel.Item0 + (1 - alpha) * bottomPixel.Item0),
                        Item1 = (byte)((alpha) * topPixel.Item1 + (1 - alpha) * bottomPixel.Item1),
                        Item2 = (byte)((alpha) * topPixel.Item2 + (1 - alpha) * bottomPixel.Item2)
                    };

                    // 写入到结果图像的对应位置
                    blendedImage.Set(y, x, blendedPixel);
                }
            }
            //Cv2.ImWrite("feathered_image1.jpg", blendedImage);
            // 处理下部羽化区域 (2025 to 2075)，权重从 0.5 到 1
            for (int y = seamPositionBottom; y < seamPositionBottom + blendHeight; y++)
            {
                // 计算羽化权重 (从 0.5 到 1)
                float alpha = 0.5f + ((float)(y - seamPositionBottom) / blendHeight) * 0.5f;
                Console.WriteLine(alpha.ToString());
                for (int x = 0; x < image.Cols; x++)
                {
                    Vec3b topPixel = Top.At<Vec3b>(y, x); // 从Top图像中获取像素
                    Vec3b bottomPixel = Bottom.At<Vec3b>(y, x); // 从Bottom图像中获取像素

                    // 计算混合后的像素
                    Vec3b blendedPixel = new Vec3b
                    {
                        Item0 = (byte)((1 - alpha) * topPixel.Item0 + alpha * bottomPixel.Item0),
                        Item1 = (byte)((1 - alpha) * topPixel.Item1 + alpha * bottomPixel.Item1),
                        Item2 = (byte)((1 - alpha) * topPixel.Item2 + alpha * bottomPixel.Item2)
                    };

                    // 写入到结果图像的对应位置
                    blendedImage.Set(y, x, blendedPixel);
                }
            }

            // 保存羽化后的结果图像
            //Cv2.ImWrite("feathered_image2.jpg", blendedImage);

            blendedImage.CopyTo(new Mat(image, new Rect(0, 1975, image.Width, 100)));
            //Cv2.ImWrite("image3.jpg", image);
        }


        public void Top_Bottom_Blended(Mat Top, Mat Bottom, Mat image, int yoffset)
        {
            int seamPositionTop = 0;  // 上部分拼接起始位置
            int seamPositionBottom = 50; // 下部分拼接起始位置
            int blendHeight = 50; // 羽化区域高度



            // 创建输出图像，存放羽化后的结果
            Mat blendedImage = new Mat(seamPositionTop + seamPositionBottom + blendHeight, image.Width, MatType.CV_8UC3);




            // 处理上部羽化区域 (seamPositionTop to seamPositionTop+blendHeight)，权重从 1 到 0.5
            for (int y = seamPositionTop; y < seamPositionBottom; y++)
            {
                // 计算羽化权重 (从 1 到 0.5)
                float alpha = 1.0f - ((float)(y - seamPositionTop) / (seamPositionBottom - seamPositionTop)) * 0.5f;
                Console.WriteLine(alpha.ToString());
                for (int x = 0; x < image.Cols; x++)
                {
                    Vec3b topPixel = Top.At<Vec3b>(y - seamPositionTop, x); // 从Top图像中获取像素
                    Vec3b bottomPixel = Bottom.At<Vec3b>(y - seamPositionTop, x); // 从Bottom图像中获取像素

                    // 计算混合后的像素
                    Vec3b blendedPixel = new Vec3b
                    {
                        Item0 = (byte)((alpha) * topPixel.Item0 + (1 - alpha) * bottomPixel.Item0),
                        Item1 = (byte)((alpha) * topPixel.Item1 + (1 - alpha) * bottomPixel.Item1),
                        Item2 = (byte)((alpha) * topPixel.Item2 + (1 - alpha) * bottomPixel.Item2)
                    };

                    // 写入到结果图像的对应位置
                    blendedImage.Set(y, x, blendedPixel);
                }
            }
            //Cv2.ImWrite("feathered_image1.jpg", blendedImage);
            // 处理下部羽化区域 (seamPositionTop+blendHeight to seamPositionBottom+blendHeight)，权重从 0.5 到 1
            for (int y = seamPositionBottom; y < seamPositionBottom + blendHeight; y++)
            {
                // 计算羽化权重 (从 0.5 到 1)
                float alpha = 0.5f + ((float)(y - seamPositionBottom) / blendHeight) * 0.5f;
                Console.WriteLine(alpha.ToString());
                for (int x = 0; x < image.Cols; x++)
                {
                    Vec3b topPixel = Top.At<Vec3b>(y, x); // 从Top图像中获取像素
                    Vec3b bottomPixel = Bottom.At<Vec3b>(y, x); // 从Bottom图像中获取像素

                    // 计算混合后的像素
                    Vec3b blendedPixel = new Vec3b
                    {
                        Item0 = (byte)((1 - alpha) * topPixel.Item0 + alpha * bottomPixel.Item0),
                        Item1 = (byte)((1 - alpha) * topPixel.Item1 + alpha * bottomPixel.Item1),
                        Item2 = (byte)((1 - alpha) * topPixel.Item2 + alpha * bottomPixel.Item2)
                    };

                    // 写入到结果图像的对应位置
                    blendedImage.Set(y, x, blendedPixel);
                }
            }


            blendedImage.CopyTo(new Mat(image, new Rect(0, image.Height - yoffset, image.Width, blendHeight * 2)));
            //Cv2.ImWrite("image3.jpg", image);
        }

    }
}