using FluentValidation;
using RentIt.Users.Contracts.Requests.Users;

namespace RentIt.Users.Application.Validators
{
    public class GetUsersRequestValidator : AbstractValidator<GetUsersRequest>
    {
        public GetUsersRequestValidator()
        {
            RuleFor(u => u.Page)
                .GreaterThan(0)
                .WithMessage("Номер страницы должен быть больше 0.");

            RuleFor(u => u.PageSize)
                .GreaterThan(0)
                .WithMessage("Размер страницы должен быть больше 0.");
        }
    }
}
