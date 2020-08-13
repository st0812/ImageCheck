using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

//参考文献　ファイルドロップ時にコマンドを実行する添付ビヘイビア - SourceChord http://sourcechord.hatenablog.com/entry/2014/03/16/174326

namespace ImageChecker.Views
{
    internal class DropBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command",
                                                typeof(DelegateCommand),
                                                typeof(DropBehavior),
                                                new PropertyMetadata(null, OnCommandChanged));
        public static DelegateCommand GetCommand(DependencyObject target)
        {
            return (DelegateCommand)target.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject target, DelegateCommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var w = sender as UIElement;
            if (w != null)
            {
                var command = GetCommand(w);
                if ((command != null) && (e.OldValue == null))
                {
                    w.AllowDrop = true;
                    w.PreviewDragOver += previewDragOver;
                    w.Drop += drop;
                }
                else if (command == null)
                {
                    w.AllowDrop = false;
                    w.PreviewDragOver -= previewDragOver;
                    w.Drop -= drop;
                }
            }
        }
        private static void previewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private static void drop(object sender, DragEventArgs e)
        {
            var element = sender as UIElement;
            if (element == null)
                return;
            var command = GetCommand(element);
            var fileInfos = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileInfos != null && command.CanExecute(null))
                command.Execute(fileInfos);
        }
    }
}
