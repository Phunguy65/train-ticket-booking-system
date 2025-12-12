using admin.ViewModels.AuditLogs;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace admin.Views.AuditLogs;

public partial class AuditLogView : ReactiveUserControl<AuditLogViewModel>
{
	public AuditLogView()
	{
		this.WhenActivated(disposables => { });
		InitializeComponent();
	}
}