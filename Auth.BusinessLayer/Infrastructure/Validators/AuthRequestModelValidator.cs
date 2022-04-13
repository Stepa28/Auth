using FluentValidation;
using Marvelous.Contracts.RequestModels;

namespace Auth.BusinessLayer.Validators;

public class AuthRequestModelValidator : AbstractValidator<AuthRequestModel>
{
    public AuthRequestModelValidator()
    {
        RuleFor(t => t.Email).NotEmpty();
        RuleFor(t => t.Password).NotEmpty();
    }
}