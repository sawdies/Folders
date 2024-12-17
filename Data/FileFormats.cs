using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
namespace Folders.Data;

public static class FileFormats
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

    public static string GetVolumeLabel(string driverPath)
    {
        DriveInfo driveInfo = new(driverPath);
        return $"{driveInfo.VolumeLabel} ({driverPath[..2]})";
    }

    public static string GetPath(string fileExtention)
    {
        fileExtention = fileExtention.ToLower();
        if (format_icon.TryGetValue(fileExtention, out string? result)) return result;
        return "/Icons/voidFile.png";
    }
    public static string GetFolderIcon(string filePath)
    {
        if (specialFolders.TryGetValue(filePath, out string? result)) return result;
        return "/Icons/folderIcon.png";
    }
    public static string GetFolderIcon(DriveType type)
    {
        if (driveType_icon.TryGetValue(type, out string? result)) return result;
        return "/Icons/driverIcon.png";
    }
    public static string GetSpesialName(string filePath)
    {
        if (specialNames.TryGetValue(filePath, out string? result)) return result;
        return DataContainer.GetFileName(filePath);
    }
    public readonly static HashSet<string> ImageFormats = ["jpg", "jpeg", "png", "gif", "bmp", "tif", "tiff"];
    private static readonly Dictionary<string, string> format_icon = new()
    {
        { "wav", "/Icons/mp3File.png" },
        { "wave", "/Icons/mp3File.png" },
        { "aiff", "/Icons/mp3File.png" },
        { "flac", "/Icons/mp3File.png" },
        { "alac", "/Icons/mp3File.png" },
        { "ape", "/Icons/mp3File.png" },
        { "mp3", "/Icons/mp3File.png" },
        { "aac", "/Icons/mp3File.png" },
        { "wma", "/Icons/mp3File.png" },
        { "mid", "/Icons/mp3File.png" },
        { "ogg", "/Icons/mp3File.png" },
        { "mp4", "/Icons/mp4File.png" },
        { "mov", "/Icons/mp4File.png" },
        { "wmv", "/Icons/mp4File.png" },
        { "avi", "/Icons/mp4File.png" },
        { "avchd", "/Icons/mp4File.png" },
        { "mkv", "/Icons/mp4File.png" },
        { "mpeg-2", "/Icons/mp4File.png" },
        { "png", "/Icons/jpgFile.png" },
        { "jpg", "/Icons/jpgFile.png" },
        { "jpeg", "/Icons/jpgFile.png" },
        { "gif", "/Icons/jpgFile.png" },
        { "raw", "/Icons/jpgFile.png" },
        { "bmp", "/Icons/jpgFile.png" },
        { "tif", "/Icons/jpgFile.png" },
        { "tiff", "/Icons/jpgFile.png" },
        { "accdb", "/Icons/accdbFile.png" },
        { "aep", "/Icons/aepFile.png" },
        { "ai", "/Icons/aiFile.png" },
        { "c", "/Icons/cFile.png" },
        { "cpp", "/Icons/cppFile.png" },
        { "cs", "/Icons/csFile.png" },
        { "dll", "/Icons/dllFile.png" },
        { "ini", "/Icons/iniFile.png" },
        { "doc", "/Icons/docFile.png" },
        { "docm", "/Icons/docmFile.png" },
        { "docx", "/Icons/docxFile.png" },
        { "exe", "/Icons/exeFile.png" },
        { "indd", "/Icons/inddFile.png" },
        { "iso", "/Icons/isoFile.png" },
        { "java", "/Icons/javaFile.png" },
        { "json", "/Icons/jsonFile.png" },
        { "mdb", "/Icons/mdbFile.png" },
        { "msi", "/Icons/msiFile.png" },
        { "pdf", "/Icons/pdfFile.png" },
        { "ppt", "/Icons/pptFile.png" },
        { "pptx", "/Icons/pptxFile.png" },
        { "pptm", "/Icons/pptmFile.png" },
        { "prproj", "/Icons/prprojFile.png" },
        { "psd", "/Icons/psdFile.png" },
        { "py", "/Icons/pyFile.png" },
        { "pyw", "/Icons/pywFile.png" },
        { "sln", "/Icons/slnFile.png" },
        { "sql", "/Icons/sqlFile.png" },
        { "txt", "/Icons/txtFile.png" },
        { "url", "/Icons/urlFile.png" },
        { "vsdm", "/Icons/vsdmFile.png" },
        { "vsdx", "/Icons/vsdxFile.png" },
        { "xaml", "/Icons/xamlFile.png" },
        { "xls", "/Icons/xlsFile.png" },
        { "xlsm", "/Icons/xlsmFile.png" },
        { "xlsx", "/Icons/xlsxFile.png" },
        { "xml", "/Icons/xmlFile.png" },
        { "rtf", "/Icons/rtfFile.png"},
        { "md", "/Icons/mdFile.png"}
    };
    private static readonly Dictionary<string, string> specialFolders = new()
    {
        {UserFolderPath, "/Icons/userFolderIcon.png"},
        {DesktopPath, "/Icons/desktopFolderIcon.png"},
        {DownloadsPath, "/Icons/downloadsFolderIcon.png"},
        {PicturesPath, "/Icons/imagesFolderIcon.png"},
        {DocumentsPath, "/Icons/documentsFolderIcon.png"},
        {VideosPath, "/Icons/videosFolderIcon.png"},
        {MusicPath, "/Icons/musicFolderIcon.png"},
        {$"{Environment.GetEnvironmentVariable("SystemDrive")}\\Windows", "/Icons/windowsFolderIcon.png" }
    };
    private static readonly Dictionary<string, string> specialNames = new()
    {
        {DesktopPath, "Рабочий стол"},
        {DownloadsPath, "Загрузки"},
        {PicturesPath, "Изображения"},
        {DocumentsPath, "Документы"},
        {VideosPath, "Видео"},
        {MusicPath, "Музыка"},
    };
    private static readonly Dictionary<DriveType, string> driveType_icon = new()
    {
        {DriveType.Unknown, "/Icons/unknownDriverIcon.png"},
        {DriveType.Removable, "/Icons/removableDriverIcon.png"},
        {DriveType.Fixed, "/Icons/driverIcon.png"},
    };

    public static readonly string[] SpesialPaths =
    {
        "***Computer",
        "***Libraries",
        "***QuickTouchPanel",
    };


    public static Dictionary<string, string[]> specialPath_childrentaths => new()
    {
        {"***Computer", Directory.GetLogicalDrives()},
        {"***Libraries", LibrariesPathsKit},
        { "***QuickTouchPanel", []}
    };
    public static readonly Dictionary<string, string> specialPath_name = new()
    {
        {"***Computer", "Компьютер" },
        {"***Libraries", "Библиотеки"},
        { "***QuickTouchPanel", "Панель быстрого доступа"}
    };
    public static readonly Dictionary<string, string> name_specialPath = new()
    {
        {"Компьютер","***Computer"},
        {"Библиотеки" , "***Libraries"},
        {"Панель быстрого доступа" , "***QuickTouchPanel"}
    };

    public static readonly Dictionary<string, string> libPath_iconPath = new()
    {
        {FileFormats.DesktopPath,"/Icons/desktopFolder.png"},
        {FileFormats.DownloadsPath, "/Icons/downloadsFolder.png"},
        {FileFormats.PicturesPath, "/Icons/picturesFolder.png"}
    };

    public static string[] LibrariesPathsKit =>
    [
        FileFormats.UserFolderPath,
        FileFormats.DesktopPath,
        FileFormats.DownloadsPath,
        FileFormats.PicturesPath,
        FileFormats.DocumentsPath,
        FileFormats.VideosPath,
        FileFormats.MusicPath,
    ];

    public static string UserFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public static string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public static string DownloadsPath => GetKnownFolderPath(DownloadsGuid);
    public static string PicturesPath => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    public static string DocumentsPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public static string VideosPath => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
    public static string MusicPath => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

    private static Guid DownloadsGuid => new Guid("374DE290-123F-4565-9164-39C4925E467B");

    public static List<byte[]> ImageHeaders { get; } =
    [
        new byte[] { 0xFF, 0xD8, 0xFF },
        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
        new byte[] { 0x47, 0x49, 0x46, 0x38 },
        new byte[] { 0x42, 0x4D },
        new byte[] { 0x49, 0x49, 0x2A, 0x00 },
        new byte[] { 0x4D, 0x4D, 0x00, 0x2A },
    ];
    private static string GetKnownFolderPath(Guid knownFolderId)
    {
        IntPtr pszPath;
        int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
        if (hr >= 0)
        {
            string path = Marshal.PtrToStringUni(pszPath);
            Marshal.FreeCoTaskMem(pszPath);
            return path;
        }
        else
        {
            throw new ExternalException("Failed to retrieve the known folder path.", hr);
        }
    }


    public static string GetDonloadsPath() => GetKnownFolderPath(DownloadsGuid);
}