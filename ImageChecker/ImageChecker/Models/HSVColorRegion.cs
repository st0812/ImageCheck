using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


//参考文献 RGBからHSVへの変換と復元 - [物理のかぎしっぽ]　http://hooktail.org/computer/index.php?RGB%A4%AB%A4%E9HSV%A4%D8%A4%CE%CA%D1%B4%B9%A4%C8%C9%FC%B8%B5
//参考文献 ツール｜色の変換（RGB・HSV・HSL）　https://yanohirota.com/color-converter/


namespace ImageChecker.Models
{
    class HSVColorRegion : NotificationObject
    {
        private double _hueStart;
        public double HueStart
        {
            get
            {
                return this._hueStart;
            }
            set
            {
                SetProperty(ref this._hueStart, value);
            }
        }

        private double _hueEnd;
        public double HueEnd
        {
            get
            {
                return this._hueEnd;
            }
            set
            {
                SetProperty(ref this._hueEnd, value);
                
            }
        }

        public bool IsHueInRegion(double h)
        {
            if (this.HueStart < this.HueEnd)
            {
                return (this.HueStart < h) && (h < this.HueEnd);
            }
            else
            {
                return (h < this.HueEnd) || (this.HueStart < h);
            }

        }
        private double _saturationStart;
        public double SaturationStart
        {
            get
            {
                return this._saturationStart;
            }
            set
            {
                SetProperty(ref this._saturationStart, value);
                
            }
        }

        private double _saturationEnd;
        public double SaturationEnd
        {
            get
            {
                return this._saturationEnd;
            }
            set
            {
                
                SetProperty(ref this._saturationEnd,value);
               
            }
        }

        public bool IsSaturationInRegion(double s)
        {
            return (this.SaturationStart < s) && (s < this.SaturationEnd);
        }

        private double _valueStart;
        public double ValueStart
        {
            get
            {
                return this._valueStart;
            }
            set
            {
                SetProperty(ref this._valueStart, value);
            }
        }

        private double _valueEnd;
        public double ValueEnd
        {
            get
            {
                return this._valueEnd;
            }
            set
            {
                SetProperty(ref this._valueEnd, value);
                
            }
        }


        public bool IsValueInTargetRegion(double v)
        {
            return (this.ValueStart < v) && (v < this.ValueEnd);
        }

        public bool IsHSVColorInRegion(double h, double s, double v)
        {



            return this.IsHueInRegion(h) && this.IsSaturationInRegion(s) && this.IsValueInTargetRegion(v);
        }

        public static Vector3 HSLtoRGB(Vector3 src)
        {

            var H = src.X;
            var S = src.Y;
            var L = src.Z;
            double Ld = 0;
            if (H == 360)
            {
                H = 0;
            }
            if (L < 50)
            {
                Ld = L;
            }
            else
            {
                Ld = 100 - L;
            }
            float MAX = (float)(2.55 * (L + Ld * S / 100));
            float MIN = (float)(2.55 * (L - Ld * S / 100));
            float f(double x)
            {
                return (float)(x / 60 * (MAX - MIN) + MIN);
            }
            if (H < 60)
            {
                return new Vector3(MAX, f(H), MIN);
            }
            else if (H < 120)
            {
                return new Vector3(f(120 - H), MAX, MIN);
            }
            else if (H < 180)
            {
                return new Vector3(MIN, MAX, f(H - 120));
            }
            else if (H < 240)
            {
                return new Vector3(MIN, f(240 - H), MAX);
            }
            else if (H < 300)
            {
                return new Vector3(f(H - 240), MIN, MAX);
            }
            else
            {
                return new Vector3(MAX, MIN, f(360 - H));
            }

        }
        public static Vector3 RGBtoHSV(Vector3 src)
        {
            var R = src.X;
            var G = src.Y;
            var B = src.Z;

            var MAX = new List<float> { R, G, B }.Max();
            var MIN = new List<float> { R, G, B }.Min();


            float H = 0.0F; ;
            if (R == MAX)
            {
                H = (G - B) / (MAX - MIN) * 60;
            }
            else if (G == MAX)
            {
                H = (B - R) / (MAX - MIN) * 60 + 120;
            }
            else if (B == MAX)
            {
                H = (R - G) / (MAX - MIN) * 60 + 120;
            }

            if (H < 0) H += 360;
            var S = (MAX - MIN) / MAX * 100;

            var V = MAX / 255 * 100;

            return new Vector3(H, S, V);
        }

        public static Vector3 HSVtoRGB(Vector3 src)
        {
            var H = src.X;
            if (H == 360.0) H = 0.0F;
            var S = src.Y / 100;
            var V = src.Z / 100;

            float R = 0.0F;
            float G = 0.0F;
            float B = 0.0F;
            if (S == 0.0)
            {
                R = G = B = V * 255;
            }

            int Hd = (int)Math.Floor(H / 60.0);
            float F = (float)(H / 60 - Hd);
            float M = V * (1 - S) * 255;
            float N = V * (1 - S * F) * 255;
            float K = V * (1 - S * (1 - F)) * 255;

            switch (Hd)
            {
                case 0:
                    R = V * 255;
                    G = K;
                    B = M;
                    break;
                case 1:
                    R = N;
                    G = V * 255;
                    B = M;
                    break;
                case 2:
                    R = M;
                    G = V * 255;
                    B = K;
                    break;
                case 3:
                    R = M;
                    G = N;
                    B = V * 255;
                    break;
                case 4:
                    R = K;
                    G = M;
                    B = V * 255;
                    break;
                case 5:
                    R = V * 255;
                    G = M;
                    B = N;
                    break;


            }

            return new Vector3(R, G, B);
        }
    }
}
