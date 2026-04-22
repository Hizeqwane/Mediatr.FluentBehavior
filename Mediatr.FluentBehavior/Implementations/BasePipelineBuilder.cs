using Mediatr.FluentBehavior.Exceptions;
using Mediatr.FluentBehavior.Extensions;
using Mediatr.FluentBehavior.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mediatr.FluentBehavior.Implementations;

/// <summary>
/// Базовый построитель пайплайна обработчиков
/// </summary>
public abstract class BasePipelineBuilder<TRequest, TResponse>(
    TRequest request,
    IServiceProvider serviceProvider) : IPipelineBuilder<TRequest, TResponse>
{
    private readonly PipelineBuilderOptions _options = 
        serviceProvider
            .GetService<IOptions<PipelineBuilderOptions>>()?.Value ?? new PipelineBuilderOptions();
    protected TRequest Request { get; } = request;

    private readonly List<IFluentBehavior<TRequest, TResponse>> _behaviors = new();
    
    public IPipelineBuilder<TRequest, TResponse> WithBehavior<TBehavior>()
        where TBehavior : IFluentBehavior<TRequest, TResponse>
    {
        var behavior = _options.IsBehaviorsInDi
            ? serviceProvider.GetRequiredService<TBehavior>()
            : serviceProvider.ConstructByServiceProvider<TBehavior>();
        
        _behaviors.Add(behavior);
        
        return this;
    }

    public IPipelineBuilder<TRequest, TResponse> WithBehavior(
        IFluentBehavior<TRequest, TResponse> behavior)
    {
        _behaviors.Add(behavior);
        
        return this;
    }

    protected abstract Task<TResponse> GetBaseCallback(CancellationToken cancellationToken);
    
    public async Task<TResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (Request == null)
            throw new CommandNotSetException();
        
        var next = () => GetBaseCallback(cancellationToken);

        foreach (var behavior in _behaviors)
        {
            var nextCopy = next;
            var behavior1 = behavior;
            next = () => behavior1.Handle(Request, nextCopy, cancellationToken);
        }

        return await next();
    }
}