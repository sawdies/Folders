using Folders.Data;
using Folders.View;
using System.IO;
using System.Windows.Controls;
namespace Folders.ViewModel;


public enum TreeElementConditions { Default, Focused, PreFocused }
public class ActualTreeElementVM(string elementPath,
                                 TreeElement container,
                                 UIElementCollection elementCollection,
                                 int layer,
                                 bool canExpand,
                                 int bottomMargin)
    : TreeElementVM(elementPath,
                    container,
                    elementCollection,
                    layer,
                    canExpand,
                    bottomMargin)
{

    protected override string[] GetChildren()
    {
        return (from item in Directory.GetDirectories(elementPath)
                select item).ToArray();
    }
    protected override TreeElement CreateChild(string childPath, int layer) => new(childPath, elementCollection, layer);
}
