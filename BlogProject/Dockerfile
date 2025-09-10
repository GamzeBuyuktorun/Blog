FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
ADD . .
RUN dotnet restore
RUN dotnet publish -c Release -o dist

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/dist .
ENTRYPOINT ["dotnet", "BlogProject.dll"]
