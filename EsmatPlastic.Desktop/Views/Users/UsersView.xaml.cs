using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Models.Users;
using EsmatPlastic.Desktop.Services.Users;
using EsmatPlastic.Desktop.ViewModels.Users;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Users;

public partial class UsersView : UserControl
{
    private readonly UsersViewModel _viewModel;

    private readonly UserService _userService;

    public UsersView()
    {
        InitializeComponent();

        _userService =
            App.ServiceProvider
                .GetRequiredService<UserService>();

        _viewModel =
            new UsersViewModel(
                _userService);

        DataContext =
            _viewModel;

        UsersList.ItemsSource =
            _viewModel.Users;

        Loaded += UsersView_Loaded;
    }

    private async void UsersView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= UsersView_Loaded;

        await _viewModel.LoadAsync();

        UpdateStatus();
    }

    private async void AddButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var window =
            new UserEditorWindow(
                _userService);

        window.Owner =
            Window.GetWindow(this);

        if (window.ShowDialog() == true)
        {
            await _viewModel.LoadAsync();
            UpdateStatus();
        }
    }

    private async void EditButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (GetSelectedUser(sender) is not UserResponse user)
            return;

        var window =
            new UserEditorWindow(
                _userService,
                user);

        window.Owner =
            Window.GetWindow(this);

        if (window.ShowDialog() == true)
        {
            await _viewModel.LoadAsync();
            UpdateStatus();
        }
    }

    private void PasswordButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (GetSelectedUser(sender) is not UserResponse user)
            return;

        var window =
            new ChangePasswordWindow(
                _userService,
                user);

        window.Owner =
            Window.GetWindow(this);

        window.ShowDialog();
    }

    private async void DeleteButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (GetSelectedUser(sender) is not UserResponse user)
            return;

        var result =
            MessageBox.Show(
                $"هل تريد حذف المستخدم {user.FullName}؟",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            await _userService.DeleteAsync(
                user.Id);

            await _viewModel.LoadAsync();

            UpdateStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "تعذر حذف المستخدم",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static UserResponse?
        GetSelectedUser(object sender)
    {
        if (sender is not Button button)
            return null;

        if (button.DataContext
            is UserResponse user)
        {
            return user;
        }

        return null;
    }

    private void UpdateStatus()
    {
        StatusText.Text =
            _viewModel.StatusMessage;
    }
}



