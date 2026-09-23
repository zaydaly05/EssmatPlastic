using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EsmatPlastic.Desktop.Services;

public sealed class AppUpdateService
{
    private const string UpdateArchiveName = "EsmatPlastic-win-x64.zip";
    private static readonly HttpClient HttpClient = CreateHttpClient();

    public bool IsNewerVersion(string publishedVersion)
    {
        if (!Version.TryParse(publishedVersion.TrimStart('v', 'V'), out var latest) ||
            latest.Build < 0)
        {
            return false;
        }

        var current = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version
                      ?? new Version(1, 1, 0, 0);
        var normalizedCurrent = new Version(current.Major, current.Minor, Math.Max(current.Build, 0));
        return latest > normalizedCurrent;
    }

    public async Task<bool> DownloadAndApplyAsync(
        LatestUpdateResponse update,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(update.DownloadUrl, UriKind.Absolute, out var downloadUri) ||
            downloadUri.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(downloadUri.Host, "github.com", StringComparison.OrdinalIgnoreCase) ||
            !downloadUri.AbsolutePath.StartsWith(
                "/zaydaly05/EssmatPlastic/releases/download/",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var installDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var applicationPath = Environment.ProcessPath ?? string.Empty;
        if (applicationPath.Length == 0 || !File.Exists(applicationPath))
            return false;

        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"EsmatPlasticUpdate-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        var archivePath = Path.Combine(temporaryDirectory, UpdateArchiveName);

        try
        {
            using (var response = await HttpClient.GetAsync(
                       downloadUri,
                       HttpCompletionOption.ResponseHeadersRead,
                       cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentLength is > 750_000_000)
                    return false;

                await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var destination = File.Create(archivePath);
                await source.CopyToAsync(destination, cancellationToken);
            }

            var scriptPath = Path.Combine(temporaryDirectory, "apply-update.ps1");
            await File.WriteAllTextAsync(scriptPath, UpdateScript, cancellationToken);

            var canWriteToInstallDirectory = CanWriteTo(installDirectory);
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = !canWriteToInstallDirectory,
                CreateNoWindow = canWriteToInstallDirectory,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = canWriteToInstallDirectory ? string.Empty : "runas"
            };
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(scriptPath);
            startInfo.ArgumentList.Add(Environment.ProcessId.ToString());
            startInfo.ArgumentList.Add(archivePath);
            startInfo.ArgumentList.Add(installDirectory);
            startInfo.ArgumentList.Add(applicationPath);

            return Process.Start(startInfo) is not null;
        }
        catch
        {
            try { Directory.Delete(temporaryDirectory, recursive: true); }
            catch { /* Temporary download cleanup is best effort. */ }
            return false;
        }
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EsmatPlastic", "1.1"));
        return client;
    }

    private static bool CanWriteTo(string directory)
    {
        var probePath = Path.Combine(directory, $".update-check-{Guid.NewGuid():N}.tmp");
        try
        {
            using (File.Create(probePath)) { }
            File.Delete(probePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private const string UpdateScript = """
        param([int]$ParentProcessId, [string]$ArchivePath, [string]$InstallDirectory, [string]$ApplicationPath)
        $ErrorActionPreference = 'Stop'
        while (Get-Process -Id $ParentProcessId -ErrorAction SilentlyContinue) { Start-Sleep -Milliseconds 400 }
        $stage = Join-Path $env:TEMP ('EsmatPlasticStage-' + [guid]::NewGuid().ToString('N'))
        try {
            New-Item -ItemType Directory -Path $stage -Force | Out-Null
            Expand-Archive -LiteralPath $ArchivePath -DestinationPath $stage -Force
            Get-ChildItem -LiteralPath $stage -Force | ForEach-Object {
                Copy-Item -LiteralPath $_.FullName -Destination $InstallDirectory -Recurse -Force
            }
            Start-Process -FilePath $ApplicationPath -WorkingDirectory $InstallDirectory
        }
        finally {
            Remove-Item -LiteralPath $stage -Recurse -Force -ErrorAction SilentlyContinue
            Remove-Item -LiteralPath $ArchivePath -Force -ErrorAction SilentlyContinue
            Remove-Item -LiteralPath $PSCommandPath -Force -ErrorAction SilentlyContinue
        }
        """;
}
