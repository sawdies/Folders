using Folders.Data;
using Folders.Model;
using Folders.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Folders.View;
public partial class NavigationBar : UserControl
{
    public NavigationBar(NavigationBarKit kit)
    {
        InitializeComponent();

        VirtualizingStackPanel newStack = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
        StackContainer.Content = newStack;
        this.DataContext = new NavigationBarVM(kit, newStack.Children);

        if (!Scroller.IsMouseOver)
        {
            (this.DataContext as NavigationBarVM)!.SmoothFocusing();
        }
    }

    private static bool CtrlIsPressed => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
    private static bool ShiftsPressed => Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
    private static bool isScrollingMove = false;
    private void ScrollingMove(object sender, MouseEventArgs e)
    {
        double visibleHeight = Scroller.ActualHeight;
        double factHeight = (StackContainer.Content as VirtualizingStackPanel)!.ActualHeight;

        if (factHeight > visibleHeight)
        {
            Scroller.Cursor = Cursors.SizeNS;
            Scroller.ScrollToVerticalOffset((factHeight - visibleHeight) * (e.GetPosition(Scroller).Y / visibleHeight));
        }
    }
    private void ScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle)
        {
            isScrollingMove = true;
            ScrollingMove(sender, e);
            ScrollViewer target = (sender as ScrollViewer)!;
            target.CaptureMouse();
            target.MouseMove += ScrollingMove;
        }
    }
    private void ScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
    {
        isScrollingMove = false;
        ScrollViewer target = (sender as ScrollViewer)!;
        target.Cursor = Cursors.Arrow;
        target.MouseMove -= ScrollingMove;
        target.ReleaseMouseCapture();
        if (!target.IsMouseOver)
        {
            (this.DataContext as NavigationBarVM)!.SmoothFocusing();
        }
    }
    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ScrollViewer target = (sender as ScrollViewer)!;
        if (!target.IsMouseCaptured)
        {
            double newOffset;
            if (ShiftsPressed) newOffset = target.VerticalOffset - target.ActualHeight * (e.Delta > 0 ? 1 : -1);
            else if (CtrlIsPressed) newOffset = target.VerticalOffset - e.Delta / 10;
            else newOffset = target.VerticalOffset - e.Delta;

            target.ScrollToVerticalOffset(newOffset);
        }
        e.Handled = true;
    }
}
