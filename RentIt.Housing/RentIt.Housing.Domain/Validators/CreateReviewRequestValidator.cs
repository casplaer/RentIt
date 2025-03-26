using FluentValidation;
using RentIt.Housing.Domain.Contracts.Requests.Reviews;

namespace RentIt.Housing.Domain.Validators
{
    public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
    {
        public CreateReviewRequestValidator()
        {
            RuleFor(x => x)
                .NotNull()
                .WithMessage("Запрос не должен быть пустым.");

            RuleFor(x => x.Rating)
                .NotEmpty()
                .LessThanOrEqualTo(5).WithMessage("Оценка не может быть выше 5.")
                .GreaterThanOrEqualTo(1).WithMessage("Оценка не может быть ниже 1.");

            RuleFor(x => x.Comment)
                .NotEmpty()
                .MinimumLength(5).WithMessage("Комментарий должен содержать больше 5 символов.")
                .MaximumLength(1500).WithMessage("Комментарий не может быть длиннее 1500 символов");
        }
    }
}
