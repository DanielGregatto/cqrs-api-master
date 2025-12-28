using Domain.DTO.Infrastructure.CQRS;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Status.Queries.GetIdStatusInativo
{
    public class GetIdStatusInativoQueryHandler : BaseQueryHandler, IRequestHandler<GetIdStatusInativoQuery, Result<Guid>>
    {
        public GetIdStatusInativoQueryHandler(
            Data.Context.AppDbContext context,
            IUser user) : base(context, user)
        {
        }

        public async Task<Result<Guid>> Handle(GetIdStatusInativoQuery request, CancellationToken cancellationToken)
        {
            var status = await _context.Status
                .Where(x => !x.Deleted && x.Bloquear && x.Escopo == "Geral")
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if(status == null) 
                return Result<Guid>.NotFound("Status Inativo não encontrado");

            return Result<Guid>.Success(status.Id);
        }
    }
}
