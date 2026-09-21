namespace EsmatPlastic.Desktop.Services.Settings;

public class AppSettings
{
    public string ApiBaseUrl { get; set; } =
        "http://localhost:5023/";

    public string Language { get; set; } =
        "ar";

    public bool RememberLanguage { get; set; } =
        true;
}
