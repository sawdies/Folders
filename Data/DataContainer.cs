using Folders.Model;
using Folders.View;
using Folders.ViewModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
namespace Folders.Data;

public static class DataContainer
{
    public static MainWindow Window { get; set; }
    public static MainVM MainDataContext { get; set; }
    public static UIElementCollection ElementCollection { get; set; }
    public static UIElementCollection TreeElementCollection { get; set; }

    public static string SystemDriver = Environment.GetEnvironmentVariable("SystemDrive");

    public static readonly List<string> ForbiddenPaths =
    [
        "System Volume Information",
        "C:\\Documents and Settings",
        "C:\\Users\\Default User",
        "C:\\Users\\sawd\\Application Data"
    ];
    public static async Task<List<DirectoryElement>> ModelsLoader(string directoryPath)
    {
        List<DirectoryElement> result = new();
        await Task.Delay(1);
        try
        {
            if (FileFormats.SpesialPaths.Contains(directoryPath))
            {
                foreach (string path in FileFormats.specialPath_childrentaths[directoryPath])
                {
                    result.Add(new(new(path, DeType.Directory)));
                }
            }
            else
            {
                foreach (string path in Directory.GetDirectories(directoryPath))
                {
                    result.Add(new(new(path, DeType.Directory)));
                }
                foreach (string path in Directory.GetFiles(directoryPath))
                {
                    result.Add(new(new(path, DeType.File)));
                }
            }
        }
        catch { }

        //foreach (DirectoryElement item in result)
        //{
        //    if ((item.DataContext as DirectoryElementVM)!.ElementPath == @"D:\Документы\desktop.ini") MessageBox.Show(File.GetAttributes((item.DataContext as DirectoryElementVM)!.ElementPath).ToString());
        //    FileAttributes targetPar = File.GetAttributes((item.DataContext as DirectoryElementVM)!.ElementPath);
        //    if (targetPar != FileAttributes.Normal)
        //    {
        //        result.Remove(item);
        //    }
        //} 
        return result;
    }
    public static bool IsEmpty(string directoryPath)
    {
        return Directory.GetDirectories(directoryPath).Length + Directory.GetFiles(directoryPath).Length == 0;
    }

    public static BitmapImage LoadFileIcon(string filePath)
    {
        Icon fileIcon = Icon.ExtractAssociatedIcon(filePath)!;

        using (Bitmap bitmap = fileIcon.ToBitmap())
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }


    public static string GetFileName(string filePath)
    {
        for (int i = filePath.Length - 1; i > 0; i--)
        {
            if (filePath[i] == '\\') return filePath[(i + 1)..];
        }
        return filePath;
    }
}
