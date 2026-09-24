using EsmatPlastic.Shared.Services;
using EsmatPlastic.Shared.Models.Auth;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EsmatPlastic.Shared.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;
    private string _username;
    private string _password;
    private string _statusMessage;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public LoginViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
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

            if (response != null)
            {
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

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
