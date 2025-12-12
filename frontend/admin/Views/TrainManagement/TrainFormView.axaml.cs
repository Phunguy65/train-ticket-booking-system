using admin.ViewModels.TrainManagement;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace admin.Views.TrainManagement;

public partial class TrainFormView : ReactiveUserControl<TrainFormViewModel>
{
	public TrainFormView()
	{
		this.WhenActivated(disposables => { });
		InitializeComponent();
	}
}