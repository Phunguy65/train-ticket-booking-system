using admin.ViewModels;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace admin.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
	public MainWindow()
	{
		this.WhenActivated(disposables => { });
		InitializeComponent();
	}
}