using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using System.Linq.Expressions;

namespace RentIt.Users.Application.Specifications.Users
{
    public class GetFilteredUsersSpecification : Specification<User>
    {
        public GetFilteredUsersSpecification(
            string? firstName,
            string? lastName,
            string? email,
            string? role,
            string? status,
            string? country,
            string? city,
            string? phoneNumber,
            int page,
            int pageSize)
            : base(BuildCriteria(firstName, lastName, email, role, country, city, phoneNumber, status))
        {
            AddInclude(u => u.Role);
            AddInclude(u => u.Profile);

            SetPagination(page, pageSize);
        }

        private static Expression<Func<User, bool>> BuildCriteria(
            string? firstName,
            string? lastName,
            string? email,
            string? role,
            string? country,
            string? city,
            string? phoneNumber,
            string? status)
        {
            UserStatus? parsedStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<UserStatus>(status, true, out var s))
            {
                parsedStatus = s;
            }

            return u =>
                (string.IsNullOrEmpty(firstName) || u.FirstName.Contains(firstName)) &&
                (string.IsNullOrEmpty(lastName) || u.LastName.Contains(lastName)) &&
                (string.IsNullOrEmpty(email) || u.Email.Contains(email)) &&
                (string.IsNullOrEmpty(role) || u.Role.RoleName.Contains(role)) &&
                (string.IsNullOrEmpty(country) || (u.Profile != null && u.Profile.Country.Contains(country))) &&
                (string.IsNullOrEmpty(city) || (u.Profile != null && u.Profile.City.Contains(city))) &&
                (string.IsNullOrEmpty(phoneNumber) || (u.Profile != null && u.Profile.PhoneNumber.Contains(phoneNumber))) &&
                (
                    string.IsNullOrEmpty(status) || (parsedStatus.HasValue && u.Status == parsedStatus.Value)
                );
        }
    }
}
