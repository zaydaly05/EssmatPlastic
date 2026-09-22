using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Models.Users;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Permissions;
using EsmatPlastic.Desktop.Services.Users;
using EsmatPlastic.Desktop.ViewModels.Users;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Users;

public partial class UsersView : UserControl
{
    private readonly UsersViewModel _viewModel;
    private readonly UserService _userService;
    private readonly LocalizationService _loc;

    public UsersView()
    {
        InitializeComponent();

        _userService = App.ServiceProvider.GetRequiredService<UserService>();
        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();
        _viewModel = new UsersViewModel(_userService);

        DataContext = _viewModel;
        ApplyLocalization();

        Loaded += UsersView_Loaded;
    }

    private void ApplyLocalization()
    {
        PageTitleText.Text = _loc.T("المستخدمون");
        PageSubtitleText.Text = _loc.T("إدارة حسابات المستخدمين والدور والصلاحيات");
        AddButton.Content = _loc.T("+  إضافة مستخدم");

        StatTotalUsersLabel.Text = _loc.T("إجمالي المستخدمين");
        StatTotalUsersSubtext.Text = _loc.T("جميع الحسابات المنسجلة");
        StatActiveUsersLabel.Text = _loc.T("الحسابات النشطة");
        StatActiveUsersSubtext.Text = _loc.T("حسابات مفعلة ومتاحة");
        StatAdminUsersLabel.Text = _loc.T("مديرو النظام");
        StatAdminUsersSubtext.Text = _loc.T("صلاحيات كاملة بالنظام");

        HeaderUser.Text = _loc.T("المستخدم");
        HeaderRole.Text = _loc.T("الدور");
        HeaderStatus.Text = _loc.T("الحالة");
        HeaderCreated.Text = _loc.T("تاريخ الإنشاء");
        HeaderActions.Text = _loc.T("الإجراءات");
    }

    private async void UsersView_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= UsersView_Loaded;
        await _viewModel.LoadAsync();
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new UserEditorWindow(_userService)
        {
            Owner = Window.GetWindow(this)
        };

        if (window.ShowDialog() == true)
        {
            await _viewModel.LoadAsync();
        }
    }

    private async void PermissionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (GetSelectedUser(sender) is not UserResponse user)
            return;

        var window = new UserPermissionsWindow(
            _userService,
            App.ServiceProvider.GetRequiredService<PermissionService>(),
            user)
        {
            Owner = Window.GetWindow(this)
        };

        if (window.ShowDialog() == true)
        {
            await _viewModel.LoadAsync();
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (GetSelectedUser(sender) is not UserResponse user)
            return;

        var window = new UserEditorWindow(_userService, user)
        {
            Owner = Window.GetWindow(this)
        };

        if (window.ShowDialog() == true)
        {
            await _viewModel.LoadAsync();
        }
    }

    private void PasswordButton_Click(object sender, RoutedEventArgs e)
    {
        if (GetSelectedUser(sender) is not UserResponse user)
            return;

        var window = new ChangePasswordWindow(_userService, user)
        {
            Owner = Window.GetWindow(this)
        };

        window.ShowDialog();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (GetSelectedUser(sender) is not UserResponse user)
            return;

        var result = MessageBox.Show(
            string.Format(_loc.T("هل تريد حذف المستخدم {0}؟"), user.FullName),
            _loc.T("تأكيد الحذف"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            await _userService.DeleteAsync(user.Id);
            await _viewModel.LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                _loc.T("تعذر حذف المستخدم"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static UserResponse? GetSelectedUser(object sender)
    {
        if (sender is Button button && button.DataContext is UserResponse user)
        {
            return user;
        }

        return null;
    }
}
