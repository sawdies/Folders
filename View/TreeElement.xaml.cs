using Folders.ViewModel;
using System.Reflection.Emit;
using System.Windows.Controls;
namespace Folders.View;

public partial class TreeElement : UserControl
{
    public TreeElement(string elementPath, UIElementCollection elementCollection, int layer = 0, bool canExpand = true, int bottomMargin = 0)
    {
        InitializeComponent();
        this.DataContext = new ActualTreeElementVM(elementPath, this, elementCollection, layer, canExpand, bottomMargin);
    }
    public TreeElement(string elementPath, string elementName, UIElementCollection elementCollection, Func<string[]> childrenPath, string definedIcon = "/Icons/folderIcon.png", int layer = 0, bool canExpand = true, int bottomMargin = 0)
    {
        InitializeComponent();
        this.DataContext = new VirtualTreeElementVM(elementPath, elementName, this, elementCollection, definedIcon, childrenPath, layer, canExpand, bottomMargin);
    }

    public enum TreeElementConditions { Default, Focused, PreFocused }
    public enum TreeElementType { Actual, Virtual }
}