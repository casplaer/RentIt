using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Specifications;

namespace RentIt.Users.Application.Specifications.Users
{
    public class GetUserByIdSpecification : Specification<User>
    {
        public GetUserByIdSpecification(Guid id)
            : base(u => u.UserId == id)
        {
            AddInclude(u => u.Role);
            AddInclude(u => u.Profile);
        }
    }
}
