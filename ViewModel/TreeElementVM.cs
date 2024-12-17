using Folders.Data;
using Folders.Model;
using Folders.View;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Folders.ViewModel;

public abstract class TreeElementVM : INotifyPropertyChanged
{
    protected const int START_INDEX = 7;
    protected const double FOCUSED_ARROW_OPACITY = 0.7;
    protected const double DISFOCUSED_ARROW_OPACITY = 0.3;
    protected const string RIGHT_ARROW_ICON_PATH = @"/Icons/rightArrowIcon.png";
    protected const string DOWN_ARROW_ICON_PATH = @"/Icons/downArrowIcon.png";
    protected static readonly Brush _defaultBrush = Brushes.Transparent;                                        // Transparent
    protected static readonly Brush _whenChildFocusedBrush = Brushes.LightGray;                                 // Gray 
    protected static readonly Brush _focusedBrush = new SolidColorBrush(Color.FromRgb(201, 227, 255));          // Light Blue
    protected static readonly Brush _mouseOnBrush = new SolidColorBrush(Color.FromRgb(242, 242, 242));          // Light Gray
    protected static readonly Dictionary<TreeElementConditions, Brush> _condition_brush = new()
    {
        { TreeElementConditions.Default, _defaultBrush},
        { TreeElementConditions.Focused, _focusedBrush},
        { TreeElementConditions.PreFocused, _whenChildFocusedBrush},
    };
    protected static Action<string> Execute = path => DataContainer.MainDataContext.PathBoxText = path;
    protected static bool isAboveArrow = false;

    protected TreeElementConditions condition = TreeElementConditions.Default;
    protected string arrowImagesourse = RIGHT_ARROW_ICON_PATH;
    protected double arrowOpacity = DISFOCUSED_ARROW_OPACITY;
    protected double arrowContainerOpacity = 1;
    protected Brush background = _defaultBrush;
    protected bool isExpanded = false;
    protected List<TreeElementVM>? children = null;
    protected string elementPath;
    protected TreeElement container;
    protected readonly UIElementCollection elementCollection;
    protected readonly int layer;
    protected string elementName;
    protected readonly bool canExpand;
    protected int bottomMargin;
    protected string iconImagesourse;
    protected Visibility arrowVisibility;

    static TreeElementVM()
    {
        _whenChildFocusedBrush.Freeze();
        _mouseOnBrush.Freeze();
        _focusedBrush.Freeze();
    }
    public TreeElementVM(string elementPath, TreeElement container, UIElementCollection elementCollection, int layer, bool canExpand, int bottomMargin)
    {
        this.elementPath = elementPath;
        this.container = container;
        this.elementCollection = elementCollection;
        this.layer = layer;
        this.canExpand = canExpand;
        this.bottomMargin = bottomMargin;

        this.elementName = Directory.GetLogicalDrives().Contains(elementPath) ? FileFormats.GetVolumeLabel(elementPath) : (canExpand ? Path.GetFileName(elementPath) : FileFormats.GetSpesialName(elementPath));
        iconImagesourse = DefineIcon(elementPath);

        if (FileFormats.SpesialPaths.Contains(elementPath)) arrowVisibility = Visibility.Visible;
        else arrowVisibility = canExpand ? (Directory.GetDirectories(elementPath).Length > 0 ? Visibility.Visible : Visibility.Hidden) : Visibility.Hidden;

    }

    protected static TreeElementVM? focusedElement { get; set; }
    protected static TreeElementVM? preFocusedElement { get; set; }
    protected bool CanBeTriggered
    {
        get => condition == TreeElementConditions.Default;
    }
    public TreeElementConditions Condition
    {
        get => condition;
        set
        {
            if (condition != value)
            {
                condition = value;
                Background = _condition_brush[condition];
                if (condition == TreeElementConditions.Focused) FocusedElement = this;
                else if (condition == TreeElementConditions.PreFocused) PreFocusedElement = this;
            }
        }
    }
    protected static TreeElementVM? FocusedElement
    {
        get => focusedElement;
        set
        {
            if (value is null && focusedElement is not null) focusedElement.Condition = TreeElementConditions.Default;
            focusedElement = value;
        }
    }
    protected static TreeElementVM? PreFocusedElement
    {
        get => preFocusedElement;
        set
        {
            if (value is null && preFocusedElement is not null) preFocusedElement.Condition = TreeElementConditions.Default;
            preFocusedElement = value;
        }
    }
    public string ElementPath
    {
        get => elementPath;
        set
        {
            elementPath = value;
            OnPropertyChanged(nameof(ElementPath));
            ElementPath = DataContainer.GetFileName(elementPath);
        }
    }
    public string ElementName
    {
        get => elementName;
        set
        {
            elementName = value;
            OnPropertyChanged(nameof(ElementName));
        }
    }
    public string IconImagesourse
    {
        get => iconImagesourse;
        set
        {
            iconImagesourse = value;
            OnPropertyChanged(nameof(IconImagesourse));
        }
    }
    public string ArrowImagesourse
    {
        get => arrowImagesourse;
        set
        {
            arrowImagesourse = value;
            OnPropertyChanged(nameof(ArrowImagesourse));
        }
    }
    public Brush Background
    {
        get => background;
        set
        {
            background = value;
            OnPropertyChanged(nameof(Background));
        }
    }
    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            isExpanded = value;
            ArrowImagesourse = isExpanded ? DOWN_ARROW_ICON_PATH : RIGHT_ARROW_ICON_PATH;
            ArrowOpacity = !isExpanded ? FOCUSED_ARROW_OPACITY : DISFOCUSED_ARROW_OPACITY;
        }
    }
    public double ArrowOpacity
    {
        get => arrowOpacity;
        set
        {
            arrowOpacity = value;
            OnPropertyChanged(nameof(ArrowOpacity));
        }
    }
    public double ArrowContainerOpacity
    {
        get => arrowContainerOpacity;
        set
        {
            arrowContainerOpacity = value;
            OnPropertyChanged(nameof(ArrowContainerOpacity));
        }
    }
    public Visibility ArrowVisibility
    {
        get => arrowVisibility;
        set
        {
            arrowVisibility = value;
            OnPropertyChanged(nameof(ArrowVisibility));
        }
    }
    public Thickness ArrowMargin
    {
        get => new(5 + layer * 10, 5, 0, 5);
    }
    public Thickness GlobalMargin
    {
        get => new(0, 0, 0, bottomMargin);
    }
    public List<TreeElementVM>? Children
    {
        get => children;
        set
        {
            children = value;
            ArrowVisibility = children is null || children.Count == 0 ? Visibility.Hidden : Visibility.Visible;
        }
    }
    public RelayCommand MouseEnter => new(obj =>
    {
        Background = _mouseOnBrush;
    }, (obj) => CanBeTriggered);
    public RelayCommand MouseLeave => new(obj =>
    {
        Background = _defaultBrush;
    }, (obj) => CanBeTriggered);
    public RelayCommand ArrowMouseEnter => new(obj =>
    {
        isAboveArrow = true;
        ArrowOpacity = isExpanded ? DISFOCUSED_ARROW_OPACITY : FOCUSED_ARROW_OPACITY;
    });
    public RelayCommand ArrowMouseLeave => new(obj =>
    {
        isAboveArrow = false;
        ArrowOpacity = !isExpanded ? DISFOCUSED_ARROW_OPACITY : FOCUSED_ARROW_OPACITY;
    });
    public RelayCommand СontrolChildrenElements => new(obj =>
    {
        if (children is null)
        {
            try
            {
                LoadChildren();
                OpenChildren();
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show($"Кажется, директория {elementPath} не существует, ссылки будут удалены.");
                RecursiveCollapsing();
                elementCollection.Remove(container);
                return;
            }
        }
        else if (isExpanded) RecursiveCollapsing();
        else OpenChildren();
    });
    public RelayCommand OpenDirectory => new(obj =>
    {
        if (focusedElement is not null || preFocusedElement is not null)
        {
            foreach (TreeElement item in from UIElement item in elementCollection
                                         where item is TreeElement
                                         where ((item as TreeElement)!.DataContext as TreeElementVM)!.condition != TreeElementConditions.Default
                                         select item)
            {
                (item.DataContext as TreeElementVM)!.Condition = TreeElementConditions.Default;
            }
        }
        Condition = TreeElementConditions.Focused;
        Execute(elementPath);
    }, (obj) => !isAboveArrow);

    public void AddDriver(string drivePath)
    {
        //elementCollection.Add(new TreeElement(TreeElementType.Actual, drivePath));
    }
    public void RemoveDriver(string drivePath)
    {
        TreeElementVM targetItem = (TreeElementVM)(from TreeElementVM item in elementCollection
                                                   where item.elementPath == drivePath
                                                   select item);
        MessageBox.Show(targetItem.elementPath);

    }
    protected static string DefineIcon(string driverPath)
    {
        if (Directory.GetLogicalDrives().Contains(driverPath))
        {
            if (driverPath == DataContainer.SystemDriver + "\\") return @"/Icons/systemDriverIcon.png";
            return FileFormats.GetFolderIcon(new DriveInfo(driverPath).DriveType);
        }
        return FileFormats.GetFolderIcon(driverPath);
    }
    protected async void LoadChildren()
    {
        children = new();
        int insertionIndex = elementCollection.IndexOf(container) + 1;
        string[] childrenPaths = GetChildren();
        int newLayer = layer + 1;
        for (int i = 0; i < childrenPaths.Length; i++)
        {
            try
            {
                var child = CreateChild(childrenPaths[i], newLayer); // new TreeElement(childrenPaths[i], elementCollection, newLayer);
                elementCollection.Insert(insertionIndex, child);
                TreeElementVM VM = (child.DataContext as TreeElementVM)!;
                Children!.Add(VM);
                insertionIndex++;
            }
            catch (UnauthorizedAccessException) { }
        }
        await Task.Delay(1);
    }
    protected void OpenChildren()
    {
        IsExpanded = true;
        Visibility settable = isExpanded ? Visibility.Visible : Visibility.Collapsed;
        foreach (var child in children!)
        {
            child.container.Visibility = Visibility.Visible;
            if (focusedElement is not null && child.elementPath == focusedElement.elementPath)
                child.Condition = TreeElementConditions.Focused;
            if (preFocusedElement is not null && child.elementPath == preFocusedElement.elementPath)
                child.Condition = TreeElementConditions.PreFocused;
        }
    }
    protected void RecursiveCollapsing()
    {
        IsExpanded = false;
        if (children is null) return;
        foreach (var child in children!) child.RecursiveCollapsing();
        for (int i = children!.Count - 1; i >= 0; i--)
        {
            elementCollection.Remove(children[i].container);
        }
        children.Clear();
        children = null;
    }
    protected bool FocusedIsDescendant(out TreeElementVM? nextAncestor)
    {
        nextAncestor = null;

        if (focusedElement is null ||
            elementCollection.IndexOf(focusedElement.container)! < elementCollection.IndexOf(container)) return false;

        bool result = focusedElement?.elementPath.StartsWith(elementPath, StringComparison.OrdinalIgnoreCase) ?? false;

        if (!result) return false;

        string relativePath = focusedElement!.elementPath.Substring(elementPath.Length);
        string[] pathParts = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        string nextAncestorPath;

        if (pathParts.Length > 0) nextAncestorPath = Path.Combine(elementPath, pathParts[0]);
        else return false;

        for (int i = elementCollection.IndexOf(container) + 1; i < elementCollection.Count; i++)
        {
            TreeElement VMContainer = (elementCollection[i] as TreeElement)!;
            TreeElementVM potentialVM = (VMContainer.DataContext as TreeElementVM)!;
            if (potentialVM.elementPath == nextAncestorPath)
            {
                nextAncestor = potentialVM;
                return true;
            }
        }

        return false;
    }
    protected abstract string[] GetChildren();
    protected abstract TreeElement CreateChild(string childPath, int layer);



    #region INotifyPropertyChanged realization
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
    #endregion
}

