using Mediatr.FluentBehavior.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mediatr.FluentBehavior.Demo.Implementations.Behaviors.Logging;

/// <summary>
/// Декоратор - логирование обработки
/// </summary>
public class LoggingBehavior<TRequest, TResponse>(
    IOptions<LoggingOptions> options,
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IFluentBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
    {
        logger.Log(
            options.Value.BaseMessagesLevel,
            "{typeName}. Начало обработки",
            GetType().Name
            );
        try
        {
            var result = await next();
            logger.Log(
                options.Value.BaseMessagesLevel,
                "{typeName}. Обработка завершена",
                GetType().Name);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.Log(
                options.Value.BaseMessagesLevel,
                "{typeName}. Ошибка: {exMessage}",
                GetType().Name,
                ex.Message);
            
            throw;
        }
    }
}