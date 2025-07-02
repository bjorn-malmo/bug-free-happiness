using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.UI.Xaml;
public static class FrameworkElementExtension
{
    private static XamlRoot? _oldXamlRoot;

    /// <summary>
    /// When tearing tabs into a new window, the context menu(s) 
    /// stops working on child objects. This workaround method will
    /// enumerate all child elements and reassign any context menu
    /// </summary>
    /// <param name="element"></param>
    public static void ReassignContextMenus(this FrameworkElement element)
    {
        if (element?.XamlRoot == null)
        {
            Debug.Assert(false, "Expected Xaml root to be set on the element");
            return;
        }

        _oldXamlRoot = element.XamlRoot;
        element.Loaded += Fe_Loaded;
    }

    private static void Fe_Loaded(object sender, RoutedEventArgs e)
    {
        var fe = (FrameworkElement)sender;
        if (fe.XamlRoot == _oldXamlRoot)
        {
            // Not changed yet, keep waiting
        }
        else
        {
            // Now we are loaded in a new window with a new xaml root
            fe.Loaded -= Fe_Loaded;
//            _oldXamlRoot = null;
            ReassignRecursive(fe, fe.XamlRoot);
        }
    }

    private static void ReassignRecursive(DependencyObject d, XamlRoot newXamlRoot)
    {
        if (d is UIElement ui)
        {
            // Verify any context menu xaml root binding
            Debug.Assert(ui.XamlRoot == newXamlRoot, "This is not expected");

            ReassignContextFlyout(ui, newXamlRoot);
        }

        // Call recursively
        var childCount = VisualTreeHelper.GetChildrenCount(d);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(d, i);
            ReassignRecursive(child, newXamlRoot);
        }
    }
    private static void ReassignContextFlyout(UIElement ui, XamlRoot newXamlRoot)
    {
        if (ui.ContextFlyout != null
            && ui.ContextFlyout.XamlRoot != newXamlRoot)
        {
            // Reassign context menu to reset XamlRoot
            var cf = ui.ContextFlyout;
            ui.ContextFlyout = null;
            ui.ContextFlyout = cf;

            Debug.Assert(ui.ContextFlyout.XamlRoot == newXamlRoot, "Now we have the correct xaml root");
        }
    }
}
