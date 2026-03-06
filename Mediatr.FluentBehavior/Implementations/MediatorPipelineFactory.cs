using MediatR;
using Mediatr.FluentBehavior.Interfaces;

namespace Mediatr.FluentBehavior.Implementations;

public class MediatorPipelineFactory(
    IMediator mediator,
    IServiceProvider serviceProvider) : IMediatorPipelineFactory
{
    public IMediatorPipelineBuilder<TResponse> ByCommand<TResponse>(
        IRequest<TResponse> request) =>
        new MediatorPipelineBuilder<TResponse>(request, mediator, serviceProvider);
}