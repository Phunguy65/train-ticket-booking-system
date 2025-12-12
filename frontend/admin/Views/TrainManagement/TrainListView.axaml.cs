using admin.ViewModels.TrainManagement;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace admin.Views.TrainManagement;

public partial class TrainListView : ReactiveUserControl<TrainListViewModel>
{
	public TrainListView()
	{
		this.WhenActivated(disposables => { });
		InitializeComponent();
	}
}