using Folders.Data;
using Folders.Model;
using Folders.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace Folders.View;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContainer.Window = this;
        InitializeComponent();
        MainVM VM = new();
        DataContainer.MainDataContext = VM;
        this.DataContext = VM;

        NBcontainer.Content = new NavigationBar(
            NavigationBarKit.Libraries |
            NavigationBarKit.Computer |
            NavigationBarKit.Libraries |
            NavigationBarKit.RemovableDisks);

    }


    private static bool CtrlIsPressed => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
    private static bool ShiftsPressed => Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);


    public void DirectoryItems_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (CtrlIsPressed)
        {
            MainVM.MouseWheelEventArgs = e;
            (this.DataContext as MainVM).ImageSizeChanger.Execute(null!);
        }
    }

    private void MainViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (CtrlIsPressed)
        {
            e.Handled = true;
            MainVM.MouseWheelEventArgs = e;
            (this.DataContext as MainVM).ImageSizeChanger.Execute(null!);
        }
        else
        {
            MainViewer.ScrollToVerticalOffset(MainViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }

    private static Border? rect;
    static Point startPoint;
    static Point currentPoint;
    private static Point CurrentMousePosition(Point e)
    {
        Point elementPos = DataContainer.Window.RectContainer.TranslatePoint(new Point(0, 0), null);
        return new Point(e.X - elementPos.X, e.Y - elementPos.Y);
    }
    private static Thickness CurrentMargin(Point target, double smallMargin = -4)
    {
        double a = Math.Min(startPoint.X, target.X) + smallMargin;
        double b = Math.Min(startPoint.Y, target.Y) + smallMargin;
        double c = DataContainer.Window.RectContainer.ActualWidth - Math.Max(startPoint.X, target.X) + smallMargin;
        double d = DataContainer.Window.RectContainer.ActualHeight - Math.Max(startPoint.Y, target.Y) + smallMargin;

        return new Thickness(a, b, c, d);
    }
    public void MainWrapMouseDown(object sender, MouseButtonEventArgs e)
    {
        //DirectoryElementVM.MakeAllDisfocused();
        RemoveRect();
        RectContainer.CaptureMouse();
        startPoint = CurrentMousePosition(e.GetPosition(this));
        rect = new()
        {
            Background = Brushes.Black,
            BorderBrush = Brushes.Black,
            CornerRadius = new(10),
            Opacity = 0.2,
            BorderThickness = new(0),
            IsEnabled = false,
            Margin = CurrentMargin(startPoint, 0)
        };

        RectContainer.Children.Clear();
        RectContainer.Children.Add(rect);
        this.MouseMove += ResizeRectangle;
        BottomInfoPanel.MouseEnter += ExtendDown;
        BottomInfoPanel.MouseLeave += (o, e) => isMoving = false;
        TopInfoPanel.MouseEnter += ExtendUp;
        TopInfoPanel.MouseLeave += (o, e) => isMoving = false;
    }
    public void CleanFrontGrid()
    {
        RectContainer.Children.Clear();
        rect = null!;
    }

    private MouseEventHandler ResizeRectangle = (object sender, MouseEventArgs e) =>
    {
        if (rect is not null)
        {
            Point currentPoint = CurrentMousePosition(e.GetPosition(DataContainer.Window));
            rect.Margin = CurrentMargin(currentPoint);


        }
    };


    private bool IsOver()
    {
        return new();
    }
    private MouseEventHandler ExtendUp = (object sender, MouseEventArgs e) => MoveUpAsync();
    private MouseEventHandler ExtendDown = (object sender, MouseEventArgs e) => MoveDownAsync();
    private static void ExtendUpF() => DataContainer.Window.MainViewer.ScrollToVerticalOffset(DataContainer.Window.MainViewer.VerticalOffset - 10);
    private static void ExtendDownF() => DataContainer.Window.MainViewer.ScrollToVerticalOffset(DataContainer.Window.MainViewer.VerticalOffset + 10);

    private void DirectoryItems_MouseUp(object sender, MouseButtonEventArgs e)
    {
        RemoveRect();
    }

    private void RemoveRect()
    {
        if (rect is not null) CleanFrontGrid();
        this.MouseMove -= ResizeRectangle;
        RectContainer.ReleaseMouseCapture();
        BottomInfoPanel.MouseEnter -= ExtendDown;
        BottomInfoPanel.MouseLeave -= (o, e) => isMoving = false;
        TopInfoPanel.MouseEnter -= ExtendUp;
        TopInfoPanel.MouseLeave -= (o, e) => isMoving = false;
    }

    private static bool isMoving = false;
    private static async void MoveUpAsync()
    {
        isMoving = true;
        while (isMoving)
        {
            ExtendUpF();
            await Task.Delay(1);
        }
    }
    private static async void MoveDownAsync()
    {
        isMoving = true;
        while (isMoving)
        {
            ExtendDownF();
            await Task.Delay(1);
        }
    }

    private void BottomInfoPanel_MouseEnter(object sender, MouseEventArgs e) => MoveDownAsync();
    private void BottomInfoPanel_MouseLeave(object sender, MouseEventArgs e) => isMoving = false;



    private void LeftPanelIncreaseF() => DataContainer.Window.LeftPanelContainer.Width += 1;
    private void LeftPanelDecreaseF() => DataContainer.Window.LeftPanelContainer.Width -= 1;
    private MouseEventHandler LeftPanelSizeUpdater = (object sender, MouseEventArgs e) =>
    {
        //int newWidth = (int)e.GetPosition(DataContainer.Window).X + 1;
        //DataContainer.Window.TreeContainer.Width = newWidth;
        //DataContainer.Window.LeftPanelContainer.Width = newWidth;
        //
        //if (DataContainer.Window.LeftPanelContainer.Width < 7)
        //{
        //    DataContainer.Window.TargetLine.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffcbe0"));
        //    DataContainer.Window.TargetLine.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffcbe0"));
        //    DataContainer.Window.TargetLine.Opacity = 1;
        //}
        //else
        //{
        //    DataContainer.Window.TargetLine.BorderBrush = Brushes.LightGray;
        //    DataContainer.Window.TargetLine.Background = Brushes.Transparent;
        //}
    };

    static int lastMouseXPosition;
    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Cursor = Cursors.SizeWE;
        lastMouseXPosition = (int)e.GetPosition(this).X;
        this.MouseMove += LeftPanelSizeUpdater;
        this.MouseUp += (o, e) =>
        {
            if (DataContainer.Window.LeftPanelContainer.Width < 7)
            {
                DataContainer.Window.LeftPanelContainer.Visibility = Visibility.Collapsed;
            }
            this.MouseMove -= LeftPanelSizeUpdater;
            Cursor = Cursors.Arrow;
        };
        this.MouseLeave += (o, e) =>
        {
            if (DataContainer.Window.LeftPanelContainer.Width < 7)
            {
                DataContainer.Window.LeftPanelContainer.Visibility = Visibility.Collapsed;
            }
            this.MouseMove -= LeftPanelSizeUpdater;
            Cursor = Cursors.Arrow;
        };
    }

    static double lastMouseYPosition;
    private static void LeftPanelScrollerF()
    {
        //double visibleHeight = DataContainer.Window.LeftViewer.ActualHeight;
        //double factHeight = DataContainer.Window.TreeContainer.ActualHeight;
        //if (factHeight > visibleHeight)
        //{
        //    DataContainer.Window.Cursor = Cursors.SizeNS;
        //    double multiplier = lastMouseYPosition/ visibleHeight;
        //    DataContainer.Window.LeftViewer.ScrollToVerticalOffset(((factHeight- visibleHeight) * multiplier));
        //}
    }
    MouseEventHandler LeftPanelScroller = (o, e) =>
    {
        //lastMouseYPosition = e.GetPosition(DataContainer.Window.LeftViewer).Y;
        //LeftPanelScrollerF();
    };
    MouseButtonEventHandler LeftPanelScrollerRemoverB = (o, e) =>
    {
        //DataContainer.Window.LeftViewer.ReleaseMouseCapture();
        //DataContainer.Window.Cursor = Cursors.Arrow;
        //DataContainer.Window.MouseMove -= DataContainer.Window.LeftPanelScroller;
        //DataContainer.Window.MouseUp -= DataContainer.Window.LeftPanelScrollerRemoverB;
        ////DataContainer.Window.MouseLeave -= DataContainer.Window.LeftPanelScrollerRemover;

    };
    MouseEventHandler LeftPanelScrollerRemover = (o, e) =>
    {
        //DataContainer.Window.LeftViewer.ReleaseMouseCapture();
        //DataContainer.Window.Cursor = Cursors.Arrow;
        //DataContainer.Window.MouseMove -= DataContainer.Window.LeftPanelScroller;
        //DataContainer.Window.MouseUp -= DataContainer.Window.LeftPanelScrollerRemoverB;
        ////DataContainer.Window.MouseLeave -= DataContainer.Window.LeftPanelScrollerRemover;

    };
    private void LeftViewer_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //if (e.ChangedButton == MouseButton.Middle)
        //{
        //    LeftViewer.CaptureMouse();
        //    lastMouseYPosition = e.GetPosition(DataContainer.Window.LeftViewer).Y;
        //    LeftPanelScrollerF();
        //    this.MouseMove += LeftPanelScroller;
        //    this.MouseUp += LeftPanelScrollerRemoverB;
        //    //this.MouseLeave += LeftPanelScrollerRemover;
        //}
    }


    static bool isScrolling = false;
    private void LeftViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {

    }


    private void BorderResizer(object sender, MouseEventArgs e)
    {
        Border border = (sender as Border)!;
        double newWidth = e.GetPosition(DataContainer.Window).X;
        if (newWidth >= LeftPanel.MinWidth)
        {
            LeftPanel.Width = newWidth;
            LeftPanelContainer.Width = newWidth - 1;
            LeftPanel.Background = Brushes.Transparent;
        }
        else
        {
            LeftPanel.Width = LeftPanel.MinWidth;
            LeftPanel.Background = Brushes.Pink;
        }
    }
    private void Border_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            LeftPanel.CaptureMouse();
            LeftPanel.MouseMove += BorderResizer;
        }
    }
    private void Border_MouseUp(object sender, MouseButtonEventArgs e)
    {
        LeftPanel.ReleaseMouseCapture();
        LeftPanel.MouseMove -= BorderResizer;
    }
}
