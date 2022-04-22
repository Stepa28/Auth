using FluentValidation;
using Marvelous.Contracts.ExchangeModels;

namespace Auth.BusinessLayer.Validators;

public class LeadShortExchangeModelValidator : AbstractValidator<LeadShortExchangeModel>
{
    public LeadShortExchangeModelValidator()
    {
        RuleFor(t => t.Email).NotEmpty();
        RuleFor(t => t.Role).NotEmpty();
    }
}