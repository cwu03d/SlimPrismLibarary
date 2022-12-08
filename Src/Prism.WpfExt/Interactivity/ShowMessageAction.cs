
using Prism.Common;
using Prism.Interactivity.InteractionRequest;
using Prism.WpfExt.Controls;
using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Prism.WpfExt.Interactivity;

/// <summary>
/// Shows a popup window in response to an <see cref="InteractionRequest"/> being raised.
/// This is a sepcialized version targeted for popups showing error messages, whose UI is simple and does't need a dedicated viewmodel.
/// This class is inspired by Prism PopupWindowAction
/// </summary>
public class ShowMessageAction : TriggerAction<FrameworkElement>
{
    /// <summary>
    /// Determines if the content should be shown in a modal window or not.
    /// </summary>
    public static readonly DependencyProperty IsModalProperty =
        DependencyProperty.Register(
            "IsModal",
            typeof(bool),
            typeof(ShowMessageAction),
            new PropertyMetadata(true, null));

    /// <summary>
    /// Determines if the content should be shown in a modal window or not.
    /// </summary>
    public static readonly DependencyProperty IsDarkenParentProperty =
        DependencyProperty.Register(
            "IsDarkenParent",
            typeof(bool),
            typeof(ShowMessageAction),
            new PropertyMetadata(true, null));

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
    /// Displays the child window and collects results for <see cref="IInteractionRequest"/>.
    /// </summary>
    /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
    protected override void Invoke(object parameter)
    {
        var args = parameter as InteractionRequestedEventArgs;
        if (args == null || !(args.Context is IMessage))
        {
            return;
        }

        IMessage message = (IMessage)args.Context;
        Window popup = GetWindow(args.Context, message);

        // We invoke the callback when the interaction's window is closed.
        var callback = args.Callback;
        EventHandler handler = null;
        handler =
            (o, e) =>
            {
                popup.Closed -= handler;
                if (callback != null) callback();
            };
        popup.Closed += handler;

        if (this.AssociatedObject != null)
        {
            // If we should center the popup over the parent window we subscribe to the SizeChanged event
            // so we can change its position after the dimensions are set.
            SizeChangedEventHandler sizeHandler = null;
            Window mnWin = Application.Current.MainWindow;
            sizeHandler =
                    (o, e) =>
                    {
                        popup.SizeChanged -= sizeHandler;
                        popup.Left = mnWin.Left + (mnWin.ActualWidth - popup.ActualWidth) / 2;
                        popup.Top = mnWin.Top + (mnWin.ActualHeight - popup.ActualHeight) / 2;
                    };
            popup.SizeChanged += sizeHandler;
        }

        if (IsModal)
        {
            if (popup.Owner != null)
            {
                if (IsDarkenParent)
                {
                    popup.Owner.Background = ShowPopupAction.BACKGROUND_BRUSH;
                }
                popup.Owner.Opacity = ShowPopupAction.BACKGROUND_OPACITY;
            }

            popup.ShowDialog();
        }
        else
        {
            popup.Show();
        }
    }


    protected virtual Window GetWindow(INotification notification, IMessage message)
    {
        Window win = null;

        if (message.MessageType == VisualMessageTypeEnum.SimpleInfo || message.MessageType == VisualMessageTypeEnum.SimpleInfoClickToDismiss)
        {
            SimpleInformationWindow infoWin = new SimpleInformationWindow();
            if (message.MessageType == VisualMessageTypeEnum.SimpleInfoClickToDismiss)
            {
                infoWin.IsClickToDismiss = true;
            }
            infoWin.Title = message.Title;
            infoWin.Text  = message.Content.ToString();
            win           = infoWin;
        }
        else if (message.IsOverlay)
        {
            MessageOverlayWindowView overlay = new MessageOverlayWindowView();
            overlay.Title    = message.Title;
            overlay.Category = message.MessageType;
            overlay.Message  = message.Content.ToString();
            win              = overlay;
        }
        else
        {
            //We will created only one instance of MessageWindowView, queue the content to it.
            MessageWindowView msgPopup =  new MessageWindowView();
            msgPopup.AddMessage(message);
            win                        = msgPopup;
        }

        if (AssociatedObject != null)
        {
            win.Owner = (AssociatedObject is Window) ? AssociatedObject as Window : Window.GetWindow(AssociatedObject);
        }

        Action<IInteractionRequestAware> setNotificationAndClose = (iira) =>
        {
            iira.Notification = notification;
            iira.FinishInteraction = () =>
            {
                win.Close();
                if (IsModal && win.Owner  != null)
                {
                    if (IsDarkenParent)
                    {
                        win.Owner.Background = Brushes.Transparent;
                    }
                    win.Owner.Opacity = 1.0;
                }
            };
        };

        MvvmHelpers.ViewAndViewModelAction(win, setNotificationAndClose);

        return win;
    }

}
