using FluentValidation;
using Marvelous.Contracts.ResponseModels;

namespace Auth.BusinessLayer.Validation;

public class ConfigResponseModelValidator : AbstractValidator<ConfigResponseModel>
{
    public ConfigResponseModelValidator()
    {
        RuleFor(t => t.Key).NotEmpty();
        RuleFor(t => t.Value).NotEmpty();
    }
}