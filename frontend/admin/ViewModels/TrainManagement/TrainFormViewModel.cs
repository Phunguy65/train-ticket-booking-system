using ReactiveUI;

namespace admin.ViewModels.TrainManagement;

public class TrainFormViewModel : ViewModelBase, IRoutableViewModel
{
	public IScreen HostScreen { get; }
	public string UrlPathSegment { get; } = "train-form";

	public TrainFormViewModel(IScreen screen)
	{
		HostScreen = screen;
	}
}