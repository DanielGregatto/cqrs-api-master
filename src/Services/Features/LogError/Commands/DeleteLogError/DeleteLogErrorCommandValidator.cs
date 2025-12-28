using FluentValidation;
using Microsoft.Extensions.Localization;
using System;

namespace Services.Features.LogError.Commands.DeleteLogError
{
    public class DeleteLogErrorCommandValidator : AbstractValidator<DeleteLogErrorCommand>
    {
        public DeleteLogErrorCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty).WithMessage(localizer["LogError_IdRequired"]);
        }
    }
}
