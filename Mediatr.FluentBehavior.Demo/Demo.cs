using System.Reflection;
using Mediatr.FluentBehavior.Demo.Implementations;
using Mediatr.FluentBehavior.Interfaces;
using Mediatr.FluentBehavior.Demo.RegisterExtensions;
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
        
        services.AddTransient(typeof(IMediatrPipelineBuilder<>), typeof(MediatorPipelineBuilder<>));

        var serviceProvider = services.BuildServiceProvider();

        var builder1 = serviceProvider.GetRequiredService<IMediatrPipelineBuilder<string>>();
        var builder2 = serviceProvider.GetRequiredService<IMediatrPipelineBuilder<string>>();
        
        var command = new Command("Hello", 2);

        var result = await builder1.SetCommand(command)
            .WithRetry(3, TimeSpan.FromSeconds(1))
            .WithLogging()
            .ExecuteAsync();

        testOutputHelper.WriteLine($"Результат: {result}");

        var command2 = new Command("Hello without retry", 0);
        var result2 = await builder2.SetCommand(command2)
            .WithLogging()
            .ExecuteAsync();

        testOutputHelper.WriteLine($"Результат 2: {result2}");
    }
}