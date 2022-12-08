using Prism.Interactivity.InteractionRequest;
using System.Windows;

namespace Prism.WpfExt.Interactivity;


/// <summary>
/// This interface is designed to pass information to the system to navigate UI from one screen to another by swaping the Windows content.
/// Please notice that the Window is destroyed and recreated each time, only its content is swapped with target resource.
/// </summary>
public interface INavigation : IConfirmation
{
    /// <summary>
    /// Get or Set the navigation target. This can be the XAML page that transition to or to be displayed as a popup.
    /// It should be the relative path of xaml view under the assembly, for example "/Views/LoginView.xaml"
    /// </summary>
    string Target { get; set; }

    /// <summary>
    /// Pass the Data forward to the target, and back from the hanlder.
    /// Since ViewModel is created with default parameterless constructor, this is a good way to pass data from source to target
    /// without an intermiate space.
    /// </summary>
    object Data { get; set; }

}
