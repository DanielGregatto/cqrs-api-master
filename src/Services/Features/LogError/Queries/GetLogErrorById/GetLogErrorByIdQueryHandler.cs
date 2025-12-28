using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.LogError.Queries.GetLogErrorById
{
    public class GetLogErrorByIdQueryHandler : BaseQueryHandler,
        IRequestHandler<GetLogErrorByIdQuery, Result<LogErrorDto>>
    {
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public GetLogErrorByIdQueryHandler(
            AppDbContext context,
            IUser user,
            IStringLocalizer<Domain.Resources.Messages> localizer) : base(context, user)
        {
            _localizer = localizer;
        }

        public async Task<Result<LogErrorDto>> Handle(GetLogErrorByIdQuery request, CancellationToken cancellationToken)
        {
            var logError = await _context.LogErrors
                .Where(x => !x.Deleted)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (logError == null)
                return Result<LogErrorDto>.NotFound(_localizer["LogError_NotFound"]);

            return Result<LogErrorDto>.Success(LogErrorDto.FromEntity(logError));
        }
    }
}
