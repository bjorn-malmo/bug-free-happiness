using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        MyItems = new List<MyItemClass>()
        {
            new MyItemClass { PositionX = 20, PositionY = 40},
            new MyItemClass { PositionX = 40, PositionY = 60},
            new MyItemClass { PositionX = 90, PositionY = 20},
        };

        this.InitializeComponent();
    }

    public List<MyItemClass> MyItems { get; }
}

public class MyItemClass
{
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public string ItemText => $"X={PositionX:0.0}, Y={PositionY:0.0}";
}
