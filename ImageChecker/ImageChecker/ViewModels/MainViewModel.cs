using ImageChecker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

//参考文献 C#における「ビットマップ形式の画像データを相互変換」まとめ - Qiita　https://qiita.com/YSRKEN/items/a24bf2173f0129a5825c


namespace ImageChecker.ViewModels
{
    class MainViewModel : NotificationObject
    {

        private string _imageFilePath;
        public string ImageFilePath
        {
            set
            {
                SetProperty(ref this._imageFilePath, value);
            }
            get { return this._imageFilePath; }
        }
        private DelegateCommand _dropCommand;
        public DelegateCommand DropCommand
        {
            get
            {
                return _dropCommand = _dropCommand ?? new DelegateCommand(FileDrop);
            }
        }

        private void FileDrop(object parameter)
        {
            var filepath = ((string[])parameter)[0];
            ImageFilePath = filepath;
            AnalyzeCommand.RaiseCanExecuteChanged();

        }

        private bool isBusy;
        private async void ExecuteCommandTask()
        {
            isBusy = true;
            AnalyzeCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            await Task.Run(() => AnalyzeImage());
            AnalyzeCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            isBusy = false;
            AnalyzeCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }
        private void AnalyzeImage()
        {

            Bitmap bitmap = new Bitmap(ImageFilePath);

            Vector3[,] hsvimage = new Vector3[bitmap.Width, bitmap.Height];
            Vector3[] features = new Vector3[bitmap.Width * bitmap.Height];
            int count = 0;
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var p = bitmap.GetPixel(x, y);
                    var tmp = HSVColorRegion.RGBtoHSV(HSVColorRegion.HSLtoRGB(new Vector3(p.GetHue(), p.GetSaturation() * 100, p.GetBrightness() * 100)));
                    features[count] = tmp;
                    hsvimage[x, y] = tmp;
                    count += 1;
                }
            }




            var a = Clustering.KMeans(features.Where(v => DstColorRegion.IsHSVColorInRegion(v.X, v.Y, v.Z)).ToArray(), ClusterNum,LoopUpperLimit,SelectedCalculateMethod.Method,SelectedCalculateMethod.Distance);
            var center = a.Item1;
            var labels = a.Item2;
            
            var tmppallets = new List<Pallet>();
            for (int i = 0; i < ClusterNum; i++)
            {
                var targetvec = center[i];
                var targetrgb = HSVColorRegion.HSVtoRGB(targetvec);
                var color = System.Drawing.Color.FromArgb((byte)targetrgb.X, (byte)targetrgb.Y, (byte)targetrgb.Z);
                tmppallets.Add(new Pallet { H = (int)targetvec.X, S = (int)targetvec.Y, V = (int)targetvec.Z, color = color });
            }
            Pallets = tmppallets;

            Bitmap dst = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            count = 0;
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {

                    var colorvec = hsvimage[x, y];
                    if (DstColorRegion.IsHSVColorInRegion(colorvec.X, colorvec.Y, colorvec.Z))
                    {

                        dst.SetPixel(x, y, Pallets[labels[count]].color);
                        count++;
                    }
                    else
                    {
                        //var srccolor = bitmap.GetPixel(x, y);
                        //dst.SetPixel(x, y, System.Drawing.Color.FromArgb(1, srccolor.R, srccolor.G, srccolor.B));
                    }


                }
            }

            DstFilePath = Path.Combine(new string[] { Path.GetDirectoryName(ImageFilePath), Path.GetFileNameWithoutExtension(ImageFilePath) + "_converted" + Path.GetExtension(ImageFilePath) });

            DstBitmap = dst;

        }

        private DelegateCommand _analyzeCommand;

        public DelegateCommand AnalyzeCommand
        {
            get
            {
                return _analyzeCommand = _analyzeCommand ?? new DelegateCommand(
                    _ => {

                        ExecuteCommandTask();
                    },

                    _ =>
                    {
                        if (!isBusy & ImageFilePath != null) return true;
                        return false;

                    }
                    );
            }
        }

        private List<Pallet> _pallets;
        public List<Pallet> Pallets
        {

            get
            {
                return this._pallets;
            }
            set
            {
                SetProperty(ref this._pallets, value);
                this._pallets = value;
                
            }

        }
        public string DstFilePath { get; set; }
        private BitmapSource _dstBitmapSource;
        public BitmapSource DstBitmapSource
        {
            get
            {
                return this._dstBitmapSource;
            }
            set
            {
                this._dstBitmapSource = value;
                RaisePropertyChanged();
            }
        }
        private Bitmap _dstBitmap;
        public Bitmap DstBitmap
        {
            get
            {
                return this._dstBitmap;
            }
            set
            {
                this._dstBitmap = value;
                using (var ms = new System.IO.MemoryStream())
                {
                    _dstBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    ms.Seek(0, System.IO.SeekOrigin.Begin);

                    DstBitmapSource =
                       System.Windows.Media.Imaging.BitmapFrame.Create(
                           ms,
                           System.Windows.Media.Imaging.BitmapCreateOptions.None,
                           System.Windows.Media.Imaging.BitmapCacheOption.OnLoad
                       );

                }
            }
        }

        private DelegateCommand _saveCommand;
        public DelegateCommand SaveCommand
        {
            get
            {
                return _saveCommand = _saveCommand ?? new DelegateCommand(
                    _ =>
                    {
                        DstBitmap.Save(DstFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                    },
                    _ =>
                    {
                        if (DstFilePath != null && DstBitmapSource != null && !isBusy) return true;
                        else return false;
                    }

                    ); 
            }
        }

       
        public HSVColorRegion DstColorRegion
        {
            get;
            set;
        }


        private int _clusterNum;
        public int ClusterNum
        {
            get
            {
                return this._clusterNum;
            }
            set
            {
                SetProperty(ref this._clusterNum, value);
            }
        }
        private int _loopUpperLimit;
        public int LoopUpperLimit
        {
            get
            {
                return this._loopUpperLimit;
            }
            set
            {
                SetProperty(ref this._loopUpperLimit, value);
            }
        }
        public class CalculateMethod
        {
            public string Name { get; set; }
            public Func<IEnumerable<Vector3>, Vector3> Method { get; set; }
            public Func<Vector3,Vector3,double> Distance { get; set; }
        }
        private List<CalculateMethod> _methodList;
        public List<CalculateMethod> MethodList
        {
            get
            {
                return this._methodList;
            }
            set
            {
                SetProperty(ref this._methodList, value);
            }
        }
        private CalculateMethod _calculateMethod;
        public  CalculateMethod SelectedCalculateMethod
        {
            get
            {
                return this._calculateMethod;
            }
            set
            {
                SetProperty(ref this._calculateMethod, value);
            }
        }
       
        public MainViewModel()
        {

            MethodList = new List<CalculateMethod>
            {
               new CalculateMethod
               {
                   Name="Average",
                   Method=HSVColorRegion.Average,
                   Distance=HSVColorRegion.LengthSquared
               },
               new CalculateMethod
               {
                   Name="Average2",
                   Method=HSVColorRegion.Average2,
                   Distance=HSVColorRegion.Distance
               }
                                           
            };
            //Method = MethodList[1].Method;
            isBusy = false;
            DstColorRegion = new HSVColorRegion()
            {
                HueStart = 0.0,
                HueEnd = 361.0,
                SaturationStart = 0.0,
                SaturationEnd = 101.0,
                ValueStart = 0.0,
                ValueEnd = 101.0,
            };
            ClusterNum = 3;
            LoopUpperLimit = 10;

        }

    }
}
