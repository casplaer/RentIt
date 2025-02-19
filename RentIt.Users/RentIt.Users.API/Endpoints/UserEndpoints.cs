using MediatR;
using RentIt.Users.Application.Commands.Users.Create;
using RentIt.Users.Application.Commands.Users.Delete;
using RentIt.Users.Application.Commands.Users.Login;
using RentIt.Users.Application.Commands.Users.Update;
using RentIt.Users.Application.Queries.Users;

namespace RentIt.Users.API.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            var usersGroup = app.MapGroup("/users");

            usersGroup.MapPost("/register", async (CreateUserCommand command, IMediator mediator) =>
            {
                var userId = await mediator.Send(command);
                return Results.Created($"/users/{userId}", new { userId });
            });

            usersGroup.MapPost("/login", async (
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
                    Expires = DateTime.UtcNow.AddMinutes(60)
                });

                return Results.Ok(result);
            });

            usersGroup.MapPut("/", async (UpdateUserCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result ? Results.Ok() : Results.NotFound();
            });

            usersGroup.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteUserCommand(id));
                return result ? Results.Ok() : Results.NotFound();
            });

            usersGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var user = await mediator.Send(new GetUserByIdQuery(id));
                return user != null ? Results.Ok(user) : Results.NotFound();
            })
            .RequireAuthorization();

            usersGroup.MapGet("/", async (IMediator mediator) =>
            {
                var users = await mediator.Send(new GetAllUsersQuery());
                return users.Any() ? Results.Ok(users) : Results.NotFound();
            });
        }
    }
}
