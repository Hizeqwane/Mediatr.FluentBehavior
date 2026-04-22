using Microsoft.Extensions.Logging;

namespace Mediatr.FluentBehavior.Demo.Implementations.Behaviors.Logging;

public class LoggingOptions
{
    public LogLevel BaseMessagesLevel { get; set; } = LogLevel.Information;
    
    public LogLevel ErrorMessagesLevel { get; set; } = LogLevel.Error;
}