using admin.ViewModels.AuditLogs;
using admin.ViewModels.Authentication;
using admin.ViewModels.TrainManagement;
using admin.ViewModels.UserManagement;
using admin.Views.AuditLogs;
using admin.Views.Authentication;
using admin.Views.TrainManagement;
using admin.Views.UserManagement;
using ReactiveUI;

namespace admin;

public class ViewLocator : IViewLocator
{
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null) => viewModel switch
    {
        LoginViewModel => new LoginView() { DataContext = viewModel },
        TrainListViewModel => new TrainListView() { DataContext = viewModel },
        TrainFormViewModel => new TrainFormView() { DataContext = viewModel },
        UserListViewModel => new UserListView() { DataContext = viewModel },
        UserFormViewModel => new UserFormView() { DataContext = viewModel },
        AuditLogViewModel => new AuditLogView() { DataContext = viewModel },
        _ => null
    };
}