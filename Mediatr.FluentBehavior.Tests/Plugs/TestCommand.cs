using MediatR;

namespace Mediatr.FluentBehavior.Tests.Plugs;

public record TestCommand(string Message) : IRequest<string>;