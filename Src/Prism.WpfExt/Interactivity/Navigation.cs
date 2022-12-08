using Prism.Interactivity.InteractionRequest;
using System.Windows;

namespace Prism.WpfExt.Interactivity;

public class Navigation : Confirmation, INavigation
{
    public string Target { get; set; }

    public object Data { get; set; }

    public Point? TopLeftPosition { get; set; }
}
