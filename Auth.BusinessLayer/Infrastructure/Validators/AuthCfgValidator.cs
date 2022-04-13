using FluentValidation;
using Marvelous.Contracts.Configurations;

namespace Auth.BusinessLayer.Validators;

public class AuthCfgValidator : AbstractValidator<AuthCfg>
{
    public AuthCfgValidator()
    {
        RuleFor(t => t.Key).NotEmpty();
        RuleFor(t => t.Value).NotEmpty();
    }
}