using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EsmatPlastic.Desktop.Models.Users;
using EsmatPlastic.Desktop.Services.Users;

namespace EsmatPlastic.Desktop.ViewModels.Users;

public class UsersViewModel : INotifyPropertyChanged
{
    private readonly UserService _userService;

    private UserResponse? _selectedUser;

    private string _statusMessage = string.Empty;

    public ObservableCollection<UserResponse> Users
    {
        get;
    } = new();

    public UserResponse? SelectedUser
    {
        get => _selectedUser;

        set
        {
            _selectedUser = value;

            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;

        private set
        {
            _statusMessage = value;

            OnPropertyChanged();
        }
    }

    public UsersViewModel(
        UserService userService)
    {
        _userService = userService;
    }

    public async Task LoadAsync()
    {
        try
        {
            var result =
                await _userService.GetAllAsync();

            Users.Clear();

            foreach (var user in result)
            {
                Users.Add(user);
            }

            StatusMessage =
                $"تم تحميل المستخدمين: {Users.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"تعذر تحميل المستخدمين: {ex.Message}";
        }
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(name));
    }
}
