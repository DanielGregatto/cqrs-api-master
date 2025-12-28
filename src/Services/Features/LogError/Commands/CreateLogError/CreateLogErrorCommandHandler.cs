using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Services.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.LogError.Commands.CreateLogError
{
    public class CreateLogErrorCommandHandler : BaseCommandHandler,
        IRequestHandler<CreateLogErrorCommand, Result<LogErrorDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateLogErrorCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public CreateLogErrorCommandHandler(
            AppDbContext context,
            IUser user,
            IUnitOfWork unitOfWork,
            IValidator<CreateLogErrorCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer) : base(context, user)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<LogErrorDto>> Handle(CreateLogErrorCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation
            var validationError = await ValidateAsync<CreateLogErrorCommand, LogErrorDto>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var logError = new Domain.LogError
            {
                UserId = request.UserId,
                Code = request.Code,
                Record = request.Record
            };

            await _context.LogErrors.AddAsync(logError, cancellationToken);

            var saved = await _unitOfWork.CommitAsync(cancellationToken);
            if (saved == 0)
                return Result<LogErrorDto>.Failure(Error.Database(_localizer["LogError_CreateError"]));

            return Result<LogErrorDto>.Success(LogErrorDto.FromEntity(logError));
        }
    }
}
