using MediatR;
using VovinamERP.Application.Students.Common;

namespace VovinamERP.Application.Students.GetStudentQr.Handlers;

public sealed class GetStudentQrQueryHandler
    : IRequestHandler<GetStudentQrQuery, GetStudentQrResult>
{
    private readonly IStudentRepository _studentRepository;

    public GetStudentQrQueryHandler(
        IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<GetStudentQrResult> Handle(
        GetStudentQrQuery request,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            throw new InvalidOperationException(
                "Student was not found.");
        }

        var qrContent =
            $"VOVINAMERP|STUDENT|{request.TenantId}|{student.QrToken}";

        return new GetStudentQrResult(
            student.Id,
            student.MemberNumber,
            student.QrToken,
            qrContent);
    }
}