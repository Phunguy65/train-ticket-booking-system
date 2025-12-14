using admin.ViewModels.AuditLogs;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace admin.Views.AuditLogs;

public partial class AuditLogView : ReactiveUserControl<AuditLogViewModel>
{
	public AuditLogView()
	{
		this.WhenActivated(disposables => { });
		InitializeComponent();
	}
}