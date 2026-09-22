using System.Net.Http;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;

namespace EsmatPlastic.Desktop.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly AuthService _authService;
    private readonly AppSession _appSession;
    private readonly LocalizationService _localization;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    public LoginViewModel(
        AuthService authService,
        AppSession appSession,
        LocalizationService localization)
    {
        _authService = authService;
        _appSession = appSession;
        _localization = localization;

        LoginCommand = new RelayCommand(
            async _ => await LoginAsync(),
            _ => !IsLoading);
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

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = _localization.T("يرجى إدخال اسم المستخدم وكلمة المرور.");
            return;
        }

        IsLoading = true;

        try
        {
            var result = await _authService.LoginAsync(
                Username,
                Password);

            if (result is null)
            {
                ErrorMessage = _localization.T("اسم المستخدم أو كلمة المرور غير صحيحة.");
                return;
            }

            _appSession.Start(result);

            LoginSucceeded?.Invoke(
                this,
                EventArgs.Empty);
        }
        catch (HttpRequestException)
        {
            ErrorMessage = _localization.T("تعذر الاتصال بالخادم.");
        }
        catch (Exception)
        {
            ErrorMessage = _localization.T("حدث خطأ أثناء تسجيل الدخول.");
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

