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
using System.Xml.Linq;

//参考文献 C#における「ビットマップ形式の画像データを相互変換」まとめ - Qiita　https://qiita.com/YSRKEN/items/a24bf2173f0129a5825c


namespace ImageChecker.ViewModels
{
    class MainViewModel : NotificationObject
    {

        private DelegateCommand _newCommand;
        public DelegateCommand NewCommand
        {
            get
            {
                return this._newCommand ?? (this._newCommand = new DelegateCommand(
                    _ =>
                    {
                        InitializeSettings();
                    }


                    ));
            }
        }
        private DelegateCommand _openFileCommand;
        public DelegateCommand OpenFileCommand
        {
            get
            {
                return this._openFileCommand ?? (this._openFileCommand = new DelegateCommand(
                    _ =>
                    {
                        this.OpenDialogCallback = OnOpenDialogCallback;
                    }


                    ));
            }
        }

        private Action<bool, string> _openDialogCallback;

        public Action<bool, string> OpenDialogCallback
        {
            get { return this._openDialogCallback; }
            private set { SetProperty(ref this._openDialogCallback, value); }
        }
        private void OnOpenDialogCallback(bool isOk, string filePath)
        {
            this.OpenDialogCallback = null;
            if (isOk) {
                OpenSettingFile(filePath);
            }
        }

        private DelegateCommand _saveFileCommand;
        public DelegateCommand SaveFileCommand
        {
            get
            {
                return this._saveFileCommand ?? (this._saveFileCommand = new DelegateCommand(
                    _ =>
                    {
                        if (_filepath != null) SaveSetttingsToFile();
                        else
                        this.SaveAsDialogCallback = OnSaveAsDialogCallback;
                    }


                    ));
            }
        }


        private DelegateCommand _saveAsFileCommand;
        public DelegateCommand SaveAsFileCommand
        {
            get
            {
                return this._saveAsFileCommand ?? (this._saveAsFileCommand = new DelegateCommand(
                    _ =>
                    {
                        this.SaveAsDialogCallback = OnSaveAsDialogCallback;
                    }


                    ));
            }
        }

        private Action<bool, string> _saveAsDialogCallback;

        public Action<bool, string> SaveAsDialogCallback
        {
            get { return this._saveAsDialogCallback; }
            private set { SetProperty(ref this._saveAsDialogCallback, value); }
        }
        private void OnSaveAsDialogCallback(bool isOk, string filePath)
        {
            this.SaveAsDialogCallback = null;
            
            if (isOk) {
                _filepath = filePath;
                SaveSetttingsToFile(); 
            }
        }


        private DelegateCommand _exitCommand;


       
        public DelegateCommand ExitCommand
        {
            get
            {
                return this._exitCommand ?? (this._exitCommand =
                    new DelegateCommand(
                        _ =>
                        {
                            OnExit();
                        }));
            }

        }

       
        private bool OnExit()
        {
            App.Current.Shutdown();
            return true;
        }



        private string _imageFilePath;
        public string ImageFilePath
        {
            set
            {
                SetProperty(ref this._imageFilePath, value);
                AnalyzeCommand.RaiseCanExecuteChanged();
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




            var a = Clustering.KMeans(features.Where(v => DstColorRegion.IsHSVColorInRegion(v.X, v.Y, v.Z)).ToArray(), ClusterNum,LoopUpperLimit,MethodList[CalculateMethodKey].Method,MethodList[CalculateMethodKey].Distance,Seed);
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

                        this.SaveImageDialogCallback = OnSaveImageDialogCallback;
                    },
                    _ =>
                    {
                        if ( DstBitmapSource != null && !isBusy) return true;
                        else return false;
                    }

                    ); 
            }
        }

       

        private Action<bool, string> _saveImageDialogCallback;

        public Action<bool, string> SaveImageDialogCallback
        {
            get { return this._saveImageDialogCallback; }
            private set { SetProperty(ref this._saveImageDialogCallback, value); }
        }
        private void OnSaveImageDialogCallback(bool isOk, string filePath)
        {
            this.SaveAsDialogCallback = null;

            if (isOk)
            {
                DstBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }



        private HSVColorRegion _dstColorRegion;
        public HSVColorRegion DstColorRegion
        {
            get
            {
                return this._dstColorRegion;
            }
            set
            {
                SetProperty(ref this._dstColorRegion, value);
            }
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
            public Func<IEnumerable<Vector3>, Vector3> Method { get; set; }
            public Func<Vector3,Vector3,double> Distance { get; set; }
        }
        private Dictionary<string,CalculateMethod> _methodList;
        public Dictionary<string,CalculateMethod> MethodList
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
        private string _calculateMethodKey;
        public string CalculateMethodKey
        {
            get
            {
                return this._calculateMethodKey;
            }
            set
            {
                SetProperty(ref this._calculateMethodKey, value);
            }
        }
        private int _seed;
        public int Seed
        {
            get
            {
                return this._seed;
            }
            set
            {
                SetProperty(ref this._seed, value);
            }
        }

        private void InitializeSettings()
        {


            _filepath = null;
            DstColorRegion = new HSVColorRegion();
            MethodList = new Dictionary<string, CalculateMethod>
            {
                {"Average",
               new CalculateMethod
               {

                   Method=HSVColorRegion.Average,
                   Distance=HSVColorRegion.LengthSquared
               }
               },
                {"Average2",
               new CalculateMethod
               {
                   Method=HSVColorRegion.Average2,
                   Distance=HSVColorRegion.Distance
               }
               }

            };
            CalculateMethodKey = MethodList.Keys.First();
            
            isBusy = false;
            DstColorRegion.HueStart = 0.0;
            DstColorRegion.HueEnd = 361.0;
            DstColorRegion.SaturationStart = 0.0;
            DstColorRegion.SaturationEnd = 101.0;
            DstColorRegion.ValueStart = 0.0;
            DstColorRegion.ValueEnd = 101.0;
           
            ClusterNum = 3;
            LoopUpperLimit = 10;
            Seed = 0;
        }

        private void OpenSettingFile(string filepath)
        {
           
            
            InitializeSettings();
            var settings = XDocument.Load(filepath).Descendants("Settings").First();
            var ImageFilePathAttribute = settings.Attribute("TargetImageFilePath");
            if(ImageFilePathAttribute!=null)ImageFilePath = ImageFilePathAttribute.Value;
            var ColorRegion=settings.Element("ColorRegion");
            DstColorRegion.HueStart = double.Parse(ColorRegion.Element("Hue").Attribute("Start").Value);
            DstColorRegion.HueEnd = double.Parse(ColorRegion.Element("Hue").Attribute("End").Value);
            DstColorRegion.SaturationStart = double.Parse(ColorRegion.Element("Saturation").Attribute("Start").Value);
            DstColorRegion.SaturationEnd = double.Parse(ColorRegion.Element("Saturation").Attribute("End").Value);
            DstColorRegion.ValueStart = double.Parse(ColorRegion.Element("Value").Attribute("Start").Value);
            DstColorRegion.ValueEnd = double.Parse(ColorRegion.Element("Value").Attribute("End").Value);
            
            var ClusteringSettings = settings.Element("ClusteringSettings");
            CalculateMethodKey = ClusteringSettings.Attribute("Method").Value;
            Seed = int.Parse(ClusteringSettings.Attribute("Seed").Value);
            ClusterNum = int.Parse(ClusteringSettings.Attribute("NumberOfClusters").Value);
            LoopUpperLimit = int.Parse(ClusteringSettings.Attribute("LoopUpperLimit").Value);
            _filepath = filepath;
        }

        private string _filepath;
        private void SaveSetttingsToFile()
        {
            
            var xdoc = new XDocument(
                new XElement("Settings",


                new XElement("ColorRegion",
                    new XElement("Hue", new XAttribute("Start", DstColorRegion.HueStart), new XAttribute("End", DstColorRegion.HueEnd)),
                    new XElement("Saturation", new XAttribute("Start", DstColorRegion.SaturationStart), new XAttribute("End", DstColorRegion.ValueEnd)),
                    new XElement("Value", new XAttribute("Start", DstColorRegion.ValueStart), new XAttribute("End", DstColorRegion.ValueEnd))
                    ),

                new XElement("ClusteringSettings",
                    new XAttribute("Method", CalculateMethodKey),
                    new XAttribute("Seed", Seed),
                    new XAttribute("NumberOfClusters", ClusterNum),
                    new XAttribute("LoopUpperLimit", LoopUpperLimit)
                    )
                )
            );
            if(ImageFilePath!=null)xdoc.Element("Settings").Add(new XAttribute("TargetImageFilePath", ImageFilePath));
            xdoc.Save(_filepath);
            
            
        }
        public MainViewModel()
        {
            InitializeSettings();
            
          
         
        }

    }
}
