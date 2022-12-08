using System;
using System.Collections;
using System.Windows;

namespace Prism.WpfExt;

public static class WpfHelper
{
    /// <summary>
    /// Load the component of type T from a XAML source file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="viewPath">The path of a XAML file </param>
    /// <returns>Loaded comnponent if the component can be created from the source XAML file; default value of T (usually null) otheriwise.</returns>
    public static T GetResourceObject<T>(string viewPath)
    {
        T element = default(T);

        if(!string.IsNullOrEmpty(viewPath))
        {
            try
            {
                Uri packUri = new Uri(viewPath, UriKind.Relative);
                var comp = Application.LoadComponent(packUri);
                element = (T)comp;
            }
            catch (Exception)
            {
                Console.WriteLine($"Cannot load component from {viewPath}");
            }
        }

        return element;
    }

    /// <summary>
    /// Search the visual tree for the parent of type T
    /// </summary>
    /// <typeparam name="T">The target type</typeparam>
    /// <param name="uiElement">The child FrameworkElement </param>
    /// <returns>The parent of T if found; null otherwise.</returns>
    public static T GetParent<T>(FrameworkElement uiElement) where T : FrameworkElement
    {
        DependencyObject parent = LogicalTreeHelper.GetParent(uiElement);
        while (parent != null)
        {
            if (parent is T)
            {
                break;
            }
            else
            {
                parent = LogicalTreeHelper.GetParent(parent);
            }
        }

        return (T)parent;
    }

    /// <summary>
    /// Find the child element of type T of a FrameworkElement
    /// </summary>
    /// <typeparam name="T">The intended type</typeparam>
    /// <param name="uiElement">The target type</param>
    /// <param name="name">The optional name to restrict the search</param>
    /// <returns>The child of T if found; null otherwise.</returns>
    public static T GetChild<T>(FrameworkElement uiElement, string name = null) where T : FrameworkElement
    {
        object founded = null;
        var childs = LogicalTreeHelper.GetChildren(uiElement);
        IEnumerator enumratator = childs.GetEnumerator();
        
        while(true)
        {
            if (!enumratator.MoveNext()) //Not every component has a child
            {
                return null;
            }

            FrameworkElement child = enumratator.Current as FrameworkElement;
            if(child == null)  // Skip since not a FrameworkElement
            {
                continue;        
            }

            if (child is T)
            {
                if(name == null || (child.Name == name)) 
                {
                    founded = child;
                    break;
                }
            }

            founded = GetChild<T>(child, name);  // search the children elements
            if(founded != null)
            { 
                break; 
            }
        }

        return (T)founded;
    }

}
