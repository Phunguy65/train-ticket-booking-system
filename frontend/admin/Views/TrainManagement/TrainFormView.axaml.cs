using admin.ViewModels.TrainManagement;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace admin.Views.TrainManagement;

public partial class TrainFormView : ReactiveUserControl<TrainFormViewModel>
{
	public TrainFormView()
	{
		this.WhenActivated(disposables => { });
		InitializeComponent();
	}
}