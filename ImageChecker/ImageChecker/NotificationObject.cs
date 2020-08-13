using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

//参考文献 » WPF 学習用ドキュメント作りました http://kisuke0303.sakura.ne.jp/blog/?p=340

namespace ImageChecker
{
    class NotificationObject:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var h = this.PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T target, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(target, value))
                return false;
            target = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}
