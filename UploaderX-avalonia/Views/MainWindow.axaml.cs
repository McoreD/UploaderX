using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace UploaderX.Views;

public partial class MainWindow : Window
{
    private TextBlock _DropState;
    private TextBlock _DragState;
    private TextBlock _Url;

    public MainWindow()
    {
        InitializeComponent();

        Debug.WriteLine("InitializeComponent");
        AvaloniaXamlLoader.Load(this);

        _DropState = this.Find<TextBlock>("DropState");
        _DragState = this.Find<TextBlock>("DragState");
        _Url = this.Find<TextBlock>("txtUrl");

        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    private async void BtnGo_ClickAsync(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Uri uri = new Uri(txtUrl.Text);
    }

    private void DragOver(object sender, DragEventArgs e)
    {
        // Only allow Copy or Link as Drop Operations.
        e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);

        // Only allow if the dragged data contains text or filenames.
        if (!e.Data.Contains(DataFormats.Text) && !e.Data.Contains(DataFormats.FileNames))
            e.DragEffects = DragDropEffects.None;
    }

    private void Drop(object sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Text))
            _DropState.Text = e.Data.GetText();
        else if (e.Data.Contains(DataFormats.FileNames))
        {
            _DropState.Text = string.Join(Environment.NewLine, e.Data.GetFileNames());
            // lbFiles.Items = e.Data.GetFileNames();

            foreach(string filePath in e.Data.GetFileNames())
            {
               Task.Run(() => Program.MyWorker.UploadFile(filePath));
            }
        }
    }
}
