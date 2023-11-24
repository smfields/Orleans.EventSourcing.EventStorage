using Microsoft.Extensions.DependencyInjection;

namespace Orleans.EventSourcing.EventStorage;

public static class LogConsistencyProviderFactory
{
    public static ILogViewAdaptorFactory Create(IServiceProvider services, string name)
    {
        return ActivatorUtilities.CreateInstance<LogConsistencyProvider>(services, Array.Empty<object>());
    }
}