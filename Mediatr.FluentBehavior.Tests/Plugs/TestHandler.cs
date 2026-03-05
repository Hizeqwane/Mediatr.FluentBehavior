using MediatR;

namespace Mediatr.FluentBehavior.Tests.Plugs;

public class TestHandler : IRequestHandler<TestCommand, string>
{
    public Task<string> Handle(TestCommand request, CancellationToken cancellationToken) => 
        Task.FromResult(request.Message);
}