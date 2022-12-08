using System;
using System.Windows;

using System.Windows.Threading;

namespace Prism.WpfExt.Controls;

/// <summary>
/// Lightweight transit window for display info, warnig, and errors on the topright of the main window
/// It will dismiss itsself in 5 seconds.
/// </summary>
public partial class MessageOverlayWindowView : Window
{
    private DispatcherTimer _timer;

    public MessageOverlayWindowView()
    {
        InitializeComponent();
    }

    public VisualMessageTypeEnum Category
    {
        get { return (VisualMessageTypeEnum)GetValue(CategoryProperty); }
        set { SetValue(CategoryProperty, value); }
    }


    public static readonly DependencyProperty CategoryProperty =
        DependencyProperty.Register("Category", typeof(VisualMessageTypeEnum), typeof(MessageOverlayWindowView), new PropertyMetadata(VisualMessageTypeEnum.Info));



    public string Message
    {
        get { return (string)GetValue(MessageProperty); }
        set { SetValue(MessageProperty, value); }
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register("Message", typeof(string), typeof(MessageOverlayWindowView), null);



    private void OnOverlayWindowView_Loaded(object sender, RoutedEventArgs e)
    {
        _timer = new DispatcherTimer();
        _timer.Tick += _timer_Tick;
        _timer.Interval = new TimeSpan(0, 0, 5);
        _timer.Start();
    }

    private void _timer_Tick(object sender, EventArgs e)
    {
        _timer.Tick -= _timer_Tick;
        _timer = null;
        this.Close();
    }

}
