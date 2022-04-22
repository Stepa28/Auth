using FluentValidation;
using Marvelous.Contracts.ResponseModels;

namespace Auth.BusinessLayer.Validators;

public class ExceptionResponseModelValidator : AbstractValidator<ExceptionResponseModel>
{
    public ExceptionResponseModelValidator()
    {
        RuleFor(t => t.Code).NotEmpty();
        RuleFor(t => t.Message).NotEmpty();
    }
}