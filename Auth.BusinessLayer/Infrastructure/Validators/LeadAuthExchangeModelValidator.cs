using FluentValidation;
using Marvelous.Contracts.ExchangeModels;

namespace Auth.BusinessLayer.Validators;

public class LeadAuthExchangeModelValidator : AbstractValidator<LeadAuthExchangeModel>
{
    public LeadAuthExchangeModelValidator()
    {
        RuleFor(t => t.Email).NotEmpty();
        RuleFor(t => t.HashPassword).NotEmpty();
    }
}