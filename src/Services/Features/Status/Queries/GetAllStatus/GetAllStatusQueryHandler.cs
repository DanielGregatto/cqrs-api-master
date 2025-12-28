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

namespace Services.Features.Status.Queries.GetAllStatus
{
    public class GetAllStatusQueryHandler : BaseQueryHandler,
        IRequestHandler<GetAllStatusQuery, Result<IEnumerable<StatusDto>>>
    {
        public GetAllStatusQueryHandler(AppDbContext context, IUser user)
            : base(context, user)
        {
        }

        public async Task<Result<IEnumerable<StatusDto>>> Handle(GetAllStatusQuery request, CancellationToken cancellationToken)
        {
            var statusList = await _context.Status
                .Where(x => !x.Deleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var statusDtos = statusList.Select(StatusDto.FromEntity).ToList();

            return Result<IEnumerable<StatusDto>>.Success(statusDtos);
        }
    }
}
