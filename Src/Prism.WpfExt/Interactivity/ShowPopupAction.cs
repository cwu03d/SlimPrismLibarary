

using Prism.Common;
using Prism.Interactivity.InteractionRequest;
using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Prism.WpfExt.Interactivity;

/// <summary>
/// Shows a popup window in response to an <see cref="InteractionRequest"/> being raised.
/// This is a general purpose action for display full-fledged dialog window with its own ViewModel, which implement IInteractionRequestAware.
/// For popups trageted for error messages, please use ShowMessageAction instead.
/// This class is inspired by Prism PopupWindowAction, amd intended to replace PopupWindowAction with more functionality.
/// </summary>
public class ShowPopupAction : TriggerAction<FrameworkElement>
{  
    public static readonly Brush BACKGROUND_BRUSH  = Brushes.Gray;
    public static double BACKGROUND_OPACITY        = 0.30;

    private Window _parentWindow;

    /// <summary>
    /// Determines if the content should be shown in a modal window or not.
    /// </summary>
    public static readonly DependencyProperty IsModalProperty =
        DependencyProperty.Register(
            "IsModal",
            typeof(bool),
            typeof(ShowPopupAction),
            new PropertyMetadata(true, null));

    /// <summary>
    /// Determines if the content should be shown in a modal window or not.
    /// </summary>
    public static readonly DependencyProperty IsDarkenParentProperty =
        DependencyProperty.Register(
            "IsDarkenParent",
            typeof(bool),
            typeof(ShowPopupAction),
            new PropertyMetadata(true, null));

    /// <summary>
    /// Determines if the content should be initially shown centered over the view that raised the interaction request or not.
    /// </summary>
    public static readonly DependencyProperty CenterOverAssociatedObjectProperty =
        DependencyProperty.Register(
            "CenterOverAssociatedObject",
            typeof(bool),
            typeof(ShowPopupAction),
            new PropertyMetadata(true, null));

    /// <summary>
    /// Determines if the content should be initially shown centered over the main application window or not.
    /// Note that this is contrast with CenterOverAssociatedObjectProperty. If both are true, this property will win.
    /// </summary>
    public static readonly DependencyProperty CenterOverMainWindowProperty =
        DependencyProperty.Register(
            "CenterOverMainWindow",
            typeof(bool),
            typeof(ShowPopupAction),
            new PropertyMetadata(false, null));

    /// <summary>
    /// If set, applies this WindowStartupLocation to the child window.
    /// </summary>
    public static readonly DependencyProperty WindowStartupLocationProperty =
        DependencyProperty.Register(
            "WindowStartupLocation",
            typeof(WindowStartupLocation?),
            typeof(ShowPopupAction),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets if the window will be modal or not.
    /// </summary>
    public bool IsModal
    {
        get { return (bool)GetValue(IsModalProperty); }
        set { SetValue(IsModalProperty, value); }
    }

    /// <summary>
    /// Gets or sets if the parenr wwindow will be darken or not.
    /// </summary>
    public bool IsDarkenParent
    {
        get { return (bool)GetValue(IsDarkenParentProperty); }
        set { SetValue(IsDarkenParentProperty, value); }
    }

    /// <summary>
    /// Gets or sets if the window will be initially shown centered over the view that raised the interaction request or not.
    /// </summary>
    public bool CenterOverAssociatedObject
    {
        get { return (bool)GetValue(CenterOverAssociatedObjectProperty); }
        set { SetValue(CenterOverAssociatedObjectProperty, value); }
    }

    // If this is true, CenterOverAssociatedObject will be ignored.
    public bool CenterOverMainWindow
    {
        get { return (bool)GetValue(CenterOverMainWindowProperty); }
        set { SetValue(CenterOverMainWindowProperty, value); }
    }

    /// <summary>
    /// Gets or sets the startup location of the Window.
    /// </summary>
    public WindowStartupLocation? WindowStartupLocation
    {
        get { return (WindowStartupLocation?)GetValue(WindowStartupLocationProperty); }
        set { SetValue(WindowStartupLocationProperty, value); }
    }


    /// <summary>
    /// Displays the child window and collects results for <see cref="IInteractionRequest"/>.
    /// The popup position is determined with the following priority
    /// 1.  WindowStartupLocation Dependency property;
    /// 2.  CenterOverMainWindow true
    /// 3.  CenterOverAssociatedObject  true by default
    /// </summary>
    /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
    protected override void Invoke(object parameter)
    {
        var args = parameter as InteractionRequestedEventArgs;
        if (args == null || !(args.Context is INotification))
        {
            return;
        }

        string naviTarget = (args.Context is INavigation)? ((INavigation)args.Context).Target : null;
        Window popup = GetWindow(args.Context, naviTarget);
        if (popup == null)
        {
            return;
        }

        // We invoke the callback when the interaction's window is closed.
        EventHandler handler = null;
        handler =
            (o, e) =>
            {
                popup.Closed -= handler;
                if (popup.Tag?.ToString() != WpfExtConstants.CLOSED)   // we will set a mark for window aleardy closed to avoid being closed more than once
                {
                    popup.Tag = WpfExtConstants.CLOSED;
                }
                if (args.Callback != null) args.Callback();
            };
        popup.Closed += handler;

        //Determine popup's startup position
        if (!WindowStartupLocation.HasValue && (this.CenterOverAssociatedObject || this.CenterOverMainWindow) && this.AssociatedObject != null)
        {
            // If we should position the popup over relative to a window we subscribe to the SizeChanged event
            // so we can change its position after the dimensions are set.
            SizeChangedEventHandler sizeHandler = null;
            sizeHandler =
                (o, e) =>
                {
                    popup.SizeChanged -= sizeHandler;

                    // If the parent window has been minimized, then the poition of the wrapperWindow is calculated to be off screen
                    // which makes it impossible to activate and bring into view.  So, we want to check to see if the parent window
                    // is minimized and automatically set the position of the wrapperWindow to be center screen.
                    if (_parentWindow != null && _parentWindow.WindowState == WindowState.Minimized)
                    {
                        popup.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                        return;
                    }

                    FrameworkElement view = CenterOverMainWindow ? Application.Current.MainWindow : this.AssociatedObject;

                    // Position is the top left position of the view from which the request was initiated.
                    // On multiple monitors, if the X or Y coordinate is negative it represent that the monitor from which
                    // the request was initiated is either on the left or above the PrimaryScreen
                    Point position = view.PointToScreen(new Point(0, 0));
                    PresentationSource source = PresentationSource.FromVisual(view);
                    position = source.CompositionTarget.TransformFromDevice.Transform(position);

                    // Find the middle of the calling view.
                    // Take the width and height of the view divided by 2 and add to the X and Y coordinates.
                    var middleOfView = new Point(position.X + (view.ActualWidth  / 2),
                                                 position.Y + (view.ActualHeight / 2));

                    // Set the coordinates for the top left part of the wrapperWindow.
                    // Take the width of the wrapperWindow, divide it by 2 and substract it from 
                    // the X coordinate of middleOfView. Do the same thing for the Y coordinate.
                    // If the wrapper window is wider or taller than the view, it will be behind the view.
                    popup.Left = middleOfView.X - ( popup.ActualWidth  / 2 );
                    popup.Top  = middleOfView.Y - ( popup.ActualHeight / 2 );

                };
            popup.SizeChanged += sizeHandler;
        }

        if (_parentWindow != null)  // Darken the parent window
        {
            if (IsDarkenParent)
            {
                _parentWindow.Background = BACKGROUND_BRUSH;
            }

            _parentWindow.Opacity = BACKGROUND_OPACITY;
        }

        if (this.IsModal)
        {
            popup.ShowDialog();
        }
        else
        {
            popup.Show();
        }
    }


    /// <summary>
    /// Create a popup window based on its name, wire-up the interaction logic
    /// </summary>
    /// <param name="notification">The notification passed through from interaction request</param>
    /// <param name="target">the xaml file for popup window, this window should implement IInteractionRequestAware,
    ///                      so that the user interaction result can be properly passed back.
    /// </param>
    /// <returns></returns>
    protected virtual Window GetWindow(INotification notification, string target)
    {
        Window popUp = null;
        if (notification.Content is Window && string.IsNullOrWhiteSpace(target))
        {
            popUp = notification.Content as Window;
        }
        else
        {
            popUp = WpfHelper.GetResourceObject<Window>(target);
        }

        if (popUp == null)
        {
            Console.WriteLine($"Cannot create a Window from {target}.");
            return popUp;
        }

        if (AssociatedObject != null)
        {
            if (AssociatedObject is Window)
            {
                popUp.Owner = AssociatedObject as Window;
            }
            else
            {
                popUp.Owner = Window.GetWindow(AssociatedObject);
            }

            _parentWindow = popUp.Owner;
        }

        // If the user has provided a startup location for a Window we set it as the window's startup location.
        if (WindowStartupLocation.HasValue)
            popUp.WindowStartupLocation = WindowStartupLocation.Value;

        Action<IInteractionRequestAware> setNotificationAndClose = (iira) =>
        {
            iira.Notification = notification;
            iira.FinishInteraction = () =>
            {
                //avoid close a window more than one time by setting and check window's property Tag value
                if (popUp.Tag?.ToString() == WpfExtConstants.CLOSED)
                {
                    return;
                }
                popUp.Tag = WpfExtConstants.CLOSED;

                popUp.Close();
                if (_parentWindow != null)  // Restore the parent window to normal opacity
                {
                    if (IsDarkenParent)
                    {
                        _parentWindow.Background = Brushes.Transparent;
                    }
                    _parentWindow.Opacity = 1.0;
                }
            };
        };

        //Trick is played here to wire up the logic to pass back interaction result. 
        MvvmHelpers.ViewAndViewModelAction(popUp, setNotificationAndClose);

        return popUp;
    }

}
