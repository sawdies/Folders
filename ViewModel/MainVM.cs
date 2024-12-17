using Folders.Data;
using Folders.Model;
using Folders.View;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace Folders.ViewModel;

public class MainVM : NotifyPropertyChangedRealization
{
    public static MouseWheelEventArgs? MouseWheelEventArgs;
    public static Action<bool> ChangeSize;
    private static bool useSmoothAnimations = false;
    private static bool isChanging = false;
    private static string pathBoxText;
    private static string directiryElementsCount;
    private static double windowBlurRadius = 0;
    private static Visibility dialogPanelVisibility = Visibility.Collapsed;
    public static ObservableCollection<DirectoryElementVM> FocusedModels { get; set; } = new();
    public string DirectiryElementsCount
    {
        get => directiryElementsCount;
        set
        {
            directiryElementsCount = value;
            OnPropertyChanged(nameof(DirectiryElementsCount));
        }
    }
    public double WindowBlurRadius
    {
        get => windowBlurRadius;
        set
        {
            windowBlurRadius = value;
            OnPropertyChanged(nameof(windowBlurRadius));
        }
    }
    public Visibility DialogPanelVisibility
    {
        get => dialogPanelVisibility;
        set
        {
            dialogPanelVisibility = value;
            OnPropertyChanged(nameof(dialogPanelVisibility));
        }
    }

    private Stack<string> travelHistory = new();
    public static MainWindow Window
    {
        get => DataContainer.Window;
        set => DataContainer.Window = value;
    }
    private static bool CtrlIsPressed => (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
    public static object Content
    {
        get => Window.DirectoryItems.Content;
        set => Window.DirectoryItems.Content = value;
    }
    private static UIElementCollection ElementCollection
    {
        get => DataContainer.ElementCollection;
        set
        {
            DataContainer.ElementCollection = value;

        }
    }
    public Visibility VoidFolderLabelVisibility
    {
        get => DataContainer.IsEmpty(pathBoxText) ? Visibility.Collapsed : Visibility.Visible;
    }
    public string PathBoxText
    {
        get => pathBoxText;
        set
        {
            string res = "";
            FileFormats.specialPath_name.TryGetValue(value, out res);
            if (value != pathBoxText && Directory.Exists(value) ||
                FileFormats.SpesialPaths.Contains(value) && pathBoxText != res) SetPath(value);
        }
    }
    public string PathBoxTextNonReturnable
    {
        get => pathBoxText;
        set
        {
            if (value != pathBoxText && Directory.Exists(value))
            {
                pathBoxText = value;
                OnPropertyChanged(nameof(PathBoxText));
                SwitchDirectory(value);
            }
        }
    }
    private string pathBoxTextPostSwitching
    {
        set
        {
            travelHistory.Push(pathBoxText);
            pathBoxText = value;
            OnPropertyChanged(nameof(PathBoxText));
        }
    }
    public MainVM()
    {
        ChangeSize = useSmoothAnimations ? SmoothSizeChanger : NoneSmoothSizeChanger;
        PathBoxText = "***Computer";
        //driversWatcher = new DriversWatcher();
    }
    private static async Task RedefineContent(WrapPanel p)
    {
        DirectoryElementVM.PostInitIsInProgress = false;
        DirectoryElementVM.LastSingleFocused = null;
        MainVM.FocusedModels.Clear();
        DataContainer.ElementCollection = p.Children;
        DirectoryElementVM.AllImagesPostInitialization();

        if (p.Children.Count == 0) Window.VoidFolderLabel.Visibility = Visibility.Visible;
        else Window.VoidFolderLabel.Visibility = Visibility.Collapsed;
        p.MouseWheel += Window.DirectoryItems_MouseWheel;
        p.MouseDown += Window.MainWrapMouseDown;
        p.Margin = new Thickness(15, 0, 0, 0);

        await Task.Delay(10);
        Content = p;

        DataContainer.Window.Cursor = Cursors.Arrow;
        GC.Collect();
    }
    public async void SwitchDirectory(string path)
    {
        Content = null!;
        DataContainer.Window.Cursor = Cursors.Wait;
        pathBoxTextPostSwitching = path;
        WrapPanel newContent = new();
        var UIcollection = await DataContainer.ModelsLoader(path);
        //foreach (var item in await DataContainer.ModelsLoader(path)) newContent.Children.Add(item);

        foreach (var item in UIcollection) newContent.Children.Add(item);
        DirectiryElementsCount = $"Элементов: {newContent.Children.Count}";
        await RedefineContent(newContent);
        OnPropertyChanged(nameof(VoidFolderLabelVisibility));
        GC.Collect(0);
    }
    public void UpdateDirectory() => PathBoxText = pathBoxText;
    private static async void HeddenVisible(WrapPanel p)
    {
        //p.Visibility = Visibility.Hidden;
        //await Task.Delay(100);
        //
        p.Opacity = 0;
        p.Visibility = Visibility.Visible;
        while (p.Opacity != 1)
        {
            p.Opacity += 0.03;
            await Task.Delay(1);
        }
    }
    private static async void SmoothSizeChanger(bool increase)
    {
        int stepModule = increase ? 1 : -1;
        int referenceValue = increase ? DirectoryElementVM.MaxImageSize : DirectoryElementVM.MinImageSize;
        if (!isChanging)
        {
            isChanging = true;
            int targetValue = DirectoryElementVM.ImageSizeStatic + Math.Max(DirectoryElementVM.ImageSizeStatic / 10, 5) * stepModule;
            int difference = DirectoryElementVM.ImageSizeStatic - targetValue;
            int iterations = Math.Max(difference, difference * -1);
            for (int i = 0; i < iterations; i++)
            {
                DirectoryElementVM.ImageSizeStatic += stepModule * 2;
                if (DirectoryElementVM.ImageSizeStatic == referenceValue)
                {
                    isChanging = false;
                    return;
                }
                if (i % 2 == 0) await Task.Delay(1);
            }
            isChanging = false;
            MouseWheelEventArgs = null;
        }
    }
    private static async void NoneSmoothSizeChanger(bool increase)
    {
        await Task.Delay(0);
        int stepModule = increase ? 1 : -1;
        DirectoryElementVM.ImageSizeStatic += Math.Max(DirectoryElementVM.ImageSizeStatic / 10, 5) * stepModule;
        MouseWheelEventArgs = null;
    }
    private async void ImagesLoader()
    {
      
    }
    private void SetPath(string value)
    {
        travelHistory.Push(pathBoxText);
        pathBoxText = value;
        OnPropertyChanged(nameof(PathBoxText));
        SwitchDirectory(value);
        if (FileFormats.SpesialPaths.Contains(value))
        {
            pathBoxTextPostSwitching = FileFormats.specialPath_name[value];
        }
    }
    public RelayCommand ImageSizeChanger => new(obj => ChangeSize(MouseWheelEventArgs?.Delta > 0));
    public RelayCommand GoHomeDirectory => new(obj =>
    {
        PathBoxText = "***Computer";
    },(obj) => pathBoxText != "Компьютер");
    public RelayCommand GoParentDirectory => new(obj =>
    {
        if (pathBoxText == "Библиотеки")
        {
            PathBoxText = "***Computer";
            return;
        }

        DirectoryInfo parentDirectory = Directory.GetParent(pathBoxText)!;
        if (parentDirectory is null) PathBoxText = "***Computer";
        else PathBoxText = parentDirectory.FullName;

    }, (obj) => pathBoxText != "Компьютер");
    public RelayCommand GoLastDirectory => new(obj =>
    {
        
    });
    public RelayCommand CancelGoLastDirectory => new(obj =>
    {

    });
    public RelayCommand СreateNewFile => new(obj =>
    {
        ImagesLoader();
        //SwitchFolder();
    });
    public RelayCommand SetNextContext => new(obj =>
    {

    });
    public RelayCommand OnOffLeftPanel => new(obj =>
    {
        //if (Window.Width < 300) Window.LeftPanel.Visibility = Visibility.Collapsed;
        //else Window.LeftPanel.Visibility = Visibility.Visible;
    });
    public RelayCommand CleanElements => new(obj =>
    {
        ElementCollection.Clear();
        GC.Collect();
    });
    public RelayCommand GoToDesktop => new(obj =>
    {
        SwitchDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    });
    public RelayCommand GoToDownloads => new(obj =>
    {
        SwitchDirectory(FileFormats.GetDonloadsPath());
    });
    public RelayCommand GoToImages => new(obj =>
    {
        SwitchDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
    });
    public RelayCommand GoToDocuments => new(obj =>
    {
        SwitchDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    });
    public RelayCommand UpdateCurrentDir => new(obj =>
    {
        SetPath(Directory.Exists(pathBoxText) ? pathBoxText : FileFormats.name_specialPath[pathBoxText]);
    });
    public RelayCommand GoToDialogPanel => new(obj =>
    {
        DataContainer.Window.CleanFrontGrid();
        DialogPanelSwitcher(true);

    });
    public RelayCommand GoFromDialogPanel => new(obj =>
    {
        //WindowBlurRadius = 0;
        //DialogPanelVisibility = Visibility.Collapsed;
        DialogPanelSwitcher(false);
    });
    public RelayCommand AddImageFile => new(obj =>
    {
        DataContainer.ElementCollection.Add(new DirectoryElement(new(@"D:\Изображения\неокотик.jpg", DeType.File)));
    });
    private async void DialogPanelSwitcher(bool to = true)
    {
        DialogPanelVisibility = to ? Visibility.Visible : Visibility.Collapsed;
        double step = to ? 1 : -1;
        double target = to ? 10 : 0;
        while (WindowBlurRadius != target)
        {
            WindowBlurRadius += 0.5 * step;
            await Task.Delay(1);
        }
    }



}
