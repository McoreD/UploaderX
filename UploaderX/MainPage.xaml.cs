using Microsoft.Extensions.Logging;

namespace UploaderX;

public partial class MainPage : ContentPage
{
	int count = 0;
	Worker _worker;

	public MainPage()
	{
		InitializeComponent();
		_worker = new Worker();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}


