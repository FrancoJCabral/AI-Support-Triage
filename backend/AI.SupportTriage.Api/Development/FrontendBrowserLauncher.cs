using System.ComponentModel;
using System.Diagnostics;

namespace AI.SupportTriage.Api.Development;

public sealed class FrontendBrowserLauncher(
    IHostApplicationLifetime applicationLifetime,
    ILogger<FrontendBrowserLauncher> logger) : BackgroundService
{
    private static readonly Uri FrontendUrl = new("http://localhost:3000");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WaitForApplicationStartedAsync(stoppingToken);

        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var response = await client.GetAsync(FrontendUrl, stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    OpenChrome();
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Next.js is still starting.
            }
            catch (TaskCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                // The request timed out; retry until Next.js is ready.
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
        }
    }

    private void OpenChrome()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "chrome.exe",
                UseShellExecute = true
            };
            startInfo.ArgumentList.Add(FrontendUrl.AbsoluteUri);
            Process.Start(startInfo);

            logger.LogInformation(
                "Development frontend is available. Google Chrome opened at {Url}.",
                FrontendUrl);
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            logger.LogWarning(
                exception,
                "Development frontend is available. Google Chrome could not be opened at {Url}.",
                FrontendUrl);
        }
    }

    private Task WaitForApplicationStartedAsync(CancellationToken stoppingToken)
    {
        if (applicationLifetime.ApplicationStarted.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var completion = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        applicationLifetime.ApplicationStarted.Register(
            () => completion.TrySetResult());
        stoppingToken.Register(() => completion.TrySetCanceled(stoppingToken));

        return completion.Task;
    }
}
