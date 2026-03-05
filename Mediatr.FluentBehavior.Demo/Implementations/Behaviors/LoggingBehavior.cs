using Mediatr.FluentBehavior.Interfaces;
using Microsoft.Extensions.Logging;

namespace Mediatr.FluentBehavior.Demo.Implementations.Behaviors;

/// <summary>
/// Декоратор - логирование обработки
/// </summary>
public partial class LoggingBehavior<TRequest, TResponse>(
    ILogger logger)
    : IFluentBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
    {
        LogInProgress(logger, GetType().Name);
        try
        {
            var result = await next();
            LogCompleted(logger, GetType().Name);
            return result;
        }
        catch (Exception ex)
        {
            LogError(logger, GetType().Name, ex.Message);
            throw;
        }
    }

    [LoggerMessage(LogLevel.Information, "{typeName}. Обработка завершена")]
    static partial void LogCompleted(
        ILogger logger,
        string typeName);

    [LoggerMessage(LogLevel.Information, "{typeName}. Начало обработки")]
    static partial void LogInProgress(
        ILogger logger,
        string typeName);
    
    [LoggerMessage(LogLevel.Error, "{typeName}. Ошибка: {exMessage}")]
    static partial void LogError(
        ILogger logger,
        string typeName,
        string exMessage);
}