using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Services.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Status.Commands.DeleteStatus
{
    public class DeleteStatusCommandHandler : BaseCommandHandler,
        IRequestHandler<DeleteStatusCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public DeleteStatusCommandHandler(AppDbContext context, IUser user, IUnitOfWork unitOfWork, IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<Result> Handle(DeleteStatusCommand request, CancellationToken cancellationToken)
        {
            var status = await _context.Status
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.Deleted, cancellationToken);

            if (status == null)
                return Result.NotFound(_localizer["Status_NotFound"]);

            // Soft delete
            status.Deleted = true;
            _context.Status.Update(status);

            var saved = await _unitOfWork.CommitAsync(cancellationToken);
            if (saved == 0)
                return Result.Failure(_localizer["Status_DeleteError"], ErrorTypes.Database);

            return Result.Success();
        }
    }
}
