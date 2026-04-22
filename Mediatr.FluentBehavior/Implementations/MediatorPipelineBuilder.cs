using MediatR;

namespace Mediatr.FluentBehavior.Implementations;

/// <summary>
/// Построитель пайплайна обработчиков для MediatR
/// </summary>
public class MediatorPipelineBuilder<TResponse>(
    IRequest<TResponse> request,
    IMediator mediator,
    IServiceProvider serviceProvider)
    : BasePipelineBuilder<IRequest<TResponse>, TResponse>(
        request,
        serviceProvider)
{
    protected override Task<TResponse> GetBaseCallback(
        CancellationToken cancellationToken) =>
        mediator.Send(Request, cancellationToken);
}