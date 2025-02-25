﻿using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Specifications;

namespace RentIt.Users.Application.Specifications.Users
{
    public class GetFilteredUsersSpecification : Specification<User>
    {
        public GetFilteredUsersSpecification(
            string? firstName,
            string? lastName,
            string? email,
            string? country,
            string? city,
            string? phoneNumber,
            int page,
            int pageSize)
            : base
            (u =>
                (string.IsNullOrEmpty(firstName) || u.FirstName.Contains(firstName)) &&
                (string.IsNullOrEmpty(lastName) || u.LastName.Contains(lastName)) &&
                (string.IsNullOrEmpty(email) || u.Email.Contains(email)) &&
                (string.IsNullOrEmpty(country) || (u.Profile != null && u.Profile.Country.Contains(country))) &&
                (string.IsNullOrEmpty(city) || (u.Profile != null && u.Profile.City.Contains(city))) &&
                (string.IsNullOrEmpty(phoneNumber) || (u.Profile != null && u.Profile.PhoneNumber.Contains(phoneNumber)))
            )
        {
            AddInclude(u => u.Role);
            AddInclude(u => u.Profile);

            SetPagination(page, pageSize);
        }
    }
}
