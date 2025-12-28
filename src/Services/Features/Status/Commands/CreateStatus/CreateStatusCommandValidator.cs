using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Status.Commands.CreateStatus
{
    public class CreateStatusCommandValidator : AbstractValidator<CreateStatusCommand>
    {
        public CreateStatusCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage(localizer["Status_NameRequired"])
                .MaximumLength(200).WithMessage(localizer["Status_NameMaxLength"]);

            RuleFor(x => x.Escopo)
                .NotEmpty().WithMessage(localizer["Status_ScopeRequired"])
                .MaximumLength(100).WithMessage(localizer["Status_ScopeMaxLength"]);

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage(localizer["Status_DescriptionMaxLength"]);
        }
    }
}
