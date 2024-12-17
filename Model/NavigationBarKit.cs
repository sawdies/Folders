namespace Folders.Model;
[Flags]
public enum NavigationBarKit
{
    None = 0,
    QuickAccessPanel = 1,
    Libraries = 2,
    LocalDisks = 4,
    RemovableDisks = 8,
    Computer = 16,
}