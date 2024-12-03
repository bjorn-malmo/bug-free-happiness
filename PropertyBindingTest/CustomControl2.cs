using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PropertyBindingTest;
public sealed class CustomControl2 : Control, INotifyPropertyChanged
{
    public CustomControl2()
    {
        this.DefaultStyleKey = typeof(CustomControl2);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    bool _showMe;

    public bool ShowMe
    {
        get => _showMe;
        set
        {
            if (_showMe != value)
            {
                _showMe = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowMe)));
            }
        }
    }
}
