namespace Mediatr.FluentBehavior.Demo.Implementations.Behaviors.Retry;

public class RetryOptions
{
    public int RetryCount { get; set; } = 3;
    
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(1);
}