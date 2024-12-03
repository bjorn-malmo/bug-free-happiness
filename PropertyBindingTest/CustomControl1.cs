using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace PropertyBindingTest;

[ObservableObject]
public partial class CustomControl1 : Control
{
    public CustomControl1()
    {
        this.DefaultStyleKey = typeof(CustomControl1);
    }

    [ObservableProperty] bool _showMe;

    [ObservableProperty] Visibility _showMe2 = Visibility.Collapsed;
}
