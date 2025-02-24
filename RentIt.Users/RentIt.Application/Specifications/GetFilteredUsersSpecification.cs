using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Specifications
{
    public class GetFilteredUsersSpecification : Specification<User>
    {
        public GetFilteredUsersSpecification(
            string? FirstName,
            string? lastName,
            string? email,
            string? country,
            string? city,
            string? phoneNumber,
            int page,
            int pageSize) 
            : base()
        {
            AddInclude(user => user.Role);
            AddInclude(user => user.Profile);
        }
    }
}
