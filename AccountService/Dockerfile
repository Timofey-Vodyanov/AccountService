FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AccountService.csproj", "."]
RUN dotnet restore "AccountService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "AccountService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccountService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountService.dll"]