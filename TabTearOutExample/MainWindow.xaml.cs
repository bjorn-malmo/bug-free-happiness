using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Linq;

namespace App2;

public sealed partial class MainWindow : Window
{
    static int _index = 0;
    MainWindow? _tearOutWindow;

    public MainWindow()
    {
        this.InitializeComponent();
        
        CreateTab();
    }

    private TabViewItem CreateTab()
    {
        _index++;
        var tab = new TabViewItem
        {
            Header = $"Tab {_index}",
            Content = new TextBlock 
            { 
                Text = $"Content {_index}", 
                VerticalAlignment = VerticalAlignment.Center, 
                HorizontalAlignment = HorizontalAlignment.Center 
            }
        };

        Demo.TabItems.Add(tab);
        return tab;
    }

    private void Demo_AddTabButtonClick(TabView sender, object args)
    {
        var tab = CreateTab();
    }

    private void Demo_TabTearOutWindowRequested(TabView sender, TabViewTabTearOutWindowRequestedEventArgs args)
    {
        _tearOutWindow = new MainWindow();
        args.NewWindowId = _tearOutWindow.AppWindow.Id;
    }

    private void Demo_TabTearOutRequested(TabView sender, TabViewTabTearOutRequestedEventArgs args)
    {
        var tab = args.Tabs.FirstOrDefault() as TabViewItem;
        if (tab != null)
        {
            sender.TabItems.Remove(tab);
            _tearOutWindow.Demo.TabItems.Add(tab);
        }
        _tearOutWindow = null;
    }

    private void Demo_ExternalTornOutTabsDropping(TabView sender, TabViewExternalTornOutTabsDroppingEventArgs args)
    {
        args.AllowDrop = true;
    }

    private void Demo_ExternalTornOutTabsDropped(TabView sender, TabViewExternalTornOutTabsDroppedEventArgs args)
    {
        // Move tab to this tab view instead
        var tab = (TabViewItem)args.Tabs.FirstOrDefault();
        var o = GetOwner(tab);

        o.TabItems.Remove(tab);
        sender.TabItems.Insert(args.DropIndex, tab);
    }

    TabView GetOwner(TabViewItem tab)
    {
        var element = (FrameworkElement)tab;
        do
        {
            if (element is TabView tabView)
            {
                return tabView;
            }

            element = (FrameworkElement)VisualTreeHelper.GetParent(element);
        } while (element != null);

        return null;
    }
}
