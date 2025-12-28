using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Status.Commands.CreateStatus
{
    public class CreateStatusCommandHandler : BaseCommandHandler,
        IRequestHandler<CreateStatusCommand, Result<StatusDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateStatusCommand> _validator;
        private readonly ILogger<CreateStatusCommandHandler> _logger;
        private readonly IStringLocalizer _localizer;

        public CreateStatusCommandHandler(
            AppDbContext context,
            IUser user,
            IUnitOfWork unitOfWork,
            IValidator<CreateStatusCommand> validator,
            ILogger<CreateStatusCommandHandler> logger,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result<StatusDto>> Handle(CreateStatusCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing CreateStatusCommand for status: {StatusName}, Escopo: {Escopo}",
                request.Nome, request.Escopo);

            var validationError = await ValidateAsync<CreateStatusCommand, StatusDto>(_validator, request, cancellationToken);
            if (validationError != null)
            {
                _logger.LogWarning("Validation failed for CreateStatusCommand. Status: {StatusName}",
                    request.Nome);
                return validationError;
            }

            var status = new Domain.Status
            {
                Escopo = request.Escopo,
                Nome = request.Nome,
                Descricao = request.Descricao,
                Bloquear = request.Bloquear,
                Ativo = request.Ativo
            };

            await _context.Status.AddAsync(status, cancellationToken);

            var saved = await _unitOfWork.CommitAsync(cancellationToken);
            if (saved == 0)
            {
                _logger.LogError("Failed to save status to database. Status: {StatusName}",
                    request.Nome);
                return Result<StatusDto>.Failure(_localizer["DatabaseError"], ErrorTypes.Database);
            }

            _logger.LogInformation("Successfully created status {StatusId} with name: {StatusName}",
                status.Id, status.Nome);

            return Result<StatusDto>.Success(StatusDto.FromEntity(status));
        }
    }
}
