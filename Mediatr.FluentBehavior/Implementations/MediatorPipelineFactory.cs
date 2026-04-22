using MediatR;
using Mediatr.FluentBehavior.Exceptions;
using Mediatr.FluentBehavior.Interfaces;

namespace Mediatr.FluentBehavior.Implementations;

/// <summary>
/// Фабрика построителей пайплайнов для MediatR
/// </summary>
public class MediatorPipelineFactory(
    IMediator mediator,
    IServiceProvider serviceProvider) 
    : IMediatorPipelineFactory
{
    public IPipelineBuilder<TRequest, TResponse> ByRequest<TRequest, TResponse>(TRequest request)
    {
        if (request == null)
            throw new CommandNotSetException();
        
        if (request is not IRequest<TResponse> mediatrRequest)
            throw new ArgumentException("Запрос не является IRequest<TResponse>");

        var mediatrPipelineBuilder =
            new MediatorPipelineBuilder<TResponse>(mediatrRequest, mediator, serviceProvider);
        
        return mediatrPipelineBuilder as IPipelineBuilder<TRequest, TResponse>
               ?? throw new InvalidOperationException("Не удалось привести построитель пайплайна для Mediatr к IPipelineBuilder<TRequest, TResponse>");
    }
}