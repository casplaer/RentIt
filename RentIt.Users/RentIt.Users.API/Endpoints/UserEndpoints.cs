using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RentIt.Users.API.Extensions;
using RentIt.Users.Application.Commands.Users.Create;
using RentIt.Users.Application.Commands.Users.Delete;
using RentIt.Users.Application.Commands.Users.Login;
using RentIt.Users.Application.Commands.Users.Logout;
using RentIt.Users.Application.Commands.Users.RefreshToken;
using RentIt.Users.Application.Commands.Users.Role;
using RentIt.Users.Application.Commands.Users.Status;
using RentIt.Users.Application.Commands.Users.Update;
using RentIt.Users.Application.Queries.Users;
using RentIt.Users.Contracts.Requests.Users;
using System.Security.Claims;

namespace RentIt.Users.API.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            var usersGroup = app.MapGroup("/users");

            usersGroup.MapGet("/{id:guid}", async (
                Guid id, 
                IMediator mediator, 
                CancellationToken cancellationToken) =>
            {
                var user = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);
                return Results.Ok(user);
            })
            .RequireAuthorization("AdminPolicy");

            usersGroup.MapGet("/", async (
                IMediator mediator, 
                CancellationToken cancellationToken) =>
            {
                var users = await mediator.Send(new GetAllUsersQuery(), cancellationToken);
                return Results.Ok(users);
            })
            .RequireAuthorization("AdminPolicy");

            usersGroup.MapGet("/search", async (
                [AsParameters] GetUsersRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var users = await mediator.Send(new GetFilteredUsersQuery(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Role,
                    request.Country,
                    request.City,
                    request.PhoneNumber,
                    request.Page,
                    request.PageSize), cancellationToken);

                return Results.Ok(users);
            })
            .RequireAuthorization();

            usersGroup.MapPut("/me", async (
                UpdateUserRequest request,
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Results.Unauthorized();
                }

                var result = await mediator.Send(new UpdateUserCommand(
                    userId,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.PhoneNumber,
                    request.Country,
                    request.City,
                    request.Address), cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization();

            usersGroup.MapPatch("/{id}/status", async (
                Guid id, 
                IMediator mediator, 
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new StatusUpdateCommand(id), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization();

            usersGroup.MapPatch("/{id}/role", async (
                UpdateUserRoleCommand request, 
                IMediator mediator, 
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(request, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization("AdminPolicy");

            usersGroup.MapPost("/register", async (
                CreateUserCommand command, 
                IMediator mediator, 
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);
                return Results.Ok("Пользователь успешно зарегистрирован.");
            });

            usersGroup.MapPost("/login", async (
                LoginUserCommand command,
                IMediator mediator,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);

                httpContext.Response.SetAuthCookies(result.AccessToken, result.RefreshToken);
                return Results.Ok();
            });

            usersGroup.MapPost("/logout", async (
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var refreshToken = httpContext.Request.Cookies["RefreshToken"];
                var accessToken = httpContext.Request.Cookies["AccessToken"];

                var result = await mediator.Send(new LogoutUserCommand(accessToken, refreshToken), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization();

            usersGroup.MapPost("/tokens/refresh", async (
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var refreshToken = httpContext.Request.Cookies["RefreshToken"];

                var result = await mediator.Send(new ValidateRefreshTokenCommand(refreshToken), cancellationToken);

                httpContext.Response.SetAuthCookies(result.NewAccessToken, result.NewRefreshToken);
                return Results.Ok();
            })
            .RequireAuthorization();

            usersGroup.MapDelete("/{id:guid}", async (
                Guid id, 
                IMediator mediator, 
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new DeleteUserCommand(id), cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization();
        }
    }
}
