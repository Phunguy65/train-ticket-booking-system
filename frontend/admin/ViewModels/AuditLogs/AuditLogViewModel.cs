using ReactiveUI;

namespace admin.ViewModels.AuditLogs;

public class AuditLogViewModel : ViewModelBase, IRoutableViewModel
{
	public IScreen HostScreen { get; }
	public string UrlPathSegment { get; } = "audit-logs";

	public AuditLogViewModel(IScreen screen)
	{
		HostScreen = screen;
	}
}