using ReactiveUI;
using System.Reactive;

namespace admin.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
	public RoutingState Router { get; } = new RoutingState();

	public MainWindowViewModel()
	{
	}
}
