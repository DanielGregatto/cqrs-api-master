using FluentValidation;
using Microsoft.Extensions.Localization;
using System;

namespace Services.Features.Status.Commands.UpdateStatus
{
    public class UpdateStatusCommandValidator : AbstractValidator<UpdateStatusCommand>
    {
        public UpdateStatusCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty).WithMessage(localizer["Status_IdRequired"]);

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
