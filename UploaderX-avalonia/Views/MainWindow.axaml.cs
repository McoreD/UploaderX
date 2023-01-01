using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace UploaderX.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();  
    }

    private async void BtnGo_ClickAsync(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Uri uri = new Uri(txtUrl.Text);
    }
}
