using Data.Context;
using Domain.DTO.Infrastructure.API;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.LogError.Queries.SearchLogErrorsPaginated
{
    public class SearchLogErrorsPaginatedQueryHandler : BaseQueryHandler,
        IRequestHandler<SearchLogErrorsPaginatedQuery, Result<PaginatedResponseDto<LogErrorDto>>>
    {
        public SearchLogErrorsPaginatedQueryHandler(
            AppDbContext context,
            IUser user) : base(context, user)
        {
        }

        public async Task<Result<PaginatedResponseDto<LogErrorDto>>> Handle(
            SearchLogErrorsPaginatedQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.LogErrors
                .Where(x => !x.Deleted)
                .AsNoTracking();

            // Apply predicate filter if provided
            if (request.Predicate != null)
                query = query.Where(request.Predicate);

            // Apply ordering if specified
            if (!string.IsNullOrEmpty(request.OrderByProperty))
            {
                query = request.OrderByDescending
                    ? query.OrderByDescending(x => EF.Property<object>(x, request.OrderByProperty))
                    : query.OrderBy(x => EF.Property<object>(x, request.OrderByProperty));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var logErrorDtos = items.Select(LogErrorDto.FromEntity).ToList();

            var paginatedResponse = new PaginatedResponseDto<LogErrorDto>
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                HasPreviousPage = request.PageIndex > 1,
                HasNextPage = request.PageIndex * request.PageSize < totalCount,
                Items = logErrorDtos
            };

            return Result<PaginatedResponseDto<LogErrorDto>>.Success(paginatedResponse);
        }
    }
}
