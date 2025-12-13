using admin.ViewModels.TrainManagement;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace admin.Views.TrainManagement;

public partial class TrainListView : ReactiveUserControl<TrainListViewModel>
{
	public TrainListView()
	{
		this.WhenActivated(disposables => { });
		InitializeComponent();
	}
}