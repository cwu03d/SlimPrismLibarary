
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
public class SwapContentAction : TriggerAction<FrameworkElement>
{

    /// <summary>
    /// Gets or sets the destination contaimer name
    /// </summary>
    public string TargetContainerName
    {
        get { return (string)GetValue(TargetContainerNameProperty); }
        set { SetValue(TargetContainerNameProperty, value); }
    }

    /// <summary>
    ///  Gets or sets the destination contaimer (normally a Grid control) name; defaulty is ClientRegion
    /// </summary>
    public static readonly DependencyProperty TargetContainerNameProperty =
        DependencyProperty.Register(
            "TargetContainerName",
            typeof(string),
            typeof(SwapContentAction),
            new PropertyMetadata("ClientRegion", null));

    /// <summary>
    /// Replace the client area with a content control represented by the XAML resource and collects results for <see cref="IInteractionRequest"/>.
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
        Console.WriteLine($"Replace the child of {TargetContainerName} with resource - {naviTarget}");
        if(string.IsNullOrWhiteSpace(TargetContainerName) || string.IsNullOrWhiteSpace(naviTarget))
        {
            return;
        }

        Grid targetGrid = WpfHelper.GetChild<Grid>(AssociatedObject, TargetContainerName);
        if (targetGrid != null) 
        {
            targetGrid.Children.Clear();

            UIElement view = WpfHelper.GetResourceObject<UserControl>(naviTarget);
            //Wireup the IInteractionRequestAware
            Action<IInteractionRequestAware> setNotificationAndClose = (iira) =>
                {
                    iira.Notification      = args.Context;
                    iira.FinishInteraction = delegate { };
                };
            MvvmHelpers.ViewAndViewModelAction(view, setNotificationAndClose);

            targetGrid.Children.Add(view);
        }
       

    }

}
