using System.Net.Http;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EsmatPlastic.Desktop.Services;

namespace EsmatPlastic.Desktop.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly AuthService _authService;
    private readonly AppSession _appSession;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    public LoginViewModel(
        AuthService authService,
        AppSession appSession)
    {
        _authService = authService;
        _appSession = appSession;

        LoginCommand = new RelayCommand(
            async _ => await LoginAsync(),
            _ => !IsLoading &&
                 !string.IsNullOrWhiteSpace(Username) &&
                 !string.IsNullOrWhiteSpace(Password));
    }

    public string Username
    {
        get => _username;
        set
        {
            if (_username == value)
                return;

            _username = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (_password == value)
                return;

            _password = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value)
                return;

            _errorMessage = value;
            OnPropertyChanged();
        }
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
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ICommand LoginCommand { get; }

    public event EventHandler? LoginSucceeded;

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;

        try
        {
            var result = await _authService.LoginAsync(
                Username,
                Password);

            if (result is null)
            {
                ErrorMessage =
                    "يرجى إدخال اسم المستخدم وكلمة المرور.";

                return;
            }

            _appSession.Start(result);

            LoginSucceeded?.Invoke(
                this,
                EventArgs.Empty);
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "تعذر الاتصال بالخادم.";

        }
        catch (Exception)
        {
            ErrorMessage =
                "حدث خطأ أثناء تسجيل الدخول.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand : ICommand
{
    private readonly Func<object?, Task>? _executeAsync;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(
        Func<object?, Task> executeAsync,
        Predicate<object?>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    public async void Execute(object? parameter)
    {
        if (_executeAsync is not null)
        {
            await _executeAsync(parameter);
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

