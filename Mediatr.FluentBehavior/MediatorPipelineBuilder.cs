using MediatR;
using Mediatr.FluentBehavior.Exceptions;
using Mediatr.FluentBehavior.Interfaces;

namespace Mediatr.FluentBehavior;

/// <summary>
/// Построитель пайплайна обработчиков
/// </summary>
public class MediatorPipelineBuilder<TResponse>(
    IMediator mediator,
    IServiceProvider serviceProvider) : IMediatrPipelineBuilder<TResponse>
{
    private IRequest<TResponse>? _command;
    private readonly List<IFluentBehavior<IRequest<TResponse>, TResponse>> _behaviors = new();

    public IMediatrPipelineBuilder<TResponse> SetCommand(IRequest<TResponse> command)
    {
        if (_command != null)
            throw new CommandAlreadySetException();
        
        _command = command;
        
        return this;
    }
    
    public IMediatrPipelineBuilder<TResponse> WithBehavior(
        IFluentBehavior<IRequest<TResponse>, TResponse> behavior)
    {
        _behaviors.Add(behavior);
        
        return this;
    }
    
    public IMediatrPipelineBuilder<TResponse> WithBehavior(
        Func<IServiceProvider, IFluentBehavior<IRequest<TResponse>, TResponse>> behaviorFunc)
    {
        var behavior = behaviorFunc(serviceProvider);
        
        _behaviors.Add(behavior);
        
        return this;
    }

    public async Task<TResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_command == null)
            throw new CommandNotSetException();
        
        var next = () => mediator.Send(_command, cancellationToken);

        foreach (var behavior in _behaviors)
        {
            var nextCopy = next;
            var behavior1 = behavior;
            next = () => behavior1.Handle(_command, nextCopy, cancellationToken);
        }

        return await next();
    }
}