using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PropertyBindingTest;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class TestPage1 : Page
{
    public TestPage1()
    {
        this.InitializeComponent();

        MyControl.PropertyChanged += MyControl_PropertyChanged;
    }

    private void MyControl_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        EventFired.Text = $"MyControl.PropertyChanged raised: PropertyName = {e.PropertyName}";
    }
}
