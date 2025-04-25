using AutoMapper;
using Moq;
using RentIt.Users.Application.Queries.Users;
using RentIt.Users.Application.Specifications.Users;
using RentIt.Users.Contracts.Dto.Users;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Core.Interfaces.Specifications;

namespace RentIt.Users.Tests.Queries
{
    public class GetFilteredUsersQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetFilteredUsersQueryHandler _handler;

        public GetFilteredUsersQueryHandlerTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetFilteredUsersQueryHandler(_userRepoMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredUsers_WhenValidFilterIsApplied()
        {
            var role = new Role { RoleId = Guid.NewGuid(), RoleName = "Admin" };
            var users = new List<User>
            {
                new User 
                { 
                    UserId = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe", 
                    Email = "john.doe@example.com", 
                    Role = role 
                },
                new User 
                {
                    UserId = Guid.NewGuid(), 
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    Role = role 
                }
            };

            var userDtos = users.Select(u => new UserDto 
                                        { 
                                            UserId = u.UserId, 
                                            FirstName = u.FirstName, 
                                            LastName = u.LastName, 
                                            Email = u.Email, 
                                            RoleName = u.Role.RoleName 
                                        }).ToList();

            var pagination = new PaginatedResult<User>
            {
                Items = users,
                CurrentPage = 1,
                TotalPages = 1
            };

            _userRepoMock.Setup(r => r.GetFilteredUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ISpecification<User> spec, CancellationToken _) =>
                {
                    if (spec.Criteria != null)
                    {
                        users = users.Where(spec.Criteria.Compile()).ToList();
                    }

                    var paged = users
                        .Skip((spec.Page - 1) * spec.PageSize)
                        .Take(spec.PageSize)
                        .ToList();

                    return new PaginatedResult<User>
                    {
                        Items = paged,
                        TotalCount = users.Count,
                        PageSize = spec.PageSize,
                        CurrentPage = spec.Page,
                        TotalPages = (int)Math.Ceiling(users.Count / (double)spec.PageSize)
                    };
                });

            _mapperMock.Setup(x => x.Map<ICollection<UserDto>>(It.IsAny<ICollection<User>>()))
               .Returns((ICollection<User> sourceUsers) =>
                   sourceUsers.Select(u => new UserDto
                   {
                       UserId = u.UserId,
                       FirstName = u.FirstName,
                       LastName = u.LastName,
                       Email = u.Email,
                       RoleName = u.Role.RoleName
                   }).ToList());

            var query = new GetFilteredUsersQuery(
                FirstName: "John",
                LastName: null,
                Email: null,
                Role: null,
                Status: null,
                Country: null,
                City: null,
                PhoneNumber: null,
                Page: 1,
                PageSize: 10
            );

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(1, result.Users.Count);
            Assert.Equal("John", result.Users.First().FirstName);

            _userRepoMock.Verify(x => x.GetFilteredUsersAsync(It.IsAny<GetFilteredUsersSpecification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoUsersMatchFilter()
        {
            var pagination = new PaginatedResult<User>
            {
                Items = new List<User>(),
                CurrentPage = 1,
                TotalPages = 1
            };

            _userRepoMock.Setup(x => x.GetFilteredUsersAsync(It.IsAny<GetFilteredUsersSpecification>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(pagination);

            _mapperMock.Setup(x => x.Map<ICollection<UserDto>>(It.Is<ICollection<User>>(u => u.Count == 0)))
                .Returns(new List<UserDto>());

            var query = new GetFilteredUsersQuery(
                FirstName: "NonExistingName",
                LastName: null,
                Email: null,
                Role: null,
                Status: null,
                Country: null,
                City: null,
                PhoneNumber: null,
                Page: 1,
                PageSize: 10
            );

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result.Users);

            _userRepoMock.Verify(x => x.GetFilteredUsersAsync(It.IsAny<GetFilteredUsersSpecification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectPagination_WhenMultiplePagesExist()
        {
            var role = new Role { RoleId = Guid.NewGuid(), RoleName = "User" };
            var users = new List<User>
            {
                new User
                { 
                    UserId = Guid.NewGuid(),
                    FirstName = "John", 
                    LastName = "Doe", 
                    Email = "john.doe@example.com", 
                    Role = role 
                },
                new User 
                { 
                    UserId = Guid.NewGuid(), 
                    FirstName = "Jane", 
                    LastName = "Smith", 
                    Email = "jane.smith@example.com", 
                    Role = role 
                }
            };

            var userDtos = users.Select(u => new UserDto 
                                        { 
                                            UserId = u.UserId, 
                                            FirstName = u.FirstName, 
                                            LastName = u.LastName, 
                                            Email = u.Email, 
                                            RoleName = u.Role.RoleName 
                                        }).ToList();

            _userRepoMock.Setup(r => r.GetFilteredUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ISpecification<User> spec, CancellationToken _) =>
                {
                    if (spec.Criteria != null)
                    {
                        users = users.Where(spec.Criteria.Compile()).ToList();
                    }

                    var paged = users
                        .Skip((spec.Page - 1) * spec.PageSize)
                        .Take(spec.PageSize)
                        .ToList();

                    return new PaginatedResult<User>
                    {
                        Items = paged,
                        TotalCount = users.Count,
                        PageSize = spec.PageSize,
                        CurrentPage = spec.Page,
                        TotalPages = (int)Math.Ceiling(users.Count / (double)spec.PageSize)
                    };
                });

            _mapperMock.Setup(x => x.Map<ICollection<UserDto>>(It.IsAny<ICollection<User>>()))
               .Returns((ICollection<User> sourceUsers) =>
                   sourceUsers.Select(u => new UserDto
                   {
                       UserId = u.UserId,
                       FirstName = u.FirstName,
                       LastName = u.LastName,
                       Email = u.Email,
                       RoleName = u.Role.RoleName
                   }).ToList());

            var query = new GetFilteredUsersQuery(
                FirstName: null,
                LastName: null,
                Email: null,
                Role: null,
                Status: null,
                Country: null,
                City: null,
                PhoneNumber: null,
                Page: 1,
                PageSize: 1
            );

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(1, result.Users.Count);
            Assert.Equal(2, result.TotalPages);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            _userRepoMock.Setup(x => x.GetFilteredUsersAsync(It.IsAny<GetFilteredUsersSpecification>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new Exception("Database error"));

            var query = new GetFilteredUsersQuery(
                FirstName: null,
                LastName: null,
                Email: null,
                Role: null,
                Status: null,
                Country: null,
                City: null,
                PhoneNumber: null,
                Page: 1,
                PageSize: 10
            );

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnUsersWithRoleFilterApplied_WhenRoleFilterIsProvided()
        {
            var roleAdmin = new Role
            { 
                RoleId = Guid.NewGuid(), 
                RoleName = "Admin" 
            };

            var roleUser = new Role 
            { 
                RoleId = Guid.NewGuid(), 
                RoleName = "User" 
            };
            
            var users = new List<User>
            {
                new User 
                { 
                    UserId = Guid.NewGuid(), 
                    FirstName = "John", 
                    LastName = "Doe", 
                    Email = "john.doe@example.com", 
                    Role = roleUser 
                },
                new User 
                { 
                    UserId = Guid.NewGuid(), 
                    FirstName = "Jane", 
                    LastName = "Smith", 
                    Email = "jane.smith@example.com", 
                    Role = roleAdmin 
                }
            };

            var userDtos = users.Select(u => new UserDto 
                                        { 
                                            UserId = u.UserId, 
                                            FirstName = u.FirstName, 
                                            LastName = u.LastName, 
                                            Email = u.Email, 
                                            RoleName = u.Role.RoleName 
                                        }).ToList();

            _userRepoMock.Setup(r => r.GetFilteredUsersAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ISpecification<User> spec, CancellationToken _) =>
                {
                    if (spec.Criteria != null)
                    {
                        users = users.Where(spec.Criteria.Compile()).ToList();
                    }

                    var paged = users
                        .Skip((spec.Page - 1) * spec.PageSize)
                        .Take(spec.PageSize)
                        .ToList();

                    return new PaginatedResult<User>
                    {
                        Items = paged,
                        TotalCount = users.Count,
                        PageSize = spec.PageSize,
                        CurrentPage = spec.Page,
                        TotalPages = (int)Math.Ceiling(users.Count / (double)spec.PageSize)
                    };
                });

            _mapperMock.Setup(x => x.Map<ICollection<UserDto>>(It.IsAny<ICollection<User>>()))
               .Returns((ICollection<User> sourceUsers) =>
                   sourceUsers.Select(u => new UserDto
                   {
                       UserId = u.UserId,
                       FirstName = u.FirstName,
                       LastName = u.LastName,
                       Email = u.Email,
                       RoleName = u.Role.RoleName
                   }).ToList());

            var query = new GetFilteredUsersQuery(
                FirstName: null,
                LastName: null,
                Email: null,
                Role: "Admin",
                Status: null,
                Country: null,
                City: null,
                PhoneNumber: null,
                Page: 1,
                PageSize: 10
            );

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result.Users);
            Assert.Equal("Admin", result.Users.First().RoleName);

            _userRepoMock.Verify(x => x.GetFilteredUsersAsync(It.IsAny<GetFilteredUsersSpecification>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}