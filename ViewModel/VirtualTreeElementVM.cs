using Folders.View;
using System.Windows.Controls;

namespace Folders.ViewModel
{
    public class VirtualTreeElementVM : TreeElementVM
    {
        private readonly Func<string[]> getChildren;
        public VirtualTreeElementVM(string elementPath, string elementName, TreeElement container, UIElementCollection elementCollection, string definedIconPath, Func<string[]> childrenPaths, int layer, bool canExpand, int bottomMargin)
        : base(elementPath, container, elementCollection, layer, true, bottomMargin)
        {
            this.elementName = elementName;
            this.getChildren = childrenPaths;
            if (definedIconPath != "") this.iconImagesourse = definedIconPath;
        }
        
        protected override string[] GetChildren() => getChildren();
        protected override TreeElement CreateChild(string childPath, int layer) => new(childPath, elementCollection, layer, canExpand);
    }
}
