using FluentValidation;
using Microsoft.Extensions.Localization;
using System;

namespace Services.Features.LogError.Commands.UpdateLogError
{
    public class UpdateLogErrorCommandValidator : AbstractValidator<UpdateLogErrorCommand>
    {
        public UpdateLogErrorCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty).WithMessage(localizer["LogError_IdRequired"]);

            RuleFor(x => x.Record)
                .NotEmpty().WithMessage(localizer["LogError_ErrorRequired"]);
        }
    }
}
