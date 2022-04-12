using FluentValidation;
using Marvelous.Contracts.ExchangeModels;

namespace Auth.BusinessLayer.Validation;

public class LeadAuthExchangeModelValidator : AbstractValidator<LeadAuthExchangeModel>
{
    public LeadAuthExchangeModelValidator()
    {
        RuleFor(t => t.Email).NotEmpty();
        RuleFor(t => t.HashPassword).NotEmpty();
    }
}