using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace EsmatPlastic.Desktop.Services;

public static class LocalApiLauncher
{
    private static Process? _apiProcess;

    public static async Task EnsureApiRunningAsync(string baseUrl = "http://localhost:5023/")
    {
        if (await IsApiRespondingAsync(baseUrl))
        {
            return;
        }

        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Search candidate paths for EsmatPlastic.API.exe
            var candidatePaths = new[]
            {
                Path.Combine(baseDir, "EsmatPlastic.API.exe"),
                Path.Combine(baseDir, "..", "..", "..", "..", "EsmatPlastic.API", "bin", "Debug", "net9.0", "EsmatPlastic.API.exe"),
                Path.Combine(baseDir, "..", "EsmatPlastic.API", "bin", "Debug", "net9.0", "EsmatPlastic.API.exe"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "EsmatPlastic.API", "bin", "Debug", "net9.0", "EsmatPlastic.API.exe"))
            };

            string? exePath = null;
            foreach (var path in candidatePaths)
            {
                if (File.Exists(path))
                {
                    exePath = path;
                    break;
                }
            }

            if (exePath != null)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = Path.GetDirectoryName(exePath)!,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _apiProcess = Process.Start(psi);
            }
            else
            {
                // Fallback: try dotnet run if API source directory is present
                var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
                var apiProjDir = Path.Combine(solutionDir, "EsmatPlastic.API");
                if (Directory.Exists(apiProjDir))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "run --no-build",
                        WorkingDirectory = apiProjDir,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    _apiProcess = Process.Start(psi);
                }
            }

            // Wait up to 10 seconds for local API server to boot up
            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(500);
                if (await IsApiRespondingAsync(baseUrl))
                {
                    break;
                }
            }
        }
        catch
        {
            // Non-fatal exception during API launcher
        }
    }

    private static async Task<bool> IsApiRespondingAsync(string baseUrl)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await client.GetAsync(new Uri(new Uri(baseUrl), "api/Health/status"));
            return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
        }
        catch
        {
            return false;
        }
    }
}
