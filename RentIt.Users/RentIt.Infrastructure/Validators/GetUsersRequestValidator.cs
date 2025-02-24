using FluentValidation;
using RentIt.Users.Contracts.Requests.Users;

namespace RentIt.Users.Infrastructure.Validators
{
    public class GetUsersRequestValidator : AbstractValidator<GetUsersRequest>
    {
        public GetUsersRequestValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Номер страницы должен быть больше 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Размер страницы должен быть больше 0.");

            
        }
    }
}
