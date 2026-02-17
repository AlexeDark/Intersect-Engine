namespace Intersect.Async;

public partial class AsyncValueGenerator<TValue> : IDisposable
{
    private readonly CancellationToken _cancellationToken;
    private Task? _task;
    private readonly Func<Task<TValue>> _valueGenerator;
    private readonly Action<TValue> _valueHandler;

    public AsyncValueGenerator(Func<Task<TValue>> valueGenerator, Action<TValue> valueHandler, CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _valueGenerator = valueGenerator;
        _valueHandler = valueHandler;
    }

    public void Dispose()
    {
        if (_task is { IsCompleted: true })
        {
            _task.Dispose();
        }
    }

    private async Task DoLoopAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            var value = await _valueGenerator().ConfigureAwait(false);
            _valueHandler(value);
        }
    }

    public AsyncValueGenerator<TValue> Start()
    {
        _task ??= Task.Run(DoLoopAsync, _cancellationToken);
        return this;
    }
}
