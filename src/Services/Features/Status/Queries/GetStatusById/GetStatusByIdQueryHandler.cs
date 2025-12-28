using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Status.Queries.GetStatusById
{
    public class GetStatusByIdQueryHandler : BaseQueryHandler,
        IRequestHandler<GetStatusByIdQuery, Result<StatusDto>>
    {
        private readonly ILogger<GetStatusByIdQueryHandler> _logger;
        private readonly IStringLocalizer _localizer;

        public GetStatusByIdQueryHandler(
            AppDbContext context,
            IUser user,
            ILogger<GetStatusByIdQueryHandler> logger,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result<StatusDto>> Handle(GetStatusByIdQuery request, CancellationToken cancellationToken)
        {
            // Correlation ID automatically prepended by CorrelationLogger
            _logger.LogInformation("Querying status by ID: {StatusId}", request.Id);

            var status = await _context.Status
                .Where(x => !x.Deleted)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (status == null)
            {
                _logger.LogWarning("Status not found with ID: {StatusId}", request.Id);
                return Result<StatusDto>.NotFound(_localizer["Status_NotFound"]);
            }

            _logger.LogInformation("Successfully retrieved status {StatusId} with name: {StatusName}",
                status.Id, status.Nome);

            return Result<StatusDto>.Success(StatusDto.FromEntity(status));
        }
    }
}
