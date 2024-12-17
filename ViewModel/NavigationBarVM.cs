using Folders.Data;
using Folders.Model;
using Folders.View;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Folders.ViewModel;

public class NavigationBarVM : NotifyPropertyChangedRealization
{
    private readonly UIElementCollection collector;
    private static string[] ComputerPathsKit => Directory.GetLogicalDrives();

    public NavigationBarVM(NavigationBarKit kit, UIElementCollection collector)
    {
        this.collector = collector;
        Split(Brushes.Transparent);
        collector.Add(new TreeElement(FileFormats.DesktopPath, collector, 0, false));
        collector.Add(new TreeElement(FileFormats.DownloadsPath, collector, 0, false));
        collector.Add(new TreeElement(FileFormats.DocumentsPath, collector, 0, false));
        collector.Add(new TreeElement(FileFormats.PicturesPath, collector, 0, false));
        collector.Add(new TreeElement(FileFormats.VideosPath, collector, 0, false));
        collector.Add(new TreeElement(FileFormats.MusicPath, collector, 0, false));


        Split(Brushes.LightGray);


        if ((kit & NavigationBarKit.Computer) != 0)
        {
            collector.Add(new TreeElement("***Computer", "Компьютер", collector, () => Directory.GetLogicalDrives(), "/Icons/computerIcon.png", 0, true));
        }
        if ((kit & NavigationBarKit.Libraries) != 0)
        {
            collector.Add(new TreeElement("***Libraries", "Библиотеки", collector, () => FileFormats.LibrariesPathsKit, "/Icons/libraryIcon.png", 0, false));
        }
        if ((kit & NavigationBarKit.RemovableDisks) != 0)
        {
            string[] drivers = Directory.GetLogicalDrives();
            if (drivers.Where(s => new DriveInfo(s).DriveType == DriveType.Removable).Count() > 0)
            {
                Split(Brushes.LightGray);
                foreach (string item in drivers)
                {
                    if (new DriveInfo(item).DriveType == DriveType.Removable)
                    {
                        collector.Add(new TreeElement(item, collector, 0, true));
                    }
                }
            }

        }

        Split(Brushes.Transparent);
    }
    private void Split(Brush borderBrush)
    {
        collector.Add(new Border()
        {
            Height = 0.3,
            Margin = new Thickness(0, 10, 0, 10),
            Background = borderBrush
        });
    }

    public RelayCommand MouseEnter => new(obj =>
    {
        isAnimation = false;
        foreach (TreeElement item in from object item in collector
                                     where item is TreeElement
                                     select item)
        {
            (item.DataContext as TreeElementVM)!.ArrowContainerOpacity = 1;
        }
    });
    public RelayCommand MouseLeave => new(obj =>
    {
        SmoothFocusing();
    });


    private bool isAnimation = false;
    public async void SmoothFocusing()
    {
        if (!isAnimation)
        {
            isAnimation = true;
            TreeElement te = (collector[1] as TreeElement)!;
            TreeElementVM vm = (te.DataContext as TreeElementVM)!;
            while (vm.ArrowContainerOpacity != 0.4)
            {
                if (!isAnimation) return;
                foreach (TreeElement item in from object item in collector
                                             where item is TreeElement
                                             select item)
                {
                    (item.DataContext as TreeElementVM)!.ArrowContainerOpacity -= 0.02;
                    if ((item.DataContext as TreeElementVM)!.ArrowContainerOpacity < 0.4)
                    {
                        (item.DataContext as TreeElementVM)!.ArrowContainerOpacity = 0.4;
                        isAnimation = false;
                    }
                }
                await Task.Delay(1);
            }
            isAnimation = false;
        }

    }
}
/*
 * HorizontalAlignment="Left"
   VirtualizingPanel.IsVirtualizing="True"
   VirtualizingPanel.VirtualizationMode="Recycling"
   Width="200">*/