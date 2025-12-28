using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.LogError.Commands.DeleteLogError
{
    public class DeleteLogErrorCommandHandler : BaseCommandHandler,
        IRequestHandler<DeleteLogErrorCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public DeleteLogErrorCommandHandler(
            AppDbContext context,
            IUser user,
            IUnitOfWork unitOfWork,
            IStringLocalizer<Domain.Resources.Messages> localizer) : base(context, user)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<Result> Handle(DeleteLogErrorCommand request, CancellationToken cancellationToken)
        {
            var logError = await _context.LogErrors
                .Where(x => !x.Deleted)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (logError == null)
                return Result.NotFound(_localizer["LogError_NotFound"]);

            logError.Deleted = true;
            _context.LogErrors.Update(logError);

            var saved = await _unitOfWork.CommitAsync(cancellationToken);
            if (saved == 0)
                return Result.Failure(Error.Database(_localizer["LogError_DeleteError"]));

            return Result.Success();
        }
    }
}
