using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VidiView.AnatomicMap;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using WinRT;
using Path = Microsoft.UI.Xaml.Shapes.Path;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App2
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private AnatomicMapSvg _map;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var file = new AnatomicMapSvg(null!);
            file.LoadResource("Test.svg");
            file.BackgroundColorHex = "#44000000";
            file.ForegroundColorHex = "#000000";
            file.DetailColorHex = "#66444444";
            _map = file;

            var svg = new SvgImageSource();
            await svg.SetSourceAsync(file.GetSvgStream().AsRandomAccessStream());

            ImageSource.Source = svg;

            var region = file.GetRegionFromId("245575001").First();
            if (region != null)
            {
                var p = CreateGeometryFromPath(region.PathData);
                Test.Child = p;
            }
        }

        Path CreateGeometryFromPath(string pathData)
        {
            var xaml = @"<Path xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                               Data=""" + pathData + @"""
                               Fill=""Gold"" 
                               Stroke=""Red"" 
                               StrokeThickness=""1""/>";
                               
            var path = XamlReader.Load(xaml).As<Path>();
            return path;
        }


        private void ImageSource_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // Get the code for this 
            var image = (Image)sender;
            var pt = e.GetCurrentPoint(image).Position;
            var x = pt.X / image.ActualWidth;
            var y = pt.Y / image.ActualHeight;
            var region = _map.GetRegionFromNormalizedPoint(x, y);

            RegionId.Text = region?.Id ?? "< none >";
        }
    }
}
