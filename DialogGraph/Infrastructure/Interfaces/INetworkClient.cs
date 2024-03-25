namespace DialogGraph;

public interface INetworkClient
{
    public Task StartClientAsync(IProgress<byte[]> progress, ILogger logger, IExceptionHandler exceptionHandler, CancellationToken cancellationToken);
}