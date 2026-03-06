using MediatR;
using Mediatr.FluentBehavior.Demo.Implementations.Behaviors;
using Mediatr.FluentBehavior.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mediatr.FluentBehavior.Demo.RegisterExtensions;

/// <summary>
/// Регистрация декораторов
/// </summary>
public static class MediatorCommandBuilderExtensions
{
    extension<TResponse>(IMediatorPipelineBuilder<TResponse> builder)
    {
        /// <summary>
        /// Декоратор повторных попыток обработки
        /// </summary>
        public IMediatorPipelineBuilder<TResponse> WithRetry(int count,
            TimeSpan delay)
        {
            builder.WithBehavior(GetRetryBehavior);
        
            return builder;

            RetryBehavior<IRequest<TResponse>, TResponse> GetRetryBehavior(IServiceProvider services)
            {
                var logger = services.GetLogger<RetryBehavior<IRequest<TResponse>, TResponse>>();

                var behaviour = new RetryBehavior<IRequest<TResponse>, TResponse>(logger, count, delay);

                return behaviour;
            }
        }

        /// <summary>
        /// Декоратор логирования
        /// </summary>
        public IMediatorPipelineBuilder<TResponse> WithLogging()
        {
            builder.WithBehavior(GetLoggingBehavior);
        
            return builder;

            LoggingBehavior<IRequest<TResponse>, TResponse> GetLoggingBehavior(IServiceProvider services)
            {
                var logger = services.GetLogger<LoggingBehavior<IRequest<TResponse>, TResponse>>();

                var behaviour = new LoggingBehavior<IRequest<TResponse>, TResponse>(logger);

                return behaviour;
            }
        }
    }

    private static ILogger<T> GetLogger<T>(this IServiceProvider services)
    {
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<T>();
        
        return logger;
    }
}