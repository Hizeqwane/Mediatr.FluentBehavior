using System.Reflection;
using Mediatr.FluentBehavior.Demo.Implementations;
using Mediatr.FluentBehavior.Demo.Implementations.Behaviors;
using Mediatr.FluentBehavior.Demo.Implementations.Behaviors.Logging;
using Mediatr.FluentBehavior.Demo.Implementations.Behaviors.Retry;
using Mediatr.FluentBehavior.Interfaces;
using Mediatr.FluentBehavior.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Mediatr.FluentBehavior.Demo;

public class Demo(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Example()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddLogging(builder =>
        {
            builder.AddXUnit(testOutputHelper);
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddScoped<IMediatorPipelineFactory, MediatorPipelineFactory>();
        services.Configure<PipelineBuilderOptions>(s =>
        {
            s.IsBehaviorsInDi = true;
        });

        services.AddScoped(typeof(RetryBehavior<,>));
        services.Configure<RetryOptions>(s =>
        {
            s.RetryCount = 3;
            s.Delay = TimeSpan.FromSeconds(1);
        });
        
        services.AddScoped(typeof(LoggingBehavior<,>));
        services.Configure<LoggingOptions>(s =>
        {
            s.BaseMessagesLevel = LogLevel.Information;
            s.ErrorMessagesLevel = LogLevel.Error;
        });

        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<IMediatorPipelineFactory>();
        
        var command = new Command("Hello", 2);

        var result = await factory
            .ByMediatorRequest(command)
            .UseRetryBehavior()
            .UseLoggingBehavior()
            .ExecuteAsync();

        testOutputHelper.WriteLine($"Результат: {result}");

        var command2 = new Command("Hello without retry", 0);
        var result2 = await factory
            .ByMediatorRequest(command2)
            .UseLoggingBehavior()
            .ExecuteAsync();

        testOutputHelper.WriteLine($"Результат 2: {result2}");
    }
}