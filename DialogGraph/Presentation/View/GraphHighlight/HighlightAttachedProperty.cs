using System.Diagnostics;
using System.Windows;
using GraphShape.Controls;

namespace DialogGraph;

public class HighlightAttachedProperty
{

    #region Active node event
    static readonly RoutedEvent ActiveNodeHighlightTriggeredEvent = EventManager.RegisterRoutedEvent(
        "ActiveNodeHighlightTriggeredEvent", RoutingStrategy.Bubble, typeof(HighlightTriggerEventHandler), typeof(CustomGraphLayout));

    public static void AddActiveNodeHighlightTriggeredHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement uiElement)
        {
            uiElement.AddHandler(ActiveNodeHighlightTriggeredEvent, handler);
        }
    }
    
    public static void RemoveActiveNodeHighlightTriggeredHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement uiElement)
        {
            uiElement.RemoveHandler(ActiveNodeHighlightTriggeredEvent, handler);
        }
    }
    #endregion
    
    // Basically the same approach as in GraphElementBehaviour.CoerceHighlightTrigger()
    private static void OnIsActiveNode(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var newValue = (bool)args.NewValue;
        var control = obj as UIElement;
        
        if (control is null) return;

        var eventArgs = new HighlightTriggeredEventArgs(ActiveNodeHighlightTriggeredEvent, obj, newValue);
        try
        {
            control.RaiseEvent(eventArgs);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception during CoerceHighlightTrigger - likely the graph is still animating: {ex}");
        }
    }
    
    public static readonly DependencyProperty IsActiveNodeProperty = DependencyProperty.RegisterAttached(
        "IsActiveNode", typeof(bool), typeof(HighlightAttachedProperty), new UIPropertyMetadata(false, OnIsActiveNode));

        
    public static bool GetIsActiveNode(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsActiveNodeProperty);
    }
        
    public static void SetIsActiveNode(DependencyObject obj, bool value)
    {
        obj.SetValue(IsActiveNodeProperty, value);
    }
}




