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
    private Border _DragMe;
    private int DragCount = 0;

    public MainWindow()
    {
        InitializeComponent();

        Debug.WriteLine("InitializeComponent");
        AvaloniaXamlLoader.Load(this);

        _DropState = this.Find<TextBlock>("DropState");
        _DragState = this.Find<TextBlock>("DragState");
        _DragMe = this.Find<Border>("DragMe");

        _DragMe.PointerPressed += DoDrag;

        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    private async void BtnGo_ClickAsync(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Uri uri = new Uri(txtUrl.Text);
    }

    private async void DoDrag(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        Debug.WriteLine("DoDrag");
        DataObject dragData = new DataObject();
        dragData.Set(DataFormats.Text, $"You have dragged text {++DragCount} times");

        var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
        switch (result)
        {
            case DragDropEffects.Copy:
                _DragState.Text = "The text was copied"; break;
            case DragDropEffects.Link:
                _DragState.Text = "The text was linked"; break;
            case DragDropEffects.None:
                _DragState.Text = "The drag operation was canceled"; break;
        }
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
            // lbFiles.SelectedIndex = 0;
            Worker worker = new Worker(Program.ConfigDir);
            foreach(string filePath in e.Data.GetFileNames())
            {
               txtUrl.Text = worker.UploadFile(filePath).URL;
               Application.Current.Clipboard.SetTextAsync(txtUrl.Text);
            }
        }
    }
}
