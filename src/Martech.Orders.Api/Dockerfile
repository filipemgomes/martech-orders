# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/Martech.Orders.Domain/*.csproj src/Martech.Orders.Domain/
COPY src/Martech.Orders.Application/*.csproj src/Martech.Orders.Application/
COPY src/Martech.Orders.Infrastructure/*.csproj src/Martech.Orders.Infrastructure/
COPY src/Martech.Orders.Api/*.csproj src/Martech.Orders.Api/
RUN dotnet restore src/Martech.Orders.Api/Martech.Orders.Api.csproj

COPY src/ src/
RUN dotnet publish src/Martech.Orders.Api/Martech.Orders.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN mkdir /app/data && chown $APP_UID /app/data
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app .

USER $APP_UID
ENTRYPOINT ["dotnet", "Martech.Orders.Api.dll"]
