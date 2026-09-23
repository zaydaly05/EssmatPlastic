using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.Sockets;

namespace EsmatPlastic.Desktop.Services;

public static class LocalApiLauncher
{
    private static Process? _apiProcess;

    public static async Task<string> EnsureApiRunningAsync(string preferredBaseUrl = "http://localhost:5023/")
    {
        if (await IsApiRespondingAsync(preferredBaseUrl))
        {
            return preferredBaseUrl;
        }

        try
        {
            var baseUrl = GetAvailableBaseUrl();
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
                var exeDirectory = Path.GetDirectoryName(exePath)!;
                var projectDirectory = Path.GetFullPath(
                    Path.Combine(exeDirectory, "..", "..", ".."));
                var workingDirectory = File.Exists(
                    Path.Combine(projectDirectory, "EsmatPlastic.API.csproj"))
                    ? projectDirectory
                    : exeDirectory;

                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Environment =
                    {
                        ["ASPNETCORE_URLS"] = baseUrl,
                        ["ASPNETCORE_ENVIRONMENT"] = "Development"
                    }
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
                        Arguments = "run --no-launch-profile",
                        WorkingDirectory = apiProjDir,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Environment =
                        {
                            ["ASPNETCORE_URLS"] = baseUrl,
                            ["ASPNETCORE_ENVIRONMENT"] = "Development"
                        }
                    };
                    _apiProcess = Process.Start(psi);
                }
            }

            if (_apiProcess is null)
            {
                return preferredBaseUrl;
            }

            // Wait up to 10 seconds for local API server to boot up
            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(500);
                if (await IsApiRespondingAsync(baseUrl))
                {
                    return baseUrl;
                }
            }

            return preferredBaseUrl;
        }
        catch
        {
            // Non-fatal exception during API launcher
            return preferredBaseUrl;
        }
    }

    private static string GetAvailableBaseUrl()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return $"http://127.0.0.1:{port}/";
    }

    private static async Task<bool> IsApiRespondingAsync(string baseUrl)
    {
        try
        {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync(new Uri(new Uri(baseUrl), "api/Health/status"));
            return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
        }
        catch
        {
            return false;
        }
    }

    public static void Stop()
    {
        var process = _apiProcess;
        _apiProcess = null;

        if (process is null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }
        }
        catch (InvalidOperationException)
        {
            // The API process has already exited.
        }
        finally
        {
            process.Dispose();
        }
    }
}
