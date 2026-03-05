using System.ComponentModel;
using System.Reflection;
using MediatR;
using Mediatr.FluentBehavior.Exceptions;
using Mediatr.FluentBehavior.Interfaces;
using Mediatr.FluentBehavior.Tests.Plugs;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Mediatr.FluentBehavior.Tests;

public class Tests()
{
    [Fact, Description("Базовое выполнение без декораторов")]
    public async Task ExecuteAsync_WithoutBehaviors_ShouldInvokeMediator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(IMediatrPipelineBuilder<>), typeof(MediatorPipelineBuilder<>));
        services.AddSingleton(Mock.Of<IMediator>());
    
        var sp = services.BuildServiceProvider();
        var mediatorMock = sp.GetRequiredService<IMediator>();
        var builder = sp.GetRequiredService<IMediatrPipelineBuilder<string>>();
        var command = new TestCommand("Hello");

        // Act
        await builder
            .SetCommand(command)
            .ExecuteAsync();

        // Assert
        Mock.Get(mediatorMock).Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact, Description("Выполнение с одним декоратором")]
    public async Task ExecuteAsync_WithOneBehavior_ShouldChainCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(IMediatrPipelineBuilder<>), typeof(MediatorPipelineBuilder<>));
    
        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<IMediatrPipelineBuilder<string>>();
        var command = new TestCommand("Hello");
        var behaviorMock = new Mock<IFluentBehavior<IRequest<string>, string>>();
    
        behaviorMock
            .Setup(x => x.Handle(command, It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()))
            .Returns<TestCommand, Func<Task<string>>, CancellationToken>((_, next, _) => next());

        // Act
        await builder
            .SetCommand(command)
            .WithBehavior(behaviorMock.Object)
            .ExecuteAsync();

        // Assert
        behaviorMock.Verify(
            x => x.Handle(command, It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }
    
    [Fact, Description("Два декоратора выполняются в правильном порядке")]
    public async Task ExecuteAsync_WithTwoBehaviors_ShouldExecuteInReverseOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(IMediatrPipelineBuilder<>), typeof(MediatorPipelineBuilder<>));
    
        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<IMediatrPipelineBuilder<string>>();
        var command = new TestCommand("Hello");
        var executionOrder = new List<string>();

        var behavior1 = new Mock<IFluentBehavior<IRequest<string>, string>>();
        behavior1
            .Setup(x => x.Handle(command, It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()))
            .Returns<TestCommand, Func<Task<string>>, CancellationToken>(async (_, next, _) =>
            {
                executionOrder.Add("Behavior1");
                return await next();
            });

        var behavior2 = new Mock<IFluentBehavior<IRequest<string>, string>>();
        behavior2
            .Setup(x => x.Handle(command, It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()))
            .Returns<TestCommand, Func<Task<string>>, CancellationToken>(async (_, next, _) =>
            {
                executionOrder.Add("Behavior2");
                return await next();
            });

        // Act
        await builder
            .SetCommand(command)
            .WithBehavior(behavior1.Object)  // добавляется первым
            .WithBehavior(behavior2.Object)  // добавляется вторым
            .ExecuteAsync();

        // Assert
        Assert.Equal(new[] { "Behavior2", "Behavior1" }, executionOrder);
    }
    
    [Fact, Description("Декоратор может прервать выполнение")]
    public async Task ExecuteAsync_WhenBehaviorThrows_ShouldNotInvokeMediator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(IMediatrPipelineBuilder<>), typeof(MediatorPipelineBuilder<>));
    
        var sp = services.BuildServiceProvider();
        var mediatorMock = new Mock<IMediator>();
        var builder = sp.GetRequiredService<IMediatrPipelineBuilder<string>>();
        var command = new TestCommand("Hello");
    
        var behaviorMock = new Mock<IFluentBehavior<IRequest<string>, string>>();
        behaviorMock
            .Setup(x => x.Handle(command, It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => builder
            .SetCommand(command)
            .WithBehavior(behaviorMock.Object)
            .ExecuteAsync());

        mediatorMock.Verify(x => x.Send(It.IsAny<IRequest<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact, Description("Декоратор может изменить результат")]
    public async Task ExecuteAsync_WhenBehaviorModifiesResult_ShouldReturnModifiedValue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(IMediatrPipelineBuilder<>), typeof(MediatorPipelineBuilder<>));
    
        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<IMediatrPipelineBuilder<string>>();
        var command = new TestCommand("Hello");
        var expectedResult = "modified";

        var behaviorMock = new Mock<IFluentBehavior<IRequest<string>, string>>();
        behaviorMock
            .Setup(x => x.Handle(command, It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await builder
            .SetCommand(command)
            .WithBehavior(behaviorMock.Object)
            .ExecuteAsync();

        // Assert
        Assert.Equal(expectedResult, result);
    }
    
    [Fact, Description("Декоратор с использованием DI фабрики")]
    public async Task ExecuteAsync_WithBehaviorFactory_ShouldResolveFromServiceProvider()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var behaviorMock = new Mock<IFluentBehavior<IRequest<string>, string>>();

        // Настраиваем возврат поведения из провайдера
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IFluentBehavior<IRequest<string>, string>)))
            .Returns(behaviorMock.Object);

        // Настраиваем mediator, чтобы цепочка выполнилась
        mediatorMock
            .Setup(m => m.Send(It.IsAny<IRequest<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("result");

        // Создаём builder напрямую, передавая моки
        var builder = new MediatorPipelineBuilder<string>(mediatorMock.Object, serviceProviderMock.Object);
        var command = new TestCommand("Hello");

        // Act
        await builder
            .SetCommand(command)
            .WithBehavior(sp => sp.GetRequiredService<IFluentBehavior<IRequest<string>, string>>())
            .ExecuteAsync();

        // Assert
        // Проверяем, что фабрика обратилась к провайдеру
        serviceProviderMock.Verify(
            sp => sp.GetService(typeof(IFluentBehavior<IRequest<string>, string>)),
            Times.Once);

        // Проверяем, что полученное поведение было вызвано
        behaviorMock.Verify(
            b => b.Handle(
                command,
                It.IsAny<Func<Task<string>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact, Description("Попытка установить команду дважды")]
    public void SetCommand_WhenCommandAlreadySet_ShouldThrowCommandAlreadySetException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(IMediatrPipelineBuilder<>), typeof(MediatorPipelineBuilder<>));
    
        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<IMediatrPipelineBuilder<string>>();
        var command1 = new TestCommand("Hello");
        var command2 = new TestCommand("Already hello");

        // Act
        builder.SetCommand(command1);

        // Assert
        Assert.Throws<CommandAlreadySetException>(() => builder.SetCommand(command2));
    }
    
    [Fact, Description("Выполнение без установленной команды")]
    public async Task ExecuteAsync_WhenCommandNotSet_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(IMediatrPipelineBuilder<>), typeof(MediatorPipelineBuilder<>));
    
        var sp = services.BuildServiceProvider();
        var builder = sp.GetRequiredService<IMediatrPipelineBuilder<string>>();

        // Act & Assert
        await Assert.ThrowsAsync<CommandNotSetException>(() => builder.ExecuteAsync());
    }
}