FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY WebApplication1/WebApplication1.csproj WebApplication1/
RUN dotnet restore WebApplication1/WebApplication1.csproj

COPY WebApplication1/ WebApplication1/
RUN dotnet publish WebApplication1/WebApplication1.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebApplication1.dll"]