using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
using Services.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.LogError.Commands.SaveError
{
    public class SaveErrorCommandHandler : BaseCommandHandler,
        IRequestHandler<SaveErrorCommand, Result<LogErrorDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<SaveErrorCommand> _validator;

        public SaveErrorCommandHandler(
            AppDbContext context,
            IUser user,
            IUnitOfWork unitOfWork,
            IValidator<SaveErrorCommand> validator) : base(context, user)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<Result<LogErrorDto>> Handle(SaveErrorCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation
            var validationError = await ValidateAsync<SaveErrorCommand, LogErrorDto>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var error = new Domain.LogError
            {
                Code = request.Code,
                Record = request.ErrorDescription,
                UserId = request.UserId
            };

            await _context.LogErrors.AddAsync(error, cancellationToken);

            var saved = await _unitOfWork.CommitAsync(cancellationToken);
            if (saved == 0)
                return Result<LogErrorDto>.Failure(Error.Database("Erro ao salvar log de erro"));

            return Result<LogErrorDto>.Success(LogErrorDto.FromEntity(error));
        }
    }
}
