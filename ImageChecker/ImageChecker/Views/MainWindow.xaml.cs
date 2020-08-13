using ImageChecker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


//参考文献 WPFとVB.NET、表示した画像をクリックした場所の色の取得はややこしい - 午後わてんのブログ https://gogowaten.hatenablog.com/entry/13952774

namespace ImageChecker
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            image.MouseDown += MouseDownOnImage;
            image.MouseMove += MouseMoveOnImage;
        }


        private void MouseDownOnImage(object sender, MouseEventArgs e)
        {

            Point point = e.GetPosition(image);
            double x = point.X;
            double y = point.Y;
            BitmapSource b = (BitmapSource)image.Source;

            Color c = GetPixelColor((int)(x / image.ActualWidth * b.PixelWidth), (int)(y / image.ActualHeight * b.PixelHeight), b);
            var hsv = HSVColorRegion.RGBtoHSV(new Vector3(c.R, c.G, c.B));
            clickhsv.Text = "H:" + ((int)hsv.X).ToString() + ",S:" + ((int)hsv.Y).ToString() + ",V:" + ((int)hsv.Z).ToString();
            clickcolor.Background = new SolidColorBrush(c);

        }
        private void MouseMoveOnImage(object sender, MouseEventArgs e)
        {

            Point point = e.GetPosition(image);
            double x = point.X;
            double y = point.Y;
            BitmapSource b = (BitmapSource)image.Source;

            Color c = GetPixelColor((int)(x / image.ActualWidth * b.PixelWidth), (int)(y / image.ActualHeight * b.PixelHeight), b);
            var hsv = HSVColorRegion.RGBtoHSV(new Vector3(c.R, c.G, c.B));
            currenthsv.Text = "H:" + ((int)hsv.X).ToString() + ",S:" + ((int)hsv.Y).ToString() + ",V:" + ((int)hsv.Z).ToString();
            currentcolor.Background = new SolidColorBrush(c);

        }

        private Color GetPixelColor(int x, int y, BitmapSource bmp)
        {
            var cb = new CroppedBitmap(bmp, new Int32Rect(x, y, 1, 1));
            var fcb = new FormatConvertedBitmap(cb, PixelFormats.Bgra32, null, 0);
            byte[] pixels = new byte[4];
            fcb.CopyPixels(pixels, 4, 0);
            var c = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
            return c;
        }
    }
}
