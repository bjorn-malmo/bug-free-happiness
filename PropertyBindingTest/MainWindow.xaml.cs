using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using Windows.Graphics.DirectX.Direct3D11;
using static System.Net.Mime.MediaTypeNames;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PropertyBindingTest
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            Custom1.PropertyChanged += Custom1_Changed;
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            Custom1.ShowMe = true;
            Custom2.ShowMe = true;
            UserControl1.ShowMe = true;
            myButton.Content = "Clicked";
        }

        void Custom1_Changed(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Custom1 property {e.PropertyName} changed");

        }
    }
}
