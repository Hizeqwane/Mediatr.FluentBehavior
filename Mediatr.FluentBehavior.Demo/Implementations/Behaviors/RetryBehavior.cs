using Mediatr.FluentBehavior.Interfaces;
using Microsoft.Extensions.Logging;

namespace Mediatr.FluentBehavior.Demo.Implementations.Behaviors;

/// <summary>
/// Декоратор - повторные попытки выполнения
/// </summary>
public partial class RetryBehavior<TRequest, TResponse>(
    ILogger<RetryBehavior<TRequest, TResponse>> logger,
    int retryCount,
    TimeSpan delay)
    : IFluentBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
    {
        var attempt = 1;
        while (true)
        {
            try
            {
                LogAttemptAttempt(logger, GetType().Name, attempt);
                
                return await next();
            }
            catch when (attempt++ <= retryCount)
            {
                LogErrorToRetry(logger, GetType().Name, delay);
                
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    [LoggerMessage(LogLevel.Information, "{typeName}. Попытка {attempt}")]
    static partial void LogAttemptAttempt(
        ILogger<RetryBehavior<TRequest, TResponse>> logger,
        string typeName,
        int attempt);
    
    [LoggerMessage(LogLevel.Information, "{typeName}. Ошибка. Повтор через {delay}")]
    static partial void LogErrorToRetry(
        ILogger<RetryBehavior<TRequest, TResponse>> logger,
        string typeName,
        TimeSpan delay);
}