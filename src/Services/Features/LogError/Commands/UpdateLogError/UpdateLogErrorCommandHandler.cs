using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.LogError.Commands.UpdateLogError
{
    public class UpdateLogErrorCommandHandler : BaseCommandHandler,
        IRequestHandler<UpdateLogErrorCommand, Result<LogErrorDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<UpdateLogErrorCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public UpdateLogErrorCommandHandler(
            AppDbContext context,
            IUser user,
            IUnitOfWork unitOfWork,
            IValidator<UpdateLogErrorCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer) : base(context, user)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<LogErrorDto>> Handle(UpdateLogErrorCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation
            var validationError = await ValidateAsync<UpdateLogErrorCommand, LogErrorDto>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var logError = await _context.LogErrors
                .Where(x => !x.Deleted)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (logError == null)
                return Result<LogErrorDto>.NotFound(_localizer["LogError_NotFound"]);

            logError.UserId = request.UserId;
            logError.Code = request.Code;
            logError.Record = request.Record;

            _context.LogErrors.Update(logError);

            var saved = await _unitOfWork.CommitAsync(cancellationToken);
            if (saved == 0)
                return Result<LogErrorDto>.Failure(Error.Database(_localizer["LogError_UpdateError"]));

            return Result<LogErrorDto>.Success(LogErrorDto.FromEntity(logError));
        }
    }
}
