using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CommandBarReuse
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void JustMoveButton_Click(object sender, RoutedEventArgs e)
        {
            var appWindow = new AppWindow();
            JustMoveElement(appWindow, FunctionPanel);

            appWindow.Activate();
        }
        private void ReassignContextMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var appWindow = new AppWindow();
            MoveAndReassignContextMenuElement(appWindow, FunctionPanel);

            appWindow.Activate();
        }
        
        private void JustMoveElement(AppWindow appWindow, StackPanel element)
        {
            // Move content from MainWindow to new AppWindow
            MainWindowContainer.Children.Remove(element);
            appWindow.NewContainer.Children.Add(element);
        }

        private void MoveAndReassignContextMenuElement(AppWindow appWindow, StackPanel element)
        {
            // Move content from MainWindow to new AppWindow
            MainWindowContainer.Children.Remove(element);

            element.ReassignContextMenus();

            appWindow.NewContainer.Children.Add(element);
        }

        private void Button1_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            args.Handled = true;
        }
    }
}
