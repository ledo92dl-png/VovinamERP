using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Common.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
