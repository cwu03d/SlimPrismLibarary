using System;
using System.Collections.Generic;
using System.Windows;

using Prism.Interactivity.InteractionRequest;
using Prism.WpfExt.Interactivity;

namespace Prism.WpfExt.Controls;

/// <summary>
/// General purpose window for display info, warnig, and errors
/// </summary>
public partial class MessageWindowView : Window, IInteractionRequestAware
{
    private Queue<IMessage> _messages;

    public MessageWindowView()
    {
        InitializeComponent();

        _messages = new Queue<IMessage>();
        DataContext = this;
    }

    public void AddMessage(IMessage message)
    {
        if (message != null)
        {
            if (CurrentMassge == null)
            {
                CurrentMassge = message;
            }
            else
            {
                if (message.MessageType != VisualMessageTypeEnum.FatalError)
                {
                    _messages.Enqueue(message);
                }
                else
                {
                    CurrentMassge = message;  // override the current none fatal error with fatal error
                }
            }
        }
    }

    private IMessage _currentMassge;
    private IMessage CurrentMassge
    {
        get { return _currentMassge; }
        set
        {
            _currentMassge = value;
            if (_currentMassge != null)
            {
                Category = _currentMassge.MessageType;
                Title = _currentMassge.Title;
                ConfirmationRequired = _currentMassge.RequireConfirmation;
                if (_currentMassge.Content is FrameworkElement)
                {
                    VisualContent = _currentMassge.Content as FrameworkElement;
                }
                else
                {
                    Line1Text = (string)_currentMassge.Content;
                    Line2Text = _currentMassge.Line2Text;
                }

                if (_currentMassge.RequireConfirmation)
                {
                    OKText = _currentMassge.OkText;
                    CancelText = _currentMassge.CancelText;
                }
                else
                {
                    CloseText = _currentMassge.CloseText;
                }
            }
        }
    }

    public bool ConfirmationRequired
    {
        get { return (bool)GetValue(ConfirmationRequiredProperty); }
        set { SetValue(ConfirmationRequiredProperty, value); }
    }

    public bool IsModal
    {
        get { return (bool)GetValue(IsModalProperty); }
        set { SetValue(IsModalProperty, value); }
    }

    public VisualMessageTypeEnum Category
    {
        get { return (VisualMessageTypeEnum)GetValue(CategoryProperty); }
        set { SetValue(CategoryProperty, value); }
    }

    public string Line1Text
    {
        get { return (string)GetValue(Line1TextProperty); }
        set { SetValue(Line1TextProperty, value); }
    }

    public string Line2Text
    {
        get { return (string)GetValue(Line2TextProperty); }
        set { SetValue(Line2TextProperty, value); }
    }

    public string OKText
    {
        get { return (string)GetValue(OkTextProperty); }
        set { SetValue(OkTextProperty, value); }
    }

    public string CancelText
    {
        get { return (string)GetValue(CancelTextProperty); }
        set { SetValue(CancelTextProperty, value); }
    }

    public string CloseText
    {
        get { return (string)GetValue(CloseTextProperty); }
        set { SetValue(CloseTextProperty, value); }
    }

    public FrameworkElement VisualContent
    {
        get { return (FrameworkElement)GetValue(VisualContentProperty); }
        set { SetValue(VisualContentProperty, value); }
    }

    public static readonly DependencyProperty ConfirmationRequiredProperty =
        DependencyProperty.Register("ConfirmationRequired", typeof(bool), typeof(MessageWindowView), new PropertyMetadata(false));

    public static readonly DependencyProperty IsModalProperty =
        DependencyProperty.Register("IsModal", typeof(bool), typeof(MessageWindowView), new PropertyMetadata(false));

    public static readonly DependencyProperty CategoryProperty =
        DependencyProperty.Register("Category", typeof(VisualMessageTypeEnum), typeof(MessageWindowView), new PropertyMetadata(VisualMessageTypeEnum.Info));

    public static readonly DependencyProperty Line1TextProperty =
        DependencyProperty.Register("Line1Text", typeof(string), typeof(MessageWindowView), new PropertyMetadata(null));

    public static readonly DependencyProperty Line2TextProperty =
        DependencyProperty.Register("Line2Text", typeof(string), typeof(MessageWindowView), new PropertyMetadata(null));

    public static readonly DependencyProperty OkTextProperty =
        DependencyProperty.Register("OkText", typeof(string), typeof(MessageWindowView), new PropertyMetadata(null));

    public static readonly DependencyProperty CancelTextProperty =
        DependencyProperty.Register("CancelText", typeof(string), typeof(MessageWindowView), new PropertyMetadata(null));

    public static readonly DependencyProperty CloseTextProperty =
        DependencyProperty.Register("CloseText", typeof(string), typeof(MessageWindowView), new PropertyMetadata(null));

    public static readonly DependencyProperty VisualContentProperty =
        DependencyProperty.Register("VisualContent", typeof(FrameworkElement), typeof(MessageWindowView), new PropertyMetadata(null));

    public Action FinishInteraction { get; set; }
    public INotification Notification { get; set; }

    private void Ok_Clicked(object sender, RoutedEventArgs e)
    {
        HandleCloseAction(true);
    }

    private void Cancel_Clicked(object sender, RoutedEventArgs e)
    {
        HandleCloseAction(false);
    }

    private void Close_Clicked(object sender, RoutedEventArgs e)
    {
        HandleCloseAction(false);
    }

    private void HandleCloseAction(bool confirmed)
    {
        if (CurrentMassge.MessageType != VisualMessageTypeEnum.FatalError && _messages.Count > 0)
        {
            CurrentMassge = _messages.Dequeue();
        }
        else
        {

            if (Notification != null && Notification is IConfirmation)
            {
                ((IConfirmation)Notification).Confirmed = confirmed;
            }

            if (this.FinishInteraction != null)
            {
                this.FinishInteraction();
            }
        }
    }


}
