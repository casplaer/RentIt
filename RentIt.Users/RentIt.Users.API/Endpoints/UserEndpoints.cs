using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RentIt.Users.Application.Commands.Users.Create;
using RentIt.Users.Application.Commands.Users.Delete;
using RentIt.Users.Application.Commands.Users.Login;
using RentIt.Users.Application.Commands.Users.RefreshToken;
using RentIt.Users.Application.Commands.Users.Role;
using RentIt.Users.Application.Commands.Users.Status;
using RentIt.Users.Application.Commands.Users.Update;
using RentIt.Users.Application.Interfaces;
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

            usersGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var user = await mediator.Send(new GetUserByIdQuery(id));
                return Results.Ok(user);
            })
            .RequireAuthorization("AdminPolicy");

            usersGroup.MapGet("/", async (IMediator mediator) =>
            {
                var users = await mediator.Send(new GetAllUsersQuery());
                return Results.Ok(users);
            });

            usersGroup.MapGet("/search", async (
                [AsParameters] GetUsersRequest request,
                IMediator mediator,
                [FromServices] IValidator<GetUsersRequest> validator) =>
            {
                await validator.ValidateAndThrowAsync(request);

                var users = await mediator
                .Send(new GetFilteredUsersQuery(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Role,
                    request.Country,
                    request.City,
                    request.PhoneNumber,
                    request.Page, 
                    request.PageSize));

                return users;
            });

            usersGroup.MapPut("/", async (
                UpdateUserRequest request, 
                HttpContext httpContext,
                IMediator mediator) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Results.Unauthorized();
                }

                var result = await mediator
                .Send(new UpdateUserCommand(
                    userId,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.PhoneNumber,
                    request.Country,
                    request.City,
                    request.Address));

                return Results.Ok();
            }).RequireAuthorization();

            usersGroup.MapPatch("/status/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var result = await mediator.Send(new StatusUpdateCommand(id));
                return Results.Ok();
            }).RequireAuthorization();

            usersGroup.MapPatch("/role", async (UpdateUserRoleCommand request, IMediator mediator) => 
            {
                var result = await mediator.Send(request);

                return Results.Ok();
            }).RequireAuthorization("AdminPolicy");

            usersGroup.MapPost("/sign-up", async (CreateUserCommand command, IMediator mediator) =>
            {
                await mediator.Send(command);
                return Results.Ok("Пользователь успешно зарегистрирован.");
            });

            usersGroup.MapPost("/sign-in", async (
                LoginUserCommand command,
                IMediator mediator,
                HttpContext httpContext
                ) =>
            {
                var result = await mediator.Send(command);

                httpContext.Response.Cookies.Append("AccessToken", result.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

                httpContext.Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

                return Results.Ok();
            });

            usersGroup.MapPost("/sign-out", async(
                HttpContext httpContext,
                IJwtProvider jwtProvider
                ) =>
            {
                var refreshToken = httpContext.Request.Cookies["RefreshToken"];
                var accessToken = httpContext.Request.Cookies["AccessToken"];

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await jwtProvider.RevokeRefreshTokenAsync(refreshToken);
                }
                if (!string.IsNullOrEmpty(accessToken))
                {
                    await jwtProvider.RevokeAccessTokenAsync(accessToken);
                }

                httpContext.Response.Cookies.Delete("AccessToken");
                httpContext.Response.Cookies.Delete("RefreshToken");

                return Results.Ok("Пользователь успешно вышел.");
            });


            usersGroup.MapPost("/refresh", async (
                HttpContext httpContext,
                IMediator mediator) =>
            {
                var refreshToken = httpContext.Request.Cookies["RefreshToken"];

                var result = await mediator.Send(new ValidateRefreshTokenCommand(refreshToken));

                httpContext.Response.Cookies.Append("AccessToken", result.NewAccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

                httpContext.Response.Cookies.Append("RefreshToken", result.NewRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

                return Results.Ok();
            });

            usersGroup.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteUserCommand(id));
                return Results.Ok();
            }).RequireAuthorization();
        }
    }
}
