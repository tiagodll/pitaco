FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
ARG buildconfig
WORKDIR /app
RUN env
RUN dotnet publish ./pitaco.Server -o /app -c Release

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime-env
ARG buildconfig
WORKDIR /publish
COPY --from=build-env /app .
ENTRYPOINT [ "dotnet","pitaco.Server.dll" ]