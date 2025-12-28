using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.LogError.Commands.CreateLogError
{
    public class CreateLogErrorCommandValidator : AbstractValidator<CreateLogErrorCommand>
    {
        public CreateLogErrorCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Record)
                .NotEmpty().WithMessage(localizer["LogError_ErrorRequired"]);
        }
    }
}
