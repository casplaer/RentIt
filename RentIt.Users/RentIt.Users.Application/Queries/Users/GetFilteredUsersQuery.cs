using MediatR;
using RentIt.Users.Contracts.Responses.Users;

namespace RentIt.Users.Application.Queries.Users
{
    public record GetFilteredUsersQuery(
        string? FirstName,
        string? LastName,
        string? Email,
        string? Role,
        string? Country,
        string? City,
        string? PhoneNumber,
        int Page,
        int PageSize
        ) : IRequest<GetUsersResponse>;
}
