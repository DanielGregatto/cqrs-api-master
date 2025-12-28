using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.LogError.Queries.GetAllLogErrors
{
    public class GetAllLogErrorsQueryHandler : BaseQueryHandler,
        IRequestHandler<GetAllLogErrorsQuery, Result<IEnumerable<LogErrorDto>>>
    {
        public GetAllLogErrorsQueryHandler(
            AppDbContext context,
            IUser user) : base(context, user)
        {
        }

        public async Task<Result<IEnumerable<LogErrorDto>>> Handle(GetAllLogErrorsQuery request, CancellationToken cancellationToken)
        {
            var logErrors = await _context.LogErrors
                .Where(x => !x.Deleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var logErrorDtos = logErrors.Select(LogErrorDto.FromEntity).ToList();

            return Result<IEnumerable<LogErrorDto>>.Success(logErrorDtos);
        }
    }
}
