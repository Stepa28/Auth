using FluentValidation;
using Marvelous.Contracts.ResponseModels;

namespace Auth.BusinessLayer.Validators;

public class ConfigResponseModelValidator : AbstractValidator<ConfigResponseModel>
{
    public ConfigResponseModelValidator()
    {
        RuleFor(t => t.Key).NotEmpty();
        RuleFor(t => t.Value).NotEmpty();
    }
}