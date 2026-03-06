using MediatR;
using Mediatr.FluentBehavior.Exceptions;
using Mediatr.FluentBehavior.Interfaces;

namespace Mediatr.FluentBehavior.Implementations;

/// <summary>
/// Построитель пайплайна обработчиков
/// </summary>
public class MediatorPipelineBuilder<TResponse>(
    IRequest<TResponse> command,
    IMediator mediator,
    IServiceProvider serviceProvider) : IMediatorPipelineBuilder<TResponse>
{
    private readonly List<IFluentBehavior<IRequest<TResponse>, TResponse>> _behaviors = new();
    
    public IMediatorPipelineBuilder<TResponse> WithBehavior(
        IFluentBehavior<IRequest<TResponse>, TResponse> behavior)
    {
        _behaviors.Add(behavior);
        
        return this;
    }
    
    public IMediatorPipelineBuilder<TResponse> WithBehavior(
        Func<IServiceProvider, IFluentBehavior<IRequest<TResponse>, TResponse>> behaviorFunc)
    {
        var behavior = behaviorFunc(serviceProvider);
        
        _behaviors.Add(behavior);
        
        return this;
    }

    public async Task<TResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new CommandNotSetException();
        
        var next = () => mediator.Send(command, cancellationToken);

        foreach (var behavior in _behaviors)
        {
            var nextCopy = next;
            var behavior1 = behavior;
            next = () => behavior1.Handle(command, nextCopy, cancellationToken);
        }

        return await next();
    }
}