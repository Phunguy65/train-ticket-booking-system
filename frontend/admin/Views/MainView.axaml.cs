using admin.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace admin.Views
{
	public partial class MainView : ReactiveUserControl<MainViewViewModel>
	{
		public MainView()
		{
			InitializeComponent();

			this.WhenActivated(disposables =>
			{
				// View activation logic can be added here if needed
			});
		}
	}
}