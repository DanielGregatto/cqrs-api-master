using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.LogError.Commands.SaveError
{
    public class SaveErrorCommandValidator : AbstractValidator<SaveErrorCommand>
    {
        public SaveErrorCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.ErrorDescription)
                .NotEmpty().WithMessage(localizer["LogError_ErrorRequired"]);
        }
    }
}
