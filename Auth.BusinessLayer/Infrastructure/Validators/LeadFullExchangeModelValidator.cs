using FluentValidation;
using Marvelous.Contracts.ExchangeModels;

namespace Auth.BusinessLayer.Validators;

public class LeadFullExchangeModelValidator : AbstractValidator<LeadFullExchangeModel>
{
    public LeadFullExchangeModelValidator()
    {
        RuleFor(t => t.Email).NotEmpty();
        RuleFor(t => t.Password).NotEmpty();
        RuleFor(t => t.IsBanned).NotNull();
    }
}