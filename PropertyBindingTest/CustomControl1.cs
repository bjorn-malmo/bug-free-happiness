using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;

namespace PropertyBindingTest;

[ObservableObject]
public sealed partial class CustomControl1 : Control
{
    public CustomControl1()
    {
        this.DefaultStyleKey = typeof(CustomControl1);
    }

    [ObservableProperty] string _myValue = "Initial value";

    [RelayCommand]
    void UpdateValue()
    {
        MyValue = "Updated value";
    }
}
