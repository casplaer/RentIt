FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RentIt.Users.API/RentIt.Users.API.csproj", "RentIt.Users.API/"]
COPY ["RentIt.Users.Application/RentIt.Users.Application.csproj", "RentIt.Users.Application/"]
COPY ["RentIt.Users.Core/RentIt.Users.Core.csproj", "RentIt.Users.Core/"]
COPY ["RentIt.Users.Infrastructure/RentIt.Users.Infrastructure.csproj", "RentIt.Users.Infrastructure/"]
COPY ["RentIt.Users.Contracts/RentIt.Users.Contracts.csproj", "RentIt.Users.Contracts/"]

RUN dotnet restore "RentIt.Users.API/RentIt.Users.API.csproj"

COPY . .

WORKDIR "/src/RentIt.Users.API"

RUN dotnet publish "RentIt.Users.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RentIt.Users.API.dll"]
