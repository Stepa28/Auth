using FluentValidation;
using Marvelous.Contracts.ResponseModels;

namespace Auth.BusinessLayer.Validators;

public class IdentityResponseModelValidator : AbstractValidator<IdentityResponseModel>
{
    public IdentityResponseModelValidator()
    {
        RuleFor(t => t.IssuerMicroservice).NotEmpty();
    }
}