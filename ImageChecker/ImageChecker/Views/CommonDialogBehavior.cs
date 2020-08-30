using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

//参考文献 » WPF 学習用ドキュメント作りました http://kisuke0303.sakura.ne.jp/blog/?p=340

namespace ImageChecker.Views
{
    enum Mode
    {
        OPEN,
        SAVE
    }
    class CommonDialogBehavior
    {
        public static readonly DependencyProperty CallbackProperty =
            DependencyProperty.RegisterAttached("Callback", typeof(Action<bool, string>), typeof(CommonDialogBehavior), new PropertyMetadata(null, OnCallbackPropertyChanged));

        public static Action<bool, string> GetCallback(DependencyObject target)
        {
            return (Action<bool, string>)target.GetValue(CallbackProperty);
        }

        public static void SetCallback(DependencyObject target, Action<bool, string> value)
        {
            target.SetValue(CallbackProperty, value);
        }


        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(CommonDialogBehavior), new PropertyMetadata("ファイルを開く"));

        public static string GetTitle(DependencyObject target)
        {
            return (string)target.GetValue(TitleProperty);
        }

        public static void SetTitle(DependencyObject target, string value)
        {
            target.SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.RegisterAttached("Filter", typeof(string), typeof(CommonDialogBehavior), new PropertyMetadata("すべてのファイル(*.*)|*.*"));

        public static string GetFilter(DependencyObject target)
        {
            return (string)target.GetValue(FilterProperty);
        }

        public static void SetFilter(DependencyObject target, string value)
        {
            target.SetValue(FilterProperty, value);
        }


        public static readonly DependencyProperty MultiselectProperty =
            DependencyProperty.RegisterAttached("Multiselect", typeof(bool),
                typeof(CommonDialogBehavior), new PropertyMetadata(true));

        public static bool GetMultiselect(DependencyObject target)
        {
            return (bool)target.GetValue(MultiselectProperty);
        }

        public static void SetMultiselect(DependencyObject target, bool value)
        {
            target.SetValue(MultiselectProperty, value);
        }

        public static readonly DependencyProperty ModeProperty =
           DependencyProperty.RegisterAttached("Mode", typeof(Mode), typeof(CommonDialogBehavior), new PropertyMetadata(Mode.OPEN));

        public static Mode GetMode(DependencyObject target)
        {
            return (Mode)target.GetValue(ModeProperty);
        }

        public static void SetMode(DependencyObject target, Mode value)
        {
            target.SetValue(ModeProperty, value);
        }



        private static void OnCallbackPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var callback = GetCallback(sender);
            if (callback != null)
            {
                var mode = GetMode(sender);
                FileDialog dlg;
                switch (mode)
                {
                    
                    case Mode.OPEN:
                        dlg = new OpenFileDialog() { Multiselect = GetMultiselect(sender) };
                        break;

                    case Mode.SAVE:
                        dlg = new SaveFileDialog();
                        break;

                    default:
                        
                        return;

                }
                dlg.Title = GetTitle(sender);
                dlg.Filter = GetFilter(sender);
                
                var owner = Window.GetWindow(sender);
                var result = dlg.ShowDialog(owner);
                callback(result.Value, dlg.FileName);

            }
        }
    }
}

