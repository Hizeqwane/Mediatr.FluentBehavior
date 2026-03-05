using MediatR;

namespace Mediatr.FluentBehavior.Demo.Implementations;

public record Command(
    string Message,
    int ErrorCount) : IRequest<string>;