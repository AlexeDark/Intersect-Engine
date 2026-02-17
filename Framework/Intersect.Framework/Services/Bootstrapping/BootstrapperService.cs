using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Intersect.Framework.Services.Bootstrapping;

public sealed class BootstrapperService : IBootstrapperService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly BootstrapServiceOptions _options;
    private readonly IServiceProvider _serviceProvider;

    private CancellationTokenSource? _cancellationTokenSource;

    public BootstrapperService(
        IHostApplicationLifetime applicationLifetime,
        BootstrapServiceOptions options,
        IServiceProvider serviceProvider
    )
    {
        _applicationLifetime = applicationLifetime;
        _options = options;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var bootstrapTasks = _serviceProvider.GetServices<IBootstrapTask>();

        try
        {
            await Task.WhenAll(bootstrapTasks.Select(task => task.ExecuteAsync(_cancellationTokenSource.Token)));
        }
        finally
        {
            _applicationLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Cancel();
        return Task.CompletedTask;
    }
}
