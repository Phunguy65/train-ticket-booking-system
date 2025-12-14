using admin.Views.TrainManagement;
using ReactiveUI;

namespace admin.ViewModels.TrainManagement;

public class TrainFormViewModel : ViewModelBase, IRoutableViewModel
{
	public IScreen HostScreen { get; }
	public string UrlPathSegment => nameof(TrainFormView);

	public TrainFormViewModel(IScreen screen)
	{
		HostScreen = screen;
	}
}