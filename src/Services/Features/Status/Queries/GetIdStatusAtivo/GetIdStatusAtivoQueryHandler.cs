using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Status.Queries.GetIdStatusAtivo
{
    public class GetIdStatusAtivoQueryHandler : BaseQueryHandler, IRequestHandler<GetIdStatusAtivoQuery, Result<System.Guid>>
    {
        public GetIdStatusAtivoQueryHandler(
            AppDbContext context,
            IUser user) : base(context, user)
        {
        }

        public async Task<Result<System.Guid>> Handle(GetIdStatusAtivoQuery request, CancellationToken cancellationToken)
        {
            var status = await _context.Status
                .Where(x => !x.Deleted && !x.Bloquear && x.Escopo == "Geral")
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if(status == null)
                return Result<System.Guid>.NotFound("Status Ativo não encontrado");

            return Result<System.Guid>.Success(status.Id);
        }
    }
}
