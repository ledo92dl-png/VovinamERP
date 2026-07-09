using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Common.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
