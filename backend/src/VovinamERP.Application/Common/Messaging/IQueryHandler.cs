using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Common.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
