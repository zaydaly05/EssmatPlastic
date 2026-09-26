using EsmatPlastic.Shared.Services;
using EsmatPlastic.Shared.Models.Auth;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace EsmatPlastic.Shared.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;
    private readonly FirebaseAuthClient _firebaseAuthClient;
    private readonly FirebaseFirestoreClient _firestoreClient;
    private string _username;
    private string _password;
    private string _statusMessage;
    private bool _isLoading;
    private readonly AsyncCommand _loginCommand;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Username
    {
        get => _username ?? string.Empty;
        set { _username = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password ?? string.Empty;
        set { _password = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage ?? string.Empty;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value)
                return;

            _isLoading = value;
            OnPropertyChanged();
            _loginCommand.NotifyCanExecuteChanged();
        }
    }

    public ICommand LoginCommand => _loginCommand;

    public LoginResponse? CurrentUser { get; private set; }

    public event Action<LoginResponse>? LoginSucceeded;

    public LoginViewModel(
        ApiClient apiClient,
        FirebaseAuthClient firebaseAuthClient,
        FirebaseFirestoreClient firestoreClient)
    {
        _apiClient = apiClient;
        _firebaseAuthClient = firebaseAuthClient;
        _firestoreClient = firestoreClient;
        _username = string.Empty;
        _password = string.Empty;
        _statusMessage = string.Empty;
        _loginCommand = new AsyncCommand(LoginFromCommandAsync, () => !IsLoading);
    }

    public async Task<bool> LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "يرجى إدخال اسم المستخدم وكلمة المرور";
            return false;
        }

        try
        {
            var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>("api/Auth/login", new LoginRequest
            {
                Username = Username,
                Password = Password
            });

            if (response is not null && !string.IsNullOrWhiteSpace(response.Token))
            {
                var firebaseIdToken = await _firebaseAuthClient.ExchangeCustomTokenAsync(
                    response.FirebaseCustomToken ?? string.Empty,
                    response.FirebaseWebApiKey ?? string.Empty);
                response.FirebaseIdToken = firebaseIdToken;
                _firestoreClient.SetIdToken(firebaseIdToken);
                CurrentUser = response;
                OnPropertyChanged(nameof(CurrentUser));
                _apiClient.SetToken(response.Token);
                return true;
            }

            StatusMessage = "فشل تسجيل الدخول. يرجى التحقق من البيانات";
            return false;
        }
        catch (Exception ex)
        {
            StatusMessage = "خطأ في الاتصال بالخادم: " + ex.Message;
            return false;
        }
    }

    private async Task LoginFromCommandAsync()
    {
        StatusMessage = string.Empty;
        IsLoading = true;

        try
        {
            if (await LoginAsync() && CurrentUser is not null)
                LoginSucceeded?.Invoke(CurrentUser);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class AsyncCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute();

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            await _execute();
        }

        public void NotifyCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
