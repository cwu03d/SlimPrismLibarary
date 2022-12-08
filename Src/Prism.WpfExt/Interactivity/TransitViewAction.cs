
using Prism.Common;
using Prism.Interactivity.InteractionRequest;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Prism.WpfExt.Interactivity;

/// <summary>
/// Replace window's content in response to an <see cref="InteractionRequest"/> being raised.
/// In system, we will keep the main Window, replace its content screen based on the user action.
/// This class is inspired by Prism PopupWindowAction
/// </summary>
public class TransitViewAction : TriggerAction<FrameworkElement>
{
    private Window _hostWindow;

    /// <summary>
    /// Displays the child window and collects results for <see cref="IInteractionRequest"/>.
    /// </summary>
    /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
    protected override void Invoke(object parameter)
    {
        InteractionRequestedEventArgs args = parameter as InteractionRequestedEventArgs;
        if (AssociatedObject == null || args == null || !(args.Context is INavigation))
        {
            return;
        }

        string naviTarget =  ((INavigation)args.Context).Target;
        Console.WriteLine($"Navigation target: {naviTarget}");
        if(string.IsNullOrWhiteSpace(naviTarget))
        {
            return;
        }

        _hostWindow = (AssociatedObject == Application.Current.MainWindow)? Application.Current.MainWindow :
                                                                            WpfHelper.GetParent<Window>(AssociatedObject) as Window;


        // This will signal the client application logic error in the case of detached view with window(memory leak)
        // For example, when transit to another view, the event hanlder is not unregistered, and one old view tries the transition
        if (_hostWindow == null)
        {
            Console.WriteLine($"Orphaned view tried to make transition. No Parent Windiow found for {AssociatedObject.GetType().ToString()}.");
            return;
        }

        //Close all windows except the Maindow
        foreach (Window win in Application.Current.Windows)
        {
            // This window is owned by WPF and should never be closed by user action
            // if(win is Microsoft.XamlDiagnostics.WpfTap.WpfVisualTreeService.Adorners.AdornerLayerWindow) not compilable because of internal type
            if (win.GetType().Name == "AdornerLayerWindow")
            {
                continue;
            }

            if (win != _hostWindow && win != Application.Current.MainWindow)
            {
                try
                {
                    if (win.Tag?.ToString() != WpfExtConstants.CLOSED)
                    {
                        win.Tag = WpfExtConstants.CLOSED;
                        win.Close();
                    }
                }
                catch (Exception) // avoid bring down the system if the window cannot be close at the moment
                {
                }
            }
        }

        if (_hostWindow != null)
        {
            //Do nothing is the target is the same as originator
            string curType = _hostWindow.Content.GetType().Name;
            Console.WriteLine($"Navigation action, current view to be swapped out {curType}");
            if (naviTarget.Contains(curType))
            {
                return;
            }

            UIElement view = WpfHelper.GetResourceObject<UserControl>(naviTarget);
            //Wireup the IInteractionRequestAware
            Action<IInteractionRequestAware> setNotificationAndClose = (iira) =>
                {
                    iira.Notification      = args.Context;
                    iira.FinishInteraction = delegate { };
                };
            MvvmHelpers.ViewAndViewModelAction(view, setNotificationAndClose);

            //Revert the dim due to popup
            _hostWindow.Background = Brushes.Transparent;
            _hostWindow.Opacity = 1.0;
            _hostWindow.Content = view;

            _hostWindow.Activate();
        }

    }

}
