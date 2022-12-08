
using Prism.Interactivity.InteractionRequest;

namespace Prism.WpfExt.Interactivity;

/// <summary>
/// This interface is crafted for dilvering the application message 
/// which will be likely displayed as a popup window.  
/// </summary>
public interface IMessage : IConfirmation
{
    VisualMessageTypeEnum MessageType { get; set; }
    bool IsOverlay { get; set; }
    string Line2Text { get; set; }
    bool RequireConfirmation { get; set; }

    string OkText { get; set; }
    string CancelText { get; set; }
    string CloseText { get; set; }
}
