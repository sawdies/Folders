using Folders.Data;
using Folders.Model;
using Folders.View;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace Folders.ViewModel;

public class DirectoryElementVM : NotifyPropertyChangedRealization
{
    public const int MinImageSize = 40;
    public const int MaxImageSize = 256;
    private static int imageSize = 90;
    private bool isFocused;
    public bool IsFocused
    {
        get => isFocused;
        set
        {
            if (isFocused != value)
            {
                isFocused = value;
                if (isFocused) Background = _focusedBrush;
                else
                {
                    Background = Brushes.Transparent;
                    EndEditName.Execute(null!);
                }

            }
        }
    }


    private static bool CtrlIsPressed => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
    private static bool ShiftIsPressed => Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

    public static DirectoryElement? LastSingleFocused { get; set; }
    public static int ImageSizeStatic
    {
        get => imageSize;
        set
        {
            if (value < MinImageSize) imageSize = MinImageSize;
            else if (value > MaxImageSize) imageSize = MaxImageSize;
            else imageSize = value;

            AllNotifySizeChanged();
        }
    }
    public static bool CanIncreaseSize => imageSize < MaxImageSize;
    public static bool CanReduceSize => imageSize > MinImageSize;
    private DateTime _lastClickTime = DateTime.MinValue;
    private DispatcherTimer _doubleClickTimer = new() { Interval = TimeSpan.FromMilliseconds(300) };
    private DirectoryElement Container { get; set; }
    public readonly DeType type;
    private string elementName;
    private string elementPath;
    private string? elementExtention;
    private Brush background = Brushes.Transparent;
    private StackPanel mainStack;
    public Brush Background
    {
        get => background;
        set
        {
            background = value;
            OnPropertyChanged(nameof(Background));
        }
    }
    public int MaxWidth => ImageSize + 20;
    public Thickness ContentImageMargin
    {
        get
        {
            int side = (int)((double)imageSize / 12);
            int bottom = (int)((double)imageSize / 6);
            return new Thickness(side, 0, side, bottom);
        }
    }
    private double imageBorserThickness = 0;
    public double ImageBorserThickness
    {
        get
        {
            if (imageBorserThickness > 0 &&
                imageSize >= 32 &&
                imageSize <= 256) return imageBorserThickness;
            else return 0;
        }
        set
        {
            if (value != imageBorserThickness)
            {
                imageBorserThickness = value;
                OnPropertyChanged(nameof(ImageBorserThickness));
            }
        }
    }
    public int ImageSize
    {
        get => imageSize;
        set
        {
            if (value < MinImageSize) imageSize = MinImageSize;
            else if (value > MaxImageSize) imageSize = MaxImageSize;
            else imageSize = value;

            AllNotifySizeChanged();
        }
    }

    private BitmapImage? imageBitmap;
    private string imagePath = "/Icons/voidFile.png";
    private BitmapImage? ImageBitmap
    {
        get => imageBitmap;
        set
        {
            imageBitmap = value;
            OnPropertyChanged("ImageSourse");
        }
    }
    private string ImagePath
    {
        get => imagePath;
        set
        {
            imagePath = value;
            OnPropertyChanged("ImageSourse");
        }

    }
    public object ImageSourse
    {
        get
        {
            if (imageSize >= 32 && imageSize <= 256) return imageBitmap is not null ? imageBitmap : imagePath;
            else return imagePath;
        }
    }

    public string ElementName
    {
        get => elementName;
        set
        {
            if (value != elementName)
            {
                elementName = value;
                OnPropertyChanged("ElementName");
            }
        }
    }
    public string ElementPath
    {
        get => elementPath;
        set
        {
            elementPath = value;
            if (Directory.GetLogicalDrives().Contains(value))
            {
                elementName = FileFormats.GetVolumeLabel(value);
                ImagePath = new DriveInfo(value).DriveType switch
                {
                    DriveType.Removable => "/Icons/removableDriverFolder.png",
                    _ => value.Contains(DataContainer.SystemDriver)?
                    "/Icons/systemDriverFolder.png" :
                    "/Icons/localDriverFolder.png"
                };
                Container.Back.Visibility = Visibility.Collapsed;
                Container.Content.Margin = new Thickness(0);
                Container.Front.Visibility = Visibility.Collapsed;
            }
            else if (FileFormats.libPath_iconPath.TryGetValue(value, out string iconPath))
            {
                ImagePath = iconPath;
                Container.Back.Visibility = Visibility.Collapsed;
                Container.Content.Margin = new Thickness(0);
                Container.Front.Visibility = Visibility.Collapsed;
            }
            if (!Directory.GetLogicalDrives().Contains(value)) elementName = Path.GetFileName(elementPath);
            OnPropertyChanged("ElementPath");
            OnPropertyChanged("ElementName");
            OnPropertyChanged("ImageSourse");
        }
    }

    void SwitchFocus(bool isFocused)
    {
        IsFocused = isFocused;
        if (isFocused)
        {
            if (!MainVM.FocusedModels.Contains(this)) MainVM.FocusedModels.Add(this);
        }
        else MainVM.FocusedModels.Remove(this);
    }
    public static void MakeAllDisfocused()
    {
        foreach (var item in MainVM.FocusedModels) item.IsFocused = false;
        MainVM.FocusedModels.Clear();
    }
    private void SelectAllFromFirstSelected()
    {
        int firstSelectedIndex = GetFirstSelectedIndex();
        int thisIndex = DataContainer.ElementCollection.IndexOf(Container);
        for (int i = Math.Min(firstSelectedIndex, thisIndex); i <= Math.Max(firstSelectedIndex, thisIndex); i++)
        {
            ((DataContainer.ElementCollection[i] as DirectoryElement).DataContext as DirectoryElementVM).SwitchFocus(true);
        }
    }

    private static int GetFirstSelectedIndex()
    {
        return LastSingleFocused is null ? 0 : DataContainer.ElementCollection.IndexOf(LastSingleFocused);
        foreach (DirectoryElement item in DataContainer.ElementCollection)
        {
            DirectoryElementVM vm = item.DataContext as DirectoryElementVM;
            if (vm.IsFocused) return DataContainer.ElementCollection.IndexOf(item);
        }
        return 0;
    }

    //public DirectoryElementVM (DirectoryItemModel model, DirectoryElement container) // КОНСТРУКТОР - КОНСТРУКТОР - КОНСТРУКТОР - КОНСТРУКТОР
    //{
    //    type = model.DeType;
    //    elementPath = model.Path;
    //    elementName = GetFileName(elementPath);

    //    Container = container;
    //    if (type == DeType.File)
    //    {
    //        elementExtention = GetSubstringAfterLastDot(elementPath);
    //        imagePath = FileFormats.GetPath(elementExtention);
    //        container.Back.Visibility = Visibility.Collapsed;
    //        container.Content.Margin = new Thickness(0);
    //        container.Front.Visibility = Visibility.Collapsed;
    //    }
    //    else if (type == DeType.Directory)
    //    {
    //        if (DataContainer.IsEmpty(elementPath)) imagePath = "";
    //        else
    //        {
    //            string[] files = Directory.GetFiles(elementPath);
    //            if (files.Length > 0)
    //            {
    //                imagePath = $"/Icons/{GetSubstringAfterLastDot(files[0])}File.png";
    //            }
    //            else imagePath = "/Icons/nullFile.png";
    //        }
    //    }
    //    if (type == DeType.File && elementExtention == "exe" || elementExtention == "msi")
    //    {
    //        ImageBitmap = IconExtractor.GetHighResIcon(elementPath);
    //        (Container.Content.MaxWidth, Container.Content.MaxHeight) = (32,32);
    //        ImageBorserThickness = 0.1;
    //    }
    //}
    public DirectoryElementVM(DirectoryItemModel model, DirectoryElement container, StackPanel mainStack)
    {
        Container = container;
        type = model.DeType;
        ElementPath = model.Path;
        elementExtention = SubstrLastDot(elementPath);

        if (Directory.GetLogicalDrives().Contains(elementPath))
        {
            DriveInfo info = new(elementPath);
            mainStack.Children.Add(new ProgressBar()
            {
                Minimum = 0,
                Maximum = info.TotalSize,
                Value = info.TotalSize - info.AvailableFreeSpace,
                Height = 10,
                Foreground = Brushes.Blue,

            });
            mainStack.Children.Add(new TextBlock()
            {
                Text = $"Всего: {(((info.TotalSize) / 1024) / 1024) / 1024} ГБ",
                HorizontalAlignment = HorizontalAlignment.Center
            });
            mainStack.Children.Add(new TextBlock()
            {
                Text = $"Занято: {(((info.TotalSize - info.AvailableFreeSpace) / 1024) / 1024) / 1024} ГБ",
                HorizontalAlignment = HorizontalAlignment.Center
            });
            mainStack.Children.Add(new TextBlock()
            {
                Text = $"Свободно: {(((info.AvailableFreeSpace) / 1024) / 1024) / 1024} ГБ",
                HorizontalAlignment = HorizontalAlignment.Center
            });

        }

        if (elementExtention == "lnk") Container.lnkIcon.Visibility = Visibility.Visible;

        if (type == DeType.File)
        {
            imagePath = FileFormats.GetPath(elementExtention);
            container.Back.Visibility = Visibility.Collapsed;
            container.Content.Margin = new Thickness(0);
            container.Front.Visibility = Visibility.Collapsed;


            if (elementExtention == "exe" || elementExtention == "ico") LoadHighResIconAsync(elementPath);
        }
        else if (type == DeType.Directory &&
                !Directory.GetLogicalDrives().Contains(elementPath) &&
                !FileFormats.libPath_iconPath.ContainsKey(elementPath)) imagePath = "/Icons/nullFile.png"; //LoadDirectoryIconAsync(elementPath);
    }

    private async void LoadHighResIconAsync(string path)
    {
        ImageBitmap = await Task.Run(() => IconExtractor.GetHighResIcon(path));
        (Container.Content.MaxWidth, Container.Content.MaxHeight) = (32, 32);
        Container.ContentBorder.Background = _mouseOnBrush;
        Container.ContentBorder.CornerRadius = new CornerRadius(5);
    }


    private async void LoadDirectoryIconAsync(string path)
    {
        string[] files = await Task.Run(() => Directory.GetFiles(path));
        if (files.Length > 0) imagePath = FileFormats.GetPath(SubstrLastDot(files[0]));
        else imagePath = "/Icons/nullFile.png";
    }



    static string SubstrLastDot(string input)
    {
        int lastDotIndex = input.LastIndexOf('.');
        if (lastDotIndex == -1)
        {
            return input;
        }
        return input[(lastDotIndex + 1)..];
    }

    public async Task ImagesPostInitialization()
    {
        try
        {
            if (imageBitmap is null)
            {
                ImageBitmap = GetBitMap(elementPath, elementExtention == "url");
                ImageBorserThickness = 1;
            }
        }
        catch { }
        await Task.Delay(1);
    }
    public static bool PostInitIsInProgress { get; set; } = false;
    public static async void AllImagesPostInitialization()
    {

        PostInitIsInProgress = true;

        List<DirectoryElementVM> list = DataContainer.ElementCollection
                                        .OfType<DirectoryElement>()
                                        .Select(item => item.DataContext)
                                        .OfType<DirectoryElementVM>()
                                        .Where(vm => vm.type == DeType.File &&
                                                     FileFormats.ImageFormats.Contains(vm.elementExtention!) &&
                                                     vm.imageBitmap == null)
                                        .ToList();

        PostInitIsInProgress = list.Count != 0;
        foreach (DirectoryElementVM element in list)
        {
            if (!PostInitIsInProgress) return;
            await element.ImagesPostInitialization();
        }

        PostInitIsInProgress = false;
        await Task.Delay(1);
        return;
    }
    private static BitmapImage? GetBitMap(string imagePath, bool isUrl)
    {
        BitmapImage bitmap;

        if (isUrl) return null;

        using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            const int maxSize = 250;
            bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();

            int originalWidth = bitmap.PixelWidth;
            int originalHeight = bitmap.PixelHeight;
            if (Math.Max(originalWidth, originalHeight) > maxSize)
            {
                bitmap = null!;
                int newWidth = originalWidth > originalHeight ? maxSize : (int)(originalWidth * ((double)maxSize / originalHeight));
                int newHeight = originalWidth > originalHeight ? (int)(originalHeight * ((double)maxSize / originalWidth)) : maxSize;
                bitmap = new(); ;
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.DecodePixelWidth = newWidth;
                bitmap.DecodePixelHeight = newHeight;
                bitmap.EndInit();
            }
        }
        GC.Collect(0);
        return bitmap;

    }
    private static void AllNotifySizeChanged()
    {
        foreach (DirectoryElement DE in DataContainer.ElementCollection)
        {
            DirectoryElementVM DEVM = DE.DataContext as DirectoryElementVM;
            DEVM.OnPropertyChanged("ImageSize");
            DEVM.OnPropertyChanged("MaxWidth");
            DEVM.OnPropertyChanged("ContentImageMargin");
            DEVM.OnPropertyChanged("ImageSourse");
            DEVM.OnPropertyChanged(nameof(ImageBorserThickness));
        }
    }
    static DirectoryElementVM()
    {
        _mouseOnBrush.Freeze();
        _focusedBrush.Freeze();
    }

    private static readonly Brush _mouseOnBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f2f2f2"));
    private static readonly Brush _focusedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c9e3ff"));
    public RelayCommand MouseEnter => new(obj =>
    {
        if (!IsFocused) Background = _mouseOnBrush;
    });
    public RelayCommand MouseLeave => new(obj =>
    {
        if (!IsFocused) Background = Brushes.Transparent;
    });
    public RelayCommand RightClick => new(obj =>
    {
        if (!IsFocused)
        {
            MakeAllDisfocused();
            SwitchFocus(true);
            LastSingleFocused = Container;
        }
    });
    public RelayCommand StartProcess => new(obj =>
    {
        bool ctrl = CtrlIsPressed;
        bool shift = ShiftIsPressed;

        if (!ctrl && !shift)
        {
            MakeAllDisfocused();
            SwitchFocus(true);
            LastSingleFocused = Container;
        }
        else if (ctrl && !shift)
        {
            SwitchFocus(!IsFocused);
            LastSingleFocused = Container;
        }
        else if (!ctrl && shift)
        {
            MakeAllDisfocused();
            SelectAllFromFirstSelected();
        }

        var timeSinceLastClick = DateTime.Now - _lastClickTime;
        if (timeSinceLastClick < TimeSpan.FromMilliseconds(500))
        {
            if (type == DeType.Directory) DataContainer.MainDataContext.SwitchDirectory(elementPath);
            else Process.Start(new ProcessStartInfo() { FileName = elementPath, UseShellExecute = true });
            _doubleClickTimer.Stop();
        }
        else
        {
            _doubleClickTimer.Start();
        }
        _lastClickTime = DateTime.Now;
    });
    public RelayCommand StartEditName => new(obj =>
    {
        if (LastSingleFocused == Container) StartEditNameAsync(500);
    });

    private async void StartEditNameAsync(int delay)
    {
        await Task.Delay(delay);
        this.Container.block.Visibility = Visibility.Collapsed;
        this.Container.box.Visibility = Visibility.Visible;
    }

    public RelayCommand EndEditName => new(obj =>
    {
        this.Container.block.Visibility = Visibility.Visible;
        this.Container.box.Visibility = Visibility.Collapsed;
    });
    public RelayCommand CheckEnterPressed => new(obj =>
    {
        KeyEventArgs k = obj as KeyEventArgs;
        if (k != null && k.Key == Key.Enter)
        {
            EndEditName.Execute(null);
        }
    });
    public RelayCommand GetFocus => new(obj =>
    {

    });
    public RelayCommand RemoveFolder => new(obj =>
    {

    });
    public RelayCommand RemoveThisFromContext => new(obj =>
    {
        foreach (DirectoryElementVM item in MainVM.FocusedModels)
        {
            DataContainer.ElementCollection.Remove(item.Container);
        }
    });
}