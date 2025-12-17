using admin.Services;
using admin.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace admin.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
	public MainWindow()
	{
		this.WhenActivated(disposables => { });
		InitializeComponent();
	}
}
