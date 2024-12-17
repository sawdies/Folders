using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Folders.Model
{
    public class DirectoryItemModel : NotifyPropertyChangedRealization
    {
        private string path;
        public readonly DeType DeType;
        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged("Path");
            }
        }
        public DirectoryItemModel(string path, DeType type)
        {
            Path = path;
            DeType = type;
        }
    }
}
