﻿using FluentValidation;
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
                    request.Country,
                    request.City,
                    request.PhoneNumber,
                    request.Page, 
                    request.PageSize));

                return users;
            });

            usersGroup.MapPut("/", async (UpdateUserCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok();
            }).RequireAuthorization();

            usersGroup.MapPut("/status/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var result = await mediator.Send(new StatusUpdateCommand(id));
                return Results.Ok();
            }).RequireAuthorization();

            usersGroup.MapPut("/role", async (UpdateUserRoleCommand request, IMediator mediator) => 
            {
                var result = await mediator.Send(request);

                return Results.Ok();
            }).RequireAuthorization("AdminPolicy");

            usersGroup.MapPost("/sign-up", async (CreateUserCommand command, IMediator mediator) =>
            {
                var userId = await mediator.Send(command);
                return Results.Created($"/users/{userId}", new { userId });
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
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(15)
                });

                return Results.Ok(result);
            });

            usersGroup.MapPost("/refresh", async (HttpContext httpContext, IMediator mediator, IJwtProvider jwtProvider) =>
            {
                var refreshToken = httpContext.Request.Cookies["RefreshToken"];

                var user = await mediator.Send(new ValidateRefreshTokenCommand(refreshToken)); 

                var newAccessToken = jwtProvider.GenerateAccessToken(user);

                httpContext.Response.Cookies.Append("AccessToken", newAccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(15)
                });

                return Results.Ok(new { accessToken = newAccessToken });
            });

            usersGroup.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteUserCommand(id));
                return Results.Ok();
            }).RequireAuthorization();
        }
    }
}
