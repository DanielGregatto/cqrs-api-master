using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Services.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Status.Commands.UpdateStatus
{
    public class UpdateStatusCommandHandler : BaseCommandHandler,
        IRequestHandler<UpdateStatusCommand, Result<StatusDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<UpdateStatusCommand> _validator;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public UpdateStatusCommandHandler(
            AppDbContext context,
            IUser user,
            IUnitOfWork unitOfWork,
            IValidator<UpdateStatusCommand> validator,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _localizer = localizer;
        }

        public async Task<Result<StatusDto>> Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
        {
            // Explicit validation
            var validationError = await ValidateAsync<UpdateStatusCommand, StatusDto>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            var status = await _context.Status
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.Deleted, cancellationToken);

            if (status == null)
                return Result<StatusDto>.NotFound(_localizer["Status_NotFound"]);

            status.Escopo = request.Escopo;
            status.Nome = request.Nome;
            status.Descricao = request.Descricao;
            status.Bloquear = request.Bloquear;
            status.Ativo = request.Ativo;

            _context.Status.Update(status);

            var saved = await _unitOfWork.CommitAsync(cancellationToken);
            if (saved == 0)
                return Result<StatusDto>.Failure(_localizer["Status_UpdateError"], ErrorTypes.Database);

            return Result<StatusDto>.Success(StatusDto.FromEntity(status));
        }
    }
}
