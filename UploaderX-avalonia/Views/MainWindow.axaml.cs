using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ShareX.HelpersLib;

namespace UploaderX.Views;

public partial class MainWindow : Window
{
    private TextBlock _DropState;
    private ListBox _Urls;

    public MainWindow()
    {
        InitializeComponent();

        Debug.WriteLine("InitializeComponent");
        AvaloniaXamlLoader.Load(this);

        _DropState = this.Find<TextBlock>("DropState");
        _Urls = this.Find<ListBox>("lbUrls");

        AddHandler(DragDrop.DropEvent, DropAsync);
        AddHandler(DragDrop.DragOverEvent, DragOver);
        _Urls.Tapped += _Urls_Tapped;
    }

    private void _Urls_Tapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string url = ((ListBox)sender).SelectedItem.ToString();
        URLHelpers.OpenURL(url);
    }

    private void DragOver(object sender, DragEventArgs e)
    {
        // Only allow Copy or Link as Drop Operations.
        e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);

        // Only allow if the dragged data contains text or filenames.
        if (!e.Data.Contains(DataFormats.Text) && !e.Data.Contains(DataFormats.FileNames))
            e.DragEffects = DragDropEffects.None;
    }

    private async Task DropAsync(object sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.FileNames))
        {
            _DropState.Text = string.Join(Environment.NewLine, e.Data.GetFileNames());
            await Task.Run(() => Program.MyWorker.UploadFiles(e.Data.GetFileNames()));
        }
    }
}
