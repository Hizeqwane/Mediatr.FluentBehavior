using MediatR;
using Microsoft.Extensions.Logging;

namespace Mediatr.FluentBehavior.Demo.Implementations;

public partial class CommandHandler(
    ILogger<CommandHandler> logger)
    : IRequestHandler<Command, string>
{
    private static int? _errorCount;
    
    public async Task<string> Handle(Command request, CancellationToken cancellationToken)
    {
        LogStart(logger, GetType().Name);

        _errorCount ??= request.ErrorCount;
        
        while (true)
        {
            await Task.Delay(100, cancellationToken);
            
            if (_errorCount-- > 0)
            {
                LogError(logger, GetType().Name, _errorCount.Value);
                throw new Exception("error");
            }
            
            LogEnd(logger, GetType().Name);
            
            break;
        }

        return request.Message;
    }

    [LoggerMessage(LogLevel.Information, "{typeName}. Начало обработки")]
    static partial void LogStart(
        ILogger<CommandHandler> logger,
        string typeName);
    
    [LoggerMessage(LogLevel.Information, "{typeName}. Выбрасываем исключение. ErrorCount = {errorCount}")]
    static partial void LogError(
        ILogger<CommandHandler> logger,
        string typeName,
        int errorCount);
    
    [LoggerMessage(LogLevel.Information, "{typeName}. Обработка прошла успешно")]
    static partial void LogEnd(
        ILogger<CommandHandler> logger,
        string typeName);
}