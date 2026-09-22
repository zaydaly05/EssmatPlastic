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
    private string _searchText = string.Empty;
    private string _statusMessage = string.Empty;

    public ObservableCollection<UserResponse> AllUsers { get; } = new();
    public ObservableCollection<UserResponse> FilteredUsers { get; } = new();

    public int TotalUsersCount => AllUsers.Count;
    public int ActiveUsersCount => AllUsers.Count(u => u.IsActive);
    public int AdminUsersCount => AllUsers.Count(u => u.Role == UserRole.Admin);
    public int StaffUsersCount => AllUsers.Count(u => u.Role != UserRole.Admin);

    public UserResponse? SelectedUser
    {
        get => _selectedUser;
        set
        {
            _selectedUser = value;
            OnPropertyChanged();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

            _searchText = value;
            OnPropertyChanged();
            ApplyFilter();
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

    public UsersViewModel(UserService userService)
    {
        _userService = userService;
    }

    public async Task LoadAsync()
    {
        try
        {
            var result = await _userService.GetAllAsync();

            AllUsers.Clear();
            foreach (var user in result)
            {
                AllUsers.Add(user);
            }

            OnPropertyChanged(nameof(TotalUsersCount));
            OnPropertyChanged(nameof(ActiveUsersCount));
            OnPropertyChanged(nameof(AdminUsersCount));
            OnPropertyChanged(nameof(StaffUsersCount));

            ApplyFilter();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل المستخدمين: {ex.Message}";
        }
    }

    public void ApplyFilter()
    {
        FilteredUsers.Clear();
        var query = SearchText.Trim().ToLowerInvariant();

        foreach (var user in AllUsers)
        {
            if (string.IsNullOrEmpty(query) ||
                user.FullName.ToLowerInvariant().Contains(query) ||
                user.Username.ToLowerInvariant().Contains(query) ||
                user.Role.ToString().ToLowerInvariant().Contains(query))
            {
                FilteredUsers.Add(user);
            }
        }

        StatusMessage = string.IsNullOrEmpty(query)
            ? $"إجمالي المستخدمين: {AllUsers.Count}"
            : $"يعرض {FilteredUsers.Count} من أصل {AllUsers.Count} مستخدم";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
