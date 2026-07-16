using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Students.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.RegenerateStudentQr.Handlers;

public sealed class RegenerateStudentQrCommandHandler
    : IRequestHandler<
        RegenerateStudentQrCommand,
        Result<RegenerateStudentQrResult>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegenerateStudentQrCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegenerateStudentQrResult>> Handle(
        RegenerateStudentQrCommand request,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Result<RegenerateStudentQrResult>.Failure(
                new Error(
                    "STUDENT_QR_001",
                    "Student was not found."));
        }

        var regenerateResult = student.RegenerateQrToken(
            request.RegeneratedByUserId);

        if (regenerateResult.IsFailure)
        {
            return Result<RegenerateStudentQrResult>.Failure(
                regenerateResult.Error);
        }

        _studentRepository.Update(student);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var qrContent =
            $"VOVINAMERP|STUDENT|{student.TenantId}|{student.QrToken}";

        return Result<RegenerateStudentQrResult>.Success(
            new RegenerateStudentQrResult(
                student.Id,
                student.MemberNumber,
                student.QrToken,
                qrContent));
    }
}