using Mediatr.FluentBehavior.Demo.Implementations.Behaviors.Logging;
using Mediatr.FluentBehavior.Demo.Implementations.Behaviors.Retry;
using Mediatr.FluentBehavior.Interfaces;

namespace Mediatr.FluentBehavior.Demo.Implementations.Behaviors;

public static class Extensions
{
    extension<TRequest, TResponse>(IPipelineBuilder<TRequest, TResponse> builder)
    {
        public IPipelineBuilder<TRequest, TResponse> UseRetryBehavior() =>
            builder.WithBehavior<RetryBehavior<TRequest, TResponse>>();

        public IPipelineBuilder<TRequest, TResponse> UseLoggingBehavior() =>
            builder.WithBehavior<LoggingBehavior<TRequest, TResponse>>();
    }
}