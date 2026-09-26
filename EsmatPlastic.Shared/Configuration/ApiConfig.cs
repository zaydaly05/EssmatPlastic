namespace EsmatPlastic.Shared.Configuration;

public static class ApiConfig
{
    public static string GetBaseUrl()
    {
        if (OperatingSystem.IsAndroid())
            return "http://10.0.2.2:5023";

        return "http://localhost:5023";
    }
}
