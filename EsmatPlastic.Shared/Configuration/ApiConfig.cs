namespace EsmatPlastic.Shared.Configuration;

public static class ApiConfig
{
    public static string GetBaseUrl()
    {
        #if ANDROID
            return "http://10.0.2.2:5023";
        #else
            return "http://localhost:5023";
        #endif
    }
}
