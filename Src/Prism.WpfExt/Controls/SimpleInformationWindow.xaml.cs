using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Prism.WpfExt.Controls;

/// <summary>
/// General purpose window for display a single line message, dismissed when clicked
/// </summary>
public partial class SimpleInformationWindow : Window, IInteractionRequestAware
{

    private DispatcherTimer _timer;

    public DelegateCommand CloseCommand { get; }

    public SimpleInformationWindow()
    {
        InitializeComponent();
        DataContext = this;

        CloseCommand = new DelegateCommand(ExecuteClose);
    }

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public bool IsClickToDismiss
    {
        get { return (bool)GetValue(IsClickToDismissProperty); }
        set { SetValue(IsClickToDismissProperty, value); }
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(SimpleInformationWindow), new PropertyMetadata(null));

    public static readonly DependencyProperty IsClickToDismissProperty =
        DependencyProperty.Register("IsClickToDismiss", typeof(bool), typeof(SimpleInformationWindow), new PropertyMetadata(false));

    public Action FinishInteraction { get; set; }
    public INotification Notification { get; set; }


    private void ExecuteClose()
    {
        if (_timer != null)
        {
            _timer.Tick -= Timer_Tick;
            _timer = null;
        }

        if (this.FinishInteraction != null)
        {
            this.FinishInteraction();
        }
        else
        {
            Close();
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (!IsClickToDismiss)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(2000);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() => { ExecuteClose(); });
    }
}
